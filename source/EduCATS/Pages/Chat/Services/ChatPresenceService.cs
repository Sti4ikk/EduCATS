using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EduCATS.Pages.Chat.Models;
using Microsoft.Maui.ApplicationModel;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Online status of chat participants.
	/// </summary>
	/// <remarks>
	/// Initial values come with the chat list (<c>isOnline</c>),
	/// changes - with the "Status" hub event.
	/// </remarks>
	public static class ChatPresenceService
	{
		static readonly ConcurrentDictionary<int, bool> _online = new ConcurrentDictionary<int, bool>();

		/// <summary>
		/// User's online status changed (raised on the main thread).
		/// </summary>
		public static event Action<int, bool> Changed;

		static ChatPresenceService()
		{
			ChatHubService.UserStatusChanged += set;
		}

		/// <summary>
		/// Start listening for status changes.
		/// </summary>
		/// <remarks>Subscribes in the static constructor.</remarks>
		public static void Initialize()
		{
		}

		/// <summary>
		/// Online status of a user.
		/// </summary>
		/// <param name="userId">User id.</param>
		/// <returns><c>null</c> if unknown.</returns>
		public static bool? Get(int userId) =>
			_online.TryGetValue(userId, out var isOnline) ? isOnline : (bool?)null;

		/// <summary>
		/// Remember statuses received with the chat list.
		/// </summary>
		/// <param name="chats">Personal chats.</param>
		public static void Seed(IEnumerable<ChatItemModel> chats)
		{
			foreach (var chat in chats)
			{
				if (!chat.IsOnline.HasValue)
				{
					continue;
				}

				// Statuses may have changed while the app was in background.
				if (Get(chat.UserId) != chat.IsOnline)
				{
					set(chat.UserId, chat.IsOnline.Value);
				}
			}
		}

		/// <summary>
		/// Forget all statuses (logout).
		/// </summary>
		public static void Clear() => _online.Clear();

		static void set(int userId, bool isOnline)
		{
			_online[userId] = isOnline;

			var changed = Changed;

			if (changed != null)
			{
				MainThread.BeginInvokeOnMainThread(() => changed(userId, isOnline));
			}
		}
	}
}
