using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EduCATS.Controls;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Pages;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Common UI of personal and group conversations.
	/// </summary>
	/// <remarks>
	/// Derived pages call <see cref="InitializeView"/> at the end of their
	/// constructor (after their own fields are set).
	/// </remarks>
	public abstract class ConversationPageViewBase : ContentPage, IHidesTabBar
	{
		/// <summary>
		/// Older messages start loading when one of the first items is visible.
		/// </summary>
		const int _loadOlderThreshold = 2;

		/// <summary>
		/// New messages scroll the list only if the user is near the bottom.
		/// </summary>
		const int _nearBottomItems = 3;

		static readonly Color _secondaryTextColor = Color.FromArgb("#8A8A8A");
		static readonly Color _failedColor = Color.FromArgb("#E53935");

		CollectionView _list;
		SearchBar _searchBar;
		Label _searchEmptyLabel;
		int _lastVisibleIndex;

		protected ConversationPageViewBase(ConversationViewModelBase viewModel)
		{
			ViewModel = viewModel;
			BindingContext = viewModel;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);

			// iOS: the input row stays above the keyboard and the home indicator
			// (Android resizes the window for the keyboard itself).
			if (DeviceInfo.Platform == DevicePlatform.iOS)
			{
				SafeAreaEdges = SafeAreaEdges.All;
			}

			viewModel.HistoryLoaded += onHistoryLoaded;
			viewModel.MessageAppended += onMessageAppended;
			viewModel.OlderMessagesLoaded += onOlderMessagesLoaded;
			viewModel.PropertyChanged += onViewModelPropertyChanged;
		}

		protected ConversationViewModelBase ViewModel { get; }

		/// <summary>
		/// Extra header icons (call, students).
		/// </summary>
		protected abstract IEnumerable<View> CreateHeaderIcons();

		/// <summary>
		/// Label under the title (e.g. online status), <c>null</c> if none.
		/// </summary>
		protected virtual View CreateSubtitle() => null;

		protected void InitializeView()
		{
			createViews();
			createHeader();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Activate();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			ViewModel.Deactivate();
		}

		/// <summary>
		/// Create a tappable header icon.
		/// </summary>
		protected static Image CreateHeaderIcon(string source, double size, string description, Func<Task> onTapped)
		{
			var icon = new Image
			{
				Source = source,
				HeightRequest = size,
				WidthRequest = size,
				VerticalOptions = LayoutOptions.Center
			};

			var tap = new TapGestureRecognizer();
			tap.Tapped += async (sender, e) => await onTapped();
			icon.GestureRecognizers.Add(tap);
			SemanticProperties.SetDescription(icon, description);
			return icon;
		}

		void createHeader()
		{
			var titleLabel = new Label
			{
				Text = ViewModel.Title,
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				TextColor = Color.FromArgb(Theme.Current.BaseAppColor),
				LineBreakMode = LineBreakMode.TailTruncation
			};

			var titleStack = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = { titleLabel }
			};

			var subtitle = CreateSubtitle();

			if (subtitle != null)
			{
				titleStack.Children.Add(subtitle);
			}

			var searchIcon = CreateHeaderIcon("icon_search.png", 23, CrossLocalization.Translate("a11y_search"), () =>
			{
				toggleSearch();
				return Task.CompletedTask;
			});

			var iconsLayout = new HorizontalStackLayout
			{
				Spacing = 12,
				VerticalOptions = LayoutOptions.Center
			};

			iconsLayout.Children.Add(searchIcon);

			foreach (var icon in CreateHeaderIcons())
			{
				iconsLayout.Children.Add(icon);
			}

			var titleGrid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				},
				HorizontalOptions = LayoutOptions.Fill,
				Padding = new Thickness(0, 0, 10, 0)
			};

			titleGrid.Add(titleStack, 0, 0);
			titleGrid.Add(iconsLayout, 1, 0);

			NavigationPage.SetTitleView(this, titleGrid);
		}

		void toggleSearch()
		{
			_searchBar.IsVisible = !_searchBar.IsVisible;

			if (_searchBar.IsVisible)
			{
				_searchBar.Focus();
			}
			else
			{
				ViewModel.SearchText = string.Empty;
			}
		}

		void createViews()
		{
			_searchBar = new SearchBar
			{
				Placeholder = CrossLocalization.Translate("chat_search_messages_placeholder"),
				IsVisible = false,
				HeightRequest = 50
			};

			_searchBar.SetBinding(SearchBar.TextProperty, nameof(ConversationViewModelBase.SearchText));

			_searchEmptyLabel = new Label
			{
				Text = CrossLocalization.Translate("chat_search_nothing"),
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 30),
				TextColor = _secondaryTextColor
			};

			var olderIndicator = new ActivityIndicator
			{
				HeightRequest = 30,
				Margin = new Thickness(0, 6)
			};

			olderIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ConversationViewModelBase.IsLoadingOlder));
			olderIndicator.SetBinding(IsVisibleProperty, nameof(ConversationViewModelBase.IsLoadingOlder));

			_list = new CollectionView
			{
				ItemTemplate = new ConversationTemplateSelector
				{
					TextMessageTemplate = new DataTemplate(() => createMessageCell(createTextContent())),
					ImageMessageTemplate = new DataTemplate(() => createMessageCell(createImageContent())),
					FileMessageTemplate = new DataTemplate(() => createMessageCell(createFileChip(), createUploadProgress())),
					DateSeparatorTemplate = new DataTemplate(createDateSeparatorCell)
				},
				SelectionMode = SelectionMode.None,
				// Older messages are inserted at the top and new ones appended
				// at the bottom: scrolling is handled manually (see below).
				ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
				Header = olderIndicator
			};

			_list.SetBinding(ItemsView.ItemsSourceProperty, nameof(ConversationViewModelBase.Messages));
			_list.Scrolled += onListScrolled;
			_list.SizeChanged += onListSizeChanged;

			var root = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			root.Add(_searchBar, 0, 0);
			root.Add(_list, 0, 1);
			root.Add(createInputRow(), 0, 2);

			Content = OfflineBanner.Wrap(root);
		}

		Grid createInputRow()
		{
			var entry = new Entry
			{
				Placeholder = CrossLocalization.Translate("chat_message_placeholder"),
				VerticalOptions = LayoutOptions.Center,
				ReturnType = ReturnType.Send,
				IsSpellCheckEnabled = false
			};

			entry.SetBinding(Entry.TextProperty, nameof(ConversationViewModelBase.MessageText));
			entry.SetBinding(Entry.ReturnCommandProperty, nameof(ConversationViewModelBase.SendCommand));

			var attachButton = new ContentView
			{
				WidthRequest = 36,
				HeightRequest = 36,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Content = new Image
				{
					Source = "attach_icon.png",
					WidthRequest = 26,
					HeightRequest = 26,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFit
				}
			};

			var attachTap = new TapGestureRecognizer();
			attachTap.SetBinding(TapGestureRecognizer.CommandProperty, nameof(ConversationViewModelBase.AttachCommand));
			attachButton.GestureRecognizers.Add(attachTap);
			SemanticProperties.SetDescription(attachButton, CrossLocalization.Translate("chat_attach_title"));

			var sendButton = new Button { Text = "➤", WidthRequest = 48 };
			sendButton.SetBinding(Button.CommandProperty, nameof(ConversationViewModelBase.SendCommand));
			SemanticProperties.SetDescription(sendButton, CrossLocalization.Translate("a11y_send"));

			var inputRow = new Grid
			{
				Padding = new Thickness(10, 6),
				ColumnSpacing = 8,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			inputRow.Add(attachButton, 0, 0);
			inputRow.Add(entry, 1, 0);
			inputRow.Add(sendButton, 2, 0);
			return inputRow;
		}

		View createDateSeparatorCell()
		{
			var label = new Label
			{
				FontSize = 12,
				Opacity = 0.6,
				HorizontalOptions = LayoutOptions.Center
			};

			label.SetBinding(Label.TextProperty, static (DateSeparatorModel d) => d.DisplayText);

			return new StackLayout
			{
				Padding = new Thickness(0, 10),
				Children = { label }
			};
		}

		// Shared by all cells: converters keep no state.
		static readonly MessageTextToFormattedStringConverter _linksConverter = new MessageTextToFormattedStringConverter();
		static readonly MessageImageSourceConverter _imageConverter = new MessageImageSourceConverter();
		static readonly BoolToBubbleColorConverter _bubbleColorConverter = new BoolToBubbleColorConverter();
		static readonly BoolToAlignmentConverter _alignmentConverter = new BoolToAlignmentConverter();
		static readonly MessageStatusToGlyphConverter _statusConverter = new MessageStatusToGlyphConverter();

		/// <summary>
		/// Text and its link preview.
		/// </summary>
		View[] createTextContent()
		{
			// Plain text for most messages: spans (FormattedText) are much
			// slower to lay out, so they're used only for messages with links.
			var textLabel = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 15
			};

			textLabel.SetBinding(Label.TextProperty, static (MessageItemModel m) => m.Text);
			textLabel.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.HasNoLinks);

			var linksLabel = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 15
			};

			linksLabel.SetBinding(Label.FormattedTextProperty, static (MessageItemModel m) => m.Text, converter: _linksConverter);
			linksLabel.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.HasLinks);

			return new View[] { textLabel, linksLabel, createLinkPreview() };
		}

		View[] createImageContent()
		{
			var attachmentImage = new Image
			{
				HeightRequest = 160,
				Aspect = Aspect.AspectFit
			};

			attachmentImage.SetBinding(Image.SourceProperty, new Binding(".", converter: _imageConverter));

			var imageTap = new TapGestureRecognizer();
			imageTap.Tapped += async (sender, e) =>
			{
				if (attachmentImage.BindingContext is MessageItemModel message)
				{
					await openImage(message);
				}
			};
			attachmentImage.GestureRecognizers.Add(imageTap);

			return new View[] { attachmentImage, createUploadProgress() };
		}

		static View createUploadProgress()
		{
			var uploadProgress = new ProgressBar { Margin = new Thickness(0, 4) };
			uploadProgress.SetBinding(ProgressBar.ProgressProperty, static (MessageItemModel m) => m.UploadProgress);
			uploadProgress.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.IsUploading);
			return uploadProgress;
		}

		View createMessageCell(params View[] content)
		{
			var contentStack = new VerticalStackLayout { Spacing = 2 };

			foreach (var view in content)
			{
				contentStack.Children.Add(view);
			}

			contentStack.Children.Add(createTimeRow());

			var bubble = new Border
			{
				Padding = new Thickness(12, 8),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
				MaximumWidthRequest = 280,
				Content = contentStack
			};

			bubble.SetBinding(Border.BackgroundColorProperty, static (MessageItemModel m) => m.IsMine, converter: _bubbleColorConverter);

			var cell = new VerticalStackLayout
			{
				Padding = new Thickness(10, 3),
				Children = { bubble, createRetryLabel() }
			};

			cell.SetBinding(View.HorizontalOptionsProperty, static (MessageItemModel m) => m.IsMine, converter: _alignmentConverter);

			return cell;
		}

		View createLinkPreview()
		{
			var image = new Image
			{
				WidthRequest = 48,
				HeightRequest = 48,
				Aspect = Aspect.AspectFill,
				VerticalOptions = LayoutOptions.Start
			};

			image.SetBinding(Image.SourceProperty, nameof(LinkPreviewModel.ImageUrl));
			image.SetBinding(IsVisibleProperty, static (LinkPreviewModel p) => p.HasImage);

			var title = new Label
			{
				FontSize = 13,
				FontAttributes = FontAttributes.Bold,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			title.SetBinding(Label.TextProperty, static (LinkPreviewModel p) => p.Title);

			var description = new Label
			{
				FontSize = 12,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.TailTruncation,
				TextColor = _secondaryTextColor
			};
			description.SetBinding(Label.TextProperty, static (LinkPreviewModel p) => p.Description);
			description.SetBinding(IsVisibleProperty, static (LinkPreviewModel p) => p.HasDescription);

			var host = new Label { FontSize = 11, TextColor = _secondaryTextColor };
			host.SetBinding(Label.TextProperty, static (LinkPreviewModel p) => p.Host);

			var grid = new Grid
			{
				ColumnSpacing = 8,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			grid.Add(image, 0, 0);
			grid.Add(new VerticalStackLayout { Spacing = 1, Children = { title, description, host } }, 1, 0);

			var card = new Border
			{
				Margin = new Thickness(0, 4, 0, 0),
				Padding = new Thickness(8),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
				BackgroundColor = Color.FromRgba(0, 0, 0, 0.06),
				Content = grid
			};

			// The card is bound to the preview, the wrapper - to the message.
			card.SetBinding(BindingContextProperty, nameof(MessageItemModel.LinkPreview));

			var tap = new TapGestureRecognizer();
			tap.Tapped += async (sender, e) =>
			{
				if (card.BindingContext is LinkPreviewModel preview)
				{
					await openUrl(preview.Url);
				}
			};
			card.GestureRecognizers.Add(tap);

			var wrapper = new ContentView { Content = card };
			wrapper.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.HasLinkPreview);
			return wrapper;
		}

		Grid createFileChip()
		{
			var fileNameLabel = new Label
			{
				FontSize = 14,
				LineBreakMode = LineBreakMode.WordWrap,
				VerticalOptions = LayoutOptions.Center
			};
			fileNameLabel.SetBinding(Label.TextProperty, static (MessageItemModel m) => m.FileDisplayText);

			var fileIcon = new Image
			{
				Source = "icon_file.png",
				WidthRequest = 20,
				HeightRequest = 20,
				VerticalOptions = LayoutOptions.Start
			};

			// Grid instead of HorizontalStackLayout: the Star column limits the
			// label width, so WordWrap really wraps long file names.
			var fileChip = new Grid
			{
				ColumnSpacing = 6,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			fileChip.Add(fileIcon, 0, 0);
			fileChip.Add(fileNameLabel, 1, 0);

			var fileTap = new TapGestureRecognizer();
			fileTap.Tapped += async (sender, e) =>
			{
				if (fileChip.BindingContext is MessageItemModel message)
				{
					await openFile(message);
				}
			};
			fileChip.GestureRecognizers.Add(fileTap);
			return fileChip;
		}

		View createTimeRow()
		{
			var timeLabel = new Label { FontSize = 11, Opacity = 0.6 };
			timeLabel.SetBinding(Label.TextProperty, static (MessageItemModel m) => m.LocalTime, stringFormat: "{0:HH:mm}");

			var statusLabel = new Label { FontSize = 11, Opacity = 0.7 };
			statusLabel.SetBinding(Label.TextProperty, static (MessageItemModel m) => m.Status, converter: _statusConverter);
			statusLabel.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.IsMine);

			return new HorizontalStackLayout
			{
				Spacing = 4,
				HorizontalOptions = LayoutOptions.End,
				Children = { timeLabel, statusLabel }
			};
		}

		Label createRetryLabel()
		{
			var retryLabel = new Label
			{
				Text = CrossLocalization.Translate("chat_retry"),
				FontSize = 12,
				TextColor = _failedColor,
				HorizontalOptions = LayoutOptions.End,
				Padding = new Thickness(4, 2)
			};

			retryLabel.SetBinding(IsVisibleProperty, static (MessageItemModel m) => m.IsFailed);

			var retryTap = new TapGestureRecognizer();
			retryTap.Tapped += async (sender, e) =>
			{
				if (retryLabel.BindingContext is MessageItemModel message)
				{
					await ViewModel.Retry(message);
				}
			};
			retryLabel.GestureRecognizers.Add(retryTap);
			return retryLabel;
		}

		void onListScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			_lastVisibleIndex = e.LastVisibleItemIndex;

			// Only real scrolling: a resize (keyboard) reports no movement
			// and must not unpin the list from the bottom.
			if (!ViewModel.IsSearching && e.VerticalDelta != 0)
			{
				_isAtBottom = isLastItemVisible();
			}

			if (!ViewModel.IsSearching &&
				e.VerticalDelta < 0 &&
				e.FirstVisibleItemIndex <= _loadOlderThreshold)
			{
				_ = ViewModel.LoadOlderAsync();
			}
		}

		void onHistoryLoaded() => Dispatcher.Dispatch(scrollToBottom);

		void onMessageAppended(bool isMine)
		{
			if (ViewModel.IsSearching)
			{
				return;
			}

			var count = ViewModel.Messages.Count;

			// Don't pull the user away from older messages they are reading.
			if (!isMine && _lastVisibleIndex < count - 1 - _nearBottomItems)
			{
				return;
			}

			_isAtBottom = true;
			Dispatcher.Dispatch(() => scrollTo(ViewModel.Messages.LastOrDefault(), ScrollToPosition.End, animate: true));

			// The list may change its size right after sending (the keyboard
			// hides after the "send" key) and stop the scrolling halfway:
			// check again once the layout is settled.
			Dispatcher.DispatchDelayed(_settleDelay, () =>
			{
				if (!ViewModel.IsSearching && !isLastItemVisible())
				{
					scrollToBottom();
				}
			});
		}

		/// <summary>
		/// Time for the keyboard and the list layout to settle.
		/// </summary>
		static readonly TimeSpan _settleDelay = TimeSpan.FromMilliseconds(350);

		/// <summary>
		/// Is the newest message on screen: the list then stays at the bottom
		/// when its size changes (keyboard shown or hidden).
		/// </summary>
		bool _isAtBottom = true;

		bool isLastItemVisible() =>
			_lastVisibleIndex >= ViewModel.Messages.Count - 1;

		void onListSizeChanged(object sender, EventArgs e)
		{
			if (_isAtBottom && !ViewModel.IsSearching)
			{
				Dispatcher.Dispatch(scrollToBottom);
			}
		}

		void onOlderMessagesLoaded(object anchor) =>
			Dispatcher.Dispatch(() => scrollTo(anchor, ScrollToPosition.Start, animate: false));

		void onViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(ConversationViewModelBase.IsSearching))
			{
				return;
			}

			if (ViewModel.IsSearching)
			{
				_list.EmptyView = _searchEmptyLabel;
				_list.SetBinding(ItemsView.ItemsSourceProperty, nameof(ConversationViewModelBase.SearchResults));
			}
			else
			{
				_list.EmptyView = null;
				_list.SetBinding(ItemsView.ItemsSourceProperty, nameof(ConversationViewModelBase.Messages));
				Dispatcher.Dispatch(scrollToBottom);
			}
		}

		void scrollToBottom()
		{
			if (!ViewModel.IsSearching)
			{
				scrollTo(ViewModel.Messages.LastOrDefault(), ScrollToPosition.End, animate: false);
			}
		}

		void scrollTo(object item, ScrollToPosition position, bool animate)
		{
			if (item == null || _list == null)
			{
				return;
			}

			try
			{
				_list.ScrollTo(item, position: position, animate: animate);
			}
			catch (Exception ex)
			{
				// The item may have been replaced in the meantime.
				AppLogs.Log(ex);
			}
		}

		async Task openImage(MessageItemModel message)
		{
			var source = MessageImageSourceConverter.Create(message);

			if (source != null)
			{
				await Navigation.PushModalAsync(new ImageViewerPage(source));
			}
		}

		async Task openFile(MessageItemModel message)
		{
			if (message == null || string.IsNullOrEmpty(message.FileContent))
			{
				return;
			}

			try
			{
				var path = !string.IsNullOrEmpty(message.LocalFilePath) && File.Exists(message.LocalFilePath) ?
					message.LocalFilePath :
					message.HasInlineFile ?
						await ChatFileCache.GetOrSaveInlineAsync(message.ChatId, message.IsGroupChat, message.FileName, message.FileContent) :
						await ChatFileCache.GetOrDownloadAsync(message.ChatId, message.IsGroupChat, message.FileContent);

				if (path == null)
				{
					await DisplayAlertAsync(
						CrossLocalization.Translate("base_error"),
						CrossLocalization.Translate("chat_file_open_error"),
						"OK");
					return;
				}

				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(path)
				});
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				await DisplayAlertAsync(
					CrossLocalization.Translate("base_error"),
					CrossLocalization.Translate("chat_file_open_error"),
					"OK");
			}
		}

		static async Task openUrl(string url)
		{
			try
			{
				await Launcher.Default.OpenAsync(new Uri(url));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}
	}
}
