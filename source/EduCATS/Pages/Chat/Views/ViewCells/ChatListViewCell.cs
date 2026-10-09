using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Chat.Models;
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

		static Thickness _padding = new Thickness(15, 10);

		public ChatListViewCell()
		{
			var avatarLayout = new Grid
			{
				WidthRequest = _avatarSize,
				HeightRequest = _avatarSize,
				Children = { createAvatar(), createStatusDot() }
			};

			var grid = new Grid
			{
				Padding = _padding,
				ColumnSpacing = 12,
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			grid.Add(avatarLayout, 0, 0);
			grid.Add(createNameLabel(), 1, 0);
			grid.Add(ChatListCellParts.CreateMarks(), 2, 0);
			grid.Add(ChatListCellParts.CreateUnreadBadge(), 3, 0);

			ChatListCellParts.AddContextActions(this);
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

			image.SetBinding(Image.SourceProperty, nameof(ChatItemModel.Img), converter: new Base64ToImageSourceConverter());

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

		/// <summary>
		/// Green dot: the other participant is online (updated live).
		/// </summary>
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

			dot.SetBinding(VisualElement.IsVisibleProperty, nameof(ChatItemModel.IsOnlineVisible));
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

			label.SetBinding(Label.TextProperty, nameof(ChatItemModel.Name));
			return label;
		}
	}
}
