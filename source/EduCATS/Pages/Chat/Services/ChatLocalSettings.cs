using System;
using System.Collections.Generic;
using System.Linq;
using EduCATS.Helpers.Logs;
using Microsoft.Maui.Storage;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Per-chat settings kept on the device: pinned and muted chats, drafts.
	/// </summary>
	/// <remarks>
	/// The server has no such settings, so they are local. Stored in
	/// preferences, which are cleared on logout.
	/// </remarks>
	public static class ChatLocalSettings
	{
		const string _pinnedKind = "PINNED";
		const string _mutedKind = "MUTED";
		const string _draftPrefix = "CHAT_DRAFT_";

		/// <summary>
		/// Pinned or muted chats changed.
		/// </summary>
		public static event Action Changed;

		public static bool IsPinned(int chatId, bool isGroup) =>
			getIds(_pinnedKind, isGroup).Contains(chatId);

		public static bool IsMuted(int chatId, bool isGroup) =>
			getIds(_mutedKind, isGroup).Contains(chatId);

		/// <summary>
		/// Ids of muted chats.
		/// </summary>
		public static IReadOnlyCollection<int> GetMuted(bool isGroup) =>
			getIds(_mutedKind, isGroup);

		/// <summary>
		/// Ids of pinned chats.
		/// </summary>
		public static IReadOnlyCollection<int> GetPinned(bool isGroup) =>
			getIds(_pinnedKind, isGroup);

		public static void SetPinned(int chatId, bool isGroup, bool isPinned) =>
			setId(_pinnedKind, chatId, isGroup, isPinned);

		public static void SetMuted(int chatId, bool isGroup, bool isMuted) =>
			setId(_mutedKind, chatId, isGroup, isMuted);

		/// <summary>
		/// Get unsent text of a chat.
		/// </summary>
		public static string GetDraft(int chatId, bool isGroup)
		{
			try
			{
				return Preferences.Default.Get(draftKey(chatId, isGroup), string.Empty);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Save unsent text of a chat (removed if empty).
		/// </summary>
		public static void SetDraft(int chatId, bool isGroup, string text)
		{
			try
			{
				var key = draftKey(chatId, isGroup);

				if (string.IsNullOrWhiteSpace(text))
				{
					Preferences.Default.Remove(key);
					return;
				}

				Preferences.Default.Set(key, text);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		static string draftKey(int chatId, bool isGroup) =>
			$"{_draftPrefix}{kindSuffix(isGroup)}_{chatId}";

		static string listKey(string kind, bool isGroup) =>
			$"CHAT_{kind}_{kindSuffix(isGroup)}";

		static string kindSuffix(bool isGroup) => isGroup ? "GROUP" : "PERSONAL";

		static HashSet<int> getIds(string kind, bool isGroup)
		{
			try
			{
				var raw = Preferences.Default.Get(listKey(kind, isGroup), string.Empty);

				return raw
					.Split(',', StringSplitOptions.RemoveEmptyEntries)
					.Select(id => int.TryParse(id, out var value) ? value : (int?)null)
					.Where(id => id.HasValue)
					.Select(id => id.Value)
					.ToHashSet();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return new HashSet<int>();
			}
		}

		static void setId(string kind, int chatId, bool isGroup, bool isSet)
		{
			try
			{
				var ids = getIds(kind, isGroup);
				var isChanged = isSet ? ids.Add(chatId) : ids.Remove(chatId);

				if (!isChanged)
				{
					return;
				}

				Preferences.Default.Set(listKey(kind, isGroup), string.Join(",", ids));
				Changed?.Invoke();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}
	}
}
