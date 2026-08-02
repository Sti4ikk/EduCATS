using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Модальный попап со списком студентов группы. Открывается из
	/// <see cref="GroupConversationPageView"/> по тапу на icon_students.
	/// </summary>
	public class GroupStudentsPageView : ContentPage
	{
		readonly GroupStudentsPageViewModel _viewModel;

		public GroupStudentsPageView(int groupId, string subtitle)
		{
			_viewModel = new GroupStudentsPageViewModel(new PlatformServices(), groupId, subtitle);
			BindingContext = _viewModel;

			BackgroundColor = Color.FromArgb("#66000000");
			NavigationPage.SetHasNavigationBar(this, false);

			createViews();
		}

		void createViews()
		{
			var titleLabel = new Label
			{
				Text = "Список студентов",
				FontSize = 20,
				FontAttributes = FontAttributes.Bold,
				TextColor = Colors.Black
			};

			var subtitleLabel = new Label
			{
				FontSize = 13,
				TextColor = Color.FromArgb("#9AA0A6")
			};
			subtitleLabel.SetBinding(Label.TextProperty, "Subtitle");

			var closeIcon = new Label
			{
				Text = "✕",
				FontSize = 18,
				TextColor = Color.FromArgb("#9AA0A6"),
				VerticalOptions = LayoutOptions.Start
			};

			var closeTap = new TapGestureRecognizer();
			closeTap.Tapped += async (sender, e) => await Navigation.PopModalAsync();
			closeIcon.GestureRecognizers.Add(closeTap);

			var titleStack = new VerticalStackLayout
			{
				Spacing = 2,
				Children = { titleLabel, subtitleLabel }
			};

			var headerGrid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				},
				Margin = new Thickness(0, 0, 0, 12)
			};

			headerGrid.Add(titleStack, 0, 0);
			headerGrid.Add(closeIcon, 1, 0);

			var list = new CollectionView
			{
				ItemTemplate = new DataTemplate(createStudentCell),
				SelectionMode = SelectionMode.None
			};
			list.SetBinding(ItemsView.ItemsSourceProperty, "Students");

			var emptyLabel = new Label
			{
				Text = "В группе пока нет студентов",
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Color.FromArgb("#9AA0A6")
			};
			emptyLabel.SetBinding(Label.IsVisibleProperty, "IsEmpty");

			var loader = new ActivityIndicator { Color = Color.FromArgb(Theme.Current.BaseAppColor) };
			loader.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");
			loader.SetBinding(IsVisibleProperty, "IsLoading");

			var closeButton = new Button
			{
				Text = "Закрыть",
				BackgroundColor = Color.FromArgb(Theme.Current.BaseAppColor),
				TextColor = Colors.White,
				CornerRadius = 8,
				HorizontalOptions = LayoutOptions.End,
				Margin = new Thickness(0, 12, 0, 0)
			};
			closeButton.Clicked += async (sender, e) => await Navigation.PopModalAsync();

			var contentStack = new VerticalStackLayout
			{
				Spacing = 0,
				Children = { headerGrid, loader, emptyLabel, list, closeButton }
			};

			Content = new Border
			{
				BackgroundColor = Colors.White,
				Padding = new Thickness(20, 18),
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
				MaximumWidthRequest = 420,
				MaximumHeightRequest = 560,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Content = contentStack
			};
		}

		View createStudentCell()
		{
			var initialsLabel = new Label
			{
				FontSize = 14,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.FromArgb("#4B5FC4"),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			initialsLabel.SetBinding(Label.TextProperty, "Initials");
			initialsLabel.SetBinding(Label.IsVisibleProperty, "NoAvatar");

			var avatarImage = new Image { Aspect = Aspect.AspectFill };
			avatarImage.SetBinding(Image.SourceProperty, "AvatarUrl");
			avatarImage.SetBinding(Image.IsVisibleProperty, "HasAvatar");

			var avatarBorder = new Border
			{
				WidthRequest = 44,
				HeightRequest = 44,
				StrokeThickness = 1,
				Stroke = Color.FromArgb("#C9D2FF"),
				BackgroundColor = Colors.White,
				StrokeShape = new Ellipse(),
				Content = new Grid { Children = { initialsLabel, avatarImage } }
			};

			var onlineDot = new Border
			{
				WidthRequest = 10,
				HeightRequest = 10,
				StrokeThickness = 1.5,
				Stroke = Colors.White,
				BackgroundColor = Color.FromArgb("#FFB800"),
				StrokeShape = new Ellipse(),
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.End
			};

			var avatarStack = new Grid
			{
				WidthRequest = 48,
				HeightRequest = 48,
				Children = { avatarBorder, onlineDot }
			};

			var nameLabel = new Label
			{
				FontSize = 15,
				VerticalOptions = LayoutOptions.Center,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			nameLabel.SetBinding(Label.TextProperty, "FullName");

			var row = new Grid
			{
				ColumnSpacing = 12,
				Padding = new Thickness(0, 10),
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			row.Add(avatarStack, 0, 0);
			row.Add(nameLabel, 1, 0);

			return row;
		}
	}
}