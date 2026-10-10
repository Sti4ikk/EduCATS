using EduCATS.Controls.RoundedListView;
using EduCATS.Fonts;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Today.Base.ViewModels;
using EduCATS.Pages.Today.Base.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;


namespace EduCATS.Pages.Today.Base.Views
{
	public class TodayPageView : ContentPage
	{
		const double _spacing = 0;
		const int _calendarItemsQuantity = 7;
		const double _calendarCarouselHeight = 100;
		const double _calendarCarouselHeightLarge = 120;
		const double _calendarDaysOfWeekCollectionHeight = 50;
		const string _calendarCollectionDataBinding = ".";

		const double _subjectsCardCornerRadius = RoundedListView.HeaderHeight / 2;

		static Thickness _newsLabelMagin = new Thickness(10);
		static Thickness _subjectsMargin = new Thickness(10, 0, 10, 5);
		static Thickness _subjectsCardPadding = new Thickness(0, RoundedListView.HeaderHeight / 2);
		static Thickness _margin = new Thickness(0, 0, 0, 1);
		static Thickness _listMargin = new Thickness(0, 1, 0, 0);
		static Thickness _subjectsLabelMargin = new Thickness(10, 10, 10, 5);

		readonly IPlatformServices _services;
		readonly TodayItemStyles _itemStyles = new TodayItemStyles();

		public TodayPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			_services = PlatformServices.Current;
			BindingContext = new TodayPageViewModel(_services);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			createViews();
		}

		void createViews()
		{

			var calendarView = createCalendar();

			var newsView = createNewsList();

			// Grid (not StackLayout) so the news CollectionView gets a finite height
			// and can virtualize its items.
			var content = new Grid
			{
				RowSpacing = _spacing,
				Margin = _margin,
				RowDefinitions = {
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star)
				}
			};

			content.Add(calendarView, 0, 0);
			content.Add(newsView, 0, 1);

			Content = content;
		}

		StackLayout createCalendar()
		{
			var calendarDaysOfWeekCollectionView = createCalendarDaysOfWeekCollectionView();
			var calendarCarouselView = createCalendarCarousel();
			
			return new StackLayout {
				Spacing = _spacing,
				Children = {
					calendarDaysOfWeekCollectionView,
					calendarCarouselView
				}
			};
		}

		CollectionView createCalendarDaysOfWeekCollectionView()
		{
			var calendarDaysOfWeekCollectionView = new CollectionView {
				BackgroundColor = Color.FromArgb(Theme.Current.TodayCalendarBackgroundColor),
				// Not tappable. Not IsEnabled = false: it turns the labels grey.
				InputTransparent = true,
				HeightRequest = _calendarDaysOfWeekCollectionHeight,
				ItemsLayout = new GridItemsLayout(_calendarItemsQuantity, ItemsLayoutOrientation.Vertical),
				ItemTemplate = new DataTemplate(
					() => new CalendarCollectionViewCell(_calendarCollectionDataBinding))
			};

			calendarDaysOfWeekCollectionView.SetBinding(
				ItemsView.ItemsSourceProperty, "CalendarDaysOfWeekList");

			return calendarDaysOfWeekCollectionView;
		}

		CarouselView createCalendarCarousel()
		{
			var heightRequest = _services.Preferences.IsLargeFont
				? _calendarCarouselHeightLarge
				: _calendarCarouselHeight;

			var calendarCarouselView = new CarouselView
			{
				BackgroundColor = Color.FromArgb(Theme.Current.TodayCalendarBackgroundColor),
				HeightRequest = heightRequest,
				ItemTemplate = new DataTemplate(typeof(CalendarCarouselViewCell)),
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Loop = false,
				IsBounceEnabled = true,
				IsScrollAnimated = true
			};

			// Привязки (обновлённые свойства для MAUI)
			calendarCarouselView.SetBinding(
				CarouselView.ItemsSourceProperty,
				"CalendarList");

			calendarCarouselView.SetBinding(
				CarouselView.PositionProperty,
				"CalendarPosition");

			calendarCarouselView.SetBinding(
				CarouselView.CurrentItemChangedCommandProperty,
				"PositionSelectedCommandProperty");

			return calendarCarouselView;
		}

		RefreshView createNewsList()
		{
			var header = new VerticalStackLayout {
				Spacing = _spacing,
				Children = {
					createSubjectsLabel(),
					createSubjectsList(),
					createNewsLabel()
				}
			};

			// Header is not a templated item, so pass binding context explicitly.
			header.BindingContext = BindingContext;

			var newsCollectionView = new CollectionView {
				Header = header,
				BackgroundColor = Color.FromArgb(Theme.Current.TodayNewsListBackgroundColor),
				SelectionMode = SelectionMode.Single,
				ItemTemplate = new DataTemplate(() => new NewsPageViewCell(_itemStyles))
			};

			newsCollectionView.SetBinding(ItemsView.ItemsSourceProperty, "NewsList");
			newsCollectionView.SetBinding(SelectableItemsView.SelectedItemProperty, "SelectedNewsItem");
			newsCollectionView.SelectionChanged += (sender, e) => {
				if (e.CurrentSelection.Count > 0) {
					((CollectionView)sender).SelectedItem = null;
				}
			};

			var refreshView = new RefreshView {
				Margin = _listMargin,
				RefreshColor = Color.FromArgb(Theme.Current.BaseActivityIndicatorColorIOS),
				Content = newsCollectionView
			};

			refreshView.SetBinding(RefreshView.CommandProperty, "NewsRefreshCommand");
			refreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsNewsRefreshing");

			return refreshView;
		}

		Label createNewsLabel()
		{
			return new Label {
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor),
				Padding = _newsLabelMagin,
				FontAttributes = FontAttributes.Bold,
				Text = CrossLocalization.Translate("today_news"),
				Style = AppStyles.GetLabelStyle(NamedSize.Large, true)
			};
		}

		View createSubjectsList()
		{
			// BindableLayout instead of a nested ListView: a list inside
			// the scrolling list header was re-measured on every scroll frame.
			var subjectsLayout = new VerticalStackLayout {
				Spacing = _spacing
			};

			BindableLayout.SetItemTemplate(
				subjectsLayout, new DataTemplate(() => new SubjectPageViewCell(_itemStyles)));
			BindableLayout.SetEmptyView(subjectsLayout, createSubjectsEmptyView());
			subjectsLayout.SetBinding(BindableLayout.ItemsSourceProperty, "NewsSubjectList");

			return new Border {
				Margin = _subjectsMargin,
				Padding = _subjectsCardPadding,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle {
					CornerRadius = new CornerRadius(_subjectsCardCornerRadius)
				},
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor),
				Content = subjectsLayout
			};
		}

		Label createSubjectsEmptyView()
		{
			return new Label {
				Style = AppStyles.GetLabelStyle(),
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Center,
				Text = CrossLocalization.Translate("base_no_data"),
				TextColor = Color.FromArgb(Theme.Current.BaseNoDataTextColor)
			};
		}

		Label createSubjectsLabel()
		{
			return new Label {
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Padding = _subjectsLabelMargin,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor),
				Text = CrossLocalization.Translate("today_subjects"),
				Style = AppStyles.GetLabelStyle(NamedSize.Large, true)
			};
		}
	}
}

