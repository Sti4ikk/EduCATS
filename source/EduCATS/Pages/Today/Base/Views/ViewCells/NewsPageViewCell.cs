using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	/// <summary>
	/// News item for <see cref="CollectionView"/>.
	/// Uses a single flat <see cref="Grid"/> and shared styles/converters
	/// to keep scrolling cheap.
	/// </summary>
	public class NewsPageViewCell : ContentView
	{
		const double _boxViewSize = 10;
		const double _iconColumnWidth = 20;
		const double _clockIconSize = 20;
		const double _viewCornerRadius = 10;
		static Thickness _framePadding = new Thickness(10);
		static Thickness _frameMargin = new Thickness(10, 0, 10, 10);

		public NewsPageViewCell(TodayItemStyles styles)
		{
			var title = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TodayNewsTitleColor),
				Style = styles.Large
			};
			title.SetBinding(Label.TextProperty, "Title");

			var subjectIndicator = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _boxViewSize,
				WidthRequest = _boxViewSize
			};
			subjectIndicator.SetBinding(Ellipse.FillProperty, "SubjectColor", converter: styles.ColorConverter);

			var subject = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsSubjectColor),
				Style = styles.Micro
			};
			subject.SetBinding(Label.TextProperty, "SubjectName");

			var clockIcon = new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Source = styles.CalendarIcon,
				HeightRequest = _clockIconSize,
				WidthRequest = _clockIconSize
			};

			var date = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayNewsDateColor),
				Style = styles.Micro
			};
			date.SetBinding(Label.TextProperty, "Date");

			var grid = new Grid
			{
				ColumnSpacing = 6,
				RowSpacing = 4,
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

			grid.Add(title, 0, 0);
			Grid.SetColumnSpan(title, 2);
			grid.Add(subjectIndicator, 0, 1);
			grid.Add(subject, 1, 1);
			grid.Add(clockIcon, 0, 2);
			grid.Add(date, 1, 2);

			Content = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(_viewCornerRadius)
				},
				Padding = _framePadding,
				Margin = _frameMargin,
				BackgroundColor = Color.FromArgb(Theme.Current.TodayNewsItemBackgroundColor),
				Content = grid
			};
		}
	}
}
