using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Pages.Testing.Base.ViewModels;
using EduCATS.Pages.Testing.Base.Views.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Testing.Base.Views
{
	public class TestingPageView : ContentPage
	{
		const double _spacing = 1;
		static Thickness _headerPadding = new Thickness(10);
		bool _isInitialAppearing = true;

		public TestingPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			BindingContext = new TestingPageViewModel(PlatformServices.Current);
			createViews();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (_isInitialAppearing)
			{
				_isInitialAppearing = false;
				return;
			}
			if (BindingContext is TestingPageViewModel pageViewModel && !pageViewModel.IsRefreshing)
			{
				pageViewModel.RefreshCommand.Execute(null);
			}
		}

		void createViews()
		{
			var headerImage = createHeaderImage();
			var subjectsView = new SubjectsPickerView();
			var testListView = createTestList(subjectsView);
			var layout = new Grid
			{
				RowSpacing = _spacing,
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			// Grid: the list needs a finite height to scroll.
			layout.Add(headerImage, 0, 0);
			layout.Add(testListView, 0, 1);
			Content = layout;
		}

		Image createHeaderImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				Source = ImageSource.FromFile(Theme.Current.TestingHeaderImage)
			};
		}

		View createTestList(View subjectsView)
		{
			var testListView = new CollectionView
			{
				IsGrouped = true,
				SelectionMode = SelectionMode.Single,
				ItemTemplate = CellTemplates.FromCell(typeof(TestingPageViewCell)),
				GroupHeaderTemplate = CellTemplates.FromCell(typeof(TestingHeaderViewCell)),
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Header = new StackLayout
				{
					BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
					Padding = _headerPadding,
					Children = {
						subjectsView
					}
				}
			};

			// The view model opens the test; the selection is reset so that
			// the same test can be opened again.
			testListView.SetBinding(SelectableItemsView.SelectedItemProperty, "SelectedItem", BindingMode.TwoWay);
			testListView.SelectionChanged += (sender, e) =>
			{
				if (testListView.SelectedItem != null)
				{
					testListView.SelectedItem = null;
				}
			};
			testListView.SetBinding(ItemsView.ItemsSourceProperty, "TestList");

			var refreshView = new RefreshView
			{
				RefreshColor = Color.FromArgb(Theme.Current.BaseActivityIndicatorColorIOS),
				Content = testListView
			};

			refreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
			refreshView.SetBinding(RefreshView.CommandProperty, "RefreshCommand");
			return refreshView;
		}
	}
}