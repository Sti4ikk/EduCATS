using System;
using System.Collections.Generic;
using EduCATS.Data.Caching;
using EduCATS.Data.User;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Chat lists and the latest messages of each chat kept on the device:
	/// screens open instantly and work without network.
	/// </summary>
	/// <remarks>
	/// Uses the app data cache (MonkeyCache), the same as the rest of the app:
	/// it expires in a week and is cleared on logout.
	/// </remarks>
	public static class ChatOfflineCache
	{
		public static List<ChatItemModel> LoadChats() =>
			load<List<ChatItemModel>>(key("PERSONAL_CHATS"));

		public static void SaveChats(List<ChatItemModel> chats) =>
			save(key("PERSONAL_CHATS"), chats);

		public static List<SubjectChatsModel> LoadGroups() =>
			load<List<SubjectChatsModel>>(key("GROUP_CHATS"));

		public static void SaveGroups(List<SubjectChatsModel> subjects) =>
			save(key("GROUP_CHATS"), subjects);

		/// <summary>
		/// Latest page of messages of a chat (newest first, as the server returns them).
		/// </summary>
		public static List<MessageItemModel> LoadMessages(int chatId, bool isGroup) =>
			load<List<MessageItemModel>>(messagesKey(chatId, isGroup));

		public static void SaveMessages(int chatId, bool isGroup, List<MessageItemModel> page) =>
			save(messagesKey(chatId, isGroup), page);

		static string messagesKey(int chatId, bool isGroup) =>
			key($"MESSAGES_{(isGroup ? "G" : "P")}{chatId}");

		/// <summary>
		/// Keys include the user: the device may be shared.
		/// </summary>
		static string key(string name) => $"CHAT_{AppUserData.UserId}_{name}";

		static T load<T>(string cacheKey) where T : class
		{
			try
			{
				var json = DataCaching<string>.Get(cacheKey);
				return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return null;
			}
		}

		static void save<T>(string cacheKey, T data)
		{
			try
			{
				DataCaching<string>.Save(cacheKey, JsonConvert.SerializeObject(data));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}
	}
}
