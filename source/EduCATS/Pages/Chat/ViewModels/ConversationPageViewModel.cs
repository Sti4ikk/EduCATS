using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Personal conversation.
	/// </summary>
	public class ConversationPageViewModel : ConversationViewModelBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="services">Platform services.</param>
		/// <param name="chatId">Chat id.</param>
		/// <param name="title">Other participant's name.</param>
		/// <param name="peerUserId">Other participant's id (for the online status), 0 if unknown.</param>
		public ConversationPageViewModel(IPlatformServices services, int chatId, string title, int peerUserId = 0)
			: base(services, chatId, title)
		{
			PeerUserId = peerUserId;
			updatePeerStatus();
			Start();
		}

		public override bool IsGroupChat => false;

		/// <summary>
		/// Other participant's id.
		/// </summary>
		public int PeerUserId { get; }

		string _peerStatusText;

		/// <summary>
		/// "в сети" / "не в сети", empty if unknown.
		/// </summary>
		public string PeerStatusText
		{
			get => _peerStatusText;
			set => SetProperty(ref _peerStatusText, value);
		}

		bool _isPeerOnline;
		public bool IsPeerOnline
		{
			get => _isPeerOnline;
			set => SetProperty(ref _isPeerOnline, value);
		}

		protected override Task<List<MessageItemModel>> LoadPageAsync(int limit, int offset) =>
			ChatApiService.GetChatMsgs(AppUserData.UserId, ChatId, limit, offset);

		protected override Task MarkReadOnServerAsync() =>
			ChatApiService.MarkChatAsRead(AppUserData.UserId, ChatId);

		protected override Task<bool> SendToHubAsync(OutgoingMessage message) =>
			ChatHubService.SendMessage(new MessageSendModel
			{
				ChatId = ChatId,
				UserId = AppUserData.UserId,
				Text = message.Text,
				FileContent = message.FileContent,
				ImageContent = message.ImageContent,
				FileSize = message.FileSize,
				IsImage = message.IsImage,
				IsFile = message.IsFile
			});

		protected override void OnActivated()
		{
			ChatPresenceService.Changed += onPresenceChanged;
			updatePeerStatus();
		}

		protected override void OnDeactivated()
		{
			ChatPresenceService.Changed -= onPresenceChanged;
		}

		void onPresenceChanged(int userId, bool isOnline)
		{
			if (userId == PeerUserId)
			{
				updatePeerStatus();
			}
		}

		void updatePeerStatus()
		{
			var isOnline = PeerUserId > 0 ? ChatPresenceService.Get(PeerUserId) : null;
			IsPeerOnline = isOnline == true;
			PeerStatusText = isOnline.HasValue ?
				CrossLocalization.Translate(isOnline.Value ? "chat_online" : "chat_offline") :
				string.Empty;
		}
	}
}
