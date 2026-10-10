using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Parental.FindGroup.Models;
using EduCATS.Pages.Statistics.Base.Views;
using EduCATS.Pages.Statistics.Base.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Parental.Statistics
{
	class ParentalStatsPageView : StatsPageView
	{
		static Thickness _padding = new Thickness(10, 1, 10, 1);
		static Thickness _headerPadding = new Thickness(0, 10, 0, 10);

		public ParentalStatsPageView(GroupInfo group)
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			var vm = new ParentalsStatsPageViewModel(PlatformServices.Current, group);
			vm.Init();
			BindingContext = vm;
			createViews();
		}

		void createViews()
		{
			var headerView = createHeaderView();

			// The header (subject picker and chart) scrolls with the list:
			// a list inside a ScrollView can't scroll or virtualize.
			Content = createRoundedList(headerView);
		}

		RoundedListView createRoundedList(View header)
		{
			var roundedListView = new RoundedListView(typeof(StatsPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = true
			};

			roundedListView.ItemTapped += (sender, e) => ((RoundedListView)sender).SelectedItem = null;
			roundedListView.SetBinding(RoundedListView.IsRefreshingProperty, "IsLoading");
			roundedListView.SetBinding(RoundedListView.RefreshCommandProperty, "RefreshCommand");
			roundedListView.SetBinding(RoundedListView.SelectedItemProperty, "SelectedItem");
			roundedListView.SetBinding(RoundedListView.ItemsSourceProperty, "PagesList");
			return roundedListView;
		}

		StackLayout createHeaderView()
		{
			var subjectsView = new SubjectsPickerView();
			var radarChartView = createFrameWithChartView();

			return new StackLayout
			{
				Padding = _headerPadding,
				Spacing = 10,
				Children = {
					subjectsView,
					radarChartView
				}
			};
		}
	}
}