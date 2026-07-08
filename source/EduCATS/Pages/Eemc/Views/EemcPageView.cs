using EduCATS.Controls.Pickers;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Eemc.ViewModels;
using EduCATS.Pages.Eemc.Views.ViewCell;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Eemc.Views
{
	public class EemcPageView : ContentPage
	{
		const double _spacing = 1;
		const int _rowsCount = 2;
		const double _buttonHeight = 50;
		static Thickness _headerPadding = new Thickness(10);
		static Thickness _subjectsMargin = new Thickness(10, 0, 10, 10);
		static Thickness _emptyViewMargin = new Thickness(10, 0);
		static Thickness _backButtonMargin = new Thickness(30, 0, 30, 15);

		public EemcPageView(int searchId)
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new EemcPageViewModel(new PlatformServices(), searchId);
			createViews();
		}

		/// <summary>
		/// Build page layout.
		/// </summary>
		/// <remarks>
		/// Uses a <see cref="Grid"/> with an explicit <c>Star</c> row for
		/// the collection, instead of a <see cref="StackLayout"/>. A
		/// StackLayout only gives each child its natural size, so when
		/// <c>Concepts</c> is empty the CollectionView (with a
		/// multi-column GridItemsLayout) can collapse to near-zero
		/// height on Android, hiding its EmptyView even though it's
		/// technically present. The Star row guarantees the collection
		/// always gets the remaining space, regardless of item count.
		/// </remarks>
		void createViews()
		{
			var headerImage = createHeaderImage();
			var subjectsView = createSubjectsPicker();
			var documentCollectionView = createCollection();
			var backButton = createBackButton();

			var grid = new Grid
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				RowSpacing = _spacing,
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			grid.Add(headerImage, 0, 0);
			grid.Add(subjectsView, 0, 1);
			grid.Add(documentCollectionView, 0, 2);
			grid.Add(backButton, 0, 3);

			Content = grid;
		}

		Image createHeaderImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				Source = ImageSource.FromFile(Theme.Current.EemcHeaderImage)
			};
		}

		CollectionView createCollection()
		{
			var documentsCollectionView = new CollectionView
			{
				SelectionMode = SelectionMode.Single,
				ItemTemplate = new DataTemplate(typeof(EemcPageViewCell)),
				ItemsLayout = new GridItemsLayout(_rowsCount, ItemsLayoutOrientation.Vertical),
				EmptyView = new StackLayout
				{
					BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
					Children = { createEmptyView() }
				}
			};

			documentsCollectionView.SetBinding(SelectableItemsView.SelectedItemProperty, "SelectedItem");
			documentsCollectionView.SetBinding(ItemsView.ItemsSourceProperty, "Concepts");
			return documentsCollectionView;
		}

		Frame createEmptyView()
		{
			return new Frame
			{
				HasShadow = false,
				Margin = _emptyViewMargin,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = new Label
				{
					Style = AppStyles.GetLabelStyle(),
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = CrossLocalization.Translate("base_no_data"),
					TextColor = Color.FromArgb(Theme.Current.BaseNoDataTextColor)
				}
			};
		}

		SubjectsPickerView createSubjectsPicker()
		{
			return new SubjectsPickerView
			{
				Margin = _subjectsMargin,
			};
		}

		Button createBackButton()
		{
			var backButton = new Button
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.End,
				CornerRadius = (int)_buttonHeight / 2,
				HeightRequest = _buttonHeight,
				Margin = _backButtonMargin,
				TextColor = Color.FromArgb(Theme.Current.EemcBackButtonTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.EemcBackButtonColor),
				Text = CrossLocalization.Translate("eemc_back_text"),
				Style = AppStyles.GetButtonStyle()
			};

			backButton.SetBinding(Button.CommandProperty, "BackCommand");
			backButton.SetBinding(IsVisibleProperty, "IsBackActionPossible");
			return backButton;
		}
	}
}