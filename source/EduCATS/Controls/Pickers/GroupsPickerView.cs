using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;


namespace EduCATS.Controls.Pickers
{
	public class GroupsPickerView : Border
	{
		public string ChosenGroupProperty { get; set; }
		public string ChooseGroupCommandProperty { get; set; }

		const string _chosenGroupPropertyDefault = "ChosenGroup";
		const string _chooseGroupCommandPropertyDefault = "ChooseGroupCommand";

		public GroupsPickerView()
		{
			ChosenGroupProperty = _chosenGroupPropertyDefault;
			ChooseGroupCommandProperty = _chooseGroupCommandPropertyDefault;

			BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor);
			StrokeThickness = 0;
			Padding = new Thickness(20, 10);
			HorizontalOptions = LayoutOptions.Fill;

			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(8)
			};

			createViews();
			setGestureRecognizer();
		}

		void createViews()
		{
			Content = createGroupLabel();
		}

		Label createGroupLabel()
		{
			var group = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.BasePickerTextColor),
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};

			group.SetBinding(Label.TextProperty, ChosenGroupProperty);
			return group;
		}

		void setGestureRecognizer()
		{
			var tapGesture = new TapGestureRecognizer();
			tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, ChooseGroupCommandProperty);
			GestureRecognizers.Add(tapGesture);
		}
	}
}