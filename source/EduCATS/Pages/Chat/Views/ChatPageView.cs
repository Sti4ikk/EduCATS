using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Pages.Chat.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
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
			var listView = new RoundedListView(typeof(ChatListViewCell))
			{
				IsPullToRefreshEnabled = true
			};

			listView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			listView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			listView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			listView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "Chats");

			Content = listView;
		}
	}
}