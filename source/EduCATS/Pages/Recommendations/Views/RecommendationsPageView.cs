using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Recommendations.ViewModels;
using EduCATS.Pages.Recommendations.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Recommendations.Views
{
	public class RecommendationsPageView : ContentPage
	{
		const double _spacing = 1;
		static Thickness _listMargin = new Thickness(10, 1, 10, 20);
		static Thickness _subjectsMargin = new Thickness(0, 10);
		static RecommendationsPageViewModel pageVM;

		public RecommendationsPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			pageVM = new RecommendationsPageViewModel(PlatformServices.Current);
			BindingContext = pageVM;
			createViews();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (!pageVM.IsLoading)
			{
				Device.BeginInvokeOnMainThread(async () => await pageVM.Update(true));
			}
		}

		void createViews()
		{
			var headerImage = createHeaderImage();
			var subjectsPickerView = createSubjectsPicker();
			var listView = createList(subjectsPickerView);
			// Grid: the list needs a finite height to scroll.
			var layout = new Grid
			{
				RowSpacing = _spacing,
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			layout.Add(headerImage, 0, 0);
			layout.Add(listView, 0, 1);
			Content = layout;
		}

		Image createHeaderImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				Source = ImageSource.FromFile(Theme.Current.RecommendationsHeaderImage)
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
			var listView = new RoundedListView(typeof(RecommendationsPageViewCell), header: header)
			{
				Margin = _listMargin,
				IsPullToRefreshEnabled = true
			};
			listView.ItemSelected += (sender, e) => { ((RoundedListView)sender).SelectedItem = null; };
			listView.SetBinding(RoundedListView.ItemsSourceProperty, "Recommendations");
			listView.SetBinding(RoundedListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(RoundedListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(RoundedListView.SelectedItemProperty, "SelectedItem", BindingMode.TwoWay);
			return listView;
		}
	}
}