using System;
using System.Threading.Tasks;

namespace EduCATS.Pages.Chat.Services
{
	public enum CallState
	{
		Idle,
		Calling,
		Incoming,
		Connecting,
		Connected,
		Rejected,
		Ended
	}

	public static class CallService
	{
		static bool _initialized;

		public static CallState State { get; private set; } = CallState.Idle;
		public static int? CurrentChatId { get; private set; }
		public static string CurrentConnectionId { get; private set; }

		public static event Action<CallState> StateChanged;
		public static event Action<int> IncomingCall;

		public static void Initialize()
		{
			if (_initialized) return;
			_initialized = true;

			ChatHubService.IncomingCallReceived += OnIncomingCall;
			ChatHubService.CallRejected += OnCallRejected;
			ChatHubService.CallDisconnected += OnCallDisconnected;
			ChatHubService.NewcomerReceived += OnNewcomer;
			ChatHubService.OfferReceived += OnOfferReceived;
			ChatHubService.AnswerReceived += OnAnswerReceived;
			ChatHubService.IceCandidateReceived += OnIceCandidateReceived;

			WebRtcService.OnConnected += () => SetState(CallState.Connected);
		}

		public static async Task StartCall(int chatId)
		{
			Initialize();
			if (State != CallState.Idle) return;

			CurrentChatId = chatId;
			SetState(CallState.Calling);
			await ChatHubService.SendCallRequest(chatId);
		}

		static void OnIncomingCall(int chatId)
		{
			Initialize();
			CurrentChatId = chatId;
			SetState(CallState.Incoming);
			IncomingCall?.Invoke(chatId);
		}

		public static async Task AcceptCall()
		{
			if (State != CallState.Incoming || !CurrentChatId.HasValue) return;

			SetState(CallState.Connecting);
			await ChatHubService.SetVoiceChatConnection(CurrentChatId.Value);
		}

		public static async Task RejectCall(string message = "Call rejected")
		{
			if (!CurrentChatId.HasValue)
			{
				Reset();
				return;
			}

			await ChatHubService.Reject(CurrentChatId.Value, message);
			SetState(CallState.Rejected);
			Reset();
		}

		public static async Task EndCall()
		{
			if (!CurrentChatId.HasValue)
			{
				Reset();
				return;
			}

			WebRtcService.Close();
			await ChatHubService.DisconnectFromChat(CurrentChatId.Value);
			SetState(CallState.Ended);
			Reset();
		}

		static async void OnNewcomer(string connectionId, int chatId)
		{
			if (CurrentChatId != chatId) return;

			CurrentConnectionId = connectionId;
			SetState(CallState.Connecting);

			await WebRtcService.InitPeerConnection(connectionId);
			var offerSdp = await WebRtcService.CreateOffer();
			await ChatHubService.SendOffer(chatId, offerSdp, connectionId);
		}

		static async void OnOfferReceived(int chatId, string offerSdp, string connectionId)
		{
			if (CurrentChatId != chatId) return;

			CurrentConnectionId = connectionId;
			SetState(CallState.Connecting);

			await WebRtcService.InitPeerConnection(connectionId);
			var answerSdp = await WebRtcService.CreateAnswer(offerSdp);
			await ChatHubService.SendAnswer(answerSdp, connectionId);
		}

		static async void OnAnswerReceived(string answerSdp, string connectionId)
		{
			await WebRtcService.SetRemoteAnswer(answerSdp);
		}

		static async void OnIceCandidateReceived(string candidateJson, string connectionId)
		{
			await WebRtcService.AddIceCandidate(candidateJson);
		}

		static void OnCallRejected(int chatId, string message)
		{
			if (CurrentChatId != chatId) return;
			SetState(CallState.Rejected);
			Reset();
		}

		static void OnCallDisconnected(int chatId, string userId)
		{
			if (CurrentChatId != chatId) return;
			WebRtcService.Close();
			SetState(CallState.Ended);
			Reset();
		}

		static void SetState(CallState state)
		{
			State = state;
			StateChanged?.Invoke(state);
		}

		public static void Reset()
		{
			WebRtcService.Close();
			CurrentChatId = null;
			CurrentConnectionId = null;
			SetState(CallState.Idle);
		}
	}
}