using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	/// <summary>
	/// Subject (schedule/consultation) item for a <see cref="BindableLayout"/>.
	/// </summary>
	class SubjectPageViewCell : ContentView
	{
		const double _boxViewSize = 10;
		const double _iconColumnWidth = 20;
		const double _clockIconSize = 20;
		const double _columnSpacing = 8;
		const double _mainLayoutSpacing = 6;
		static Thickness _framePadding = new Thickness(15, 12, 15, 12);

		public SubjectPageViewCell(TodayItemStyles styles)
		{
			var dateColor = Color.FromArgb(Theme.Current.TodayNewsDateColor);

			var clockIcon = new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Source = styles.ClockIcon,
				HeightRequest = _clockIconSize,
				WidthRequest = _clockIconSize
			};

			var date = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = dateColor,
				Style = styles.Small
			};
			date.SetBinding(Label.TextProperty, "Date");

			var address = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = dateColor,
				Style = styles.Micro
			};
			address.SetBinding(Label.TextProperty, "Address");

			var dateLayout = new HorizontalStackLayout
			{
				Spacing = _columnSpacing,
				Children = { date, address }
			};

			var subjectIndicator = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _boxViewSize,
				WidthRequest = _boxViewSize
			};
			subjectIndicator.SetBinding(Ellipse.FillProperty, "Color", converter: styles.ColorConverter);

			var subject = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsTitleColor),
				Style = styles.Small
			};
			subject.SetBinding(Label.TextProperty, "Name");

			var type = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = dateColor,
				Style = styles.Micro
			};
			type.SetBinding(Label.TextProperty, "Type");

			var teacher = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = dateColor,
				Style = styles.Micro
			};
			teacher.SetBinding(Label.TextProperty, "TeacherFullName");

			var informationLayout = new HorizontalStackLayout
			{
				Spacing = _columnSpacing,
				Children = { type, teacher }
			};

			var grid = new Grid
			{
				Padding = _framePadding,
				ColumnSpacing = _columnSpacing,
				RowSpacing = _mainLayoutSpacing,
				BackgroundColor = Color.FromArgb(Theme.Current.TodayNewsItemBackgroundColor),
				ColumnDefinitions = {
					new ColumnDefinition(_iconColumnWidth),
					new ColumnDefinition(GridLength.Star)
				},
				RowDefinitions = {
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto)
				}
			};

			grid.Add(clockIcon, 0, 0);
			grid.Add(dateLayout, 1, 0);
			grid.Add(subjectIndicator, 0, 1);
			grid.Add(subject, 1, 1);
			grid.Add(informationLayout, 1, 2);

			Content = grid;
		}
	}
}
