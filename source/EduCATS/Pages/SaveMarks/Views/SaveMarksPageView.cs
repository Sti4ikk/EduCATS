using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.SaveMarks.ViewModels;
using EduCATS.Pages.Statistics.Marks.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.SaveMarks.Views
{
	public class SaveMarksPageView : ContentPage
	{

		public string _title { get; set; }

		static Thickness _padding = new Thickness(10, 1);
		static Thickness _headerPadding = new Thickness(0, 10, 0, 10);
		static int _heightRequest = 40;
		private string _groupName;
		const double _controlHeight = 50;

		public SaveMarksPageView(int subjectId, int groupId, string title, string groupName)
		{
			_title = title;
			_groupName = groupName;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new SaveMarksPageViewModel(
				new PlatformServices(), subjectId, groupId, title);
			createView();
		}

		void createView()
		{
			var group = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				Text = CrossLocalization.Translate("choose_group") + " " + _groupName,
				HorizontalOptions = LayoutOptions.Center,
			};
			var stackLayout = new StackLayout();
			var resultsListView = new RoundedListView(typeof(VisitingPageViewCell));
			var resultsListViewSubGroup = new RoundedListView(typeof(VisitingPageViewCell));
			var saveDate = stackView();
			if (_title == CrossLocalization.Translate("stats_page_lectures_visiting"))
			{
				var dateforLectures = dateLecturesPicker();
				resultsListView = new RoundedListView(typeof(VisitingPageViewCell))
				{
					IsPullToRefreshEnabled = false,
				};
				resultsListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "LecturesMarks");
				stackLayout = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Padding = _headerPadding,
					Children =
					{
						saveDate,
						group,
						dateforLectures,
						resultsListView,
					}
				};
			}
			else if (_title == CrossLocalization.Translate("stats_page_labs_visiting"))
			{
				var dateforLabs = dateLabsPicker();
				var subGroupLabsVisiting = subGroupPicker();
				resultsListViewSubGroup = new RoundedListView(typeof(VisitingPageViewCell))
				{
					IsPullToRefreshEnabled = false,
				};
				resultsListViewSubGroup.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "LabsVisitingMarksSubGroup");
				stackLayout = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Padding = _headerPadding,
					Children =
					{
						saveDate,
						group,
						subGroupLabsVisiting,
						dateforLabs,
						resultsListViewSubGroup,
					}
				};
			}
			else if (_title == CrossLocalization.Translate("practiñe_visiting"))
			{
				var dateforPractice = datePractPicker();
				resultsListView = new RoundedListView(typeof(VisitingPageViewCell))
				{
					IsPullToRefreshEnabled = false,
				};
				resultsListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "LecturesMarks");
				stackLayout = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Padding = _headerPadding,
					Children =
					{
						saveDate,
						group,
						dateforPractice,
						resultsListView,
					}
				};
			}
			Content = stackLayout;
		}

		StackLayout stackView()
		{

			var save = saveMarksButton();
			var form = new StackLayout
			{
				Padding = _headerPadding,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					save,
				}
			};
			return form;
		}

		Picker subGroupPicker()
		{
			var subGroupPicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _controlHeight
			};
			subGroupPicker.SetBinding(Picker.ItemsSourceProperty, "SubGroup");
			subGroupPicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedSubGroup"));
			return subGroupPicker;
		}

		Picker dateLecturesPicker()
		{
			var datePicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _controlHeight
			};
			datePicker.SetBinding(Picker.ItemsSourceProperty, "Date");
			datePicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedDate"));
			return datePicker;
		}

		Picker dateLabsPicker()
		{
			var datePicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _controlHeight
			};
			datePicker.SetBinding(Picker.ItemsSourceProperty, "DateLabs");
			datePicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedLabDate"));
			return datePicker;
		}

		Picker datePractPicker()
		{
			var datePicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _heightRequest
			};
			datePicker.SetBinding(Picker.ItemsSourceProperty, "Date");
			datePicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedPracDate"));
			return datePicker;
		}

		Button saveMarksButton()
		{
			var saveButton = new Button()
			{
				FontAttributes = FontAttributes.Bold,
				Text = CrossLocalization.Translate("save_marks"),
				TextColor = Color.FromArgb(Theme.Current.LoginButtonTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.LoginButtonBackgroundColor),
				HeightRequest = _controlHeight,
				Style = AppStyles.GetButtonStyle(bold: true)
			};
			saveButton.SetBinding(Button.CommandProperty, "SaveMarksCommand");
			return saveButton;
		}

	}
}

