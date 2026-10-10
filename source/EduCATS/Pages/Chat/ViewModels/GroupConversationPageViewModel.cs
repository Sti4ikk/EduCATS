using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// Group (subject) conversation.
	/// </summary>
	/// <remarks>
	/// Kept separate from <see cref="ConversationPageViewModel"/>: personal
	/// and group chat ids come from separate DB tables (so they could
	/// collide), see <see cref="ConversationViewModelBase"/>.
	/// </remarks>
	public class GroupConversationPageViewModel : ConversationViewModelBase
	{
		readonly string _role;

		public GroupConversationPageViewModel(IPlatformServices services, int chatId, string role, string title)
			: base(services, chatId, title)
		{
			_role = role;
			Start();
		}

		public override bool IsGroupChat => true;

		protected override Task<List<MessageItemModel>> LoadPageAsync(int limit, int offset) =>
			ChatApiService.GetGroupMsgs(AppUserData.UserId, ChatId, limit, offset);

		protected override Task MarkReadOnServerAsync() =>
			ChatApiService.MarkGroupChatAsRead(AppUserData.UserId, ChatId);

		protected override Task<bool> SendToHubAsync(OutgoingMessage message) =>
			ChatHubService.SendGroupMessage(new GroupMessageSendModel
			{
				ChatId = ChatId,
				UserId = AppUserData.UserId,
				Text = message.Text,
				FileContent = message.FileContent,
				ImageContent = message.ImageContent,
				FileSize = message.FileSize,
				IsImage = message.IsImage,
				IsFile = message.IsFile
			}, _role);
	}
}
