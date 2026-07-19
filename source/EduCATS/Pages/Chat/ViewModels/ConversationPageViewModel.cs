using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EduCATS.Pages.Chat.ViewModels
{
	public class ConversationPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;
		readonly int _chatId;

		public ConversationPageViewModel(IPlatformServices services, int chatId, string title)
		{
			_services = services;
			_chatId = chatId;
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

		/// <summary>
		/// Send command.
		/// </summary>
		/// <remarks>
		/// No <c>canExecute</c> parameter here on purpose: MAUI's
		/// <see cref="Command"/> evaluates <c>canExecute</c> once at
		/// bind time (when <see cref="MessageText"/> is still null/empty)
		/// and disables the bound Button forever unless something calls
		/// <c>ChangeCanExecute()</c> on every keystroke - which nothing
		/// did here, so the Send button (and Enter, if wired to the same
		/// command) stayed permanently disabled. The empty-text check is
		/// done inside <see cref="send"/> instead.
		/// </remarks>
		public Command SendCommand
		{
			get { return _sendCommand ??= new Command(async () => await send()); }
		}

		async Task loadHistory()
		{
			_services.Dialogs.ShowLoading();

			try
			{
				var history = await ChatApiService.GetChatMsgs(AppUserData.UserId, _chatId)
					?? new System.Collections.Generic.List<MessageItemModel>();

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

				// Mark as read only after messages are actually shown.
				_ = ChatApiService.MarkChatAsRead(AppUserData.UserId, _chatId);
			}
			finally
			{
				// Guaranteed to run even if something above throws, so
				// the loading overlay never gets stuck blocking the
				// whole screen (input field, send button, everything).
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

			var payload = new MessageSendModel
			{
				Text = text,
				ChatId = _chatId,
				UserId = AppUserData.UserId
			};

			await ChatHubService.SendMessage(payload);
			// The message we just sent comes back through "GetMessage"
			// (Clients.Caller.SendAsync in MessageHub.SendMessage), so
			// it isn't added to the list here - doing so would duplicate it.
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