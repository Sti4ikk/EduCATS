using EduCATS.Controls.Pickers;
using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Converters;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Files.ViewModels;
using EduCATS.Pages.Files.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Files.Views
{
	public class FilesPageView : ContentPage
	{
		const double _spacing = 1;
		static Thickness _headerPadding = new Thickness(10);
		static Thickness _subjectsMargin = new Thickness(0, 0, 0, 10);
		static Thickness _filesListMargin = new Thickness(10, 0, 10, 20);
		static Thickness _downloadCardPadding = new Thickness(20);
		static Thickness _downloadCardMargin = new Thickness(40, 0);

		static readonly PercentageToProgressConverter _percentageConverter = new PercentageToProgressConverter();

		public FilesPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new FilesPageViewModel(new PlatformServices());
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			createViews();
		}

		void createViews()
		{
			var headerImage = createHeaderImage();
			var headerLabel = createHeaderLabel();
			var subjectPickerView = createSubjectsPicker();
			var filesListView = createList(new StackLayout
			{
				Spacing = _spacing,
				Children = {
					headerLabel,
					subjectPickerView
				}
			});

			var mainContent = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Children = {
					headerImage,
					filesListView
				}
			};

			var downloadOverlay = createDownloadOverlay();

			Content = new Grid
			{
				Children = {
					mainContent,
					downloadOverlay
				}
			};
		}

		Image createHeaderImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFit,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				Source = ImageSource.FromFile(Theme.Current.FilesHeaderImage)
			};
		}

		Label createHeaderLabel()
		{
			return new Label
			{
				Padding = _headerPadding,
				FontAttributes = FontAttributes.Bold,
				Text = CrossLocalization.Translate("files_header"),
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Large, true)
			};
		}

		SubjectsPickerView createSubjectsPicker()
		{
			return new SubjectsPickerView
			{
				Margin = _subjectsMargin
			};
		}

		RoundedListView createList(View header)
		{
			var filesListView = new RoundedListView(typeof(FilesPageViewCell), header: header)
			{
				IsPullToRefreshEnabled = true,
				Margin = _filesListMargin
			};
			filesListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "FileList");
			filesListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem", BindingMode.TwoWay);
			filesListView.SetBinding(ListView.IsRefreshingProperty, "IsLoading");
			filesListView.SetBinding(ListView.RefreshCommandProperty, "RefreshCommand");
			return filesListView;
		}

		/// <summary>
		/// Create a dimmed overlay with a progress card, shown while a
		/// file download is in progress.
		/// </summary>
		Grid createDownloadOverlay()
		{
			var overlay = new Grid
			{
				BackgroundColor = Color.FromArgb("#80000000")
			};
			overlay.SetBinding(IsVisibleProperty, "IsDownloading");

			var fileNameLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor),
				HorizontalTextAlignment = TextAlignment.Center,
				LineBreakMode = LineBreakMode.MiddleTruncation
			};
			fileNameLabel.SetBinding(Label.TextProperty, "DownloadingFileName");

			var progressBar = new ProgressBar
			{
				HeightRequest = 8,
				Margin = new Thickness(0, 15, 0, 5)
			};
			progressBar.SetBinding(ProgressBar.ProgressProperty, new Binding("DownloadPercentage", converter: _percentageConverter));

			var percentageLabel = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromArgb(Theme.Current.BaseSectionTextColor)
			};
			percentageLabel.SetBinding(Label.TextProperty, new Binding("DownloadPercentage", stringFormat: "{0}%"));

			var cancelButton = new Button
			{
				Text = CrossLocalization.Translate("base_cancel"),
				Margin = new Thickness(0, 15, 0, 0)
			};
			cancelButton.SetBinding(Button.CommandProperty, "CancelDownloadCommand");

			var card = new Frame
			{
				Padding = _downloadCardPadding,
				Margin = _downloadCardMargin,
				CornerRadius = 12,
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Fill,
				Content = new StackLayout
				{
					Children = {
						fileNameLabel,
						progressBar,
						percentageLabel,
						cancelButton
					}
				}
			};

			overlay.Children.Add(card);
			return overlay;
		}
	}
}