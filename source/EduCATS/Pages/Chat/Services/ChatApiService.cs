using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EduCATS.Networking;
using EduCATS.Pages.Chat.Models;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Wrapper for the chat microservice's REST endpoints.
	/// </summary>
	/// <remarks>
	/// Uses a plain <see cref="HttpClient"/> call (like
	/// <c>SaveMarksPageViewModel.getLecturesVisiting</c> does for the
	/// main API) rather than the <c>DataAccess&lt;T&gt;</c> helper,
	/// since the chat service is a separate backend with its own base
	/// path and, unlike the main API, currently requires no auth token.
	/// </remarks>
	public static class ChatApiService
	{
		/// <summary>
		/// Is the last request considered failed.
		/// </summary>
		public static bool IsError { get; private set; }

		/// <summary>
		/// Get all personal chats for a user.
		/// </summary>
		/// <param name="userId">Current user's id.</param>
		/// <returns>List of chats, or an empty list on failure.</returns>
		public static async Task<List<ChatItemModel>> GetChats(int userId)
		{
			IsError = false;

			try
			{
				using var client = new HttpClient
				{
					Timeout = TimeSpan.FromSeconds(RequestController.RequestTimeoutSeconds)
				};

				var link = $"{ChatLinks.GetAllChats}?userId={userId}";
				var response = await client.GetAsync(link);

				if (!response.IsSuccessStatusCode)
				{
					IsError = true;
					return new List<ChatItemModel>();
				}

				var body = await response.Content.ReadAsStringAsync();
				var chats = JsonConvert.DeserializeObject<List<ChatItemModel>>(body);
				return chats ?? new List<ChatItemModel>();
			}
			catch (Exception)
			{
				IsError = true;
				return new List<ChatItemModel>();
			}
		}

		public static async Task MarkChatAsRead(int userId, int chatId)
		{
			try
			{
				using var client = new HttpClient
				{
					Timeout = TimeSpan.FromSeconds(RequestController.RequestTimeoutSeconds)
				};

				var link = $"{ChatLinks.UpdateReadChat}?userId={userId}&chatId={chatId}";
				await client.GetAsync(link);
			}
			catch (Exception)
			{
				// Best-effort: if this fails, the chat just stays marked
				// unread in the list - not critical enough to show an error.
			}
		}

		public static async Task<List<MessageItemModel>> GetChatMsgs(int userId, int chatId, int limit = 30, int offset = 0)
		{
			IsError = false;

			try
			{
				using var client = new HttpClient
				{
					Timeout = TimeSpan.FromSeconds(RequestController.RequestTimeoutSeconds)
				};

				var link = $"{ChatLinks.GetChatMsgs}?userId={userId}&chatId={chatId}&limit={limit}&offset={offset}";
				var response = await client.GetAsync(link);

				if (!response.IsSuccessStatusCode)
				{
					IsError = true;
					return new List<MessageItemModel>();
				}

				var body = await response.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine($"=== RAW GetChatMsgs body: {body}");
				var messages = JsonConvert.DeserializeObject<List<MessageItemModel>>(body);
				return messages ?? new List<MessageItemModel>();
			}
			catch (Exception)
			{
				IsError = true;
				return new List<MessageItemModel>();
			}
		}
	}
}