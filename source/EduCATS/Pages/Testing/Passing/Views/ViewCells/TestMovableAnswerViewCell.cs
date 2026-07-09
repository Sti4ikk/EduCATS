using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Testing.Passing.Views.ViewCells
{
	public class TestMovableAnswerViewCell : ViewCell
	{
		const double _arrowLayoutWidth = 50;
		const float _frameRadius = 10;
		static Thickness _frameMargin = new Thickness(10, 5);
		static Thickness _framePadding = new Thickness(15, 18);

		public TestMovableAnswerViewCell()
		{
			var answer = createAnswerLabel();
			var upArrow = createArrow(true, "UpMovableAnswerCommand");
			var downArrow = createArrow(false, "DownMovableAnswerCommand");

			View = new Border
			{
				Margin = _frameMargin,
				Padding = _framePadding,
				Stroke = null,
				StrokeShape = new RoundRectangle { CornerRadius = _frameRadius },
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Children = {
						upArrow,
						answer,
						downArrow
					}
				}
			};
		}

		Label createAnswerLabel()
		{
			var answer = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestPassingAnswerColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};
			answer.SetBinding(Label.TextProperty, "Content");
			return answer;
		}

		StackLayout createArrow(bool up, string commandString)
		{
			var arrowIcon = createArrowIcon(up);
			var arrowLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = up ? LayoutOptions.Start : LayoutOptions.End,
				WidthRequest = _arrowLayoutWidth,
				Children = {
					arrowIcon
				}
			};

			var gestureRecognizer = new TapGestureRecognizer();
			gestureRecognizer.SetBinding(TapGestureRecognizer.CommandParameterProperty, "Id");
			gestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, commandString);
			arrowLayout.GestureRecognizers.Add(gestureRecognizer);
			return arrowLayout;
		}

		Image createArrowIcon(bool up)
		{
			return new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(
					up ?
					Theme.Current.TestPassingArrowUpIcon :
					Theme.Current.TestPassingArrowDownIcon)
			};
		}
	}
}