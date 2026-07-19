using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Chat.Views.ViewCells
{
	public class ChatListViewCell : ViewCell
	{
		const double _avatarSize = 50;
		const double _statusDotSize = 12;
		const double _unreadBadgeSize = 22;

		static Thickness _padding = new Thickness(15, 10);

		public ChatListViewCell()
		{
			var avatar = createAvatar();
			var statusDot = createStatusDot();
			var avatarLayout = new Grid
			{
				WidthRequest = _avatarSize,
				HeightRequest = _avatarSize,
				Children = { avatar, statusDot }
			};

			var nameLabel = createNameLabel();
			var unreadBadge = createUnreadBadge();

			var grid = new Grid
			{
				Padding = _padding,
				ColumnSpacing = 12,
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			grid.Add(avatarLayout, 0, 0);
			grid.Add(nameLabel, 1, 0);
			grid.Add(unreadBadge, 2, 0);

			View = grid;
		}

		Border createAvatar()
		{
			var image = new Image
			{
				Aspect = Aspect.AspectFill,
				WidthRequest = _avatarSize,
				HeightRequest = _avatarSize
			};

			image.SetBinding(Image.SourceProperty, "Img", converter: new Base64ToImageSourceConverter());

			return new Border
			{
				WidthRequest = _avatarSize,
				HeightRequest = _avatarSize,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(_avatarSize / 2) },
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = image
			};
		}

		Ellipse createStatusDot()
		{
			var dot = new Ellipse
			{
				WidthRequest = _statusDotSize,
				HeightRequest = _statusDotSize,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.End,
				Fill = Colors.LimeGreen,
				Stroke = Colors.White,
				StrokeThickness = 2
			};

			dot.SetBinding(VisualElement.IsVisibleProperty, "IsOnline");
			return dot;
		}

		Label createNameLabel()
		{
			var label = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle(),
				TextColor = Color.FromArgb(Theme.Current.StatisticsBaseTitleColor),
				LineBreakMode = LineBreakMode.TailTruncation
			};

			label.SetBinding(Label.TextProperty, "Name");
			return label;
		}

		Border createUnreadBadge()
		{
			var countLabel = new Label
			{
				TextColor = Colors.White,
				FontSize = 12,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			countLabel.SetBinding(Label.TextProperty, "Unread");

			var badge = new Border
			{
				WidthRequest = _unreadBadgeSize,
				HeightRequest = _unreadBadgeSize,
				VerticalOptions = LayoutOptions.Center,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(_unreadBadgeSize / 2) },
				BackgroundColor = Color.FromArgb(Theme.Current.AppStatusBarBackgroundColor),
				Content = countLabel
			};

			// Исправлено: добавлен VisualElement. перед IsVisibleProperty
			badge.SetBinding(VisualElement.IsVisibleProperty, new Binding("Unread", converter: new UnreadToVisibilityConverter()));
			return badge;
		}
	}
}