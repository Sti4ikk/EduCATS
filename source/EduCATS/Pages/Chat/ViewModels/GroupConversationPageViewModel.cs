using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Group (subject) conversation ViewModel.
	/// </summary>
	/// <remarks>
	/// Deliberately a separate class from <see cref="ConversationPageViewModel"/>
	/// rather than a shared/unified one: the server sends the same
	/// "GetMessage" event for both personal and group messages, with no
	/// type flag in the payload, and personal/group chat ids come from
	/// separate DB tables (so they *could* numerically collide). Keeping
	/// two independent screens means each only ever trusts events while
	/// it itself is the active screen for that specific chatId - same
	/// residual risk profile as the existing personal chat screen.
	/// </remarks>
	public class GroupConversationPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;
		readonly int _chatId;
		readonly string _role;

		public GroupConversationPageViewModel(IPlatformServices services, int chatId, string role, string title)
		{
			_services = services;
			_chatId = chatId;
			_role = role;
			Title = title;
			Messages = new ObservableCollection<object>();

			ChatHubService.MessageReceived += onMessageReceived;

			_ = loadHistory();
		}

		public string Title { get; }

		ObservableCollection<object> _messages;

		public ObservableCollection<object> Messages
		{
			get { return _messages; }
			set { SetProperty(ref _messages, value); }
		}

		string _messageText;

		public string MessageText
		{
			get { return _messageText; }
			set { SetProperty(ref _messageText, value); }
		}

		Command _sendCommand;

		public Command SendCommand
		{
			get { return _sendCommand ??= new Command(async () => await send()); }
		}

		async Task loadHistory()
		{
			_services.Dialogs.ShowLoading();

			try
			{
				var history = await ChatApiService.GetGroupMsgs(AppUserData.UserId, _chatId)
					?? new List<MessageItemModel>();

				DateTime? lastDate = null;

				foreach (var msg in history.OrderBy(m => m.Time))
				{
					msg.Text = HtmlHelper.StripHtml(msg.Text);
					msg.IsMine = msg.Name == AppUserData.Name;

					var msgDate = msg.Time.Date;

					if (lastDate == null || msgDate != lastDate.Value)
					{
						Messages.Add(new DateSeparatorModel(msgDate));
						lastDate = msgDate;
					}

					Messages.Add(msg);
				}

				_ = ChatApiService.MarkGroupChatAsRead(AppUserData.UserId, _chatId);
			}
			finally
			{
				_services.Dialogs.HideLoading();
			}
		}

		async Task send()
		{
			if (string.IsNullOrWhiteSpace(MessageText))
			{
				return;
			}

			var text = MessageText;
			MessageText = string.Empty;

			var payload = new GroupMessageSendModel
			{
				Text = text,
				ChatId = _chatId,
				UserId = AppUserData.UserId
			};

			await ChatHubService.SendGroupMessage(payload, _role);
		}

		void onMessageReceived(MessageItemModel msg)
		{
			if (msg.ChatId != _chatId)
			{
				return;
			}

			msg.Text = HtmlHelper.StripHtml(msg.Text);
			msg.IsMine = msg.Name == AppUserData.Name;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				var lastMsgDate = Messages
					.OfType<MessageItemModel>()
					.LastOrDefault()?.Time.Date;

				if (lastMsgDate == null || msg.Time.Date != lastMsgDate.Value)
				{
					Messages.Add(new DateSeparatorModel(msg.Time.Date));
				}

				Messages.Add(msg);
			});
		}

		public void Cleanup()
		{
			ChatHubService.MessageReceived -= onMessageReceived;
		}
	}
}