using System;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Themes.Interfaces;
using EduCATS.Themes.Templates;
using Microsoft.Maui.ApplicationModel;

namespace EduCATS.Themes
{
	/// <summary>
	/// Application theme helper.
	/// </summary>
	public class AppTheme
	{
		/// <summary>
		/// Dark theme key.
		/// </summary>
		public const string ThemeDark = "THEME_DARK";

		/// <summary>
		/// Default (light) theme key.
		/// </summary>
		public const string ThemeDefault = "THEME_DEFAULT";

		/// <summary>
		/// Theme following the system light/dark setting.
		/// </summary>
		public const string ThemeSystem = "THEME_SYSTEM";

		/// <summary>
		/// Platform services.
		/// </summary>
		readonly IPlatformServices _services;

		/// <summary>
		/// Current theme.
		/// </summary>
		static ITheme _currentTheme = new DefaultTheme();

		public AppTheme(IPlatformServices services)
		{
			_services = services;
		}

		/// <summary>
		/// Is the dark theme in use now.
		/// </summary>
		public static bool IsDarkApplied => _currentTheme is DarkTheme;

		/// <summary>
		/// Set current theme with application <see cref="AppPrefs"/>.
		/// </summary>
		public void SetCurrentTheme() => SetTheme(_services.Preferences.Theme, true);

		/// <summary>
		/// Set theme with theme key.
		/// </summary>
		/// <param name="theme">Theme key.</param>
		public void SetTheme(string theme, bool fromPrefs = false)
		{
			_currentTheme = ShouldUseDark(theme, isSystemDark()) ? new DarkTheme() : new DefaultTheme();

			Theme.Set(_services, _currentTheme);

			if (!fromPrefs) {
				_services.Preferences.Theme = theme;
			}
		}

		/// <summary>
		/// Should the dark theme be used.
		/// </summary>
		/// <param name="theme">Theme key.</param>
		/// <param name="isSystemDark">Is the system in dark mode.</param>
		/// <returns><c>true</c> for the dark theme.</returns>
		public static bool ShouldUseDark(string theme, bool isSystemDark) =>
			theme == ThemeDark || (theme == ThemeSystem && isSystemDark);

		static bool isSystemDark()
		{
			try
			{
				return AppInfo.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}
	}
}
