using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Statistics.Marks.Views.ViewCells
{
	public class VisitingPageViewCell : ViewCell
	{
		static Thickness _gridPadding = new Thickness(15);

		const double _controlHeight = 50;

		public List<string> listOfMarks = new List<string> { "", "1", "2", "3", "4" };

		public BindableProperty HeightRequestProperty { get; private set; }
		public BindableProperty BackgroundColorProperty { get; private set; }

		public VisitingPageViewCell()
		{
			var entryStyle = getEntryStyle();

			var inicials = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				FontSize = 20,
				Style = AppStyles.GetLabelStyle()
			};
			inicials.SetBinding(Label.TextProperty, "Title");
			inicials.SetBinding(VisualElement.IsVisibleProperty, "IsTitle");

			var markLabel = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb(Theme.Current.StatisticsDetailsTitleColor),
				Style = AppStyles.GetLabelStyle(),
				Text = CrossLocalization.Translate("skipped_hours"),
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
				HeightRequest = 60,
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
				HeightRequest = 40,
				ItemsSource = listOfMarks,
				HorizontalTextAlignment = TextAlignment.Center
			};
			markPicker.SetBinding(Picker.SelectedIndexProperty, "Mark");
			markPicker.SetBinding(Picker.SelectedItemProperty, "Mark");

			var commentEntry = new Entry
			{
				Style = entryStyle,
				ReturnType = ReturnType.Done,
				HeightRequest = 40
			};
			commentEntry.SetBinding(Entry.TextProperty, "Comment");

			// ==================== Grid ====================
			var gridLayout = new Grid
			{
				Padding = _gridPadding,
				VerticalOptions = LayoutOptions.Center,
				ColumnDefinitions =
		{
			new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },   // колонка 0
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },   // колонка 1 (не используется)
            new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }    // колонка 2 — для picker и entry
        }
			};

			// RowDefinitions
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			// Добавление элементов (MAUI-способ)
			gridLayout.Add(inicials, 0, 0);
			gridLayout.Add(markLabel, 0, 1);
			gridLayout.Add(markPicker, 2, 1);
			gridLayout.Add(commentLabel, 0, 2);
			gridLayout.Add(commentEntry, 2, 2);
			gridLayout.Add(showCommentLabel, 0, 3);
			gridLayout.Add(showComment, 2, 3);

			// ColumnSpan
			Grid.SetColumnSpan(inicials, 3);
			Grid.SetColumnSpan(showCommentLabel, 2);
			Grid.SetColumnSpan(commentLabel, 2);
			Grid.SetColumnSpan(markLabel, 2);

			// ==================== Root View ====================
			View = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Children = { gridLayout }
			};
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

