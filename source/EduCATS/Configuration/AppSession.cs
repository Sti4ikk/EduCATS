using System;
using System.Threading.Tasks;
using EduCATS.Constants;
using EduCATS.Data;
using EduCATS.Data.Caching;
using EduCATS.Data.Models;
using EduCATS.Data.User;
using EduCATS.Demo;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Json;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Services;

namespace EduCATS.Configuration
{
	/// <summary>
	/// Authorized user session.
	/// </summary>
	public static class AppSession
	{
		/// <summary>
		/// Server value of <see cref="UserProfileModel.UserType"/> for professors.
		/// </summary>
		const string _professorType = "1";

		/// <summary>
		/// Restore the session saved during the previous login.
		/// </summary>
		/// <remarks>
		/// Synchronous on purpose: user data must be in place before the
		/// main page (and its view models) is created. The profile is taken
		/// from cache here and refreshed by <see cref="RefreshProfileAndStartChat"/>.
		/// </remarks>
		/// <param name="services">Platform services.</param>
		/// <returns><c>true</c> if the user is logged in and the session is restored.</returns>
		public static bool TryRestore(IPlatformServices services)
		{
			try
			{
				var preferences = services.Preferences;

				if (!preferences.IsLoggedIn ||
					preferences.UserId <= 0 ||
					string.IsNullOrEmpty(preferences.UserLogin) ||
					string.IsNullOrEmpty(preferences.AccessToken))
				{
					return false;
				}

				AppUserData.SetLoginData(services, preferences.UserId, preferences.UserLogin);
				AppUserData.GroupId = preferences.GroupId;
				AppUserData.GroupName = preferences.GroupName;
				AppUserData.Avatar = preferences.Avatar;

				var cachedProfile = DataCaching<string>.Get(GlobalConsts.DataProfileKey);
				AppUserData.SetProfileData(
					services, JsonController<UserProfileModel>.ConvertJsonToObject(cachedProfile));

				return true;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}

		/// <summary>
		/// Refresh the profile of a restored session and connect to chat.
		/// </summary>
		/// <param name="services">Platform services.</param>
		/// <returns>Task.</returns>
		public static async Task RefreshProfileAndStartChat(IPlatformServices services)
		{
			try
			{
				var profile = await DataAccess.GetProfileInfo(services.Preferences.UserLogin);

				// Expired session is handled by the main page:
				// it shows the error and opens the login page.
				if (profile.IsSessionExpiredError)
				{
					return;
				}

				AppUserData.SetProfileData(services, profile.Data);
				StartChat(profile.Data);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Connect to chat and start listening for calls.
		/// </summary>
		/// <param name="profile">User profile.</param>
		public static void StartChat(UserProfileModel profile)
		{
			if (profile == null || AppUserData.UserId <= 0)
			{
				return;
			}

			// Subscribe to hub events before connecting.
			ChatPresenceService.Initialize();
			ChatUnreadService.Initialize();

			var role = profile.UserType == _professorType ? ChatRoles.Lecturer : ChatRoles.Student;
			_ = connectToChat(AppUserData.UserId, role);

			CallService.Initialize();
			CallNavigationService.Initialize();
		}

		/// <summary>
		/// The app returned from background: the chat connection may have
		/// dropped, counters and online statuses may be stale.
		/// </summary>
		/// <param name="services">Platform services.</param>
		/// <returns>Task.</returns>
		public static async Task OnAppResumed(IPlatformServices services)
		{
			try
			{
				if (AppUserData.UserId <= 0 || !services.Preferences.IsLoggedIn)
				{
					return;
				}

				// Reconnecting raises ChatHubService.ConnectionRestored:
				// the open conversation loads missed messages.
				await ChatHubService.EnsureConnectedAsync();
				await ChatUnreadService.RefreshAsync(services);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// End the session: leave the chat, delete user data,
		/// token and cache, and open the login page.
		/// </summary>
		/// <remarks>
		/// Used for logout, account deletion and expired session.
		/// </remarks>
		/// <param name="services">Platform services.</param>
		public static void Logout(IPlatformServices services)
		{
			_ = LeaveChat();
			ChatUnreadService.Clear();
			ChatPresenceService.Clear();
			ChatActivityService.Clear();
			ChatFileCache.Clear();
			AppDemo.Instance.IsDemoAccount = false;
			services.Preferences.ResetPrefs();
			AppUserData.Clear();
			DataAccess.ResetData();
			services.Navigation.OpenLogin();
		}

		/// <summary>
		/// Disconnect from chat.
		/// </summary>
		/// <returns>Task.</returns>
		public static async Task LeaveChat()
		{
			try
			{
				await ChatHubService.DisconnectAsync();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		static async Task connectToChat(int userId, string role)
		{
			try
			{
				// Counters for the chat tab title before the chat tab is opened.
				_ = ChatUnreadService.RefreshAsync(PlatformServices.Current);
				await ChatHubService.ConnectAndJoin(userId, role);
			}
			catch (Exception ex)
			{
				// Messages sending reconnects on demand, so it's not fatal.
				AppLogs.Log(ex);
			}
		}
	}
}
