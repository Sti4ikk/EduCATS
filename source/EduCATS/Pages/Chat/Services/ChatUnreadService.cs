using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using Microsoft.Maui.ApplicationModel;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Unread messages counters of personal and group chats.
	/// </summary>
	/// <remarks>
	/// The "GetMessage" hub event doesn't tell whether a message belongs
	/// to a personal or a group chat (and their ids may collide), so
	/// counters aren't incremented locally: a new message triggers
	/// a (debounced) refresh of the counters from the server.
	/// </remarks>
	public static class ChatUnreadService
	{
		const int _refreshDelayMilliseconds = 1500;

		static readonly object _sync = new object();
		static Dictionary<int, int> _personal = new Dictionary<int, int>();
		static Dictionary<int, int> _group = new Dictionary<int, int>();
		static Timer _refreshTimer;
		static int? _activeChatId;
		static bool _isActiveChatGroup;

		/// <summary>
		/// Counters changed (raised on the main thread).
		/// </summary>
		public static event Action Changed;

		/// <summary>
		/// Unread messages in all chats except muted ones.
		/// </summary>
		public static int TotalUnread { get; private set; }

		static ChatUnreadService()
		{
			ChatHubService.MessageReceived += onMessageReceived;
			ChatLocalSettings.Changed += recalculate;
		}

		/// <summary>
		/// Start listening for new messages.
		/// </summary>
		/// <remarks>Subscribes in the static constructor.</remarks>
		public static void Initialize()
		{
		}

		public static int Get(int chatId, bool isGroup)
		{
			lock (_sync)
			{
				var counters = isGroup ? _group : _personal;
				return counters.TryGetValue(chatId, out var unread) ? unread : 0;
			}
		}

		/// <summary>
		/// Set counters received with the personal chats list.
		/// </summary>
		public static void SetPersonal(IEnumerable<ChatItemModel> chats) =>
			setCounters(chats, isGroup: false);

		/// <summary>
		/// Set counters received with the group chats list.
		/// </summary>
		public static void SetGroups(IEnumerable<GroupChatModel> chats) =>
			setCounters(chats, isGroup: true);

		/// <summary>
		/// Chat that is open now: its messages are read right away.
		/// </summary>
		public static void SetActiveChat(int chatId, bool isGroup)
		{
			lock (_sync)
			{
				_activeChatId = chatId;
				_isActiveChatGroup = isGroup;
			}

			MarkRead(chatId, isGroup);
		}

		/// <summary>
		/// The chat is no longer open.
		/// </summary>
		public static void ClearActiveChat(int chatId, bool isGroup)
		{
			lock (_sync)
			{
				if (_activeChatId == chatId && _isActiveChatGroup == isGroup)
				{
					_activeChatId = null;
				}
			}
		}

		public static void MarkRead(int chatId, bool isGroup)
		{
			lock (_sync)
			{
				(isGroup ? _group : _personal)[chatId] = 0;
			}

			recalculate();
		}

		/// <summary>
		/// Reload counters from the server.
		/// </summary>
		/// <param name="services">Platform services.</param>
		/// <returns>Task.</returns>
		public static async Task RefreshAsync(IPlatformServices services)
		{
			try
			{
				var userId = AppUserData.UserId;

				if (userId <= 0)
				{
					return;
				}

				// Sequential: ChatApiService.IsError describes the last request.
				var chats = await ChatApiService.GetChats(userId);

				if (!ChatApiService.IsError)
				{
					ChatPresenceService.Seed(chats);
					SetPersonal(chats);
				}

				var subjects = await ChatApiService.GetGroups(userId, ChatRoles.Current(services));

				if (!ChatApiService.IsError)
				{
					SetGroups(subjects
						.Where(subject => subject.Groups != null)
						.SelectMany(subject => subject.Groups));
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Forget all counters (logout).
		/// </summary>
		public static void Clear()
		{
			lock (_sync)
			{
				_personal = new Dictionary<int, int>();
				_group = new Dictionary<int, int>();
				_activeChatId = null;
				_refreshTimer?.Dispose();
				_refreshTimer = null;
			}

			recalculate();
		}

		static void setCounters(IEnumerable<ChatListItemModel> chats, bool isGroup)
		{
			lock (_sync)
			{
				var counters = new Dictionary<int, int>();

				foreach (var chat in chats)
				{
					var isOpen = _activeChatId == chat.Id && _isActiveChatGroup == isGroup;
					counters[chat.Id] = isOpen ? 0 : chat.Unread;
				}

				if (isGroup)
				{
					_group = counters;
				}
				else
				{
					_personal = counters;
				}
			}

			recalculate();
		}

		static void onMessageReceived(MessageItemModel message)
		{
			if (message == null || message.IsWrittenBy(AppUserData.Name))
			{
				return;
			}

			lock (_sync)
			{
				// Personal or group - unknown, but the open chat is read anyway.
				if (_activeChatId == message.ChatId)
				{
					return;
				}

				_refreshTimer?.Dispose();
				_refreshTimer = new Timer(
					_ => _ = RefreshAsync(PlatformServices.Current),
					null,
					_refreshDelayMilliseconds,
					Timeout.Infinite);
			}
		}

		static void recalculate()
		{
			int total;
			var mutedPersonal = ChatLocalSettings.GetMuted(isGroup: false);
			var mutedGroup = ChatLocalSettings.GetMuted(isGroup: true);

			lock (_sync)
			{
				total =
					_personal.Where(pair => !mutedPersonal.Contains(pair.Key)).Sum(pair => pair.Value) +
					_group.Where(pair => !mutedGroup.Contains(pair.Key)).Sum(pair => pair.Value);
			}

			TotalUnread = total;

			var changed = Changed;

			if (changed != null)
			{
				MainThread.BeginInvokeOnMainThread(() => changed());
			}
		}
	}
}
