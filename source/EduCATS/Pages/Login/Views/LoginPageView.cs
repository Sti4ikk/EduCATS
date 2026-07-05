using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Login.ViewModels;
using EduCATS.Themes;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Collections.Generic;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;


namespace EduCATS.Pages.Login.Views
{
	public class LoginPageView : ContentPage
	{
		readonly string[] _backgrounds = {
			Theme.Current.LoginBackground1Image,
			Theme.Current.LoginBackground2Image,
			Theme.Current.LoginBackground3Image
		};

		const double _controlHeight = 50;
		const double _mascotImageHeight = 200;
		const double _mascotTailPadding = 30;
		const double _loginFormSpacing = 0;
		const double _settingsIconSize = 45;
		const double _showPasswordIconSize = 30;
		const double _mascotTailAnimationRotation = 35;
		const uint _mascotTailAnimationTime = 2000;

		static Thickness _loginFormPadding = new(20, 0);
		static Thickness _baseSpacing = new(0, 10, 0, 0);
		static Thickness _iosSettingsMargin = new(20, 60);
		static Thickness _androidSettingsMargin = new(30);
		static Thickness _showPasswordIconMargin = new(0, 10, 5, 0);

		Image _mascotTailImage;
		bool _isTailAnimationRunning;

		public LoginPageView()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			BindingContext = new LoginPageViewModel(new PlatformServices());
			createViews();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			animateMascotTail();
		}

		void createViews()
		{
			var backgroundImage = createBackgroundImage();
			var settingsIcon = createSettingsIcon();
			var mainLayout = createLoginForm();

			var scrollView = new ScrollView
			{
				VerticalOptions = LayoutOptions.Fill,
				Content = new Grid
				{
					Children = { mainLayout }
				}
			};

			Content = new Grid
			{
				Children = {
			backgroundImage,
			scrollView,
			settingsIcon
		}
			};
		}

		StackLayout createLoginForm()
		{
			var mascotImage = createMascotImage();
			var entryStyle = getEntryStyle();
			var usernameEntry = createUsernameEntry(entryStyle);
			var passwordEntryGrid = createPasswordGrid(entryStyle);
			var loginButton = createLoginButton();
			var parentalButton = createParentalButton();
			var forgotPasswordButton = createForgotPasswordButton();
			var activityIndicator = createActivityIndicator();
			var chekInGrid = createCheckInGrid();

			var mainStackLayout = new StackLayout
			{
				Spacing = _loginFormSpacing,
				Padding = _loginFormPadding,
				VerticalOptions = LayoutOptions.Center,
				Children = {
					mascotImage,
					usernameEntry,
					passwordEntryGrid,
					forgotPasswordButton,
					loginButton,
					parentalButton,
					chekInGrid,
					activityIndicator,

				}
			};

			mainStackLayout.SetBinding(IsEnabledProperty, "IsLoadingCompleted");
			return mainStackLayout;
		}

		Grid createCheckInGrid()
		{
			var chekInButton = createChekInButton();
			var chekInLabel = createCheckInLabel();
			var gridPanel = new Grid
			{
				RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
				ColumnDefinitions = {
			new ColumnDefinition { Width = GridLength.Auto },
			new ColumnDefinition { Width = GridLength.Auto } // ← добавили вторую колонку
		},
				ColumnSpacing = 5,
				HorizontalOptions = LayoutOptions.Center // по эталону надпись с ссылкой стоят по центру внизу
			};

			Grid.SetColumn(chekInLabel, 0);
			Grid.SetRow(chekInLabel, 0);
			gridPanel.Children.Add(chekInLabel);

			Grid.SetColumn(chekInButton, 1);
			Grid.SetRow(chekInButton, 0);
			gridPanel.Children.Add(chekInButton);

			return gridPanel;
		}

		Label createCheckInLabel()
		{
			var new_user = new Label
			{
				Text = CrossLocalization.Translate("new_user_check_in"),
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Start,
				FontSize = 18,
			};
			return new_user;
		}

		Image createBackgroundImage()
		{
			return new Image
			{
				Aspect = Aspect.AspectFill,
				Source = ImageSource.FromFile(getRandomBackgroundImage())
			};
		}

		Image createSettingsIcon()
		{
			var settingsIcon = new Image
			{
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Start,
				Margin = DeviceInfo.Platform == DevicePlatform.iOS ? _iosSettingsMargin : _androidSettingsMargin,
				Source = ImageSource.FromFile(Theme.Current.MainSettingsIcon),
				Aspect = Aspect.AspectFill,
				HeightRequest = _settingsIconSize,
				WidthRequest = _settingsIconSize,
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "SettingsCommand");
			settingsIcon.GestureRecognizers.Add(tapGestureRecognizer);
			return settingsIcon;
		}

