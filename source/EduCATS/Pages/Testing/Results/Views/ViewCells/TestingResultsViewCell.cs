using EduCATS.Data.Models;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Testing.Results.Views.ViewCells
{
	public class TestingResultsViewCell : ViewCell
	{
		const double _boxSize = 26;
		const float _frameRadius = 10;
		const double _indicatorSpacing = 12;

		static Thickness _frameMargin = new Thickness(10, 5);
		static Thickness _frameContentPadding = new Thickness(15, 18);

		readonly Ellipse _indicator;
		readonly Label _answer;

		TestResultsModel _results;

		public TestingResultsViewCell()
		{
			_answer = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestResultsAnswerTextColor),
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Style = AppStyles.GetLabelStyle()
			};

			_indicator = new Ellipse
			{
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _boxSize,
				WidthRequest = _boxSize
			};

			var border = new Border
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(_frameRadius)
				},
				Margin = _frameMargin,
				Padding = _frameContentPadding,
				Content = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Spacing = _indicatorSpacing,
					Children = {
						_answer,
						_indicator
					}
				}
			};

			View = border;
		}

		protected override void OnBindingContextChanged()
		{
			_results = (TestResultsModel)BindingContext;

			if (_results != null)
			{
				_answer.Text = $"{_results.Number}. {_results.QuestionTitle}";

				_indicator.Fill = _results.Points == 0 ?
					Color.FromArgb(Theme.Current.TestResultsNotCorrectAnswerColor) :
					Color.FromArgb(Theme.Current.TestResultsCorrectAnswerColor);
			}

			base.OnBindingContextChanged();
		}
	}
}