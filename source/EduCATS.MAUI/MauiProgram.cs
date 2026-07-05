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

#if ANDROID
			Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(IEntry.Background), (handler, view) =>
			{
				var color = handler.PlatformView.CurrentTextColor; // не используем, просто для примера
				var drawable = new Android.Graphics.Drawables.GradientDrawable();
				drawable.SetShape(Android.Graphics.Drawables.ShapeType.Rectangle);
				drawable.SetCornerRadius(20); // радиус скругления углов, поставьте 0 для прямых углов
				drawable.SetColor(Android.Graphics.Color.ParseColor(EduCATS.Themes.Theme.Current.LoginEntryBackgroundColor));

				handler.PlatformView.Background = drawable;
				handler.PlatformView.BackgroundTintList = null;
			});
#endif
#if DEBUG
			builder.Logging.AddDebug();
#endif
			return builder.Build();
		}
	}
}