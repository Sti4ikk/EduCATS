using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Files.ViewModels;
using EduCATS.Pages.Files.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Files.Views
{
	public class FilesPageView : ContentPage
	{
		const double _spacing = 1;
		static Thickness _headerPadding = new Thickness(10);
		static Thickness _subjectsMargin = new Thickness(0, 0, 0, 10);
		static Thickness _filesListMargin = new Thickness(10, 0, 10, 20);
		public FilesPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new FilesPageViewModel(new PlatformServices());
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			createViews();
		}
		void createViews()
		{
			var headerImage = createHeaderImage();
			var headerLabel = createHeaderLabel();
			var subjectPickerView = createSubjectsPicker();
			var filesListView = createList(new StackLayout
			{
				Spacing = _spacing,
				Children = {
					headerLabel,
					subjectPickerView
				}
			});
			Content = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Children = {
					headerImage,
					filesListView
				}
			};
		}
		Image createHeaderImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				Source = ImageSource.FromFile(Theme.Current.FilesHeaderImage)
			};
		}
		Label createHeaderLabel()
		{
			return new Label
			{
				Padding = _headerPadding,
				FontAttributes = FontAttributes.Bold,
				Text = CrossLocalization.Translate("files_header"),
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Large, true)
			};
		}
		SubjectsPickerView createSubjectsPicker()
		{
			return new SubjectsPickerView
			{
				Margin = _subjectsMargin
			};
		}
		RoundedListView createList(View header)
		{
			var filesListView = new RoundedListView(typeof(FilesPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = true,
				Margin = _filesListMargin
			};
			filesListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "FileList");
			filesListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem", BindingMode.TwoWay);
			filesListView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			filesListView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			return filesListView;
		}
	}
}