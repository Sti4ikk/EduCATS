using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Helpers.Logs;
using EduCATS.Networking;
using EduCATS.Pages.Chat.Models;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Wrapper for the chat microservice's REST endpoints.
	/// </summary>
	/// <remarks>
	/// The chat service is a separate backend with its own base path and,
	/// unlike the main API, requires no auth token (same as the web client).
	/// </remarks>
	public static class ChatApiService
	{
		/// <summary>
		/// Timeout of regular requests.
		/// </summary>
		static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Shared client: creating a client per request
		/// costs a new TCP/TLS handshake every time.
		/// </summary>
		/// <remarks>
		/// No client-wide timeout: uploads may take long,
		/// regular requests use <see cref="_requestTimeout"/>.
		/// </remarks>
		static readonly HttpClient _client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

		/// <summary>
		/// Is the last request considered failed.
		/// </summary>
		public static bool IsError { get; private set; }

		/// <summary>
		/// Get all personal chats for a user.
		/// </summary>
		/// <param name="userId">Current user's id.</param>
		/// <returns>List of chats, or an empty list on failure.</returns>
		public static Task<List<ChatItemModel>> GetChats(int userId) =>
			getList<ChatItemModel>($"{ChatLinks.GetAllChats}?userId={userId}");

		public static Task MarkChatAsRead(int userId, int chatId) =>
			sendBestEffort($"{ChatLinks.UpdateReadChat}?userId={userId}&chatId={chatId}");

		/// <summary>
		/// Get a page of personal chat messages, newest first.
		/// </summary>
		/// <returns>Messages, or <c>null</c> on failure.</returns>
		public static Task<List<MessageItemModel>> GetChatMsgs(int userId, int chatId, int limit = 30, int offset = 0) =>
			getList<MessageItemModel>(nullOnError: true,
				$"{ChatLinks.GetChatMsgs}?userId={userId}&chatId={chatId}&limit={limit}&offset={offset}");

		public static Task<List<SubjectChatsModel>> GetGroups(int userId, string role, bool completed = false) =>
			getList<SubjectChatsModel>(
				$"{ChatLinks.GetAllGroups}?userId={userId}&role={role}&completed={completed}");

		/// <summary>
		/// Get a page of group chat messages, newest first.
		/// </summary>
		/// <returns>Messages, or <c>null</c> on failure.</returns>
		public static Task<List<MessageItemModel>> GetGroupMsgs(int userId, int chatId, int limit = 30, int offset = 0) =>
			getList<MessageItemModel>(nullOnError: true,
				$"{ChatLinks.GetGroupMsgs}?userId={userId}&chatId={chatId}&limit={limit}&offset={offset}");

		public static Task MarkGroupChatAsRead(int userId, int chatId) =>
			sendBestEffort($"{ChatLinks.UpdateReadGroupChat}?userId={userId}&chatId={chatId}");

		/// <summary>
		/// Search messages of a chat on the server, newest first.
		/// </summary>
		/// <returns>Messages, or <c>null</c> on failure.</returns>
		public static Task<List<MessageItemModel>> SearchMessages(
			int userId, int chatId, bool isGroupChat, string searchText, int limit = 50, int offset = 0) =>
			getList<MessageItemModel>(nullOnError: true,
				$"{ChatLinks.SearchMessages}?userId={userId}&chatId={chatId}" +
				$"&isGroupChat={isGroupChat.ToString().ToLowerInvariant()}" +
				$"&searchText={Uri.EscapeDataString(searchText ?? string.Empty)}&limit={limit}&offset={offset}");

		/// <summary>
		/// Get the roster of an academic group.
		/// </summary>
		/// <param name="groupId">Academic group id (not chat id - see
		/// <c>GroupChatModel.GroupId</c>).</param>
		/// <returns>List of students, or an empty list on failure.</returns>
		public static Task<List<StudentItemModel>> GetStudentsByGroupId(int groupId) =>
			getList<StudentItemModel>($"{ChatLinks.GetStudentsByGroupId}?groupId={groupId}");

		/// <summary>
		/// URL of a chat attachment.
		/// </summary>
		/// <param name="chatId">Chat id the attachment belongs to.</param>
		/// <param name="fileName">Stored file name
		/// (<see cref="MessageItemModel.FileContent"/>).</param>
		/// <returns>URL.</returns>
		public static string GetFileUrl(int chatId, string fileName) =>
			$"{ChatLinks.DownloadFile}?chatId={chatId}&file={Uri.EscapeDataString(fileName)}";

		/// <summary>
		/// Download a chat attachment's raw bytes.
		/// </summary>
		/// <param name="chatId">Chat id the attachment belongs to.</param>
		/// <param name="fileName">Stored file name (as returned in
		/// <see cref="MessageItemModel.FileContent"/>).</param>
		/// <returns>File bytes, or <c>null</c> on failure.</returns>
		public static async Task<byte[]> DownloadFile(int chatId, string fileName)
		{
			IsError = false;

			try
			{
				using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
				using var response = await sendAsync(HttpMethod.Get, GetFileUrl(chatId, fileName), null, cancellation.Token).ConfigureAwait(false);

				if (!response.IsSuccessStatusCode)
				{
					IsError = true;
					return null;
				}

				return await response.Content.ReadAsByteArrayAsync(cancellation.Token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				IsError = true;
				return null;
			}
		}

		/// <summary>
		/// Upload a file to a chat (same contract as the web client:
		/// multipart form with the file under its name and <c>ChatId</c>).
		/// </summary>
		/// <remarks>
		/// After the upload a message with <see cref="MessageItemModel.FileContent"/>
		/// set to <paramref name="fileName"/> is sent through the hub.
		/// </remarks>
		/// <param name="chatId">Chat id.</param>
		/// <param name="filePath">Local file path.</param>
		/// <param name="fileName">File name on the server.</param>
		/// <param name="progress">Upload progress from 0 to 1.</param>
		/// <returns><c>true</c> on success.</returns>
		public static async Task<bool> UploadFile(int chatId, string filePath, string fileName, IProgress<double> progress)
		{
			try
			{
				await using var file = File.OpenRead(filePath);
				using var fileContent = new ProgressStreamContent(file, progress);
				using var form = new MultipartFormDataContent();
				form.Add(fileContent, fileName, fileName);
				form.Add(new StringContent(chatId.ToString()), "ChatId");

				using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(10));
				using var response = await sendAsync(HttpMethod.Post, ChatLinks.UploadFile, form, cancellation.Token).ConfigureAwait(false);

				if (!response.IsSuccessStatusCode)
				{
					AppLogs.Log($"Upload failed: {(int)response.StatusCode}", nameof(UploadFile));
					return false;
				}

				progress?.Report(1);
				return true;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}

		/// <summary>
		/// Send a request with the access token (see <see cref="ChatAuth"/>).
		/// </summary>
		static Task<HttpResponseMessage> sendAsync(
			HttpMethod method, string url, HttpContent content, CancellationToken cancellationToken)
		{
			var request = new HttpRequestMessage(method, url) { Content = content };
			ChatAuth.Apply(request);
			return _client.SendAsync(request, cancellationToken);
		}

		static Task<List<T>> getList<T>(string link) => getList<T>(false, link);

		/// <summary>
		/// Get a list.
		/// </summary>
		/// <param name="nullOnError">
		/// Return <c>null</c> on failure instead of an empty list. Lets callers tell
		/// a failure from an empty result without the shared <see cref="IsError"/>
		/// flag, which concurrent requests may overwrite.
		/// </param>
		/// <param name="link">URL.</param>
		/// <returns>List.</returns>
		static async Task<List<T>> getList<T>(bool nullOnError, string link)
		{
			IsError = false;

			try
			{
				using var cancellation = new CancellationTokenSource(_requestTimeout);
				using var response = await sendAsync(HttpMethod.Get, link, null, cancellation.Token).ConfigureAwait(false);

				if (!response.IsSuccessStatusCode)
				{
					IsError = true;
					return nullOnError ? null : new List<T>();
				}

				var body = await response.Content.ReadAsStringAsync(cancellation.Token).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<List<T>>(body) ?? new List<T>();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				IsError = true;
				return nullOnError ? null : new List<T>();
			}
		}

		/// <summary>
		/// Request whose failure isn't critical (e.g. marking a chat read:
		/// if it fails, the chat just stays marked unread in the list).
		/// </summary>
		static async Task sendBestEffort(string link)
		{
			try
			{
				using var cancellation = new CancellationTokenSource(_requestTimeout);
				using var response = await sendAsync(HttpMethod.Get, link, null, cancellation.Token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Stream content reporting upload progress.
		/// </summary>
		class ProgressStreamContent : HttpContent
		{
			const int _bufferSize = 81920;

			readonly Stream _stream;
			readonly IProgress<double> _progress;

			public ProgressStreamContent(Stream stream, IProgress<double> progress)
			{
				_stream = stream;
				_progress = progress;
				Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			}

			protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
			{
				var buffer = new byte[_bufferSize];
				var total = _stream.Length;
				long sent = 0;
				int read;

				while ((read = await _stream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
				{
					await stream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
					sent += read;

					if (total > 0)
					{
						_progress?.Report((double)sent / total);
					}
				}
			}

			protected override bool TryComputeLength(out long length)
			{
				length = _stream.Length;
				return true;
			}
		}
	}
}
