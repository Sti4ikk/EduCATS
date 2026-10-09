using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Fonts;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.SaveLabsAndPracticeMarks.Views;
using EduCATS.Pages.Statistics.Marks.Views.ViewCells;
using EduCATS.Pages.Statistics.Students.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.SaveLabsAndPracticeMarks.ViewModels
{
	public class SavePracticeAndLabsPageView : ContentPage
	{
		static Thickness _padding = new Thickness(10, 1);
		static Thickness _headerPadding = new Thickness(0, 10, 0, 10);

		private string _groupName;

		public string _title { get; set; }

		public SavePracticeAndLabsPageView(string title, int subjectId, int groupId, string groupName)
		{
			_title = title;
			_groupName = groupName;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new SavePracticeAndLabsPageViewModel(
				PlatformServices.Current, subjectId, groupId, title);

			if (_title == CrossLocalization.Translate("practice_mark"))
			{
				createViews();
			}
			else if (_title == CrossLocalization.Translate("stats_page_labs_rating"))
			{
				createLabsMarks();
			}

		}

		void createLabsMarks()
		{
			var group = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				FontSize = 18,
				Text = CrossLocalization.Translate("choose_group") + " " + _groupName,
				HorizontalOptions = LayoutOptions.Center,
			};

			// The group label and the sub-group picker are the list header:
			// the list needs a finite height to scroll (it couldn't inside a StackLayout).
			var header = new StackLayout
			{
				Padding = _headerPadding,
				Children = { group, subGroupPicker() }
			};

			var resultsListViewSubGroup = new RoundedListView(typeof(StudentsPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = false,
			};
			resultsListViewSubGroup.ItemTapped += (sender, e) => ((RoundedListView)sender).SelectedItem = null;
			resultsListViewSubGroup.SetBinding(RoundedListView.SelectedItemProperty, "SelectedItem");
			resultsListViewSubGroup.SetBinding(RoundedListView.ItemsSourceProperty, "LabsVisitingMarksSubGroup");

			Content = resultsListViewSubGroup;
		}

		void createViews()
		{
			var roundedListView = createRoundedListView();
			Content = roundedListView;
		}

		Picker subGroupPicker()
		{
			var subGroupPicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = 70,
			};
			subGroupPicker.SetBinding(Picker.ItemsSourceProperty, "SubGroup");
			subGroupPicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedSubGroup"));
			return subGroupPicker;
		}

		RoundedListView createRoundedListView()
		{
			var roundedListView = new RoundedListView(typeof(StudentsPageViewCell))
			{
				IsPullToRefreshEnabled = false
			};
			roundedListView.ItemTapped += (sender, e) => ((RoundedListView)sender).SelectedItem = null;
			roundedListView.SetBinding(RoundedListView.SelectedItemProperty, "SelectedItem");
			roundedListView.SetBinding(RoundedListView.ItemsSourceProperty, "Students");
			return roundedListView;
		}
	}
}

