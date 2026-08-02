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

		public static string CurrentRole => _joinedRole;
		public static bool IsConnected => _connection?.State == HubConnectionState.Connected;

		// CHAT EVENTS
		public static event Action<MessageItemModel> MessageReceived;
		public static event Action<int, bool> UserStatusChanged;
		public static event Action<string, string> MessageRemoved;
		public static event Action<string, string, string> MessageEdited;

		// CALL EVENTS
		public static event Action<int> IncomingCallReceived;
		public static event Action<int, string> CallDisconnected;
		public static event Action<string, int> NewcomerReceived;
		public static event Action<int, string, string> OfferReceived;
		public static event Action<string, string> AnswerReceived;
		public static event Action<string, string> IceCandidateReceived;
		public static event Action<int, string> CallRejected;

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

				RegisterChatEvents();
				RegisterCallEvents();

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

		static void RegisterChatEvents()
		{
			_connection.On<MessageItemModel>("GetMessage", msg => MessageReceived?.Invoke(msg));
			_connection.On<int, bool>("Status", (uid, online) => UserStatusChanged?.Invoke(uid, online));
			_connection.On<string, string>("RemovedMessage", (chatId, msgId) => MessageRemoved?.Invoke(chatId, msgId));
			_connection.On<string, string, string>("EditedMessage", (chatId, msgId, text) => MessageEdited?.Invoke(chatId, msgId, text));
		}

		static void RegisterCallEvents()
		{
			_connection.On<int>("HandleIncomeCall", chatId => IncomingCallReceived?.Invoke(chatId));
			_connection.On<int, string>("HandleDisconnection", (chatId, userId) => CallDisconnected?.Invoke(chatId, userId));
			_connection.On<string, int>("AddNewcomer", (connectionId, chatId) => NewcomerReceived?.Invoke(connectionId, chatId));

			_connection.On<int, object, string>("RegisterOffer", (chatId, offer, connectionId) =>
			{
				OfferReceived?.Invoke(chatId, offer?.ToString(), connectionId);
			});

			_connection.On<object, string>("RegisterAnswer", (answer, connectionId) =>
			{
				AnswerReceived?.Invoke(answer?.ToString(), connectionId);
			});

			_connection.On<object, string>("HandleNewCandidate", (candidate, connectionId) =>
			{
				IceCandidateReceived?.Invoke(candidate?.ToString(), connectionId);
			});

			_connection.On<int, string>("HandleRejection", (chatId, message) => CallRejected?.Invoke(chatId, message));
		}

		public static async Task SendCallRequest(int chatId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("SendCallRequest", _joinedUserId?.ToString(), chatId);
		}

		public static async Task SetVoiceChatConnection(int chatId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("SetVoiceChatConnection", chatId, _joinedUserId?.ToString());
		}

		public static async Task SendOffer(int chatId, string offerSdp, string fromConnectionId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("SendOffer", chatId, offerSdp, fromConnectionId);
		}

		public static async Task SendAnswer(string answerSdp, string fromConnectionId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("SendAnswer", answerSdp, fromConnectionId);
		}

		public static async Task FireCandidate(string candidateJson, string connectionId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("FireCandidate", candidateJson, connectionId);
		}

		public static async Task Reject(int chatId, string message)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("Reject", chatId, message);
		}

		public static async Task DisconnectFromChat(int chatId)
		{
			if (!IsConnected) return;
			await _connection.InvokeAsync("DisconnectFromChat", _joinedUserId?.ToString(), chatId);
		}

		public static async Task SendMessage(MessageSendModel message)
		{
			if (!IsConnected) return;
			var json = JsonConvert.SerializeObject(message);
			await _connection.InvokeAsync("SendMessage", message.UserId.ToString(), json);
		}

		public static async Task SendGroupMessage(GroupMessageSendModel message, string role)
		{
			if (!IsConnected) return;
			var json = JsonConvert.SerializeObject(message);
			await _connection.InvokeAsync("SendGroupMessage", message.UserId.ToString(), role, json);
		}

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