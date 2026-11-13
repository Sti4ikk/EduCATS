using EduCATS.Configuration;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Networking;
using EduCATS.Pages.Login.Views;

namespace EduCATS.MAUI
{
	public partial class App : Application
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="services">Platform services from the DI container (MauiProgram).</param>
		public App(IPlatformServices services)
		{
			InitializeComponent();

			_services = services;
			initialize(_services);

			// Crashes go to the app log (sent to Sentry too if it's configured).
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				logCrash(e.ExceptionObject as Exception, "Unhandled");

			TaskScheduler.UnobservedTaskException += (sender, e) =>
			{
				logCrash(e.Exception, "UnobservedTask");
				e.SetObserved();
			};

			_ = MathJaxCache.LoadAsync();

			_isSessionRestored = AppSession.TryRestore(_services);
			MainPage = createStartPage();
			RequestedThemeChanged += onRequestedThemeChanged;
		}

		readonly IPlatformServices _services;

		static void logCrash(Exception exception, string source)
		{
			try
			{
				EduCATS.Helpers.Logs.AppLogs.Log(exception, source);
				// The app may be terminating: write the log right now.
				EduCATS.Helpers.Logs.AppLogs.Flush();
			}
			catch
			{
				// Logging must never crash the crash handler.
			}
		}

		/// <summary>
		/// Is the previous session restored (autologin) and the main page
		/// still has to be opened.
		/// </summary>
		bool _isSessionRestored;

		/// <summary>
		/// The system switched light/dark mode.
		/// </summary>
		void onRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			// iOS switches light/dark and back while the app goes to background
			// (snapshots for the app switcher): pages must not be rebuilt for that.
			// A real change is applied on resume.
			if (_isInBackground)
			{
				return;
			}

			applySystemTheme();
		}

		/// <summary>
		/// Is the app in background (between sleep and resume).
		/// </summary>
		bool _isInBackground;

		/// <summary>
		/// Apply the system light/dark mode if the theme follows the system.
		/// </summary>
		void applySystemTheme()
		{
			if (_services.Preferences.Theme != EduCATS.Themes.AppTheme.ThemeSystem)
			{
				return;
			}

			var wasDark = EduCATS.Themes.AppTheme.IsDarkApplied;
			new EduCATS.Themes.AppTheme(_services).SetCurrentTheme();

			if (wasDark == EduCATS.Themes.AppTheme.IsDarkApplied)
			{
				return;
			}

			// Theme colors are applied when pages are built (same as the theme settings page).
			if (_services.Preferences.IsLoggedIn)
			{
				_services.Navigation.OpenMain();
			}
			else
			{
				_services.Navigation.OpenLogin();
			}
		}

		protected override void OnSleep()
		{
			base.OnSleep();
			_isInBackground = true;

			// The system may kill the app in the background.
			EduCATS.Helpers.Logs.AppLogs.Flush();
		}

		protected override void OnResume()
		{
			base.OnResume();
			_isInBackground = false;

			// The system theme may have changed while the app was in background.
			try
			{
				applySystemTheme();
			}
			catch (Exception ex)
			{
				EduCATS.Helpers.Logs.AppLogs.Log(ex);
			}

			_ = AppSession.OnAppResumed(_services);
		}

		protected override void OnStart()
		{
			base.OnStart();

			if (!_isSessionRestored)
			{
				return;
			}

			// Once only: the user may log out before the window is recreated.
			_isSessionRestored = false;

			// The main page is created only when the window is up and running:
			// its view models show loading dialogs right from their constructors.
			Dispatcher.Dispatch(() =>
			{
				_services.Navigation.OpenMain();
				_ = AppSession.RefreshProfileAndStartChat(_services);
			});
		}

		void initialize(IPlatformServices services)
		{
			// Localization, logs, cache, theme and fonts (each set up once:
			// localization files used to be parsed twice on start).
			EduCATS.Configuration.AppConfig.InitialSetup(services);
		}

		/// <summary>
		/// Login page, or an empty placeholder if the session is restored
		/// (replaced by the main page in <see cref="OnStart"/>).
		/// </summary>
		/// <returns>Start page.</returns>
		Page createStartPage()
		{
			if (!_isSessionRestored)
			{
				return new NavigationPage(new LoginPageView());
			}

			return new ContentPage
			{
				BackgroundColor = Color.FromArgb(EduCATS.Themes.Theme.Current.AppBackgroundColor)
			};
		}
	}
}