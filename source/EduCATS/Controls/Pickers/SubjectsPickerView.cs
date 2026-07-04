using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;   // ← добавить
using Microsoft.Maui.Graphics;
using Microsoft.Maui;


namespace EduCATS.Controls.Pickers
{
	public class SubjectsPickerView : Frame
	{
		public double IndicatorSize { get; set; }
		public string ChosenSubjectProperty { get; set; }
		public string ChosenSubjectColorProperty { get; set; }
		public string ChooseSubjectCommandProperty { get; set; }

		const double _indicatorSizeDefault = 10;
		const string _chosenSubjectPropertyDefault = "ChosenSubject";
		const string _chosenSubjectColorPropertyDefault = "ChosenSubjectColor";
		const string _chooseSubjectCommandPropertyDefault = "ChooseSubjectCommand";

		public SubjectsPickerView()
		{
			HasShadow = false;
			IndicatorSize = _indicatorSizeDefault;
			ChosenSubjectProperty = _chosenSubjectPropertyDefault;
			ChosenSubjectColorProperty = _chosenSubjectColorPropertyDefault;
			ChooseSubjectCommandProperty = _chooseSubjectCommandPropertyDefault;
			BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor);

			createViews();
			setGestureRecognizer();
		}

		void createViews()
		{
			Content = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children = {
					createSubjectIndicatorView(),
					createSubjectLabel()
				}
			};
		}

		Ellipse createSubjectIndicatorView()   // ← было BoxView
		{
			var indicator = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = IndicatorSize,
				WidthRequest = IndicatorSize
			};

			indicator.SetBinding(
				Ellipse.FillProperty,
				ChosenSubjectColorProperty,
				converter: new StringToColorConverter());

			return indicator;
		}

		Label createSubjectLabel()
		{
			var subject = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.BasePickerTextColor),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};

			subject.SetBinding(Label.TextProperty, ChosenSubjectProperty);
			return subject;
		}

		void setGestureRecognizer()
		{
			var tapGesture = new TapGestureRecognizer();
			tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, ChooseSubjectCommandProperty);
			GestureRecognizers.Add(tapGesture);
		}
	}
}