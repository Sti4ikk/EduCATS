using EduCATS.Fonts;
using EduCATS.Helpers.Logs;
using EduCATS.Networking;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;


namespace EduCATS.Helpers.Forms.Settings
{
	/// <summary>
	/// Application preferences (settings/saved variables).
	/// </summary>
	public class AppPrefs : IPreferences
	{
		/// <summary>
		/// Language code key.
		/// </summary>
		const string _languageCodeKey = "APP_LANG_CODE";

		/// <summary>
		/// Default language code.
		/// </summary>
		readonly string _languageCodeDefault = Languages.SYSTEM.LangCode;

		/// <summary>
		/// Language code.
		/// </summary>
		public string LanguageCode {
			get => Preferences.Get(_languageCodeKey, _languageCodeDefault);
			set => Preferences.Set(_languageCodeKey, value);
		}

		/// <summary>
		/// Theme key.
		/// </summary>
		const string _themeKey = "APP_THEME";

		/// <summary>
		/// Default theme.
		/// </summary>
		const string _themeDefault = Themes.AppTheme.ThemeSystem;

		/// <summary>
		/// Theme.
		/// </summary>
		public string Theme {
			get => Preferences.Get(_themeKey, _themeDefault);
			set => Preferences.Set(_themeKey, value);
		}

		/// <summary>
		/// Server key.
		/// </summary>
		const string _serverKey = "APP_SERVER";

		/// <summary>
		/// Default server.
		/// </summary>
		const string _serverDefault = Servers.EduCatsBntuAddress;

		/// <summary>
		/// Current server.
		/// </summary>
		public string Server {
			get => Preferences.Get(_serverKey, _serverDefault);
			set => Preferences.Set(_serverKey, value);
		}

		/// <summary>
		/// Is authorized key.
		/// </summary>
		const string _isLoggedInKey = "IS_LOGGED_IN";

		/// <summary>
		/// Default authorized value.
		/// </summary>
		const bool _isLoggedInDefault = false;

		/// <summary>
		/// Is authorized.
		/// </summary>
		public bool IsLoggedIn {
			get => Preferences.Get(_isLoggedInKey, _isLoggedInDefault);
			set => Preferences.Set(_isLoggedInKey, value);
		}

		/// <summary>
		/// Username key.
		/// </summary>
		const string _userLoginKey = "USER_LOGIN";

		/// <summary>
		/// Default username.
		/// </summary>
		const string _userLoginDefault = null;

		/// <summary>
		/// Username.
		/// </summary>
		public string UserLogin {
			get => Preferences.Get(_userLoginKey, _userLoginDefault);
			set => Preferences.Set(_userLoginKey, value);
		}

		/// <summary>
		/// User ID key.
		/// </summary>
		const string _userIdKey = "USER_ID";

		/// <summary>
		/// Default user ID.
		/// </summary>
		const int _userIdDefault = 0;

		/// <summary>
		/// User ID.
		/// </summary>
		public int UserId {
			get => Preferences.Get(_userIdKey, _userIdDefault);
			set => Preferences.Set(_userIdKey, value);
		}

		/// <summary>
		/// Chosen subject ID key.
		/// </summary>
		const string _chosenSubjectIdKey = "CHOSEN_SUBJECT_ID";

		/// <summary>
		/// Default chosen ID.
		/// </summary>
		readonly int _chosenSubjectIdDefault = 0;

		/// <summary>
		/// Chosen subject ID.
		/// </summary>
		public int ChosenSubjectId {
			get => Preferences.Get(_chosenSubjectIdKey, _chosenSubjectIdDefault);
			set => Preferences.Set(_chosenSubjectIdKey, value);
		}

		/// <summary>
		/// Group ID key.
		/// </summary>
		const string _groupIdKey = "USER_GROUP_ID";

		/// <summary>
		/// Default group ID.
		/// </summary>
		readonly int _groupIdDefault = -1;

		/// <summary>
		/// Group ID.
		/// </summary>
		public int GroupId {
			get => Preferences.Get(_groupIdKey, _groupIdDefault);
			set => Preferences.Set(_groupIdKey, value);
		}

		/// <summary>
		/// Group name key.
		/// </summary>
		const string _groupNameKey = "USER_GROUP_NAME";

		/// <summary>
		/// Default group name.
		/// </summary>
		readonly string _groupNameDefault = "";

		/// <summary>
		/// Group name.
		/// </summary>
		public string GroupName {
			get => Preferences.Get(_groupNameKey, _groupNameDefault);
			set => Preferences.Set(_groupNameKey, value);
		}

		/// <summary>
		/// Group avatar key.
		/// </summary>
		const string _avatarKey = "USER_AVATAR";

		/// <summary>
		/// Default avatar string.
		/// </summary>
		readonly String _avatarDefault = "";

		/// <summary>
		/// Avatar.
		/// </summary>
		public String Avatar {
			get => Preferences.Get(_avatarKey, _avatarDefault);
			set => Preferences.Set(_avatarKey, value);
		}

		/// <summary>
		/// Chosen group ID key.
		/// </summary>
		const string _chosenGroupIdKey = "CHOSEN_GROUP_ID";

		/// <summary>
		/// Default chosen group ID.
		/// </summary>
		readonly int _chosenGroupIdDefault = 0;

