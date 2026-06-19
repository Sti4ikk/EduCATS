using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Statistics.Students.Views.ViewCells
{
	public class StudentsPageViewCell : ViewCell
	{
		static Thickness _padding = new Thickness(20);

		public StudentsPageViewCell()
		{
			var nameLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.StatisticsBaseTitleColor),
				Style = AppStyles.GetLabelStyle()
			};

			nameLabel.SetBinding(Label.TextProperty, "Name");

			View = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Padding = _padding,
				Children = {
					nameLabel
				}
			};
		}
	}
}

