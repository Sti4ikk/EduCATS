using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Message to hand to the chat hub.
	/// </summary>
	public class OutgoingMessage
	{
		public string Text { get; set; }

		public string FileContent { get; set; }

		public string ImageContent { get; set; }

		public string FileSize { get; set; }

		public bool? IsImage { get; set; }

		public bool? IsFile { get; set; }
	}

	/// <summary>
	/// Common logic of personal and group conversations.
	/// </summary>
	/// <remarks>
	/// The server sends the same "GetMessage" event for personal and group
	/// messages with no type flag, and personal/group chat ids come from
	/// separate DB tables (so they could collide). So a conversation only
	/// trusts live events while it is the active screen (see <see cref="Activate"/>).
	/// </remarks>
	public abstract class ConversationViewModelBase : ViewModel
	{
		/// <summary>
		/// Messages per history page (same as the web client).
		/// </summary>
		public const int PageSize = 30;

		const int _searchLimit = 50;
		const int _searchDelayMilliseconds = 400;

		/// <summary>
		/// Attachments bigger than this can't be sent through the hub
		/// (fallback when the upload endpoint is unavailable).
		/// </summary>
		const long _legacyAttachmentMaxBytes = 5 * 1024 * 1024;

		const int _attachCamera = 1;
		const int _attachGallery = 2;
		const int _attachFile = 3;

		protected readonly IPlatformServices Services;

		/// <summary>
		/// Messages loaded from the server (offset of the next history page).
		/// </summary>
		int _serverOffset;

		bool _isActive;
		bool _wasDeactivated;

		/// <summary>
		/// Is history being (re)loaded. Accessed on the main thread only.
		/// </summary>
		bool _isLoadingHistory;

		/// <summary>
		/// Live messages received while history was loading. Main thread only.
		/// </summary>
		readonly List<MessageItemModel> _receivedWhileLoading = new List<MessageItemModel>();

		CancellationTokenSource _searchCancellation;

		/// <summary>
		/// History is loaded or reloaded (the view scrolls to the bottom).
		/// </summary>
		public event Action HistoryLoaded;

		/// <summary>
		/// A message is added at the bottom; the argument tells whether it's own.
		/// </summary>
		public event Action<bool> MessageAppended;

		/// <summary>
		/// Older messages are added at the top; the argument is the item
		/// that was the first one before (the view keeps it in place).
		/// </summary>
		public event Action<object> OlderMessagesLoaded;

		protected ConversationViewModelBase(IPlatformServices services, int chatId, string title)
		{
			Services = services;
			ChatId = chatId;
			Title = title;
			Messages = new ObservableCollection<object>();
			SearchResults = new ObservableCollection<object>();
			_messageText = ChatLocalSettings.GetDraft(chatId, IsGroupChat);
		}

		public int ChatId { get; }

		public string Title { get; }

		public abstract bool IsGroupChat { get; }

		ObservableCollection<object> _messages;
		public ObservableCollection<object> Messages
		{
			get => _messages;
			set => SetProperty(ref _messages, value);
		}

		string _messageText;

		/// <summary>
		/// Text being typed (saved as a draft).
		/// </summary>
		public string MessageText
		{
			get => _messageText;
			set
			{
				if (SetProperty(ref _messageText, value))
				{
					ChatLocalSettings.SetDraft(ChatId, IsGroupChat, value);
				}
			}
		}

		bool _hasMoreMessages;
		public bool HasMoreMessages
		{
			get => _hasMoreMessages;
			set => SetProperty(ref _hasMoreMessages, value);
		}

		bool _isLoadingOlder;
		public bool IsLoadingOlder
		{
			get => _isLoadingOlder;
			set => SetProperty(ref _isLoadingOlder, value);
		}

		string _searchText;

		/// <summary>
		/// Text to search on the server.
		/// </summary>
		public string SearchText
		{
			get => _searchText;
			set
			{
				if (SetProperty(ref _searchText, value))
				{
					startSearch(value);
				}
			}
		}

		bool _isSearching;

		/// <summary>
		/// Are search results shown instead of the conversation.
		/// </summary>
		public bool IsSearching
		{
			get => _isSearching;
			set => SetProperty(ref _isSearching, value);
		}

		ObservableCollection<object> _searchResults;
		public ObservableCollection<object> SearchResults
		{
			get => _searchResults;
			set => SetProperty(ref _searchResults, value);
		}

		Command _sendCommand;
		public Command SendCommand => _sendCommand ??= new Command(async () => await send());

		Command _attachCommand;

		/// <summary>
		/// Choose what to attach: camera, gallery or file.
		/// </summary>
		public Command AttachCommand => _attachCommand ??= new Command(showAttachOptions);

		Command _attachOptionCommand;
		Command attachOptionCommand => _attachOptionCommand ??= new Command(async option => await attach(option));

		/// <summary>
		/// Load a page of messages, newest first.
		/// </summary>
		protected abstract Task<List<MessageItemModel>> LoadPageAsync(int limit, int offset);

		/// <summary>
		/// Mark the chat as read on the server.
		/// </summary>
		protected abstract Task MarkReadOnServerAsync();

		/// <summary>
		/// Hand a message to the chat hub.
		/// </summary>
		/// <returns><c>true</c> on success.</returns>
		protected abstract Task<bool> SendToHubAsync(OutgoingMessage message);

		/// <summary>
		/// Called when the screen becomes active.
		/// </summary>
		protected virtual void OnActivated()
		{
		}

		/// <summary>
		/// Called when the screen is hidden.
		/// </summary>
		protected virtual void OnDeactivated()
		{
		}

		/// <summary>
		/// Start receiving messages and load history.
		/// </summary>
		/// <remarks>Called at the end of derived constructors.</remarks>
		protected void Start()
		{
			Activate();
			_ = loadHistory();
		}

		/// <summary>
		/// Start receiving live messages.
		/// </summary>
		/// <remarks>
		/// If the screen was hidden before, messages could have been missed
		/// in the meantime, so history is reloaded.
		/// </remarks>
		public void Activate()
		{
			if (_isActive)
			{
				return;
			}

			_isActive = true;
			ChatHubService.MessageReceived += onMessageReceived;
			ChatHubService.ConnectionRestored += onConnectionRestored;
			ChatUnreadService.SetActiveChat(ChatId, IsGroupChat);
			OnActivated();

			if (_wasDeactivated)
			{
				_ = catchUp();
			}
		}

		/// <summary>
		/// Stop receiving live messages.
		/// </summary>
		public void Deactivate()
		{
			if (!_isActive)
			{
				return;
			}

			_isActive = false;
			_wasDeactivated = true;
			ChatHubService.MessageReceived -= onMessageReceived;
			ChatHubService.ConnectionRestored -= onConnectionRestored;
			ChatUnreadService.ClearActiveChat(ChatId, IsGroupChat);

			// Messages received while the chat was open are read.
			_ = MarkReadOnServerAsync();
			OnDeactivated();
		}

		/// <summary>
		/// Load the previous page of history.
		/// </summary>
		/// <returns>Task.</returns>
		public async Task LoadOlderAsync()
		{
			if (_isLoadingHistory || IsLoadingOlder || !HasMoreMessages || IsSearching)
			{
				return;
			}

			IsLoadingOlder = true;

			try
			{
				var page = await LoadPageAsync(PageSize, _serverOffset);

				if (page == null)
				{
					// Keep HasMoreMessages: scrolling up again retries.
					return;
				}

				HasMoreMessages = page.Count == PageSize;
				_serverOffset += page.Count;

				// New messages shift the server window, so a page may repeat known ones.
				var knownIds = Messages.OfType<MessageItemModel>().Select(m => m.Id).ToHashSet();
				var older = page
					.Where(m => !knownIds.Contains(m.Id))
					.Select(prepare)
					.OrderBy(m => m.Time)
					.ToList();

				if (older.Count == 0)
				{
					return;
				}

				// The first day of the loaded part may continue the older page.
				if (Messages.FirstOrDefault() is DateSeparatorModel separator &&
					separator.Date == older.Last().LocalTime.Date)
				{
					Messages.RemoveAt(0);
				}

				var anchor = Messages.FirstOrDefault();
				var items = buildItems(older);

				for (var index = 0; index < items.Count; index++)
				{
					Messages.Insert(index, items[index]);
				}

				loadLinkPreviews(older);
				OlderMessagesLoaded?.Invoke(anchor);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
			finally
			{
				IsLoadingOlder = false;
			}
		}

		/// <summary>
		/// Send a failed message again.
		/// </summary>
		/// <param name="message">Failed message.</param>
		/// <returns>Task.</returns>
		public async Task Retry(MessageItemModel message)
		{
			if (message == null || message.Status != MessageSendStatus.Failed)
			{
				return;
			}

			await deliver(message);
		}

		/// <summary>
		/// The connection dropped (e.g. the app was in background) and is back:
		/// messages sent meanwhile weren't delivered live.
		/// </summary>
		void onConnectionRestored() =>
			MainThread.BeginInvokeOnMainThread(() => _ = catchUp());

		/// <summary>
		/// Add messages missed while the screen was hidden.
		/// </summary>
		/// <remarks>
		/// Keeps the scroll position and loaded older pages (unlike a full reload),
		/// e.g. after closing the full-screen image viewer.
		/// </remarks>
		async Task catchUp()
		{
			if (_isLoadingHistory)
			{
				return;
			}

			_isLoadingHistory = true;
			var isReloadNeeded = false;

			try
			{
				var page = await LoadPageAsync(PageSize, 0);

				if (page == null)
				{
					return;
				}

				var knownIds = Messages.OfType<MessageItemModel>().Select(m => m.Id).ToHashSet();
				var missed = page.Where(m => !knownIds.Contains(m.Id)).ToList();

				// More than a page was missed: there would be a gap.
				if (missed.Count == page.Count && page.Count == PageSize)
				{
					isReloadNeeded = true;
					return;
				}

				// The server window moved by the number of new messages.
				_serverOffset += missed.Count;
				_receivedWhileLoading.AddRange(missed.Select(prepare).OrderBy(m => m.Time));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
			finally
			{
				_isLoadingHistory = false;

				if (isReloadNeeded)
				{
					_receivedWhileLoading.Clear();
				}
				else
				{
					// Also adds live messages received meanwhile (duplicates are skipped by id).
					flushReceivedWhileLoading();
				}
			}

			if (isReloadNeeded)
			{
				await loadHistory(showLoading: false);
			}
		}

		async Task loadHistory(bool showLoading = true)
		{
			if (_isLoadingHistory)
			{
				return;
			}

			_isLoadingHistory = true;

			// Messages saved on the device first: the chat opens instantly
			// (and works offline), the server page replaces them.
			if (showCachedMessages())
			{
				showLoading = false;
			}

			if (showLoading)
			{
				Services.Dialogs.ShowLoading();
			}

			try
			{
				// Failed reload must not wipe already shown messages.
				var page = await LoadPageAsync(PageSize, 0);

				if (page == null)
				{
					return;
				}

				ChatOfflineCache.SaveMessages(ChatId, IsGroupChat, page);
				var history = await Task.Run(() => page.Select(prepare).OrderBy(m => m.Time).ToList());

				// Own messages not accepted by the server yet stay in the feed.
				var unsent = Messages
					.OfType<MessageItemModel>()
					.Where(m => m.LocalId != null && m.Status != MessageSendStatus.Sent)
					.ToList();

				Messages = new ObservableCollection<object>(buildItems(history.Concat(unsent)));
				_serverOffset = page.Count;
				HasMoreMessages = page.Count == PageSize;

				ChatUnreadService.MarkRead(ChatId, IsGroupChat);
				_ = MarkReadOnServerAsync();
				loadLinkPreviews(history);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
			finally
			{
				_isLoadingHistory = false;
				flushReceivedWhileLoading();

				if (showLoading)
				{
					Services.Dialogs.HideLoading();
				}

				HistoryLoaded?.Invoke();
			}
		}

		/// <summary>
		/// Show the cached latest page if nothing is shown yet.
		/// </summary>
		/// <returns><c>true</c> if cached messages are shown.</returns>
		bool showCachedMessages()
		{
			if (Messages.Count > 0)
			{
				return false;
			}

			var cached = ChatOfflineCache.LoadMessages(ChatId, IsGroupChat);

			if (cached == null || cached.Count == 0)
			{
				return false;
			}

			var history = cached.Select(prepare).OrderBy(m => m.Time).ToList();
			Messages = new ObservableCollection<object>(buildItems(history));
			_serverOffset = cached.Count;
			HasMoreMessages = cached.Count == PageSize;
			loadLinkPreviews(history);
			HistoryLoaded?.Invoke();
			return true;
		}

		async Task send()
		{
			var text = MessageText?.Trim();

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			MessageText = string.Empty;

			var message = createOwnMessage();
			message.Text = text;

			appendMessage(message);
			MessageAppended?.Invoke(true);
			loadLinkPreview(message);

			await deliver(message);
		}

		void showAttachOptions()
		{
			var options = new Dictionary<int, string>();

			if (ChatAttachmentService.IsCaptureSupported)
			{
				options.Add(_attachCamera, CrossLocalization.Translate("chat_attach_camera"));
			}

			options.Add(_attachGallery, CrossLocalization.Translate("chat_attach_gallery"));
			options.Add(_attachFile, CrossLocalization.Translate("chat_attach_file"));

			Services.Dialogs.ShowSheet(
				CrossLocalization.Translate("chat_attach_title"), options, attachOptionCommand);
		}

		async Task attach(object option)
		{
			try
			{
				var choice = Convert.ToInt32(option);
				var file = choice switch
				{
					_attachCamera => await ChatAttachmentService.CapturePhotoAsync(),
					_attachGallery => await ChatAttachmentService.PickPhotoAsync(),
					_attachFile => await ChatAttachmentService.PickFileAsync(),
					_ => null
				};

				if (file == null)
				{
					return;
				}

				var attachment = await ChatAttachmentService.PrepareAsync(file, isCapturedPhoto: choice == _attachCamera);

				if (attachment.Size > ChatAttachmentService.MaxFileSizeMegabytes * 1024L * 1024L)
				{
					File.Delete(attachment.FilePath);
					Services.Dialogs.ShowError(string.Format(
						CrossLocalization.Translate("chat_file_too_large"),
						ChatAttachmentService.MaxFileSizeMegabytes));
					return;
				}

				var message = createOwnMessage();
				message.IsImage = attachment.IsImage ? true : (bool?)null;
				message.IsFile = attachment.IsImage ? (bool?)null : true;
				message.FileContent = attachment.FileName;
				message.FileSize = ChatAttachmentService.FormatSize(attachment.Size);
				message.LocalFilePath = attachment.FilePath;

				appendMessage(message);
				MessageAppended?.Invoke(true);

				await deliver(message);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				Services.Dialogs.ShowError(CrossLocalization.Translate("chat_send_error"));
			}
		}

		async Task deliver(MessageItemModel message)
		{
			message.Status = MessageSendStatus.Sending;
			message.UploadProgress = 0;

			bool isSent;

			try
			{
				isSent = message.IsAttachment ?
					await deliverAttachment(message) :
					await SendToHubAsync(new OutgoingMessage { Text = message.Text });
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				isSent = false;
			}

			message.Status = isSent ? MessageSendStatus.Sent : MessageSendStatus.Failed;
		}

		async Task<bool> deliverAttachment(MessageItemModel message)
		{
			// Created on the main thread: reports come back to it.
			var progress = new Progress<double>(value => message.UploadProgress = value);
			var isUploaded = await ChatApiService.UploadFile(
				ChatId, message.LocalFilePath, message.FileContent, progress);

			if (isUploaded)
			{
				return await SendToHubAsync(new OutgoingMessage
				{
					FileContent = message.FileContent,
					FileSize = message.FileSize,
					IsImage = message.IsImage,
					IsFile = message.IsFile
				});
			}

			// Fallback: the file inside the hub message (how the app sent files before).
			if (new FileInfo(message.LocalFilePath).Length > _legacyAttachmentMaxBytes)
			{
				return false;
			}

			var base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(message.LocalFilePath));
			return await SendToHubAsync(new OutgoingMessage
			{
				Text = message.FileContent,
				ImageContent = message.IsImageMessage ? base64 : null,
				FileContent = message.IsImageMessage ? null : base64,
				FileSize = message.FileSize,
				IsImage = message.IsImage,
				IsFile = message.IsFile
			});
		}

		MessageItemModel createOwnMessage() =>
			new MessageItemModel
			{
				LocalId = Guid.NewGuid().ToString("N"),
				ChatId = ChatId,
				IsGroupChat = IsGroupChat,
				IsMine = true,
				Name = AppUserData.Name,
				Time = DateTime.UtcNow,
				Status = MessageSendStatus.Sending
			};

		void onMessageReceived(MessageItemModel message)
		{
			if (message == null || message.ChatId != ChatId)
			{
				return;
			}

			prepare(message);

			MainThread.BeginInvokeOnMainThread(() =>
			{
				// History is about to replace Messages - keep the message
				// and add it after loading (unless history already has it).
				if (_isLoadingHistory)
				{
					_receivedWhileLoading.Add(message);
					return;
				}

				addReceived(message);
			});
		}

		void addReceived(MessageItemModel message)
		{
			if (message.Id != 0 && Messages.OfType<MessageItemModel>().Any(m => m.Id == message.Id))
			{
				return;
			}

			// The server echoes own messages back: replace the local copy.
			var local = message.IsMine ? findLocalCopy(message) : null;

			if (local != null)
			{
				message.LinkPreview = local.LinkPreview;
				Messages[Messages.IndexOf(local)] = message;
				return;
			}

			appendMessage(message);
			MessageAppended?.Invoke(message.IsMine);
			loadLinkPreview(message);
		}

		MessageItemModel findLocalCopy(MessageItemModel echo) =>
			Messages
				.OfType<MessageItemModel>()
				.FirstOrDefault(m =>
					m.LocalId != null &&
					m.Status != MessageSendStatus.Failed &&
					(echo.IsAttachment ?
						m.IsImageMessage == echo.IsImageMessage && m.IsFileMessage == echo.IsFileMessage :
						!m.IsAttachment && string.Equals(m.Text?.Trim(), echo.Text?.Trim(), StringComparison.Ordinal)));

		void flushReceivedWhileLoading()
		{
			foreach (var message in _receivedWhileLoading)
			{
				addReceived(message);
			}

			_receivedWhileLoading.Clear();
		}

		void appendMessage(MessageItemModel message)
		{
			var lastDate = Messages
				.OfType<MessageItemModel>()
				.LastOrDefault()?.LocalTime.Date;

			if (lastDate == null || message.LocalTime.Date != lastDate.Value)
			{
				Messages.Add(new DateSeparatorModel(message.LocalTime.Date));
			}

			Messages.Add(message);
		}

		MessageItemModel prepare(MessageItemModel message)
		{
			if (message.IsPlainText)
			{
				message.Text = HtmlHelper.StripHtml(message.Text);
			}

			message.IsGroupChat = IsGroupChat;
			message.IsMine = message.IsWrittenBy(AppUserData.Name);

			if (message.IsMine && message.Status == MessageSendStatus.None)
			{
				message.Status = MessageSendStatus.Sent;
			}

			return message;
		}

		static List<object> buildItems(IEnumerable<MessageItemModel> messages)
		{
			var items = new List<object>();
			DateTime? lastDate = null;

			foreach (var message in messages)
			{
				var date = message.LocalTime.Date;

				if (lastDate == null || date != lastDate.Value)
				{
					items.Add(new DateSeparatorModel(date));
					lastDate = date;
				}

				items.Add(message);
			}

			return items;
		}

		void loadLinkPreviews(IEnumerable<MessageItemModel> messages)
		{
			foreach (var message in messages)
			{
				loadLinkPreview(message);
			}
		}

		async void loadLinkPreview(MessageItemModel message)
		{
			try
			{
				if (!message.IsPlainText || message.LinkPreview != null)
				{
					return;
				}

				var url = MessageLinkParser.GetFirstUrl(message.Text);

				if (url == null)
				{
					return;
				}

				var preview = await LinkPreviewService.GetAsync(url);

				if (preview != null)
				{
					MainThread.BeginInvokeOnMainThread(() => message.LinkPreview = preview);
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		void startSearch(string text)
		{
			_searchCancellation?.Cancel();
			_searchCancellation = new CancellationTokenSource();
			_ = search(text?.Trim(), _searchCancellation.Token);
		}

		async Task search(string text, CancellationToken token)
		{
			try
			{
				if (string.IsNullOrEmpty(text))
				{
					IsSearching = false;
					SearchResults = new ObservableCollection<object>();
					return;
				}

				await Task.Delay(_searchDelayMilliseconds, token);

				var results = await ChatApiService.SearchMessages(
					AppUserData.UserId, ChatId, IsGroupChat, text, _searchLimit) ??
					new List<MessageItemModel>();

				if (token.IsCancellationRequested)
				{
					return;
				}

				var prepared = results.Select(prepare).OrderBy(m => m.Time).ToList();
				SearchResults = new ObservableCollection<object>(buildItems(prepared));
				IsSearching = true;
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}
	}
}
