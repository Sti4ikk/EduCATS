using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Testing.Base.Views.ViewCells
{
	public class TestingPageViewCell : ViewCell
	{
		const double _iconHeight = 30;
		static Thickness _frameMargin = new Thickness(10);

		public TestingPageViewCell()
		{
			var frame = new Frame
			{
				HasShadow = false,
				Margin = _frameMargin,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor)
			};

			var stackLayout = new StackLayout();

			var titleLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var titleLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestingTitleColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};
			titleLabel.SetBinding(Label.TextProperty, "Title");

			var icon = new Image
			{
				HeightRequest = _iconHeight,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.BaseArrowForwardIcon)
			};

			titleLayout.Children.Add(titleLabel);
			titleLayout.Children.Add(icon);

			var descriptionLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestingDescriptionColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Micro)
			};
			descriptionLabel.SetBinding(Label.TextProperty, "Description");

			stackLayout.Children.Add(titleLayout);
			stackLayout.Children.Add(descriptionLabel);

			frame.Content = stackLayout;
			View = frame;
		}
	}
}