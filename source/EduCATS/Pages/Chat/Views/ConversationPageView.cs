using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using EduCATS.Helpers.Forms.Converters;

namespace EduCATS.Pages.Chat.Views
{
	public class ConversationPageView : ContentPage
	{
		readonly ConversationPageViewModel _viewModel;

		public ConversationPageView(int chatId, string title)
		{
			_viewModel = new ConversationPageViewModel(new PlatformServices(), chatId, title);
			BindingContext = _viewModel;
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			createViews();
		}

		CollectionView _list;
		Entry _entry;

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_viewModel.Cleanup();
		}

		void createViews()
		{
			_list = new CollectionView
			{
				ItemTemplate = new ConversationTemplateSelector
				{
					MessageTemplate = new DataTemplate(createMessageCell),
					DateSeparatorTemplate = new DataTemplate(createDateSeparatorCell)
				},
				SelectionMode = SelectionMode.None
			};

			_list.SetBinding(ItemsView.ItemsSourceProperty, "Messages");

			_entry = new Entry
			{
				Placeholder = "Message...",
				VerticalOptions = LayoutOptions.Center,
				ReturnType = ReturnType.Send
			};

			_entry.SetBinding(Entry.TextProperty, "MessageText");
			// Explicitly wire Enter/Return to the send command - relying
			// only on Button.CommandProperty binding doesn't cover the
			// keyboard's Enter/Send key.
			_entry.SetBinding(Entry.ReturnCommandProperty, "SendCommand");

			var sendButton = new Button { Text = "➤", WidthRequest = 48 };
			sendButton.SetBinding(Button.CommandProperty, "SendCommand");

			var inputRow = new Grid
			{
				Padding = new Thickness(10),
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			inputRow.Add(_entry, 0, 0);
			inputRow.Add(sendButton, 1, 0);

			var root = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			root.Add(_list, 0, 0);
			root.Add(inputRow, 0, 1);

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

			var contentStack = new VerticalStackLayout
			{
				Spacing = 2,
				Children = { textLabel, timeLabel }
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
	}
}