using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using Microsoft.Maui.Controls;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Group (subject) conversation.
	/// </summary>
	public class GroupConversationPageView : ConversationPageViewBase
	{
		readonly int _groupId;

		public GroupConversationPageView(int chatId, int groupId, string role, string title)
			: base(new GroupConversationPageViewModel(PlatformServices.Current, chatId, role, title))
		{
			_groupId = groupId;
			InitializeView();
		}

		protected override IEnumerable<View> CreateHeaderIcons()
		{
			// Roster of the group (modal popup).
			yield return CreateHeaderIcon("icon_students.png", 23, CrossLocalization.Translate("chat_students_title"), openStudents);

			// TODO: логика звонка для группового чата.
			yield return CreateHeaderIcon("icon_phone.png", 18, CrossLocalization.Translate("a11y_call"), () => Task.CompletedTask);
		}

		Task openStudents() =>
			Navigation.PushModalAsync(new GroupStudentsPageView(_groupId, ViewModel.Title));
	}
}
