using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Testing.Passing.Views.ViewCells
{
	public class TestEditableAnswerViewCell : ViewCell
	{
		const float _frameRadius = 10;

		static Thickness _frameMargin = new Thickness(10);

		public TestEditableAnswerViewCell()
		{
			var answerEntry = new Entry {
				HorizontalOptions = LayoutOptions.Fill,
				BackgroundColor = Color.FromArgb(Theme.Current.TestPassingEntryColor),
				TextColor = Color.FromArgb(Theme.Current.TestPassingAnswerColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetEntryStyle()
			};

			answerEntry.SetBinding(Entry.TextProperty, "ContentToAnswer");

			View = new Frame {
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				HasShadow = false,
				Margin = _frameMargin,
				CornerRadius = _frameRadius,
				Content = new StackLayout {
					Orientation = StackOrientation.Horizontal,
					Children = {
						answerEntry
					}
				}
			};
		}
	}
}

