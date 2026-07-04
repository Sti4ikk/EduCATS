using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;   // ← добавить
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Today.Base.Views.ViewCells
{
	public class CalendarCollectionViewCell : ContentView
	{
		const double _baseRowHeight = 30;

		public CalendarCollectionViewCell(string labelBinding, bool selectionEnabled = false)
		{
			var colorConverter = new StringToColorConverter();
			var grid = new Grid
			{
				HeightRequest = _baseRowHeight
			};

			var contentLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.TodayCalendarBaseTextColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};

			if (selectionEnabled)
			{
				contentLabel.SetBinding(Label.TextColorProperty, "TextColor", converter: colorConverter);
			}

			contentLabel.SetBinding(Label.TextProperty, labelBinding);

			if (selectionEnabled)
			{
				var selectedEllipse = new Ellipse   // ← было BoxView
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					HeightRequest = _baseRowHeight,
					WidthRequest = _baseRowHeight
				};
				selectedEllipse.SetBinding(Ellipse.FillProperty, "SelectionColor", converter: colorConverter);
				grid.Children.Add(selectedEllipse);
			}
			else
			{
				contentLabel.FontSize = 10;
			}

			grid.Children.Add(contentLabel);
			Content = grid;
		}
	}
}