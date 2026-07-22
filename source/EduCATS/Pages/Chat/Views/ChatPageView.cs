using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Pages.Chat.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Chat.Views
{
	public class ChatPageView : ContentPage
	{
		public ChatPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			BindingContext = new ChatPageViewModel(new PlatformServices());
			createViews();
		}

		void createViews()
		{
			var switcher = createSwitcher();
			var searchEntry = createSearchEntry();
			var personalList = createPersonalList();
			var groupList = createGroupList();

			Content = new StackLayout
			{
				Spacing = 0,
				Children = { switcher, searchEntry, personalList, groupList }
			};
		}

		Grid createSwitcher()
		{
			var personalButton = new Button
			{
				Text = "Личные",
				CornerRadius = 0
			};
			personalButton.SetBinding(Button.CommandProperty, "ShowPersonalCommand");

			var groupButton = new Button
			{
				Text = "Группы",
				CornerRadius = 0
			};
			groupButton.SetBinding(Button.CommandProperty, "ShowGroupCommand");

			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			grid.Add(personalButton, 0, 0);
			grid.Add(groupButton, 1, 0);

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

			listView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			listView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "Chats");
			listView.SetBinding(IsVisibleProperty, "IsPersonalMode");

			return listView;
		}

		RoundedListView createGroupList()
		{
			var listView = new RoundedListView(typeof(GroupChatListViewCell))
			{
				IsPullToRefreshEnabled = true
			};

			listView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			listView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(ListView.SelectedItemProperty, "SelectedGroupItem");
			listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "GroupChats");
			listView.SetBinding(IsVisibleProperty, "IsGroupMode");

			return listView;
		}
	}
}