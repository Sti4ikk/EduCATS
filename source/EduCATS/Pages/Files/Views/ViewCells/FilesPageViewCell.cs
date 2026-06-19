using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Files.Views.ViewCells
{
	public class FilesPageViewCell : ViewCell
	{
		const double _iconDownloadHeight = 30;
		static Thickness _padding = new Thickness(20);
		public FilesPageViewCell()
		{
			var title = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.FilesTitleColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};
			title.SetBinding(Label.TextProperty, "Name");
			var description = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.FilesSizeColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			description.SetBinding(Label.TextProperty, "Size");
			var downloadedIcon = new Image
			{
				HeightRequest = _iconDownloadHeight,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.End,
				Source = ImageSource.FromFile(Theme.Current.FilesDownloadedIcon)
			};
			downloadedIcon.SetBinding(VisualElement.IsVisibleProperty, "IsDownloaded");
			View = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Padding = _padding,
				Children = {
					new StackLayout {
						Children = {
							title,
							description
						}
					},
					downloadedIcon
				}
			};
		}
	}
}