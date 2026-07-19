using System;
using System.Threading.Tasks;
using EduCATS.Networking;
using EduCATS.Pages.Chat.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Services
{
	public static class ChatHubService
	{
		static HubConnection _connection;
		static int? _joinedUserId;
		static string _joinedRole;

		public static event Action<MessageItemModel> MessageReceived;
		public static event Action<int, bool> UserStatusChanged;
		public static event Action<string, string> MessageRemoved;
		public static event Action<string, string, string> MessageEdited;

		public static bool IsConnected =>
			_connection?.State == HubConnectionState.Connected;

		public static async Task ConnectAndJoin(int userId, string role)
		{
			if (IsConnected && _joinedUserId == userId)
			{
				return;
			}

			if (_connection == null)
			{
				_connection = new HubConnectionBuilder()
					.WithUrl(ChatLinks.ChatHub)
					.WithAutomaticReconnect()
					.Build();

				_connection.On<MessageItemModel>("GetMessage", msg =>
					MessageReceived?.Invoke(msg));

				_connection.On<int, bool>("Status", (uid, online) =>
					UserStatusChanged?.Invoke(uid, online));

				_connection.On<string, string>("RemovedMessage", (chatId, msgId) =>
					MessageRemoved?.Invoke(chatId, msgId));

				_connection.On<string, string, string>("EditedMessage", (chatId, msgId, text) =>
					MessageEdited?.Invoke(chatId, msgId, text));

				// Uses the fields below rather than a closure, so it
				// always re-joins as whichever user is currently
				// connected, even if that changed since the handler
				// was first registered.
				_connection.Reconnected += async _ =>
				{
					if (_joinedUserId.HasValue)
					{
						await _connection.InvokeAsync("Join", _joinedUserId.Value.ToString(), _joinedRole);
					}
				};
			}

			if (_connection.State == HubConnectionState.Disconnected)
			{
				await _connection.StartAsync();
			}

			await _connection.InvokeAsync("Join", userId.ToString(), role);
			_joinedUserId = userId;
			_joinedRole = role;
		}

		public static async Task SendMessage(MessageSendModel message)
		{
			if (!IsConnected)
			{
				return;
			}

			var json = JsonConvert.SerializeObject(message);
			await _connection.InvokeAsync("SendMessage", message.UserId.ToString(), json);
		}

		/// <summary>
		/// Fully disconnect and forget the joined user - call this on
		/// logout so a subsequent login (as any user, including the
		/// same one) always re-joins cleanly.
		/// </summary>
		public static async Task DisconnectAsync()
		{
			_joinedUserId = null;
			_joinedRole = null;

			if (_connection != null)
			{
				await _connection.StopAsync();
			}
		}
	}
}