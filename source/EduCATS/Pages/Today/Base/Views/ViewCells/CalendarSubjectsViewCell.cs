using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	public class CalendarSubjectsViewCell : ViewCell
	{
		const double _boxViewSize = 10;

		static Thickness _padding = new Thickness(20, 10);
		static Thickness _boxViewMargin = new Thickness(10, 0);

		public CalendarSubjectsViewCell()
		{
			var subjectColorView = new BoxView {
				Margin = _boxViewMargin,
				HeightRequest = _boxViewSize,
				WidthRequest = _boxViewSize,
				CornerRadius = _boxViewSize / 2,
				VerticalOptions = LayoutOptions.Center
			};

			subjectColorView.SetBinding(BoxView.ColorProperty, "Color", converter: new StringToColorConverter());

			var subject = new Label {
				TextColor = Color.FromArgb(Theme.Current.TodayCalendarSubjectTextColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};

			subject.SetBinding(Label.TextProperty, "Subject");

			View = new StackLayout {
				BackgroundColor = Color.FromArgb(Theme.Current.TodaySubjectBackgroundColor),
				Padding = _padding,
				Orientation = StackOrientation.Horizontal,
				Children = {
					subjectColorView,
					subject
				}
			};
		}
	}
}

