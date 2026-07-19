using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Chat list page ViewModel.
	/// </summary>
	public class ChatPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;

		public ChatPageViewModel(IPlatformServices services)
		{
			_services = services;
			_ = update(true);
		}

		List<ChatItemModel> _chats;

		/// <summary>
		/// Personal chats list.
		/// </summary>
		public List<ChatItemModel> Chats
		{
			get { return _chats; }
			set { SetProperty(ref _chats, value); }
		}

		bool _isLoading;

		/// <summary>
		/// Is loading (used for pull-to-refresh spinner).
		/// </summary>
		public bool IsLoading
		{
			get { return _isLoading; }
			set { SetProperty(ref _isLoading, value); }
		}

		object _selectedItem;

		/// <summary>
		/// Selected chat item.
		/// </summary>
		public object SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				SetProperty(ref _selectedItem, value);
				openChat(_selectedItem);
			}
		}

		Command _refreshCommand;

		/// <summary>
		/// Pull-to-refresh command.
		/// </summary>
		public Command RefreshCommand
		{
			get { return _refreshCommand ??= new Command(async () => await update(false)); }
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

			var chats = await ChatApiService.GetChats(AppUserData.UserId);

			if (ChatApiService.IsError)
			{
				_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
			}

			Chats = chats;

			if (showDialog)
			{
				_services.Dialogs.HideLoading();
			}
			else
			{
				IsLoading = false;
			}
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
	}
}