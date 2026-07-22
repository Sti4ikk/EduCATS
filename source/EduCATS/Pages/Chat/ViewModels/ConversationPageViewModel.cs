using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EduCATS.Pages.Chat.ViewModels
{
	public class ConversationPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;
		readonly int _chatId;

		public event System.Action HistoryLoaded;

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

		public Command SendCommand
		{
			get { return _sendCommand ??= new Command(async () => await send()); }
		}

		Command _attachCommand;

		/// <summary>
		/// Открывает системный пикер файлов, кодирует выбранный файл в
		/// base64 и отправляет его через тот же SignalR-хаб, что и
		/// текстовые сообщения (см. <see cref="MessageSendModel"/>).
		/// </summary>
		public Command AttachCommand
		{
			get { return _attachCommand ??= new Command(async () => await attach()); }
		}

		async Task loadHistory()
		{
			_services.Dialogs.ShowLoading();

			try
			{
				var history = await ChatApiService.GetChatMsgs(AppUserData.UserId, _chatId)
					?? new System.Collections.Generic.List<MessageItemModel>();

				// 1. Выносим всю тяжелую подготовку и сортировку списка в фоновый поток (Task.Run)
				var preparedItems = await Task.Run(() =>
				{
					var itemsList = new System.Collections.Generic.List<object>();
					DateTime? lastDate = null;

					foreach (var msg in history.OrderBy(m => m.Time))
					{
						if (msg.IsPlainText)
						{
							msg.Text = HtmlHelper.StripHtml(msg.Text);
						}

						msg.IsMine = msg.Name == AppUserData.Name;

						var msgDate = msg.Time.Date;

						if (lastDate == null || msgDate != lastDate.Value)
						{
							itemsList.Add(new DateSeparatorModel(msgDate));
							lastDate = msgDate;
						}

						itemsList.Add(msg);
					}

					return itemsList;
				});

				// 2. Быстро присваиваем уже сформированную коллекцию на UI-потоке
				Messages = new ObservableCollection<object>(preparedItems);

				_ = ChatApiService.MarkChatAsRead(AppUserData.UserId, _chatId);
			}
			finally
			{
				_services.Dialogs.HideLoading();
				HistoryLoaded?.Invoke();
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
		}

		async Task attach()
		{
			FileResult picked;

			try
			{
				picked = await FilePicker.Default.PickAsync(PickOptions.Default);
			}
			catch (Exception)
			{
				// Пользователь отменил выбор или нет разрешений - молча выходим.
				return;
			}

			if (picked == null)
			{
				return;
			}

			_services.Dialogs.ShowLoading();

			try
			{
				using var stream = await picked.OpenReadAsync();
				using var ms = new MemoryStream();
				await stream.CopyToAsync(ms);

				var bytes = ms.ToArray();
				var base64 = Convert.ToBase64String(bytes);
				var isImage = isImageExtension(picked.FileName);

				var payload = new MessageSendModel
				{
					ChatId = _chatId,
					UserId = AppUserData.UserId,
					Text = picked.FileName,
					IsImage = isImage,
					IsFile = !isImage,
					ImageContent = isImage ? base64 : null,
					FileContent = isImage ? null : base64,
					FileSize = formatFileSize(bytes.Length)
				};

				await ChatHubService.SendMessage(payload);
			}
			finally
			{
				_services.Dialogs.HideLoading();
			}
		}

		static bool isImageExtension(string fileName)
		{
			var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
			return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
		}

		static string formatFileSize(long bytes)
		{
			return bytes < 1024 * 1024
				? $"{bytes / 1024.0:F1} KB"
				: $"{bytes / 1024.0 / 1024.0:F1} MB";
		}

		void onMessageReceived(MessageItemModel msg)
		{
			if (msg.ChatId != _chatId)
			{
				return;
			}

			if (msg.IsPlainText)
			{
				msg.Text = HtmlHelper.StripHtml(msg.Text);
			}

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