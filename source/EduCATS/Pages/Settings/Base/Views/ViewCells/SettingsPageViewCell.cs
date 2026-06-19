using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Settings.Views.Base.ViewCells
{
	public class SettingsPageViewCell : ViewCell
	{
		const double _forwardIcon = 20;
		const double _settingsIcon = 40;
		static Thickness _padding = new Thickness(20);
		static Thickness _settingsTitleMargin = new Thickness(10, 0, 0, 0);

		public SettingsPageViewCell()
		{
			var settingsIcon = new Image
			{
				HeightRequest = _settingsIcon,
				VerticalOptions = LayoutOptions.Center
			};
			settingsIcon.SetBinding(Image.SourceProperty, "Icon",
				converter: new StringToImageSourceConverter());

			var settingsTitle = new Label
			{
				Margin = _settingsTitleMargin,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.SettingsTitleColor),
				Style = AppStyles.GetLabelStyle()
			};
			settingsTitle.SetBinding(Label.TextProperty, "Title");

			var forwardIcon = new Image
			{
				HeightRequest = _forwardIcon,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.BaseArrowForwardIcon)
			};

			View = new StackLayout
			{
				Padding = _padding,
				Orientation = StackOrientation.Horizontal,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Children = {
					settingsIcon,
					settingsTitle,
					forwardIcon
				}
			};
		}
	}
}