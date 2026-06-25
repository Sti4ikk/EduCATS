using Controls.UserDialogs.Maui;
using EduCATS.Helpers.Forms.Effects;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace EduCATS.MAUI
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseUserDialogs()
				.UseMauiCompatibility()
				.ConfigureSyncfusionCore()
				.ConfigureEffects(effects =>
				{
#if ANDROID
					effects.Add<DisabledShiftEffect, EduCATS.MAUI.Platforms.Android.DisabledShiftEffect>();
#endif
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif
			return builder.Build();
		}
	}
}