using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
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
			_ = update(true);
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
				applyFilter();
			}
		}

		// Динамический текст для плейсхолдера поиска
		public string SearchPlaceholder => IsPersonalMode
			? "ФИО студента или преподавателя"
			: "Название предмета";

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
			if (showDialog)
			{
				_services.Dialogs.ShowLoading();
			}
			else
			{
				IsLoading = true;
			}

			if (IsPersonalMode)
			{
				var chats = await ChatApiService.GetChats(AppUserData.UserId);

				if (ChatApiService.IsError)
				{
					_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
				}

				_allChats = chats ?? new List<ChatItemModel>();
			}
			else
			{
				var role = ChatHubService.CurrentRole ?? getFallbackRole();
				var subjects = await ChatApiService.GetGroups(AppUserData.UserId, role);

				if (ChatApiService.IsError)
				{
					_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
				}

				_allGroupChats = flatten(subjects);
			}

			applyFilter();

			if (showDialog)
			{
				_services.Dialogs.HideLoading();
			}
			else
			{
				IsLoading = false;
			}
		}

		void applyFilter()
		{
			var query = SearchText?.Trim();

			Chats = string.IsNullOrEmpty(query)
				? _allChats
				: _allChats
					.Where(c => !string.IsNullOrEmpty(c.Name) &&
						c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
					.ToList();

			// ИСПРАВЛЕНИЕ: Добавлен поиск по SubjectName (названию предмета)
			GroupChats = string.IsNullOrEmpty(query)
				? _allGroupChats
				: _allGroupChats
					.Where(g =>
						(!string.IsNullOrEmpty(g.SubjectName) && g.SubjectName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
						(!string.IsNullOrEmpty(g.DisplayName) && g.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)))
					.ToList();
		}

		string getFallbackRole()
		{
			var isProfessor = string.IsNullOrEmpty(_services.Preferences.GroupName);
			return isProfessor ? "lector" : "student";
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
			_ = _services.Navigation.OpenConversation(chat.Id, chat.Name);
		}

		void openGroupChat(object selectedObject)
		{
			if (selectedObject == null || !(selectedObject is GroupChatModel group))
			{
				return;
			}

			SelectedGroupItem = null;
			var role = ChatHubService.CurrentRole ?? getFallbackRole();
			_ = _services.Navigation.OpenGroupConversation(group.Id, role, group.DisplayName);
		}
	}
}