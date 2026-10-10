using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Pages.Chat.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	public class ChatPageView : ContentPage
	{
		readonly ChatPageViewModel _viewModel;

		public ChatPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			_viewModel = new ChatPageViewModel(PlatformServices.Current);
			BindingContext = _viewModel;
			createViews();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.Activate();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_viewModel.Deactivate();
		}

		void createViews()
		{
			var switcher = createSwitcher();
			var searchEntry = createSearchEntry();
			var personalList = createPersonalList();
			var groupList = createGroupList();

			// Grid: the lists need a finite height to scroll.
			// Both lists share the last row, only one of them is visible.
			var layout = new Grid
			{
				RowSpacing = 0,
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			layout.Add(switcher, 0, 0);
			layout.Add(searchEntry, 0, 1);
			layout.Add(personalList, 0, 2);
			layout.Add(groupList, 0, 2);
			Content = layout;
		}

		Grid createSwitcher()
		{
			var personalButton = new Button
			{
				Text = CrossLocalization.Translate("chat_personal"),
				CornerRadius = 0
			};
			personalButton.SetBinding(Button.CommandProperty, "ShowPersonalCommand");

			var groupButton = new Button
			{
				Text = CrossLocalization.Translate("chat_groups"),
				CornerRadius = 0
			};
			groupButton.SetBinding(Button.CommandProperty, "ShowGroupCommand");

			// Search through messages of all chats.
			var searchButton = new ImageButton
			{
				Source = "icon_search.png",
				WidthRequest = 44,
				HeightRequest = 44,
				Padding = new Thickness(11),
				BackgroundColor = Colors.Transparent
			};
			searchButton.SetBinding(ImageButton.CommandProperty, nameof(ChatPageViewModel.SearchMessagesCommand));
			SemanticProperties.SetDescription(searchButton, CrossLocalization.Translate("chat_search_all_title"));

			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			grid.Add(personalButton, 0, 0);
			grid.Add(groupButton, 1, 0);
			grid.Add(searchButton, 2, 0);

			return grid;
		}

		Border createSearchEntry()
		{
			var entry = new Entry
			{
				BackgroundColor = Colors.Transparent
			};

			// Привязываем текст плейсхолдера к нашему новому свойству
			entry.SetBinding(Entry.PlaceholderProperty, "SearchPlaceholder");
			entry.SetBinding(Entry.TextProperty, "SearchText");

			return new Border
			{
				Margin = new Thickness(15, 10),
				Padding = new Thickness(12, 4),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(10)
				},
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = entry
			};
		}

		RoundedListView createPersonalList()
		{
			var listView = new RoundedListView(typeof(ChatListViewCell))
			{
				IsPullToRefreshEnabled = true
			};

			listView.ItemTapped += (sender, e) => ((RoundedListView)sender).SelectedItem = null;
			listView.SetBinding(RoundedListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(RoundedListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(RoundedListView.SelectedItemProperty, "SelectedItem");
			listView.SetBinding(RoundedListView.ItemsSourceProperty, "Chats");
			listView.SetBinding(IsVisibleProperty, "IsPersonalMode");

			return listView;
		}

		RoundedListView createGroupList()
		{
			var listView = new RoundedListView(typeof(GroupChatListViewCell))
			{
				IsPullToRefreshEnabled = true
			};

			listView.ItemTapped += (sender, e) => ((RoundedListView)sender).SelectedItem = null;
			listView.SetBinding(RoundedListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(RoundedListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(RoundedListView.SelectedItemProperty, "SelectedGroupItem");
			listView.SetBinding(RoundedListView.ItemsSourceProperty, "GroupChats");
			listView.SetBinding(IsVisibleProperty, "IsGroupMode");

			return listView;
		}
	}
}