using System.Collections.Generic;
using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Data.Models;
using EduCATS.Fonts;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Networking;
using EduCATS.Pages.Statistics.Students.ViewModels;
using EduCATS.Pages.Statistics.Students.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Statistics.Students.Views
{
	public class StudentsPageView : ContentPage
	{
		static Thickness _padding = new Thickness(20, 10);
		static Thickness _headerPadding = new Thickness(0, 10, 0, 10);
		static Thickness _searchBarMargin = new Thickness(0, 10, 0, 0);

		public string TitleOfButton { get; set; }
		public int pageInd { get; set; }

		public StudentsPageView(int pageIndex, int subjectId, List<StatsStudentModel> students)
		{
			pageInd = pageIndex;
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			var studentsPageViewModel = new StudentsPageViewModel(new PlatformServices(), subjectId, students, pageIndex);
			studentsPageViewModel.Init();
			BindingContext = studentsPageViewModel;
			createViews();
		}

		void createViews()
		{
			var headerView = createHeaderView();
			var roundedListView = createRoundedListView(headerView);
			Content = roundedListView;
		}

		StackLayout createHeaderView()
		{
			var groupsPicker = new GroupsPickerView
			{
				Margin = new Thickness(0, 10, 0, 10),
				HeightRequest = 45,
			};
			var searchBar = createSearchBar();
			var addMarks = createAddMarksButton();
			return new StackLayout
			{
				Padding = _headerPadding,
				Children = {
					groupsPicker,
					addMarks,
					searchBar
				}
			};
		}

		Button createAddMarksButton()
		{
			if (pageInd == 1 || pageInd == 2 || pageInd == 3)
			{
				TitleOfButton = CrossLocalization.Translate("set_visiting");
			}
			else
			{
				TitleOfButton = CrossLocalization.Translate("set_marks");
			}
			var addMarks = new Button
			{
				Text = TitleOfButton,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.FromArgb(Theme.Current.LoginButtonTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.LoginButtonBackgroundColor),
				Style = AppStyles.GetButtonStyle(bold: true)
			};
			addMarks.SetBinding(Button.CommandProperty, "AddMarksCommand");
			return addMarks;
		}

		SearchBar createSearchBar()
		{
			var searchBar = new SearchBar
			{
				Margin = _searchBarMargin,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				CancelButtonColor = Color.FromArgb(Theme.Current.BaseAppColor),
				TextColor = Color.FromArgb(Theme.Current.StatisticsBaseTitleColor),
				Placeholder = CrossLocalization.Translate("stats_students_search_text"),
				FontFamily = FontsController.GetCurrentFont(),
				FontSize = FontSizeController.GetSize(NamedSize.Medium, typeof(SearchBar))
			};

			searchBar.SetBinding(SearchBar.TextProperty, "SearchText");
			return searchBar;
		}

		RoundedListView createRoundedListView(View header)
		{
			var roundedListView = new RoundedListView(typeof(StudentsPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = true
			};

			roundedListView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			roundedListView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			roundedListView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			roundedListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			roundedListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "Students");
			return roundedListView;
		}
	}
}

