using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	class SubjectPageViewCell : ViewCell
	{
		const double _boxViewSize = 10;
		const double _boxViewLayoutSize = 20;
		const double _clockIconSize = 20;
		const double _dateLayoutSpacing = 8;
		const double _subjectLayoutSpacing = 8;
		const double _informationLayoutSpacing = 8;
		const double _mainLayoutSpacing = 6;
		static Thickness _framePadding = new Thickness(15, 12, 15, 12);
		static Thickness _informationPadding = new Thickness(28, 2, 0, 0);

		public SubjectPageViewCell()
		{
			var clockIcon = new Image
			{
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.TodayNewsDateIcon),
				HeightRequest = _clockIconSize
			};

			var date = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsDateColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			date.SetBinding(Label.TextProperty, "Date");

			var address = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsDateColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Micro)
			};
			address.SetBinding(Label.TextProperty, "Address");

			var dateLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = _dateLayoutSpacing,
				Children = {
					clockIcon,
					date,
					address
				}
			};

			var subjectIndicator = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _boxViewSize,
				WidthRequest = _boxViewSize,
				Margin = new Thickness(0, 5, 0, 0)
			};
			subjectIndicator.SetBinding(
				Ellipse.FillProperty, "Color", converter: new StringToColorConverter());

			var boxViewLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _boxViewLayoutSize,
				WidthRequest = _boxViewLayoutSize,
				Children = {
					subjectIndicator
				}
			};

			var subject = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsTitleColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			subject.SetBinding(Label.TextProperty, "Name");

			var subjectLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = _subjectLayoutSpacing,
				Children = {
					boxViewLayout,
					subject
				}
			};

			var type = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsDateColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Micro)
			};
			type.SetBinding(Label.TextProperty, "Type");

			var teacher = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsDateColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Micro)
			};
			teacher.SetBinding(Label.TextProperty, "TeacherFullName");

			var informationLayout = new StackLayout
			{
				Padding = _informationPadding,
				Orientation = StackOrientation.Horizontal,
				Spacing = _informationLayoutSpacing ,
				Children = {
					type,
					teacher
				}
			};

			View = new StackLayout
			{
				Padding = _framePadding,
				Spacing = _mainLayoutSpacing,
				BackgroundColor = Color.FromArgb(Theme.Current.TodayNewsItemBackgroundColor),
				Children = {
					dateLayout,
					subjectLayout,
					informationLayout
				}
			};
		}
	}
}