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
	class ParentalStatsPageView : StatsPageView  // ← наследуем StatsPageView
	{
		static Thickness _padding = new Thickness(10, 1, 10, 1);
		static Thickness _headerPadding = new Thickness(0, 10, 0, 10);

		public ParentalStatsPageView(GroupInfo group)
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			var vm = new ParentalsStatsPageViewModel(new PlatformServices(), group);
			vm.Init();
			BindingContext = vm;
			createViews();
		}

		void createViews()
		{
			var headerView = createHeaderView();
			var roundedListView = createRoundedList();

			// Оборачиваем всё в ScrollView вместо вложения в ListView.Header
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children = {
				headerView,
				roundedListView
			}
				}
			};
		}

		RoundedListView createRoundedList()  // без header параметра
		{
			var roundedListView = new RoundedListView(typeof(StatsPageViewCell))
			{
				IsPullToRefreshEnabled = true
			};

			roundedListView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			roundedListView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			roundedListView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			roundedListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			roundedListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "PagesList");
			return roundedListView;
		}

		RoundedListView createRoundedList(View header)
		{
			var roundedListView = new RoundedListView(typeof(StatsPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = true
			};

			roundedListView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			roundedListView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			roundedListView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			roundedListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			roundedListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "PagesList");
			return roundedListView;
		}

		StackLayout createHeaderView()
		{
			var subjectsView = new SubjectsPickerView();
			var radarChartView = createFrameWithChartView();

			return new StackLayout
			{
				Padding = _headerPadding,
				Children = {
					subjectsView,
					radarChartView
				}
			};
		}
	}
}