		RelativeLayout createMascotImage()
		{
			var relativeLayout = new RelativeLayout();

			_mascotTailImage = new Image
			{
				HeightRequest = _mascotImageHeight,
				Source = ImageSource.FromFile(Theme.Current.LoginMascotTailImage)
			};

			var mascotImage = new Image
			{
				HeightRequest = _mascotImageHeight,
				Source = ImageSource.FromFile(Theme.Current.LoginMascotImage)
			};

			mascotImage.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() => animateMascotTail())
			});

			relativeLayout.Children.Add(
				_mascotTailImage,
				Constraint.RelativeToView(mascotImage, (parent, sibling) => parent.Width / 2 + _mascotTailPadding),
				Constraint.Constant(0));

			relativeLayout.Children.Add(
				mascotImage,
				Constraint.RelativeToParent(parent => {
					var width = mascotImage?.Measure(parent.Width, parent.Height).Width ?? -1;
					return parent.Width / 2 - width / 2;
				}),
				Constraint.Constant(0));

			return relativeLayout;
		}

		Entry createUsernameEntry(Style style)
		{
			var username = new Entry
			{
				Style = style,
				ReturnType = ReturnType.Next,
				Placeholder = CrossLocalization.Translate("login_username")
			};

			username.SetBinding(Entry.TextProperty, "Username");
			return username;
		}

		Grid createPasswordGrid(Style style)
		{
			var passwordEntry = createPasswordEntry(style);
			var showPasswordImage = createShowPasswordImage();

			return new Grid
			{
				Children = {
					passwordEntry,
					showPasswordImage
				}
			};
		}

		Entry createPasswordEntry(Style style)
		{
			var password = new Entry
			{
				Style = style,
				IsPassword = true,
				ReturnType = ReturnType.Done,
				Margin = _baseSpacing,
				Placeholder = CrossLocalization.Translate("login_password")
			};

			password.SetBinding(Entry.TextProperty, "Password");
			password.SetBinding(Entry.IsPasswordProperty, "IsPasswordHidden");
			return password;
		}

		Button createParentalButton()
		{
			var parentalButton = new Button
			{
				Text = CrossLocalization.Translate("parental_login"),
				FontAttributes = FontAttributes.None,
				TextTransform = TextTransform.None,
				TextColor = Colors.White,
				BackgroundColor = Colors.Transparent,
				BorderColor = Colors.White,
				BorderWidth = 1,
				Margin = _baseSpacing,
				HeightRequest = _controlHeight,
				Style = AppStyles.GetButtonStyle(bold: true)
			};

			parentalButton.SetBinding(Button.CommandProperty, "ParentalCommand");
			return parentalButton;
		}

		Label createChekInButton()
		{
			var chekInButton = new Label
			{
				Text = CrossLocalization.Translate("chek_In"),
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Start,
				FontSize = 18,
				Padding = new Thickness(0, 15, 0, 15),
				TextDecorations = TextDecorations.Underline
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "RegistrationOpenCommand");
			chekInButton.GestureRecognizers.Add(tapGestureRecognizer);
			return chekInButton;
		}

		Label createForgotPasswordButton()
		{
			var ForgotPasswordButton = new Label
			{
				Text = CrossLocalization.Translate("forgot_password"),
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.End,
				FontSize = 18,
				Padding = new Thickness(0, 10, 0, 15),
				TextDecorations = TextDecorations.Underline
			};

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "ForgotPasswordCommand");
			ForgotPasswordButton.GestureRecognizers.Add(tapGestureRecognizer);
			return ForgotPasswordButton;
		}


		Button createLoginButton()
		{
			var loginButton = new Button
			{
				Text = CrossLocalization.Translate("login_text"),
				FontAttributes = FontAttributes.None,
				TextTransform = TextTransform.None,
				TextColor = Color.FromArgb(Theme.Current.LoginButtonTextColor),
				BackgroundColor = Color.FromArgb(Theme.Current.LoginButtonBackgroundColor),
				Margin = _baseSpacing,
				HeightRequest = _controlHeight,
				Style = AppStyles.GetButtonStyle(bold: true)
			};

			loginButton.SetBinding(Button.CommandProperty, "LoginCommand");
			return loginButton;
		}

		Image createShowPasswordImage()
		{
			var showPasswordImage = new Image
			{
				HeightRequest = _showPasswordIconSize,
				Aspect = Aspect.AspectFit,
				Margin = _showPasswordIconMargin,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center,
				Source = ImageSource.FromFile(Theme.Current.LoginShowPasswordImage)
			};

			var showPasswordTapGesture = new TapGestureRecognizer();
			showPasswordTapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "HidePasswordCommand");
			showPasswordImage.GestureRecognizers.Add(showPasswordTapGesture);
			return showPasswordImage;
		}

		ActivityIndicator createActivityIndicator()
		{
			var activityIndicator = new ActivityIndicator
			{
				HorizontalOptions = LayoutOptions.Center,
				Margin = _baseSpacing,
				Color = Colors.White
			};

			activityIndicator.SetBinding(IsVisibleProperty, "IsLoading");
			activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");
			return activityIndicator;
		}

		Style getEntryStyle()
		{
			var style = AppStyles.GetEntryStyle();

			style.Setters.Add(new Setter
			{
				Property = HeightRequestProperty,
				Value = _controlHeight
			});

			style.Setters.Add(new Setter
			{
				Property = BackgroundColorProperty,
				Value = Theme.Current.LoginEntryBackgroundColor
			});

			return style;
		}

		string getRandomBackgroundImage()
		{
			var random = new Random();
			var randomBackgroundIndex = random.Next(0, _backgrounds.Length);
			return _backgrounds[randomBackgroundIndex];
		}

		void animateMascotTail()
		{
			MainThread.BeginInvokeOnMainThread(async () => {
				if (_isTailAnimationRunning)
				{
					return;
				}

				_isTailAnimationRunning = true;
				_mascotTailImage.AnchorX = 0;
				await _mascotTailImage.RotateToAsync(_mascotTailAnimationRotation, _mascotTailAnimationTime);
				await _mascotTailImage.RotateToAsync(0, _mascotTailAnimationTime);
				_isTailAnimationRunning = false;
			});
		}
	}
}