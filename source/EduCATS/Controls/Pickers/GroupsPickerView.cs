using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;


namespace EduCATS.Controls.Pickers
{
	/// <summary>
	/// Groups picker view.
	/// </summary>

	public class GroupsPickerView : Border
	{
		/// <summary>
		/// Chosen group property.
		/// </summary>
		/// <remarks>
		/// <c>"ChosenGroup"</c> by default.
		/// </remarks>
		public string ChosenGroupProperty { get; set; }

		/// <summary>
		/// Chosen group command property.
		/// </summary>
		/// <remarks>
		/// <c>"ChooseGroupCommand"</c> by default.
		/// </remarks>
		public string ChooseGroupCommandProperty { get; set; }

		/// <summary>
		/// Default chosen group property.
		/// </summary>
		const string _chosenGroupPropertyDefault = "ChosenGroup";

		/// <summary>
		/// Default choose group command property.
		/// </summary>
		const string _chooseGroupCommandPropertyDefault = "ChooseGroupCommand";

		/// <summary>
		/// Constructor.
		/// </summary>
		public GroupsPickerView()
		{
			ChosenGroupProperty = _chosenGroupPropertyDefault;
			ChooseGroupCommandProperty = _chooseGroupCommandPropertyDefault;

			BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor);

			StrokeThickness = 0;

			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(8)
			};

			createViews();
			setGestureRecognizer();
		}

		/// <summary>
		/// Create views.
		/// </summary>
		void createViews()
		{
			Content = new StackLayout {
				HorizontalOptions = LayoutOptions.Center,
				Children = {
					createGroupLabel()
				}
			};
		}

		/// <summary>
		/// Create group label.
		/// </summary>
		/// <returns>Group label.</returns>
		Label createGroupLabel()
		{
			var group = new Label {
				TextColor = Color.FromArgb(Theme.Current.BasePickerTextColor),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};

			group.SetBinding(Label.TextProperty, ChosenGroupProperty);
			return group;
		}

		/// <summary>
		/// Set tap gesture recognizer.
		/// </summary>
		void setGestureRecognizer()
		{
			var tapGesture = new TapGestureRecognizer();
			tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, ChooseGroupCommandProperty);
			GestureRecognizers.Add(tapGesture);
		}
	}
}

