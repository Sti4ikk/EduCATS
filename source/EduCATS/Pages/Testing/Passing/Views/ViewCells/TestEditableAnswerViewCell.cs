using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Testing.Passing.Views.ViewCells
{
	public class TestEditableAnswerViewCell : ViewCell
	{
		const float _frameRadius = 10;

		static Thickness _frameMargin = new Thickness(10, 5);
		static Thickness _framePadding = new Thickness(15, 12);

		public TestEditableAnswerViewCell()
		{
			var answerEntry = new Entry
			{
				HorizontalOptions = LayoutOptions.Fill,
				BackgroundColor = Color.FromArgb(Theme.Current.TestPassingEntryColor),
				TextColor = Color.FromArgb(Theme.Current.TestPassingAnswerColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetEntryStyle()
			};

			answerEntry.SetBinding(Entry.TextProperty, "ContentToAnswer");

			View = new Border
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Stroke = null,
				Margin = _frameMargin,
				Padding = _framePadding,
				StrokeShape = new RoundRectangle { CornerRadius = _frameRadius },
				Content = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Children = {
						answerEntry
					}
				}
			};
		}
	}
}