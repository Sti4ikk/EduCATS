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
	public class GroupChatListViewCell : ViewCell
	{
		const double _avatarSize = 50;

		static readonly Base64ToImageSourceConverter _avatarConverter = new Base64ToImageSourceConverter();

		static Thickness _padding = new Thickness(15, 10);

		public GroupChatListViewCell()
		{
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

			grid.Add(createAvatar(), 0, 0);
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

			image.SetBinding(Image.SourceProperty, static (GroupChatModel g) => g.Img, converter: _avatarConverter);

			return new Border
			{
				WidthRequest = _avatarSize,
				HeightRequest = _avatarSize,
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = image
			};
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

			label.SetBinding(Label.TextProperty, static (GroupChatModel g) => g.DisplayName);
			return label;
		}
	}
}
