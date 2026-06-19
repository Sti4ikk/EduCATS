	using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Networking.Models.SaveMarks;
using EduCATS.Networking.Models.SaveMarks.LabSchedule;
using EduCATS.Pages.SaveLabsAndPracticeMarks.ViewModels;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.SaveLabsAndPracticeMarks.Views
{
	public class SaveSingleStudentMarkPageView : ContentPage
	{
		static Thickness _gridPadding = new Thickness(15);
		const double _controlHeight = 50;
		const double _heightRequest = 40;

		static Thickness _padding = new Thickness(10, 1);
		static Thickness _studentNameMargin = new Thickness(1, 5);
		public List<int> Marks = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		public List<string> NameOfLabOrPractice = new List<string>();
		public string _title { get; set; }

		public SaveSingleStudentMarkPageView(string title, string name, LabsVisitingList marks, TakedLabs prOrLabStat, int subGruop)
		{
			_title = title;

			if (title == CrossLocalization.Translate("practice_mark"))
			{
				foreach (var pract in prOrLabStat.Practicals)
				{
					NameOfLabOrPractice.Add(pract.ShortName);
				}
				BindingContext = new SaveSingleStudentMarkPageViewModel(new PlatformServices(),
					NameOfLabOrPractice, marks, prOrLabStat, title, name, subGruop);
			}
			else if (title == CrossLocalization.Translate("stats_page_labs_rating"))
			{
				foreach (var lab in prOrLabStat.Labs)
				{
					if (lab.SubGroup == subGruop)
					{
						NameOfLabOrPractice.Add(lab.ShortName);
					}
				}
				BindingContext = new SaveSingleStudentMarkPageViewModel(new PlatformServices(),
					NameOfLabOrPractice, marks, prOrLabStat, title, name, subGruop);
			}
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			NavigationPage.SetHasNavigationBar(this, false);
			var entryStyle = getEntryStyle();
			var inicials = new Label
			{
				Margin = _studentNameMargin,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsNameColor),
				HorizontalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				Style = AppStyles.GetLabelStyle(NamedSize.Large),
				Text = name,
			};

			var nameOfPrOrLb = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _controlHeight,
				ItemsSource = NameOfLabOrPractice,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			nameOfPrOrLb.SetBinding(Picker.SelectedItemProperty, "SelectedShortName");

			var markLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				Text = CrossLocalization.Translate("mark"),
			};

			var dateLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				Text = CrossLocalization.Translate("date"),
			};

			var commentLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Fill,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				Text = CrossLocalization.Translate("comment"),
			};

			var showCommentLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				HeightRequest = 50,
				Text = CrossLocalization.Translate("show_for_student"),
			};

			var showComment = new Switch
			{
				IsToggled = false,
			};

			showComment.SetBinding(Switch.IsToggledProperty, "ShowForStud");

			var markPicker = new Picker
			{
				BackgroundColor = Colors.White,
				HeightRequest = _heightRequest,
				ItemsSource = Marks,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			markPicker.SetBinding(Picker.SelectedItemProperty, "MarkStudent");

			nameOfPrOrLb.SelectedIndexChanged += (sender, e) =>
			{
				var selectedLab = nameOfPrOrLb.SelectedItem;
				var vm = BindingContext as SaveSingleStudentMarkPageViewModel;
				vm.setMarks(selectedLab);
				markPicker.SetBinding(Picker.SelectedItemProperty, "MarkStudent");
			};

			var datePicker = new Entry
			{
				Style = entryStyle,
				ReturnType = ReturnType.Done,
				TextColor = Colors.Black,
				IsReadOnly = true,
				HeightRequest = _heightRequest
			};
			datePicker.SetBinding(Entry.TextProperty, "SelectedDate");

			var commentEntry = new Entry
			{
				Style = entryStyle,
				ReturnType = ReturnType.Done,
			};

			commentEntry.SetBinding(Entry.TextProperty, "Comment");

			var gridLayout = new Grid
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Padding = _gridPadding,
				VerticalOptions = LayoutOptions.Start,
			};

			var saveBut = new Button
			{
				FontAttributes = FontAttributes.Bold,
				Text = CrossLocalization.Translate("save_marks"),
				TextColor = Color.FromArgb(Theme.Current.LoginButtonTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.LoginButtonBackgroundColor),
				HeightRequest = _controlHeight,
				Style = AppStyles.GetButtonStyle(bold: true)
			};

			saveBut.SetBinding(Button.CommandProperty, "SaveMarksButton");

			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Auto) });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Auto) });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6, GridUnitType.Auto) });

			Grid.SetRow(inicials, 0); Grid.SetColumn(inicials, 0); Grid.SetColumnSpan(inicials, 3);
			Grid.SetRow(nameOfPrOrLb, 1); Grid.SetColumn(nameOfPrOrLb, 0); Grid.SetColumnSpan(nameOfPrOrLb, 3);
			Grid.SetRow(markLabel, 2); Grid.SetColumn(markLabel, 0); Grid.SetColumnSpan(markLabel, 2);
			Grid.SetRow(markPicker, 2); Grid.SetColumn(markPicker, 2);
			Grid.SetRow(dateLabel, 3); Grid.SetColumn(dateLabel, 0); Grid.SetColumnSpan(dateLabel, 2);
			Grid.SetRow(datePicker, 3); Grid.SetColumn(datePicker, 2);
			Grid.SetRow(commentLabel, 4); Grid.SetColumn(commentLabel, 0); Grid.SetColumnSpan(commentLabel, 2);
			Grid.SetRow(commentEntry, 4); Grid.SetColumn(commentEntry, 2);
			Grid.SetRow(showCommentLabel, 5); Grid.SetColumn(showCommentLabel, 0); Grid.SetColumnSpan(showCommentLabel, 2);
			Grid.SetRow(showComment, 5); Grid.SetColumn(showComment, 2);
			Grid.SetRow(saveBut, 6); Grid.SetColumn(saveBut, 0); Grid.SetColumnSpan(saveBut, 3);

			gridLayout.Children.Add(inicials);
			gridLayout.Children.Add(nameOfPrOrLb);
			gridLayout.Children.Add(markLabel);
			gridLayout.Children.Add(markPicker);
			gridLayout.Children.Add(dateLabel);
			gridLayout.Children.Add(datePicker);
			gridLayout.Children.Add(commentLabel);
			gridLayout.Children.Add(commentEntry);
			gridLayout.Children.Add(showCommentLabel);
			gridLayout.Children.Add(showComment);
			gridLayout.Children.Add(saveBut);

			gridLayout.RowSpacing = 10;

			Content = gridLayout;
		}

		Style getEntryStyle()
		{
			var style = AppStyles.GetEntryStyle();

			style.Setters.Add(new Setter
			{
				Property = HeightRequestProperty,
				Value = _controlHeight
			});

			style.Setters.Add(new Setter
			{
				Property = BackgroundColorProperty,
				Value = Theme.Current.LoginEntryBackgroundColor
			});

			return style;
		}
	}
}

