using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;


namespace EduCATS.Pages.Chat.Views
{
	public class ConversationPageView : ContentPage
	{
		readonly ConversationPageViewModel _viewModel;
		CollectionView _list;
		Entry _entry;
		SearchBar _searchBar;

		public ConversationPageView(int chatId, string title)
		{
			_viewModel = new ConversationPageViewModel(new PlatformServices(), chatId, title);
			BindingContext = _viewModel;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);

			_viewModel.HistoryLoaded += onHistoryLoaded;

			createViews();
			createToolbarItems();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_viewModel.HistoryLoaded -= onHistoryLoaded;
			_viewModel.Cleanup();
		}

		void onHistoryLoaded()
		{
			Dispatcher.Dispatch(() =>
			{
				if (_searchBar == null || string.IsNullOrWhiteSpace(_searchBar.Text))
				{
					ScrollToBottomInstant();
				}
			});
		}

		void ScrollToBottomInstant()
		{
			if (_viewModel.Messages != null && _viewModel.Messages.Count > 0 && _list != null)
			{
				var lastItem = _viewModel.Messages.Last();
				_list.ScrollTo(lastItem, position: ScrollToPosition.End, animate: false);
			}
		}

		void FilterMessages(string searchText)
		{
			if (_viewModel?.Messages == null || _list == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(searchText))
			{
				_list.SetBinding(ItemsView.ItemsSourceProperty, "Messages");
				ScrollToBottomInstant();
				return;
			}

			var result = new List<object>();
			DateSeparatorModel pendingSeparator = null;
			DateTime? lastEmittedDate = null;

			foreach (var item in _viewModel.Messages)
			{
				if (item is DateSeparatorModel separator)
				{
					pendingSeparator = separator;
					continue;
				}

				if (item is MessageItemModel message)
				{
					var matches = !string.IsNullOrEmpty(message.Text) &&
						message.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase);

					if (!matches)
					{
						continue;
					}

					if (pendingSeparator != null && lastEmittedDate != pendingSeparator.Date)
					{
						result.Add(pendingSeparator);
						lastEmittedDate = pendingSeparator.Date;
					}

					result.Add(message);
				}
			}

