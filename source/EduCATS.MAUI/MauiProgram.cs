using Controls.UserDialogs.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Syncfusion.Maui.Core.Hosting;
using EduCATS.Helpers.Forms.Effects;

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