using System;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Helpers.Logs;
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

		/// <summary>
		/// Hub URL the current connection was built for.
		/// </summary>
		static string _connectionUrl;

		/// <summary>
		/// Serializes connecting and disconnecting.
		/// </summary>
		static readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

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
			await _gate.WaitAsync();

			try
			{
				var url = ChatLinks.ChatHub;

				if (IsConnected && _joinedUserId == userId && _connectionUrl == url)
				{
					return;
				}

				// The hub URL is fixed when a connection is built: after a server
				// change (or for another user) the old connection can't be reused.
				if (_connection != null &&
					(_connectionUrl != url || (_joinedUserId.HasValue && _joinedUserId != userId)))
				{
					await disposeConnection();
				}

				if (_connection == null)
				{
					_connection = createConnection(url);
					_connectionUrl = url;
				}

				// Remembered before connecting: if this attempt fails (e.g. no network
				// on app start), sending a message can retry with the same user.
				_joinedUserId = userId;
				_joinedRole = role;

				if (_connection.State == HubConnectionState.Disconnected)
				{
					await _connection.StartAsync();
				}

				await _connection.InvokeAsync("Join", userId.ToString(), role);
			}
			finally
			{
				_gate.Release();
			}

			ConnectionRestored?.Invoke();
		}

		/// <summary>
		/// The connection is (re)established: messages sent meanwhile may have been missed.
		/// </summary>
		/// <remarks>Raised on a background thread.</remarks>
		public static event Action ConnectionRestored;

		/// <summary>
		/// Make sure the connection is alive (e.g. after the app returns from background).
		/// </summary>
		/// <returns><c>true</c> if connected.</returns>
		public static async Task<bool> EnsureConnectedAsync()
		{
			try
			{
				return await ensureConnected();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}

		static HubConnection createConnection(string url)
		{
			var connection = new HubConnectionBuilder()
				.WithUrl(url, options =>
				{
					// Lets the server verify the user (see ChatAuth).
					options.AccessTokenProvider = () => Task.FromResult(ChatAuth.GetToken());
				})
				.WithAutomaticReconnect()
				.Build();

			registerChatEvents(connection);
			registerCallEvents(connection);

			connection.Reconnected += async _ =>
			{
				try
				{
					if (_joinedUserId.HasValue)
					{
						await connection.InvokeAsync("Join", _joinedUserId.Value.ToString(), _joinedRole);
						ConnectionRestored?.Invoke();
					}
				}
				catch (Exception ex)
				{
					AppLogs.Log(ex);
				}
			};

			return connection;
		}

		/// <summary>
		/// Stop and release the current connection.
		/// </summary>
		/// <remarks>Must be called under <see cref="_gate"/>.</remarks>
		/// <returns>Task.</returns>
		static async Task disposeConnection()
		{
			var connection = _connection;
			_connection = null;
			_connectionUrl = null;

			if (connection == null)
			{
				return;
			}

			try
			{
				await connection.DisposeAsync();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		static void registerChatEvents(HubConnection connection)
		{
			connection.On<MessageItemModel>("GetMessage", msg => MessageReceived?.Invoke(msg));
			connection.On<int, bool>("Status", (uid, online) => UserStatusChanged?.Invoke(uid, online));
			connection.On<string, string>("RemovedMessage", (chatId, msgId) => MessageRemoved?.Invoke(chatId, msgId));
			connection.On<string, string, string>("EditedMessage", (chatId, msgId, text) => MessageEdited?.Invoke(chatId, msgId, text));
		}

		static void registerCallEvents(HubConnection connection)
		{
			connection.On<int>("HandleIncomeCall", chatId => IncomingCallReceived?.Invoke(chatId));
			connection.On<int, string>("HandleDisconnection", (chatId, userId) => CallDisconnected?.Invoke(chatId, userId));
			connection.On<string, int>("AddNewcomer", (connectionId, chatId) => NewcomerReceived?.Invoke(connectionId, chatId));

			connection.On<int, object, string>("RegisterOffer", (chatId, offer, connectionId) =>
			{
				OfferReceived?.Invoke(chatId, offer?.ToString(), connectionId);
			});

			connection.On<object, string>("RegisterAnswer", (answer, connectionId) =>
			{
				AnswerReceived?.Invoke(answer?.ToString(), connectionId);
			});

			connection.On<object, string>("HandleNewCandidate", (candidate, connectionId) =>
			{
				IceCandidateReceived?.Invoke(candidate?.ToString(), connectionId);
			});

			connection.On<int, string>("HandleRejection", (chatId, message) => CallRejected?.Invoke(chatId, message));
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

		/// <summary>
		/// Send personal message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <returns><c>true</c> if the message was handed to the server.</returns>
		public static async Task<bool> SendMessage(MessageSendModel message)
		{
			var json = JsonConvert.SerializeObject(message);
			return await invokeWithReconnect("SendMessage", message.UserId.ToString(), json);
		}

		/// <summary>
		/// Send group message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="role">User role.</param>
		/// <returns><c>true</c> if the message was handed to the server.</returns>
		public static async Task<bool> SendGroupMessage(GroupMessageSendModel message, string role)
		{
			var json = JsonConvert.SerializeObject(message);
			return await invokeWithReconnect("SendGroupMessage", message.UserId.ToString(), role, json);
		}

		/// <summary>
		/// Invoke hub method, restoring the connection first if it was lost.
		/// </summary>
		/// <param name="methodName">Hub method name.</param>
		/// <param name="args">Hub method arguments.</param>
		/// <returns><c>true</c> on success.</returns>
		static async Task<bool> invokeWithReconnect(string methodName, params object[] args)
		{
			try
			{
				if (!await ensureConnected())
				{
					return false;
				}

				await _connection.InvokeCoreAsync(methodName, args);
				return true;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}

		/// <summary>
		/// Make sure the connection is up.
		/// </summary>
		/// <remarks>
		/// If automatic reconnect has given up (state is <c>Disconnected</c>),
		/// tries to connect and join again with the last known user.
		/// </remarks>
		/// <returns><c>true</c> if connected.</returns>
		static async Task<bool> ensureConnected()
		{
			if (IsConnected)
			{
				return true;
			}

			if (_connection == null ||
				_connection.State != HubConnectionState.Disconnected ||
				!_joinedUserId.HasValue)
			{
				return false;
			}

			await ConnectAndJoin(_joinedUserId.Value, _joinedRole);
			return IsConnected;
		}

		/// <summary>
		/// Leave the chat (logout, expired session, server change).
		/// </summary>
		/// <returns>Task.</returns>
		public static async Task DisconnectAsync()
		{
			await _gate.WaitAsync();

			try
			{
				_joinedUserId = null;
				_joinedRole = null;
				await disposeConnection();
			}
			finally
			{
				_gate.Release();
			}
		}
	}
}