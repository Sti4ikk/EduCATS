using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Controls.SwitchFrame
{
	/// <summary>
	/// Container with switch.
	/// </summary>
	public class SwitchFrame : Border
	{
		/// <summary>
		/// Switch control.
		/// </summary>
		public Switch Switch { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		public SwitchFrame(string title, string description)
		{
			BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor);

			StrokeThickness = 0;

			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(8)
			};

			Padding = 10;

			Switch = CreateSwitch();

			CreateViews(title, description);
		}

		/// <summary>
		/// Create views.
		/// </summary>
		void CreateViews(string title, string description)
		{
			var titleLayout = CreateTitleLayout(title, description);

			Content = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children =
				{
					titleLayout,
					Switch
				}
			};
		}

		/// <summary>
		/// Create switch.
		/// </summary>
		Switch CreateSwitch()
		{
			return new Switch
			{
				HorizontalOptions = LayoutOptions.End
			};
		}

		/// <summary>
		/// Create title layout.
		/// </summary>
		StackLayout CreateTitleLayout(string title, string description)
		{
			var titleLabel = CreateTitleLabel(title);
			var descriptionLabel = CreateDescriptionLabel(description);

			return new StackLayout
			{
				Children =
				{
					titleLabel,
					descriptionLabel
				}
			};
		}

		/// <summary>
		/// Create title label.
		/// </summary>
		Label CreateTitleLabel(string title)
		{
			return new Label
			{
				TextColor = Color.FromArgb(Theme.Current.SwitchFrameTextColor),
				Text = title,
				Style = AppStyles.GetLabelStyle()
			};
		}

		/// <summary>
		/// Create description label.
		/// </summary>
		Label CreateDescriptionLabel(string description)
		{
			return new Label {
				Text = description,
				TextColor = Color.FromArgb(Theme.Current.SwitchFrameDescriptionColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
		}
	}
}

