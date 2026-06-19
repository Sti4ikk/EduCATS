using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportEffect(typeof(EduCATS.MAUI.Platforms.Android.DisabledShiftEffect), "DisabledShiftEffect")]

namespace EduCATS.MAUI.Platforms.Android
{
	public class DisabledShiftEffect : PlatformEffect
	{
		protected override void OnAttached() { }
		protected override void OnDetached() { }
	}
}