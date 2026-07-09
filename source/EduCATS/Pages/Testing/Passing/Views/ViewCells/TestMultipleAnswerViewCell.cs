using EduCATS.Fonts;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Testing.Passing.Models;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Testing.Passing.Views.ViewCells
{
	public class TestMultipleAnswerViewCell : ViewCell
	{
		const double _boxSize = 26;
		const float _frameRadius = 10;
		const double _indicatorSpacing = 12;

		static Thickness _frameMargin = new Thickness(10, 5);
		static Thickness _framePadding = new Thickness(15, 18);

		TestPassingAnswerModel _answerModel;
		readonly Label _answer;
		readonly Ellipse _indicator;

		public TestMultipleAnswerViewCell()
		{
			_indicator = new Ellipse
			{
				HeightRequest = _boxSize,
				WidthRequest = _boxSize,
				Fill = Color.FromArgb(Theme.Current.AppBackgroundColor),
				VerticalOptions = LayoutOptions.Start
			};

			_answer = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestPassingAnswerColor),
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				LineBreakMode = LineBreakMode.WordWrap,
				Style = AppStyles.GetLabelStyle()
			};

			_answer.SetBinding(Label.TextProperty, "Content");

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
					Spacing = _indicatorSpacing,
					Children = {
						_indicator,
						_answer
					}
				}
			};
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			_answerModel = (TestPassingAnswerModel)BindingContext;

			if (_answerModel == null)
			{
				return;
			}

			if (_answerModel.IsSelected)
			{
				_indicator.Fill = Color.FromArgb(Theme.Current.TestPassingSelectionColor);
				_answer.TextColor = Color.FromArgb(Theme.Current.TestPassingSelectionColor);
				_answer.FontAttributes = FontAttributes.Bold;
				_answer.FontFamily = FontsController.GetCurrentFont(true);
			}
			else
			{
				_indicator.Fill = Color.FromArgb(Theme.Current.AppBackgroundColor);
				_answer.TextColor = Color.FromArgb(Theme.Current.TestPassingAnswerColor);
				_answer.FontAttributes = FontAttributes.None;
				_answer.FontFamily = FontsController.GetCurrentFont(false);
			}
		}
	}
}