		/// <summary>
		/// Chosen group ID.
		/// </summary>
		public int ChosenGroupId {
			get => Preferences.Get(_chosenGroupIdKey, _chosenGroupIdDefault);
			set => Preferences.Set(_chosenGroupIdKey, value);
		}

		/// <summary>
		/// Font key.
		/// </summary>
		const string _fontKey = "FONT_KEY";

		/// <summary>
		/// Default font.
		/// </summary>
		readonly string _fontDefault = FontsController.DefaultFont;

		/// <summary>
		/// Font.
		/// </summary>
		public string Font {
			get => Preferences.Get(_fontKey, _fontDefault);
			set => Preferences.Set(_fontKey, value);
		}

		/// <summary>
		/// Is large font key.
		/// </summary>
		const string _isLargeFontKey = "IS_LARGE_FONT";

		/// <summary>
		/// Is large font default.
		/// </summary>
		readonly bool _isLargeFontDefault = false;

		/// <summary>
		/// Is large font.
		/// </summary>
		public bool IsLargeFont {
			get => Preferences.Get(_isLargeFontKey, _isLargeFontDefault);
			set => Preferences.Set(_isLargeFontKey, value);
		}

		/// <summary>
		/// Access token key in secure storage.
		/// </summary>
		const string _accessTokenKey = "APP_ACCESS_TOKEN";

		/// <summary>
		/// Legacy key the token used to be stored under in plain preferences.
		/// </summary>
		const string _legacyAccessTokenKey = "";

		/// <summary>
		/// Access token cache sync root.
		/// </summary>
		static readonly object _accessTokenSync = new object();

		/// <summary>
		/// Access token cached in memory.
		/// </summary>
		/// <remarks>
		/// Every network request reads the token synchronously,
		/// so secure storage is only read once.
		/// </remarks>
		static string _accessToken;

		/// <summary>
		/// Is <see cref="_accessToken"/> loaded from secure storage.
		/// </summary>
		static bool _isAccessTokenLoaded;

		/// <summary>
		/// Access token.
		/// </summary>
		/// <remarks>
		/// Persisted in <see cref="SecureStorage"/> (Keychain / Android Keystore),
		/// not in plain preferences.
		/// </remarks>
		public string AccessToken {
			get {
				lock (_accessTokenSync) {
					if (!_isAccessTokenLoaded) {
						_accessToken = loadAccessToken();
						_isAccessTokenLoaded = true;
					}

					return _accessToken ?? string.Empty;
				}
			}
			set {
				lock (_accessTokenSync) {
					_accessToken = value;
					_isAccessTokenLoaded = true;
				}

				saveAccessToken(value);
			}
		}

		/// <summary>
		/// Load access token from secure storage.
		/// </summary>
		/// <remarks>
		/// Moves the token from the legacy plain preferences key if needed.
		/// </remarks>
		/// <returns>Access token or <c>null</c>.</returns>
		static string loadAccessToken()
		{
			try {
				if (Preferences.ContainsKey(_legacyAccessTokenKey)) {
					var legacyToken = Preferences.Get(_legacyAccessTokenKey, string.Empty);
					Preferences.Remove(_legacyAccessTokenKey);

					if (!string.IsNullOrEmpty(legacyToken)) {
						saveAccessToken(legacyToken);
						return legacyToken;
					}
				}

				// Task.Run: never block on the platform call from the UI thread's context.
				return Task.Run(() => SecureStorage.Default.GetAsync(_accessTokenKey))
					.GetAwaiter().GetResult();
			} catch (Exception ex) {
				// Secure storage can become unreadable (e.g. restored backup,
				// changed device lock). Drop the token - the user will log in again.
				AppLogs.Log(ex);
				SecureStorage.Default.Remove(_accessTokenKey);
				return null;
			}
		}

		/// <summary>
		/// Save access token to secure storage (or remove it if empty).
		/// </summary>
		/// <param name="token">Access token.</param>
		static void saveAccessToken(string token)
		{
			// Written in the background (Keystore encryption is slow, and the
			// token is set from the UI thread on login); the value in memory
			// is used meanwhile. Writes are queued to keep their order.
			lock (_accessTokenSync) {
				_accessTokenWrite = _accessTokenWrite.ContinueWith(
					_ => writeAccessToken(token),
					CancellationToken.None,
					TaskContinuationOptions.None,
					TaskScheduler.Default);
			}
		}

		/// <summary>
		/// The last queued secure storage write.
		/// </summary>
		static Task _accessTokenWrite = Task.CompletedTask;

		static void writeAccessToken(string token)
		{
			try {
				if (string.IsNullOrEmpty(token)) {
					SecureStorage.Default.Remove(_accessTokenKey);
					return;
				}

				SecureStorage.Default.SetAsync(_accessTokenKey, token).GetAwaiter().GetResult();
			} catch (Exception ex) {
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Delete all preferences and the access token
		/// except Font, Theme and Server.
		/// </summary>
		public void ResetPrefs()
		{
			var fontPrefs = Font;
			var themePrefs = Theme;
			var isLargeFont = IsLargeFont;
			var server = Server;

			Preferences.Clear();
			AccessToken = string.Empty;

			Font = fontPrefs;
			Theme = themePrefs;
			IsLargeFont = isLargeFont;
			Server = server;
		}
	}
}

