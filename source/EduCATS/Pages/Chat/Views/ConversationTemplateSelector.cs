using EduCATS.Pages.Chat.Models;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Separate templates for each kind of message: a cell has only the views
	/// it shows (a text message doesn't build an image, a file chip and so on).
	/// </summary>
	/// <remarks>
	/// The kind of a message never changes, so cells are recycled
	/// within the same kind.
	/// </remarks>
	public class ConversationTemplateSelector : DataTemplateSelector
	{
		public DataTemplate TextMessageTemplate { get; set; }
		public DataTemplate ImageMessageTemplate { get; set; }
		public DataTemplate FileMessageTemplate { get; set; }
		public DataTemplate DateSeparatorTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is not MessageItemModel message)
			{
				return DateSeparatorTemplate;
			}

			if (message.IsImageMessage)
			{
				return ImageMessageTemplate;
			}

			return message.IsFileMessage ? FileMessageTemplate : TextMessageTemplate;
		}
	}
}
