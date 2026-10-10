using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Search through messages of all chats.
	/// </summary>
	/// <remarks>
	/// The server searches only inside one chat, so every chat is queried
	/// (a few requests at a time) and the results are merged.
	/// </remarks>
	public class ChatSearchPageViewModel : ViewModel
	{
		const int _minQueryLength = 2;
		const int _resultsPerChat = 5;
		const int _maxParallelRequests = 4;
		const int _searchDelayMilliseconds = 500;

		readonly IPlatformServices _services;

		List<ChatItemModel> _personalChats;
		List<GroupChatModel> _groupChats;
		CancellationTokenSource _searchCancellation;

		public ChatSearchPageViewModel(IPlatformServices services)
		{
			_services = services;
			Results = new List<ChatSearchResultModel>();
			HintText = CrossLocalization.Translate("chat_search_all_hint");
		}

		string _searchText;
		public string SearchText
		{
			get => _searchText;
			set
			{
				if (SetProperty(ref _searchText, value))
				{
					_searchCancellation?.Cancel();
					_searchCancellation = new CancellationTokenSource();
					_ = search(value?.Trim(), _searchCancellation.Token);
				}
			}
		}

		List<ChatSearchResultModel> _results;
		public List<ChatSearchResultModel> Results
		{
			get => _results;
			set => SetProperty(ref _results, value);
		}

		bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			set => SetProperty(ref _isBusy, value);
		}

		/// <summary>
		/// Text shown when there are no results.
		/// </summary>
		string _hintText;
		public string HintText
		{
			get => _hintText;
			set => SetProperty(ref _hintText, value);
		}

		object _selectedItem;
		public object SelectedItem
		{
			get => _selectedItem;
			set
			{
				SetProperty(ref _selectedItem, value);

				if (value is ChatSearchResultModel result)
				{
					SelectedItem = null;
					_ = open(result);
				}
			}
		}

		async Task search(string query, CancellationToken token)
		{
			try
			{
				if (string.IsNullOrEmpty(query) || query.Length < _minQueryLength)
				{
					Results = new List<ChatSearchResultModel>();
					HintText = CrossLocalization.Translate("chat_search_all_hint");
					IsBusy = false;
					return;
				}

				await Task.Delay(_searchDelayMilliseconds, token);
				IsBusy = true;

				await loadChats();
				var results = await searchAllChats(query, token);

				if (token.IsCancellationRequested)
				{
					return;
				}

				Results = results;
				HintText = CrossLocalization.Translate("chat_search_nothing");
				IsBusy = false;
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				IsBusy = false;
			}
		}

		async Task loadChats()
		{
			var userId = AppUserData.UserId;

			if (_personalChats == null)
			{
				var chats = await ChatApiService.GetChats(userId);
				_personalChats = ChatApiService.IsError ? null : chats;
			}

			if (_groupChats == null)
			{
				var subjects = await ChatApiService.GetGroups(userId, ChatRoles.Current(_services));
				_groupChats = ChatApiService.IsError ? null : subjects
					.Where(subject => subject.Groups != null)
					.SelectMany(subject => subject.Groups.Select(group =>
					{
						group.SubjectName = string.IsNullOrEmpty(subject.ShortName) ? subject.Name : subject.ShortName;
						return group;
					}))
					.ToList();
			}
		}

		async Task<List<ChatSearchResultModel>> searchAllChats(string query, CancellationToken token)
		{
			var userId = AppUserData.UserId;
			using var throttle = new SemaphoreSlim(_maxParallelRequests);

			async Task<IEnumerable<ChatSearchResultModel>> searchChat(
				int chatId, bool isGroup, Func<MessageItemModel, ChatSearchResultModel> toResult)
			{
				await throttle.WaitAsync(token);

				try
				{
					var messages = await ChatApiService.SearchMessages(
						userId, chatId, isGroup, query, _resultsPerChat);
					return messages?.Select(toResult) ?? Enumerable.Empty<ChatSearchResultModel>();
				}
				finally
				{
					throttle.Release();
				}
			}

			var tasks = new List<Task<IEnumerable<ChatSearchResultModel>>>();

			foreach (var chat in _personalChats ?? new List<ChatItemModel>())
			{
				tasks.Add(searchChat(chat.Id, false, message => new ChatSearchResultModel
				{
					ChatId = chat.Id,
					PeerUserId = chat.UserId,
					ChatTitle = chat.Name,
					Snippet = snippet(message),
					Time = message.LocalTime
				}));
			}

			foreach (var group in _groupChats ?? new List<GroupChatModel>())
			{
				tasks.Add(searchChat(group.Id, true, message => new ChatSearchResultModel
				{
					ChatId = group.Id,
					IsGroupChat = true,
					GroupId = group.GroupId,
					ChatTitle = group.DisplayName,
					Snippet = snippet(message),
					Time = message.LocalTime
				}));
			}

			var found = await Task.WhenAll(tasks);
			return found
				.SelectMany(results => results)
				.OrderByDescending(result => result.Time)
				.ToList();
		}

		static string snippet(MessageItemModel message)
		{
			var text = message.IsPlainText ?
				Helpers.Forms.HtmlHelper.StripHtml(message.Text) :
				message.FileDisplayText;

			return string.IsNullOrEmpty(message.Name) ? text : $"{message.Name}: {text}";
		}

		Task open(ChatSearchResultModel result) =>
			result.IsGroupChat ?
				_services.Navigation.OpenGroupConversation(
					result.ChatId, result.GroupId, ChatRoles.Current(_services), result.ChatTitle) :
				_services.Navigation.OpenConversation(result.ChatId, result.ChatTitle, result.PeerUserId);
	}
}
