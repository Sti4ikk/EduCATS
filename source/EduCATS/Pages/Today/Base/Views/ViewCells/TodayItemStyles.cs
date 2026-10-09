using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	/// <summary>
	/// Styles, converters and images shared by all items on the Today page,
	/// so that item templates don't allocate them for every element.
	/// </summary>
	public class TodayItemStyles
	{
		public Style Large { get; } = AppStyles.GetLabelStyle(NamedSize.Large);
		public Style Small { get; } = AppStyles.GetLabelStyle(NamedSize.Small);
		public Style Micro { get; } = AppStyles.GetLabelStyle(NamedSize.Micro);
		public StringToColorConverter ColorConverter { get; } = new StringToColorConverter();
		public ImageSource CalendarIcon { get; } = ImageSource.FromFile(Theme.Current.StatisticsCalendarIcon);
		public ImageSource ClockIcon { get; } = ImageSource.FromFile(Theme.Current.TodayNewsDateIcon);
	}
}
