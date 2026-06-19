using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Statistics.Base.Views.ViewCells
{
	public class StatsPageViewCell : ViewCell
	{
		static Thickness _padding = new Thickness(20);

		public StatsPageViewCell()
		{
			var menuLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.StatisticsBaseTitleColor),
				Style = AppStyles.GetLabelStyle()
			};

			menuLabel.SetBinding(Label.TextProperty, "Title");

			View = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Padding = _padding,
				Children = {
					menuLabel
				}
			};
		}
	}
}

