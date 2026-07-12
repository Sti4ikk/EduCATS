using EduCATS.Fonts;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Networking;
using EduCATS.Pages.Login.Views;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.MAUI
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				var ex = e.ExceptionObject as Exception;
				System.Diagnostics.Debug.WriteLine($"=== UNHANDLED: {ex}");
			};

			TaskScheduler.UnobservedTaskException += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine($"=== TASK UNHANDLED: {e.Exception}");
				e.SetObserved();
			};

			initialize();

			_ = MathJaxCache.LoadAsync();

			MainPage = new NavigationPage(new LoginPageView());
		}

		void initialize()
		{
			var assembly = typeof(EduCATS.Constants.GlobalConsts).Assembly;
			CrossLocalization.Initialize(
				assembly,
				"EduCATS",
				"Localization"
			);
			var services = new PlatformServices();
			EduCATS.Configuration.AppConfig.InitialSetup(services);
			FontsController.Initialize(services);
		}
	}
}