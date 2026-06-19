using EduCATS.Controls.RoundedListView;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Settings.Base.ViewModels;
using EduCATS.Pages.Settings.Profile.Views;
using EduCATS.Pages.Settings.Views.Base.ViewCells;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;


namespace EduCATS.Pages.Settings.Base.Views
{
	public class SettingsPageView : ContentPage
	{
		const double _avatarHeight = 60;
		const double _userLayoutSpacing = 15;
		const double _forwardIcon = 20;
		static Thickness _listMargin = new(10, 0, 10, 0);
		static Thickness _userFrameMargin = new(0, 0, 0, 10);
		const double _buttonHeight = 10;
		readonly IPlatformServices _services;

		public SettingsPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			_services = new PlatformServices();
			BindingContext = new SettingsPageViewModel(_services);

			createViews();
		}

		void createViews()
		{
			var userLayout = createUserLayout();
			var settingsListView = createList(userLayout);

			Content = new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Children = {
					settingsListView
				}
			};
		}

		Frame createUserLayout()
		{
			var avatar = createAvatar();
			var userLabel = createUserLabel();

			var userRoleLabel = createSubtitleLabel();
			userRoleLabel.SetBinding(Label.TextProperty, "Role");

			var userGroupLabel = createSubtitleLabel();
			userGroupLabel.SetBinding(Label.TextProperty, "Group");

			var forwardIcon = new Image
			{
				HeightRequest = _forwardIcon,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.BaseArrowForwardIcon)
			};

			var usernameLayout = new StackLayout
			{
				Children = {
					userLabel,
					userRoleLabel,
					userGroupLabel,
				}
			};

			var userLayout = new StackLayout
			{
				Spacing = _userLayoutSpacing,
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill,
				Children = {
					avatar,
					usernameLayout,
					forwardIcon,
				}
			};

			var userFrame = new Frame
			{
				HasShadow = false,
				Margin = _userFrameMargin,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = userLayout
			};

			var profileTitle = CrossLocalization.Translate("settings_about_profile");
			userFrame.SetBinding(IsVisibleProperty, "IsLoggedIn");
			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += async (s, e) =>
			{
				await _services.Navigation.OpenProfileAbout(profileTitle);
			};
			userFrame.GestureRecognizers.Add(tapGestureRecognizer);

			return userFrame;
		}

		Image createAvatar()
		{
			var avatarImage = new Image
			{
				HeightRequest = _avatarHeight,
				WidthRequest = _avatarHeight,
				VerticalOptions = LayoutOptions.Center,
				Aspect = Aspect.AspectFill,
				Clip = new EllipseGeometry
				{
					Center = new Point(_avatarHeight / 2, _avatarHeight / 2),
					RadiusX = _avatarHeight / 2,
					RadiusY = _avatarHeight / 2
				}
			};

			avatarImage.SetBinding(Image.SourceProperty, "Avatar",
				converter: new Base64ToImageSourceConverter());
			return avatarImage;
		}

		Label createUserLabel()
		{
			var userLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.SettingsTitleColor),
				VerticalOptions = LayoutOptions.Center,
				Style = AppStyles.GetLabelStyle()
			};

			userLabel.SetBinding(Label.TextProperty, "Username");
			return userLabel;
		}

		Label createSubtitleLabel()
		{
			return new Label
			{
				TextColor = Color.FromArgb(Theme.Current.SettingsGroupUserColor),
				Style = AppStyles.GetLabelStyle()
			};
		}

		RoundedListView createList(View header)
		{
			var settingsListView = new RoundedListView(
				typeof(SettingsPageViewCell),
				header: header,
				headerTopPadding: 10,
				footerBottomPadding: 10)
			{
				Margin = _listMargin
			};

			settingsListView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;
			settingsListView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			settingsListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "SettingsList");
			return settingsListView;
		}
	}
}