using EduCATS.Helpers.Forms.Converters;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Chat.Views.ViewCells
{
	/// <summary>
	/// Parts shared by personal and group chat list cells:
	/// pin/mute marks, unread badge and the long-press menu.
	/// </summary>
	static class ChatListCellParts
	{
		const double _unreadBadgeSize = 22;

		static readonly Color _mutedBadgeColor = Color.FromArgb("#9E9E9E");

		/// <summary>
		/// "📌" and "🔕" marks shown next to the name.
		/// </summary>
		public static HorizontalStackLayout CreateMarks()
		{
			var pinnedLabel = new Label { Text = "📌", FontSize = 12, VerticalOptions = LayoutOptions.Center };
			pinnedLabel.SetBinding(VisualElement.IsVisibleProperty, static (ChatListItemModel c) => c.IsPinned);

			var mutedLabel = new Label { Text = "🔕", FontSize = 12, VerticalOptions = LayoutOptions.Center };
			mutedLabel.SetBinding(VisualElement.IsVisibleProperty, static (ChatListItemModel c) => c.IsMuted);

			return new HorizontalStackLayout
			{
				Spacing = 4,
				VerticalOptions = LayoutOptions.Center,
				Children = { pinnedLabel, mutedLabel }
			};
		}

		/// <summary>
		/// Unread counter (grey for muted chats).
		/// </summary>
		public static Border CreateUnreadBadge()
		{
			var countLabel = new Label
			{
				TextColor = Colors.White,
				FontSize = 12,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			countLabel.SetBinding(Label.TextProperty, nameof(ChatListItemModel.Unread));

			var badge = new Border
			{
				MinimumWidthRequest = _unreadBadgeSize,
				HeightRequest = _unreadBadgeSize,
				Padding = new Thickness(6, 0),
				VerticalOptions = LayoutOptions.Center,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(_unreadBadgeSize / 2) },
				Content = countLabel
			};

			var activeColor = Color.FromArgb(Theme.Current.AppStatusBarBackgroundColor);
			badge.SetBinding(VisualElement.BackgroundColorProperty, new Binding(
				nameof(ChatListItemModel.IsMuted),
				converter: new BoolToObjectConverter<Color>(_mutedBadgeColor, activeColor)));
			badge.SetBinding(VisualElement.IsVisibleProperty, new Binding(
				nameof(ChatListItemModel.Unread), converter: new UnreadToVisibilityConverter()));
			return badge;
		}

		/// <summary>
		/// Long-press (Android) / swipe (iOS) menu: pin and mute.
		/// </summary>
		public static void AddContextActions(ViewCell cell)
		{
			var pinItem = new MenuItem();
			pinItem.SetBinding(MenuItem.TextProperty, nameof(ChatListItemModel.PinActionText));
			pinItem.Clicked += (sender, e) =>
			{
				if (cell.BindingContext is ChatListItemModel chat)
				{
					ChatLocalSettings.SetPinned(chat.Id, chat.IsGroupChat, !chat.IsPinned);
				}
			};

			var muteItem = new MenuItem();
			muteItem.SetBinding(MenuItem.TextProperty, nameof(ChatListItemModel.MuteActionText));
			muteItem.Clicked += (sender, e) =>
			{
				if (cell.BindingContext is ChatListItemModel chat)
				{
					ChatLocalSettings.SetMuted(chat.Id, chat.IsGroupChat, !chat.IsMuted);
				}
			};

			cell.ContextActions.Add(pinItem);
			cell.ContextActions.Add(muteItem);
		}

		/// <summary>
		/// Picks one of two values by a boolean.
		/// </summary>
		class BoolToObjectConverter<T> : IValueConverter
		{
			readonly T _trueValue;
			readonly T _falseValue;

			public BoolToObjectConverter(T trueValue, T falseValue)
			{
				_trueValue = trueValue;
				_falseValue = falseValue;
			}

			public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
				value is true ? _trueValue : _falseValue;

			public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
				throw new System.NotSupportedException();
		}
	}
}
