using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Settings.Views.Base.ViewCells
{
	/// <summary>
	/// Checkbox view cell.
	/// </summary>
	public class CheckboxViewCell : ViewCell
	{
		/// <summary>
		/// Checkbox height.
		/// </summary>
		const double _checkboxHeight = 25;
		/// <summary>
		/// Checkbox padding.
		/// </summary>
		static Thickness _padding = new Thickness(20);
		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckboxViewCell()
		{
			createViews(false);
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="isCustomFont">Is using custom font.</param>
		public CheckboxViewCell(bool isCustomFont)
		{
			createViews(isCustomFont);
		}
		/// <summary>
		/// Create views.
		/// </summary>
		void createViews(bool isCustomFont)
		{
			var titleLayout = createTitleLayout(isCustomFont);
			var checkbox = createCheckbox();
			View = new StackLayout
			{
				Padding = _padding,
				Orientation = StackOrientation.Horizontal,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Children = {
					titleLayout,
					checkbox
				}
			};
		}
		/// <summary>
		/// Create checkbox.
		/// </summary>
		/// <returns>Checkbox image.</returns>
		Image createCheckbox()
		{
			var checkbox = new Image
			{
				HeightRequest = _checkboxHeight,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Start,
				Source = ImageSource.FromFile(Theme.Current.CheckboxIcon)
			};
			checkbox.SetBinding(VisualElement.IsVisibleProperty, "IsChecked");
			return checkbox;
		}
		/// <summary>
		/// Create title layout.
		/// </summary>
		/// <returns>Title layout.</returns>
		StackLayout createTitleLayout(bool isCustomFont)
		{
			var title = createTitle(isCustomFont);
			var description = createDescription();
			return new StackLayout
			{
				Children = {
					title,
					description
				}
			};
		}
		/// <summary>
		/// Create title label.
		/// </summary>
		/// <returns>Title label.</returns>
		Label createTitle(bool isCustomFont)
		{
			var title = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.SettingsTitleColor),
				Style = AppStyles.GetLabelStyle()
			};

			title.SetBinding(Label.TextProperty, "Title");
			if (isCustomFont)
			{
				title.SetBinding(Label.FontFamilyProperty, "FontFamily");
			}
			return title;
		}
		/// <summary>
		/// Create description label.
		/// </summary>
		/// <returns>Description label.</returns>
		Label createDescription()
		{
			var description = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.CheckboxDescriptionColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Small)
			};
			description.SetBinding(Label.TextProperty, "Description");
			return description;
		}
	}
}