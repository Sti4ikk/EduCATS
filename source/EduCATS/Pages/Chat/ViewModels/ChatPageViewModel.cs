using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.ViewModels
{
	public class ChatPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;

		public ChatPageViewModel(IPlatformServices services)
		{
			_services = services;
			IsPersonalMode = true;
		}

		bool _isActive;

		/// <summary>
		/// Chats are loaded when the tab is opened for the first time,
		/// not when the app starts (all tabs are created at once).
		/// </summary>
		bool _isLoaded;

		/// <summary>
		/// Start following online statuses, unread counters and local settings.
		/// </summary>
		/// <remarks>
		/// Subscriptions to static services live only while the tab is shown,
		/// so the view model isn't kept alive after logout.
		/// </remarks>
		public void Activate()
		{
			if (_isActive)
			{
				return;
			}

			_isActive = true;
			ChatPresenceService.Changed += onPresenceChanged;
			ChatUnreadService.Changed += onUnreadChanged;
			ChatLocalSettings.Changed += onLocalSettingsChanged;
			ChatActivityService.Changed += onActivityChanged;

			if (!_isLoaded)
			{
				_isLoaded = true;
				_ = update(true);
				return;
			}

			// Statuses and counters could change while the tab was hidden.
			syncWithServices();
			applyFilter();
		}

		public void Deactivate()
		{
			if (!_isActive)
			{
				return;
			}

			_isActive = false;
			ChatPresenceService.Changed -= onPresenceChanged;
			ChatUnreadService.Changed -= onUnreadChanged;
			ChatLocalSettings.Changed -= onLocalSettingsChanged;
			ChatActivityService.Changed -= onActivityChanged;
		}

		Command _searchMessagesCommand;

		/// <summary>
		/// Open search through messages of all chats.
		/// </summary>
		public Command SearchMessagesCommand =>
			_searchMessagesCommand ??= new Command(async () => await _services.Navigation.OpenChatSearch());

		void onPresenceChanged(int userId, bool isOnline)
		{
			foreach (var chat in _allChats.Where(c => c.UserId == userId))
			{
				chat.IsOnline = isOnline;
			}
		}

		void onUnreadChanged()
		{
			foreach (var chat in _allChats)
			{
				chat.Unread = ChatUnreadService.Get(chat.Id, false);
			}

			foreach (var group in _allGroupChats)
			{
				group.Unread = ChatUnreadService.Get(group.Id, true);
			}
		}

		void onLocalSettingsChanged()
		{
			syncWithServices();
			applyFilter();
		}

		/// <summary>
		/// Apply online statuses, unread counters and pin/mute settings to the items.
		/// </summary>
		void syncWithServices()
		{
			var pinnedPersonal = ChatLocalSettings.GetPinned(isGroup: false);
			var mutedPersonal = ChatLocalSettings.GetMuted(isGroup: false);
			var pinnedGroup = ChatLocalSettings.GetPinned(isGroup: true);
			var mutedGroup = ChatLocalSettings.GetMuted(isGroup: true);

			foreach (var chat in _allChats)
			{
				chat.IsPinned = pinnedPersonal.Contains(chat.Id);
				chat.IsMuted = mutedPersonal.Contains(chat.Id);
				chat.IsOnline = ChatPresenceService.Get(chat.UserId) ?? chat.IsOnline;
			}

			foreach (var group in _allGroupChats)
			{
				group.IsPinned = pinnedGroup.Contains(group.Id);
				group.IsMuted = mutedGroup.Contains(group.Id);
			}
		}

		List<ChatItemModel> _allChats = new List<ChatItemModel>();
		List<GroupChatModel> _allGroupChats = new List<GroupChatModel>();

		List<ChatItemModel> _chats;
		public List<ChatItemModel> Chats
		{
			get { return _chats; }
			set { SetProperty(ref _chats, value); }
		}

		List<GroupChatModel> _groupChats;
		public List<GroupChatModel> GroupChats
		{
			get { return _groupChats; }
			set { SetProperty(ref _groupChats, value); }
		}

		string _searchText;
		public string SearchText
		{
			get { return _searchText; }
			set
			{
				SetProperty(ref _searchText, value);
				scheduleFilter();
			}
		}

		// Динамический текст для плейсхолдера поиска
		public string SearchPlaceholder => IsPersonalMode
			? CrossLocalization.Translate("chat_search_contacts_placeholder")
			: CrossLocalization.Translate("chat_search_subjects_placeholder");

		bool _isPersonalMode;
		public bool IsPersonalMode
		{
			get { return _isPersonalMode; }
			set
			{
				SetProperty(ref _isPersonalMode, value);
				OnPropertyChanged(nameof(IsGroupMode));
				OnPropertyChanged(nameof(SearchPlaceholder)); // Обновляем текст плейсхолдера

				// Очищаем поиск при переключении вкладок
				SearchText = string.Empty;
			}
		}

		public bool IsGroupMode => !IsPersonalMode;

		bool _isLoading;
		public bool IsLoading
		{
			get { return _isLoading; }
			set { SetProperty(ref _isLoading, value); }
		}

		object _selectedItem;
		public object SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				SetProperty(ref _selectedItem, value);
				openChat(_selectedItem);
			}
		}

		object _selectedGroupItem;
		public object SelectedGroupItem
		{
			get { return _selectedGroupItem; }
			set
			{
				SetProperty(ref _selectedGroupItem, value);
				openGroupChat(_selectedGroupItem);
			}
		}

		Command _refreshCommand;
		public Command RefreshCommand
		{
			get { return _refreshCommand ??= new Command(async () => await update(false)); }
		}

		Command _showPersonalCommand;
		public Command ShowPersonalCommand
		{
			get
			{
				return _showPersonalCommand ??= new Command(() => {
					IsPersonalMode = true;
				});
			}
		}

		Command _showGroupCommand;
		public Command ShowGroupCommand
		{
			get
			{
				return _showGroupCommand ??= new Command(async () => {
					IsPersonalMode = false;

					// ИСПРАВЛЕНИЕ: Проверяем исходный список, а не _groupChats
					if (_allGroupChats == null || _allGroupChats.Count == 0)
					{
						await update(true);
					}
				});
			}
		}

		async Task update(bool showDialog)
		{
			// Cached lists first: the tab opens instantly (and works offline),
			// the network result replaces them.
			var hasCachedData = await showCachedLists();
			var isDialogShown = showDialog && !hasCachedData;

			if (isDialogShown)
			{
				_services.Dialogs.ShowLoading();
			}
			else
			{
				IsLoading = true;
			}

			try
			{
				bool isError;

				if (IsPersonalMode)
				{
					var chats = await ChatApiService.GetChats(AppUserData.UserId);
					isError = ChatApiService.IsError;

					if (!isError)
					{
						_allChats = chats;
						// Avatars are base64: serializing and writing them takes a while.
						_ = Task.Run(() => ChatOfflineCache.SaveChats(chats));
						ChatPresenceService.Seed(_allChats);
						ChatUnreadService.SetPersonal(_allChats);
					}
				}
				else
				{
					var subjects = await ChatApiService.GetGroups(AppUserData.UserId, ChatRoles.Current(_services));
					isError = ChatApiService.IsError;

					if (!isError)
					{
						_allGroupChats = flatten(subjects);
						_ = Task.Run(() => ChatOfflineCache.SaveGroups(subjects));
						ChatUnreadService.SetGroups(_allGroupChats);
					}
				}

				// Offline: the "no connection" banner is shown and cached chats stay.
				if (isError && _services.Device.CheckConnectivity())
				{
					_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
				}

				syncWithServices();
				applyFilter();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
			finally
			{
				if (isDialogShown)
				{
					_services.Dialogs.HideLoading();
				}
				else
				{
					IsLoading = false;
				}
			}
		}

		/// <summary>
		/// Show chats saved on the device if nothing is shown yet.
		/// </summary>
		/// <returns><c>true</c> if cached chats are shown.</returns>
		async Task<bool> showCachedLists()
		{
			// The cache is read and parsed off the UI thread.
			if (IsPersonalMode && _allChats.Count == 0)
			{
				var cachedChats = await Task.Run(ChatOfflineCache.LoadChats);

				if (cachedChats?.Count > 0 && _allChats.Count == 0)
				{
					_allChats = cachedChats;
				}
			}
			else if (!IsPersonalMode && _allGroupChats.Count == 0)
			{
				var cachedGroups = await Task.Run(ChatOfflineCache.LoadGroups);

				if (cachedGroups?.Count > 0 && _allGroupChats.Count == 0)
				{
					_allGroupChats = flatten(cachedGroups);
				}
			}

			var hasData = IsPersonalMode ? _allChats.Count > 0 : _allGroupChats.Count > 0;

			if (hasData)
			{
				syncWithServices();
				applyFilter();
			}

			return hasData;
		}

		/// <summary>
		/// Typing pause before filtering: the list was rebuilt on every key press.
		/// </summary>
		const int _filterDelayMs = 250;

		int _filterVersion;

		async void scheduleFilter()
		{
			var version = ++_filterVersion;

			// Cleared search is applied right away.
			if (!string.IsNullOrWhiteSpace(_searchText))
			{
				await Task.Delay(_filterDelayMs);

				if (version != _filterVersion)
				{
					return;
				}
			}

			applyFilter();
		}

		void applyFilter()
		{
			var query = SearchText?.Trim();

			// Pinned chats first, then chats with the newest messages.
			Chats = ChatActivityService.Sort(string.IsNullOrEmpty(query)
				? _allChats
				: _allChats
					.Where(c => !string.IsNullOrEmpty(c.Name) &&
						c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));

			// ИСПРАВЛЕНИЕ: Добавлен поиск по SubjectName (названию предмета)
			GroupChats = ChatActivityService.Sort(string.IsNullOrEmpty(query)
				? _allGroupChats
				: _allGroupChats
					.Where(g =>
						(!string.IsNullOrEmpty(g.SubjectName) && g.SubjectName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
						(!string.IsNullOrEmpty(g.DisplayName) && g.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))));
		}

		/// <summary>
		/// A chat got a new message: re-sort if the order changed.
		/// </summary>
		void onActivityChanged()
		{
			var chats = ChatActivityService.Sort(Chats ?? new List<ChatItemModel>());
			var groups = ChatActivityService.Sort(GroupChats ?? new List<GroupChatModel>());

			if (Chats != null && !chats.SequenceEqual(Chats))
			{
				Chats = chats;
			}

			if (GroupChats != null && !groups.SequenceEqual(GroupChats))
			{
				GroupChats = groups;
			}
		}

		List<GroupChatModel> flatten(List<SubjectChatsModel> subjects)
		{
			if (subjects == null)
			{
				return new List<GroupChatModel>();
			}

			return subjects
				.Where(s => s.Groups != null)
				.SelectMany(s => s.Groups.Select(g => {
					g.SubjectName = string.IsNullOrEmpty(s.ShortName) ? s.Name : s.ShortName;
					return g;
				}))
				.ToList();
		}

		void openChat(object selectedObject)
		{
			if (selectedObject == null || !(selectedObject is ChatItemModel chat))
			{
				return;
			}

			SelectedItem = null;
			_ = _services.Navigation.OpenConversation(chat.Id, chat.Name, chat.UserId);
		}

		void openGroupChat(object selectedObject)
		{
			if (selectedObject == null || !(selectedObject is GroupChatModel group))
			{
				return;
			}

			SelectedGroupItem = null;
			_ = _services.Navigation.OpenGroupConversation(
				group.Id, group.GroupId, ChatRoles.Current(_services), group.DisplayName);
		}
	}
}