			_list.ItemsSource = result;
		}

		void createToolbarItems()
		{
			var titleLabel = new Label
			{
				Text = _viewModel.Title,
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				TextColor = Color.FromArgb(Theme.Current.BaseAppColor),
				VerticalOptions = LayoutOptions.Center,
				LineBreakMode = LineBreakMode.TailTruncation
			};

			var searchIcon = new Image
			{
				Source = "icon_search.png",
				HeightRequest = 23,
				WidthRequest = 23,
				VerticalOptions = LayoutOptions.Center
			};

			var searchTap = new TapGestureRecognizer();
			searchTap.Tapped += (sender, e) =>
			{
				_searchBar.IsVisible = !_searchBar.IsVisible;

				if (_searchBar.IsVisible)
				{
					_searchBar.Focus();
				}
				else
				{
					_searchBar.Text = string.Empty;
				}
			};
			searchIcon.GestureRecognizers.Add(searchTap);

			var phoneIcon = new Image
			{
				Source = "icon_phone.png",
				HeightRequest = 18,
				WidthRequest = 18,
				VerticalOptions = LayoutOptions.Center
			};

			var phoneTap = new TapGestureRecognizer();
			phoneTap.Tapped += (sender, e) =>
			{
				// Сюда добавим логику звонков
			};
			phoneIcon.GestureRecognizers.Add(phoneTap);

			var iconsLayout = new HorizontalStackLayout
			{
				Spacing = 12,
				VerticalOptions = LayoutOptions.Center,
				Children = { searchIcon, phoneIcon }
			};

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

			titleGrid.Add(titleLabel, 0, 0);
			titleGrid.Add(iconsLayout, 1, 0);

			NavigationPage.SetTitleView(this, titleGrid);
		}

		void createViews()
		{
			_searchBar = new SearchBar
			{
				Placeholder = "Поиск сообщений...",
				IsVisible = false,
				HeightRequest = 50
			};

			_searchBar.TextChanged += (sender, e) => FilterMessages(e.NewTextValue);

			_list = new CollectionView
			{
				ItemTemplate = new ConversationTemplateSelector
				{
					MessageTemplate = new DataTemplate(createMessageCell),
					DateSeparatorTemplate = new DataTemplate(createDateSeparatorCell)
				},
				SelectionMode = SelectionMode.None,
				ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView
			};

			_list.SetBinding(ItemsView.ItemsSourceProperty, "Messages");

			_entry = new Entry
			{
				Placeholder = "Message...",
				VerticalOptions = LayoutOptions.Center,
				ReturnType = ReturnType.Send,
				IsSpellCheckEnabled = false,
			};

			_entry.SetBinding(Entry.TextProperty, "MessageText");
			_entry.SetBinding(Entry.ReturnCommandProperty, "SendCommand");

			// 1. Создаем сам значок скрепки со строгими размерами
			var attachImage = new Image
			{
				Source = "attach_icon.png",
				WidthRequest = 26,
				HeightRequest = 26,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Aspect = Aspect.AspectFit
			};

			// 2. Оборачиваем в кликабельный контейнер (область нажатия 36x36px)
			var attachButton = new ContentView
			{
				WidthRequest = 36,
				HeightRequest = 36,
				Content = attachImage,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			// 3. Привязываем команду нажатия (AttachCommand)
			var attachTap = new TapGestureRecognizer();
			attachTap.SetBinding(TapGestureRecognizer.CommandProperty, "AttachCommand");
			attachButton.GestureRecognizers.Add(attachTap);

			var sendButton = new Button { Text = "➤", WidthRequest = 48 };
			sendButton.SetBinding(Button.CommandProperty, "SendCommand");

			var inputRow = new Grid
			{
				Padding = new Thickness(10, 6),
				ColumnSpacing = 8,
				ColumnDefinitions =
		{
			new ColumnDefinition { Width = GridLength.Auto },  // скрепка
			new ColumnDefinition { Width = GridLength.Star },  // поле ввода
			new ColumnDefinition { Width = GridLength.Auto }   // отправка
		}
			};

			inputRow.Add(attachButton, 0, 0);
			inputRow.Add(_entry, 1, 0);
			inputRow.Add(sendButton, 2, 0);

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
			root.Add(inputRow, 0, 2);

			Content = root;
		}
		View createDateSeparatorCell()
		{
			var label = new Label
			{
				FontSize = 12,
				Opacity = 0.6,
				HorizontalOptions = LayoutOptions.Center
			};

			label.SetBinding(Label.TextProperty, "DisplayText");

			return new StackLayout
			{
				Padding = new Thickness(0, 10),
				Children = { label }
			};
		}

		View createMessageCell()
		{
			var timeLabel = new Label
			{
				FontSize = 11,
				Opacity = 0.6,
				HorizontalOptions = LayoutOptions.End
			};

			timeLabel.SetBinding(Label.TextProperty, new Binding("LocalTime", stringFormat: "{0:HH:mm}"));

			var textLabel = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 15
			};

			textLabel.SetBinding(Label.TextProperty, "Text");
			textLabel.SetBinding(Label.IsVisibleProperty, "IsPlainText");

			var attachmentImage = new Image
			{
				HeightRequest = 160,
				Aspect = Aspect.AspectFit
			};

			attachmentImage.SetBinding(Image.SourceProperty, new Binding(
				"ImageContent[0]", converter: new Base64ToImageSourceConverter()));
			attachmentImage.SetBinding(Image.IsVisibleProperty, "IsImageMessage"); // было "IsImage"

			var fileNameLabel = new Label
			{
				FontSize = 14,
				LineBreakMode = LineBreakMode.WordWrap,
				VerticalOptions = LayoutOptions.Center
			};
			fileNameLabel.SetBinding(Label.TextProperty, "FileDisplayText");

			var fileIcon = new Image
			{
				Source = "icon_file.png",
				WidthRequest = 20,
				HeightRequest = 20,
				VerticalOptions = LayoutOptions.Start
			};

			// Grid вместо HorizontalStackLayout: колонка с иконкой - Auto,
			// колонка с текстом - Star, чтобы Label получил ограничение по
			// ширине и WordWrap реально переносил длинные имена файлов.
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

			fileChip.SetBinding(Grid.IsVisibleProperty, "IsFileMessage"); // было "IsFile"

			// Тап по чипу файла - скачивает и открывает файл системным средством.
			var fileTap = new TapGestureRecognizer();
			fileTap.Tapped += async (sender, e) =>
			{
				if (fileChip.BindingContext is MessageItemModel msg)
				{
					await openFileAsync(msg);
				}
			};
			fileChip.GestureRecognizers.Add(fileTap);

			var contentStack = new VerticalStackLayout
			{
				Spacing = 2,
				Children = { textLabel, attachmentImage, fileChip, timeLabel }
			};

			var bubble = new Border
			{
				Padding = new Thickness(12, 8),
				StrokeThickness = 0,
				StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
				{
					CornerRadius = new CornerRadius(14)
				},
				MaximumWidthRequest = 280,
				Content = contentStack
			};

			bubble.SetBinding(Border.BackgroundColorProperty, new Binding("IsMine",
				converter: new BoolToBubbleColorConverter()));

			var row = new Grid { Padding = new Thickness(10, 3) };
			row.Add(bubble);

			row.SetBinding(View.HorizontalOptionsProperty, new Binding("IsMine",
				converter: new BoolToAlignmentConverter()));

			return row;
		}

		async Task openFileAsync(MessageItemModel msg)
		{
			if (msg == null || string.IsNullOrEmpty(msg.FileContent))
			{
				return;
			}

			try
			{
				var fileName = msg.FileContent;
				var bytes = await Pages.Chat.Services.ChatApiService.DownloadFile(msg.ChatId, fileName);

				if (bytes == null)
				{
					await DisplayAlert("Ошибка", "Не удалось скачать файл.", "ОК");
					return;
				}

				var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
				await File.WriteAllBytesAsync(localPath, bytes);

				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(localPath)
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"openFileAsync error: {ex}");
				await DisplayAlert("Ошибка", "Не удалось открыть файл.", "ОК");
			}
		}
	}
}