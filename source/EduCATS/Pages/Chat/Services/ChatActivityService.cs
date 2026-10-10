using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EduCATS.Data.User;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Time of the latest message of each chat: chats with new messages
	/// go to the top of the list.
	/// </summary>
	/// <remarks>
	/// Kept on the device (per user): the server doesn't return the chats
	/// ordered by the latest message.
	/// </remarks>
	public static class ChatActivityService
	{
		/// <summary>
		/// Only the latest chats are kept.
		/// </summary>
		const int _maxChats = 300;

		static readonly object _sync = new object();
		static Dictionary<string, DateTime> _times;
		static int _loadedForUserId = -1;

		/// <summary>
		/// Activity of a chat changed (raised on the main thread).
		/// </summary>
		public static event Action Changed;

		/// <summary>
		/// Time (UTC) of the latest known message of a chat.
		/// </summary>
		public static DateTime? Get(int chatId, bool isGroup)
		{
			lock (_sync)
			{
				ensureLoaded();
				return _times.TryGetValue(key(chatId, isGroup), out var time) ? time : (DateTime?)null;
			}
		}

		/// <summary>
		/// A message appeared in the chat.
		/// </summary>
		/// <param name="chatId">Chat ID.</param>
		/// <param name="isGroup">Is group chat.</param>
		/// <param name="time">Message time.</param>
		public static void Touch(int chatId, bool isGroup, DateTime time)
		{
			var utc = toUtc(time);

			lock (_sync)
			{
				ensureLoaded();
				var chatKey = key(chatId, isGroup);

				if (_times.TryGetValue(chatKey, out var known) && known >= utc)
				{
					return;
				}

				_times[chatKey] = utc;
				save();
			}

			var changed = Changed;

			if (changed != null)
			{
				MainThread.BeginInvokeOnMainThread(() => changed());
			}
		}

		/// <summary>
		/// Forget everything (logout).
		/// </summary>
		public static void Clear()
		{
			lock (_sync)
			{
				_times = null;
				_loadedForUserId = -1;
			}
		}

		/// <summary>
		/// Order of a chat list: pinned first, then by the latest message
		/// (known on the device or reported by the server), then chats
		/// with unread messages, the server order otherwise.
		/// </summary>
		public static List<T> Sort<T>(IEnumerable<T> chats) where T : ChatListItemModel =>
			chats
				.OrderByDescending(c => c.IsPinned)
				.ThenByDescending(c => getLatest(c) ?? DateTime.MinValue)
				.ThenByDescending(c => c.Unread > 0)
				.ToList();

		static DateTime? getLatest(ChatListItemModel chat)
		{
			var local = Get(chat.Id, chat.IsGroupChat);
			var server = chat is ChatItemModel personal && personal.LastMessageDate.HasValue ?
				toUtc(personal.LastMessageDate.Value) : (DateTime?)null;

			if (local == null)
			{
				return server;
			}

			return server == null || local > server ? local : server;
		}

		static DateTime toUtc(DateTime time) =>
			time.Kind switch
			{
				DateTimeKind.Local => time.ToUniversalTime(),
				DateTimeKind.Unspecified => DateTime.SpecifyKind(time, DateTimeKind.Utc),
				_ => time
			};

		static string key(int chatId, bool isGroup) => $"{(isGroup ? "g" : "p")}{chatId}";

		static string prefsKey(int userId) => $"CHAT_ACTIVITY_{userId}";

		static void ensureLoaded()
		{
			var userId = AppUserData.UserId;

			if (_times != null && _loadedForUserId == userId)
			{
				return;
			}

			_times = new Dictionary<string, DateTime>();
			_loadedForUserId = userId;

			try
			{
				var raw = Preferences.Default.Get(prefsKey(userId), string.Empty);

				foreach (var entry in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
				{
					var parts = entry.Split('=');

					if (parts.Length == 2 &&
						long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
					{
						_times[parts[0]] = new DateTime(ticks, DateTimeKind.Utc);
					}
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		static void save()
		{
			try
			{
				var entries = _times
					.OrderByDescending(pair => pair.Value)
					.Take(_maxChats)
					.Select(pair => $"{pair.Key}={pair.Value.Ticks.ToString(CultureInfo.InvariantCulture)}");

				Preferences.Default.Set(prefsKey(_loadedForUserId), string.Join(";", entries));
			}
			catch (Exception ex)
			{
				// Not available in unit tests; the order is kept in memory anyway.
				AppLogs.Log(ex);
			}
		}
	}
}
