using System.Collections.Generic;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Statistics.Results.Views.ViewCells
{
	public class StatsResultsPageViewCell : ViewCell
	{
		const double _iconSize = 20;
		const double _infoSpacing = 10;
		const double _iconTextSpacing = 8;   // ← добавили
		static Thickness _gridPadding = new Thickness(15);
		static Thickness _resultLabelMargin = new Thickness(10, 0, 0, 0);   // ← добавили

		public StatsResultsPageViewCell()
		{
			var titleLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle()
			};
			titleLabel.SetBinding(Label.TextProperty, "Title");
			titleLabel.SetBinding(VisualElement.IsVisibleProperty, "IsTitle");

			var dateIcon = new Image
			{
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.StatisticsCalendarIcon),
				HeightRequest = _iconSize,
				WidthRequest = _iconSize
			};

			var dateLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			dateLabel.SetBinding(Label.TextProperty, "Date");

			var dateLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				Spacing = _iconTextSpacing,   // ← добавили
				Children = { dateIcon, dateLabel }
			};
			dateLayout.SetBinding(VisualElement.IsVisibleProperty, "IsDate");

			var commentIcon = new Image
			{
				VerticalOptions = LayoutOptions.Start,
				Source = ImageSource.FromFile(Theme.Current.StatisticsCommentIcon),
				HeightRequest = _iconSize,
				WidthRequest = _iconSize
			};

			var commentLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			commentLabel.SetBinding(Label.TextProperty, "Comment");

			var commentLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				Spacing = _iconTextSpacing,   // ← добавили
				Children = { commentIcon, commentLabel }
			};
			commentLayout.SetBinding(VisualElement.IsVisibleProperty, "IsComment");

			var infoLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Start,
				Spacing = _infoSpacing,
				Children = { titleLabel, dateLayout, commentLayout }
			};

			var resultLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsResultsColor),
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				Margin = _resultLabelMargin,   // ← добавили
				Style = AppStyles.GetLabelStyle(NamedSize.Large)
			};
			resultLabel.SetBinding(Label.TextProperty, "Result");

			var gridLayout = new Grid
			{
				Padding = _gridPadding,
				VerticalOptions = LayoutOptions.Fill,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				}
			};

			gridLayout.Add(infoLayout, 0, 0);
			gridLayout.Add(resultLabel, 1, 0);

			View = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Children = { gridLayout }
			};
		}
	}
}