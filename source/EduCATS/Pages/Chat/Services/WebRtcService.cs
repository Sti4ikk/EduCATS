using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SIPSorcery.Net;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Services
{
	public static class WebRtcService
	{
		static RTCPeerConnection _peerConnection;
		static string _targetConnectionId;

		// Буфер для кандидатов, пришедших до готовности remote description
		static readonly List<RTCIceCandidateInit> _pendingCandidates = new();
		static bool _remoteDescriptionSet;

		public static event Action OnConnected;

		public static async Task InitPeerConnection(string targetConnectionId)
		{
			Close();
			_targetConnectionId = targetConnectionId;

			_remoteDescriptionSet = false;
			_pendingCandidates.Clear();

			// Проверка статуса безопасна из любого потока
			var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

			// Запрашиваем только если разрешение еще не получено
			if (status != PermissionStatus.Granted)
			{
				status = await MainThread.InvokeOnMainThreadAsync(async () =>
				{
					return await Permissions.RequestAsync<Permissions.Microphone>();
				});
			}

			if (status != PermissionStatus.Granted)
			{
				Console.WriteLine("[WebRTC] Microphone permission denied — прерываем инициализацию.");
				return;
			}

			var config = new RTCConfiguration
			{
				iceServers = new List<RTCIceServer>
				{
					new RTCIceServer { urls = "stun:stun.l.google.com:19302" },
					new RTCIceServer { urls = "stun:stun1.l.google.com:19302" }

					// Если тестируете локально (эмулятор + веб на одном хосте/сети)
					// и хотите проверить работу без STUN — закомментируйте две строки выше
					// и оставьте пустой список: iceServers = new List<RTCIceServer>()
				}
			};

			_peerConnection = new RTCPeerConnection(config);

			// Добавляем аудио-трек (микрофон)
			var audioTrack = new MediaStreamTrack(SIPSorceryMedia.Abstractions.SDPWellKnownMediaFormatsEnum.PCMU);
			_peerConnection.addTrack(audioTrack);

			_peerConnection.onicecandidate += async (candidate) =>
			{
				if (candidate != null)
				{
					Console.WriteLine($"[WebRTC] Local ICE candidate found: {candidate.candidate}");

					var jsonCandidate = JsonConvert.SerializeObject(new
					{
						candidate = candidate.candidate,
						sdpMid = candidate.sdpMid,
						sdpMLineIndex = candidate.sdpMLineIndex
					});

					await ChatHubService.FireCandidate(jsonCandidate, _targetConnectionId);
				}
				else
				{
					Console.WriteLine("[WebRTC] ICE gathering finished (candidate == null).");
				}
			};

			// ДИАГНОСТИКА: состояние сбора кандидатов
			_peerConnection.onicegatheringstatechange += (state) =>
			{
				Console.WriteLine($"[WebRTC] ICE gathering state: {state}");
			};

			// ДИАГНОСТИКА: состояние ICE-соединения (важнее всего для поиска проблемы)
			_peerConnection.oniceconnectionstatechange += (state) =>
			{
				Console.WriteLine($"[WebRTC] ICE connection state: {state}");
			};

			// ДИАГНОСТИКА: состояние обмена SDP
			_peerConnection.onsignalingstatechange += () =>
			{
				Console.WriteLine($"[WebRTC] Signaling state: {_peerConnection.signalingState}");
			};

			_peerConnection.onconnectionstatechange += (state) =>
			{
				Console.WriteLine($"[WebRTC] Connection state: {state}");

				if (state == RTCPeerConnectionState.connected)
				{
					OnConnected?.Invoke();
				}
				else if (state == RTCPeerConnectionState.failed || state == RTCPeerConnectionState.disconnected)
				{
					Console.WriteLine($"[WebRTC] Соединение не удалось / разорвано: {state}");
				}
			};
		}

		public static async Task<string> CreateOffer()
		{
			if (_peerConnection == null)
			{
				Console.WriteLine("[WebRTC] CreateOffer: _peerConnection == null");
				return null;
			}

			var offer = _peerConnection.createOffer(null);
			await _peerConnection.setLocalDescription(offer);

			Console.WriteLine("[WebRTC] Offer created and set as local description.");
			return offer.sdp;
		}

		public static async Task<string> CreateAnswer(string offerSdp)
		{
			if (_peerConnection == null)
			{
				Console.WriteLine("[WebRTC] CreateAnswer: _peerConnection == null");
				return null;
			}

			_peerConnection.setRemoteDescription(new RTCSessionDescriptionInit
			{
				type = RTCSdpType.offer,
				sdp = offerSdp
			});

			_remoteDescriptionSet = true;
			FlushPendingCandidates();

			var answer = _peerConnection.createAnswer(null);
			await _peerConnection.setLocalDescription(answer);

			Console.WriteLine("[WebRTC] Answer created and set as local description.");
			return answer.sdp;
		}

		public static Task SetRemoteAnswer(string answerSdp)
		{
			if (_peerConnection != null)
			{
				_peerConnection.setRemoteDescription(new RTCSessionDescriptionInit
				{
					type = RTCSdpType.answer,
					sdp = answerSdp
				});

				_remoteDescriptionSet = true;
				FlushPendingCandidates();

				Console.WriteLine("[WebRTC] Remote answer set.");
			}
			else
			{
				Console.WriteLine("[WebRTC] SetRemoteAnswer: _peerConnection == null");
			}

			return Task.CompletedTask;
		}

		public static Task AddIceCandidate(string candidateJson)
		{
			if (string.IsNullOrEmpty(candidateJson))
			{
				return Task.CompletedTask;
			}

			try
			{
				dynamic obj = JsonConvert.DeserializeObject(candidateJson);

				// Кандидат может прийти без candidate-строки (end-of-candidates) — пропускаем такие
				string candidateStr = obj.candidate;
				if (string.IsNullOrEmpty(candidateStr))
				{
					Console.WriteLine("[WebRTC] Получен пустой/end-of-candidates кандидат — пропускаем.");
					return Task.CompletedTask;
				}

				var init = new RTCIceCandidateInit
				{
					candidate = candidateStr,
					sdpMid = obj.sdpMid,
					// Защита от null — раньше здесь падало с NullReferenceException,
					// если браузер присылал кандидата без sdpMLineIndex
					sdpMLineIndex = obj.sdpMLineIndex != null ? (ushort)obj.sdpMLineIndex : (ushort)0
				};

				if (_peerConnection != null && _remoteDescriptionSet)
				{
					_peerConnection.addIceCandidate(init);
					Console.WriteLine($"[WebRTC] Remote ICE candidate added: {init.candidate}");
				}
				else
				{
					_pendingCandidates.Add(init);
					Console.WriteLine("[WebRTC] Remote description ещё не готов — кандидат отложен в буфер.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[WebRTC] ICE Parsing error: {ex}");
			}

			return Task.CompletedTask;
		}

		static void FlushPendingCandidates()
		{
			if (_peerConnection == null)
			{
				return;
			}

			Console.WriteLine($"[WebRTC] Применяем {_pendingCandidates.Count} отложенных кандидатов.");

			foreach (var candidate in _pendingCandidates)
			{
				_peerConnection.addIceCandidate(candidate);
			}

			_pendingCandidates.Clear();
		}

		public static void Close()
		{
			if (_peerConnection != null)
			{
				_peerConnection.close();
				_peerConnection = null;
				Console.WriteLine("[WebRTC] Peer connection closed.");
			}

			_targetConnectionId = null;
			_remoteDescriptionSet = false;
			_pendingCandidates.Clear();
		}
	}
}