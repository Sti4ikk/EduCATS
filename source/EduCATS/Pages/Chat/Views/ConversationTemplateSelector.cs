using EduCATS.Pages.Chat.Models;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.Views
{
	public class ConversationTemplateSelector : DataTemplateSelector
	{
		public DataTemplate MessageTemplate { get; set; }
		public DataTemplate DateSeparatorTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return item is DateSeparatorModel ? DateSeparatorTemplate : MessageTemplate;
		}
	}
}