using EduCATS.Controls.RoundedListView.Selectors;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Input;

namespace EduCATS.Controls.RoundedListView
{
	/// <summary>
	/// Rounded list view.
	/// </summary>
	/// <remarks>
	/// Built on <see cref="CollectionView"/> (the old <see cref="ListView"/> is
	/// deprecated and slower) with pull-to-refresh by <see cref="RefreshView"/>.
	/// Keeps the ListView-like API used by the pages: <see cref="ItemsSource"/>,
	/// <see cref="SelectedItem"/>, <see cref="IsRefreshing"/>,
	/// <see cref="RefreshCommand"/> and <see cref="ItemTapped"/>.
	/// Cells are still <see cref="ViewCell"/>s (see <see cref="CellTemplates"/>).
	/// </remarks>
	public class RoundedListView : ContentView
	{
		/// <summary>
		/// Base spacing.
		/// </summary>
		const double _spacing = 0;

		/// <summary>
		/// Base padding.
		/// </summary>
		const double _padding = 0;

		/// <summary>
		/// Corner radius & sharp layout height.
		/// </summary>
		readonly double _capHeight;

		/// <summary>
		/// Empty view.
		/// </summary>
		readonly StackLayout _emptyView;

		readonly CollectionView _collectionView;
		readonly RefreshView _refreshView;

		public const double HeaderHeight = 14;

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
			nameof(ItemsSource), typeof(IEnumerable), typeof(RoundedListView),
			propertyChanged: (bindable, oldValue, newValue) => ((RoundedListView)bindable).onItemsSourceChanged());

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
			nameof(SelectedItem), typeof(object), typeof(RoundedListView), defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (bindable, oldValue, newValue) => ((RoundedListView)bindable)._collectionView.SelectedItem = newValue);

		public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create(
			nameof(IsRefreshing), typeof(bool), typeof(RoundedListView), defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (bindable, oldValue, newValue) => ((RoundedListView)bindable)._refreshView.IsRefreshing = (bool)newValue);

		public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create(
			nameof(RefreshCommand), typeof(ICommand), typeof(RoundedListView),
			propertyChanged: (bindable, oldValue, newValue) => ((RoundedListView)bindable)._refreshView.Command = (ICommand)newValue);

		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public bool IsRefreshing
		{
			get => (bool)GetValue(IsRefreshingProperty);
			set => SetValue(IsRefreshingProperty, value);
		}

		public ICommand RefreshCommand
		{
			get => (ICommand)GetValue(RefreshCommandProperty);
			set => SetValue(RefreshCommandProperty, value);
		}

		/// <summary>
		/// Is pull-to-refresh enabled.
		/// </summary>
		/// <remarks>
		/// Not <c>IsEnabled</c>: it disables the whole list content (labels turn
		/// grey, entries and switches stop working), only the gesture is turned off.
		/// </remarks>
		public bool IsPullToRefreshEnabled
		{
			get => _refreshView.IsRefreshEnabled;
			set => _refreshView.IsRefreshEnabled = value;
		}

		/// <summary>
		/// An item is tapped (raised after <see cref="SelectedItem"/> is set).
		/// </summary>
		public event EventHandler<ItemTappedEventArgs> ItemTapped;

		/// <summary>
		/// Selected item changed.
		/// </summary>
		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public RoundedListView(
			Type type,
			bool checkbox = false,
			View header = null,
			double headerTopPadding = 0,
			double footerBottomPadding = 0,
			Func<object> func = null,
			IPlatformServices services = null)
		{
			_capHeight = HeaderHeight / 2;
			_emptyView = createEmptyView();

			_collectionView = new CollectionView
			{
				ItemTemplate = CellTemplates.Adapt(func == null ?
					new RoundedListTemplateSelector(type, checkbox) :
					new RoundedListTemplateSelector(func, checkbox)),
				SelectionMode = SelectionMode.Single,
				VerticalScrollBarVisibility = ScrollBarVisibility.Never,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Header = createHeader(header, headerTopPadding),
				Footer = createFooterCap(footerBottomPadding)
			};

			_collectionView.SelectionChanged += onSelectionChanged;

			_refreshView = new RefreshView
			{
				IsRefreshEnabled = false,
				RefreshColor = Color.FromArgb(
					DeviceInfo.Platform == DevicePlatform.Android ?
						Theme.Current.BaseActivityIndicatorColorAndroid :
						Theme.Current.BaseActivityIndicatorColorIOS),
				Content = _collectionView
			};

			// Pulling sets IsRefreshing on the RefreshView: pass it to the binding.
			_refreshView.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == RefreshView.IsRefreshingProperty.PropertyName &&
					IsRefreshing != _refreshView.IsRefreshing)
				{
					IsRefreshing = _refreshView.IsRefreshing;
				}
			};

			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Content = _refreshView;
		}

		/// <summary>
		/// Scroll to an item.
		/// </summary>
		public void ScrollTo(object item, ScrollToPosition position, bool animated) =>
			_collectionView.ScrollTo(item, position: position, animate: animated);

		void onSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = e.CurrentSelection.FirstOrDefault();
			var previous = SelectedItem;

			SelectedItem = item;
			ItemSelected?.Invoke(this, new SelectedItemChangedEventArgs(item, -1));

			if (item != null && !Equals(item, previous))
			{
				ItemTapped?.Invoke(this, new ItemTappedEventArgs(null, item, -1));
			}
		}

		StackLayout createHeader(View view, double topPadding = 0)
		{
			var cap = createHeaderCap();

			var header = new StackLayout {
				Padding = _padding,
				Spacing = _spacing
			};

			if (view != null) {
				header.Children.Add(view);
			}

			header.Children.Add(cap);
			header.Children.Add(_emptyView);
			header.Padding = new Thickness(0, topPadding, 0, 0);
			return header;
		}

		Grid createHeaderCap()
		{
			var stackLayout = new StackLayout {
				HeightRequest = _capHeight,
				VerticalOptions = LayoutOptions.End,
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor)
			};

			var frame = new Border
			{
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius((float)_capHeight)
				}
			};

			return new Grid {
				HeightRequest = HeaderHeight,
				Children = {
					stackLayout,
					frame
				}
			};
		}

		Grid createFooterCap(double bottomPadding)
		{
			var stackLayout = new StackLayout {
				HeightRequest = _capHeight,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor)
			};

			var frame = new Border
			{
				Padding = new Thickness(0, 0, 0, bottomPadding),
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius((float)_capHeight)
				}
			};

			return new Grid {
				Padding = new Thickness(0, 0, 0, bottomPadding),
				HeightRequest = HeaderHeight,
				Children = {
					stackLayout,
					frame
				}
			};
		}

		StackLayout createEmptyView()
		{
			return new StackLayout {
				Spacing = _spacing,
				BackgroundColor = Color.FromArgb(Theme.Current.RoundedListViewBackgroundColor),
				Children = {
					new Label {
						Style = AppStyles.GetLabelStyle(),
						HorizontalTextAlignment = TextAlignment.Center,
						HorizontalOptions = LayoutOptions.Center,
						Text = CrossLocalization.Translate("base_no_data"),
						TextColor = Color.FromArgb(Theme.Current.BaseNoDataTextColor)
					}
				}
			};
		}

		void onItemsSourceChanged()
		{
			_collectionView.ItemsSource = ItemsSource;

			try {
				_emptyView.IsVisible = ItemsSource == null || !ItemsSource.Cast<object>().Any();
			} catch (Exception) {
				_emptyView.IsVisible = false;
			}
		}
	}
}
