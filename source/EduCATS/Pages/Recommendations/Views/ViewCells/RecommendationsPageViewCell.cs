using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Recommendations.Views.ViewCells
{
	public class RecommendationsPageViewCell : ViewCell
	{
		static Thickness _padding = new Thickness(20);

		public RecommendationsPageViewCell()
		{
			var title = new Label {
				TextColor = Color.FromArgb(Theme.Current.RecommendationsTitleColor),
				Style = AppStyles.GetLabelStyle()
			};

			title.SetBinding(Label.TextProperty, "Text");

			View = new StackLayout {
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Padding = _padding,
				Children = {
					title
				}
			};
		}
	}
}

