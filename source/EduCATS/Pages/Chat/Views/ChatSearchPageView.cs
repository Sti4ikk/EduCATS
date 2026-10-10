using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Search through messages of all chats.
	/// </summary>
	public class ChatSearchPageView : ContentPage
	{
		static readonly Color _secondaryTextColor = Color.FromArgb("#8A8A8A");

		public ChatSearchPageView(string title)
		{
			Title = title;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			BindingContext = new ChatSearchPageViewModel(PlatformServices.Current);

			var searchBar = new SearchBar
			{
				Placeholder = CrossLocalization.Translate("chat_search_messages_placeholder")
			};
			searchBar.SetBinding(SearchBar.TextProperty, nameof(ChatSearchPageViewModel.SearchText));

			var busyIndicator = new ActivityIndicator { HeightRequest = 30 };
			busyIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ChatSearchPageViewModel.IsBusy));
			busyIndicator.SetBinding(IsVisibleProperty, nameof(ChatSearchPageViewModel.IsBusy));

			var hintLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(20, 30),
				TextColor = _secondaryTextColor
			};
			hintLabel.SetBinding(Label.TextProperty, nameof(ChatSearchPageViewModel.HintText));

			var results = new CollectionView
			{
				SelectionMode = SelectionMode.Single,
				ItemTemplate = new DataTemplate(createResultCell),
				EmptyView = hintLabel
			};
			results.SetBinding(ItemsView.ItemsSourceProperty, nameof(ChatSearchPageViewModel.Results));
			results.SetBinding(SelectableItemsView.SelectedItemProperty, nameof(ChatSearchPageViewModel.SelectedItem));

			var root = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			root.Add(searchBar, 0, 0);
			root.Add(busyIndicator, 0, 1);
			root.Add(results, 0, 2);
			Content = root;

			Loaded += (sender, e) => searchBar.Focus();
		}

		static View createResultCell()
		{
			var titleLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 15,
				LineBreakMode = LineBreakMode.TailTruncation,
				TextColor = Color.FromArgb(Theme.Current.StatisticsBaseTitleColor)
			};
			titleLabel.SetBinding(Label.TextProperty, nameof(ChatSearchResultModel.ChatTitle));

			var timeLabel = new Label { FontSize = 12, TextColor = _secondaryTextColor, VerticalOptions = LayoutOptions.Center };
			timeLabel.SetBinding(Label.TextProperty, nameof(ChatSearchResultModel.DisplayTime));

			var snippetLabel = new Label
			{
				FontSize = 14,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.TailTruncation,
				TextColor = _secondaryTextColor
			};
			snippetLabel.SetBinding(Label.TextProperty, nameof(ChatSearchResultModel.Snippet));

			var header = new Grid
			{
				ColumnSpacing = 8,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};
			header.Add(titleLabel, 0, 0);
			header.Add(timeLabel, 1, 0);

			return new VerticalStackLayout
			{
				Padding = new Thickness(15, 10),
				Spacing = 2,
				Children = { header, snippetLabel }
			};
		}
	}
}
