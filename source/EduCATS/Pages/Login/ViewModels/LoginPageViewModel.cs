using EduCATS.Configuration;
using EduCATS.Data;
using EduCATS.Data.Models;
using EduCATS.Data.User;
using EduCATS.Demo;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Networking;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.Controls;
using Nyxbull.Plugins.CrossLocalization;
using System;
using System.Threading.Tasks;

namespace EduCATS.Pages.Login.ViewModels
{
	/// <summary>
	/// Login page ViewModel.
	/// </summary>
	public class LoginPageViewModel : ViewModel
	{
		/// <summary>
		/// Platform services.
		/// </summary>
		readonly IPlatformServices _services;

		/// <summary>
		/// Login page ViewModel constructor.
		/// </summary>
		public LoginPageViewModel(IPlatformServices services)
		{
			_services = services;
			IsLoadingCompleted = true;
			IsPasswordHidden = true;
		}

		bool _isLoading;

		/// <summary>
		/// Property for checking loading status.
		/// </summary>
		public bool IsLoading
		{
			get { return _isLoading; }
			set { SetProperty(ref _isLoading, value); }
		}

		bool _isLoadingCompleted;

		/// <summary>
		/// Property for checking loading status completion.
		/// </summary>
		public bool IsLoadingCompleted
		{
			get { return _isLoadingCompleted; }
			set { SetProperty(ref _isLoadingCompleted, value); }
		}

		string _username;

		/// <summary>
		/// Username property.
		/// </summary>
		public string Username
		{
			get { return _username; }
			set { SetProperty(ref _username, value); }
		}

		string _password;

		/// <summary>
		/// Password property.
		/// </summary>
		public string Password
		{
			get { return _password; }
			set { SetProperty(ref _password, value); }
		}

		bool _isPasswordHidden;

		/// <summary>
		/// Property for checking if password is hidden.
		/// </summary>
		public bool IsPasswordHidden
		{
			get { return _isPasswordHidden; }
			set { SetProperty(ref _isPasswordHidden, value); }
		}

		Command _loginCommand;

		/// <summary>
		/// Login command.
		/// </summary>
		public Command LoginCommand
		{
			get
			{
				return _loginCommand ?? (_loginCommand = new Command(async () => await startLogin()));
			}
		}

		Command _hidePasswordCommand;

		/// <summary>
		/// Hide password command.
		/// </summary>
		public Command HidePasswordCommand
		{
			get
			{
				return _hidePasswordCommand ?? (_hidePasswordCommand = new Command(hidePassword));
			}
		}

		Command _settingsCommand;

		/// <summary>
		/// Settigns command.
		/// </summary>
		public Command SettingsCommand
		{
			get
			{
				return _settingsCommand ?? (_settingsCommand = new Command(
					async () => await openSettings()));
			}
		}

		Command _parentalCommand;
		public Command ParentalCommand
		{
			get
			{
				return _parentalCommand ?? (_parentalCommand = new Command(
					async () => await openParental()));
			}
		}

		Command _registrationCommand;
		public Command RegistrationOpenCommand
		{
			get
			{
				return _registrationCommand ?? (_registrationCommand = new Command(
					async () => await openRegistration()));
			}
		}

		Command _forgotPasswordCommand;
		public Command ForgotPasswordCommand
		{
			get
			{
				return _forgotPasswordCommand ?? (_forgotPasswordCommand = new Command(
					async () => await openForgotPassword()));
			}
		}

		public async Task openForgotPassword()
		{
			if (_services.Device.CheckConnectivity())
			{
				await _services.Navigation.OpenForgotPassword(CrossLocalization.Translate("reset_password"));
			}
			else
			{
				_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
			}
		}

		protected async Task openRegistration()
		{
			if (_services.Device.CheckConnectivity())
			{
				await _services.Navigation.OpenRegistration(CrossLocalization.Translate("chek_In"));
			}
			else
			{
				_services.Dialogs.ShowError(CrossLocalization.Translate("base_connection_error"));
			}
		}

		protected async Task openParental()
		{
			await _services.Navigation.OpenFindGroup(CrossLocalization.Translate("parental_login"));
		}

		/// <summary>
		/// Authorization method.
		/// </summary>
		/// <returns>Task.</returns>
		protected async Task startLogin()
		{
			try
			{
				if (checkCredentials())
				{
					setLoading(true, CrossLocalization.Translate("login_loading"));
					var user = await loginRequest();
					await loginCompleted(user);
				}
				else
				{
					_services.Dialogs.ShowError(CrossLocalization.Translate("login_empty_credentials_error"));
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Is called on login completed.
		/// </summary>
		/// <param name="user">User model with error details.</param>
		/// <returns>Task.</returns>
		async Task loginCompleted(DataResult<UserModel> user)
		{
			setLoading(false);
			if (user.IsError)
			{
				_services.Dialogs.ShowError(user.ErrorMessage);
			}
			else if (string.IsNullOrWhiteSpace(user.Data?.Username))
			{
				_services.Dialogs.ShowError(CrossLocalization.Translate("base_something_went_wrong"));
			}
			else
			{
				setLoading(true, CrossLocalization.Translate("login_profile_loading"));
				var profile = await getProfileData(user.Data.Username);
				setLoading(false);
				profileRetrieved(profile);
			}
		}

		/// <summary>
		/// Is called after successful login.
		/// </summary>
		/// <param name="profileResult">Profile model with error details.</param>
		void profileRetrieved(DataResult<UserProfileModel> profileResult)
		{
			var profile = profileResult.Data;

			if (profile != null && !profileResult.IsError)
			{
				_services.Preferences.GroupId = profile.GroupId;
				_services.Preferences.IsLoggedIn = !AppDemo.Instance.IsDemoAccount;

				AppSession.StartChat(profile);
				_services.Navigation.OpenMain();
			}
			else if (profile != null && profileResult.IsError)
			{
				_services.Dialogs.ShowError(profileResult.ErrorMessage);
			}
			else
			{
				_services.Dialogs.ShowError(CrossLocalization.Translate("login_user_profile_error"));
			}
		}

		/// <summary>
		/// Hides or shows a password.
		/// </summary>
		protected void hidePassword()
		{
			IsPasswordHidden = !IsPasswordHidden;
		}

		protected async Task openSettings()
		{
			try
			{
				await _services.Navigation.OpenSettings(
					CrossLocalization.Translate("main_settings"));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Gets user data (username and user's id) by provided credentials and saves it.
		/// </summary>
		/// <returns>
		/// <see cref="UserModel"/> with error details of the failed request (if any).
		/// </returns>
		async Task<DataResult<UserModel>> loginRequest()
		{
			try
			{
				if (AppDemo.Instance.IsDemoAccount)
				{
					var userLogin = await DataAccess.Login(Username, Password);
					AppUserData.SetLoginData(_services, userLogin.Data.UserId, userLogin.Data.Username);
					return userLogin;
				}

				RequestController.ResetHttpClient();
				_services.Preferences.AccessToken = string.Empty;

				// Добавь логи

				var tokenResult = await DataAccess.GetToken(Username, Password);
				var tokenData = tokenResult.Data;


				if (tokenResult.IsError || tokenData == null || string.IsNullOrWhiteSpace(tokenData.Token))
				{
					AppLogs.Log("Token request failed", nameof(loginRequest));
					return tokenResult.Map(_ => new UserModel());
				}

				_services.Preferences.AccessToken = tokenData.Token;

				var accountResult = await DataAccess.GetAccountData();
				var accountData = accountResult.Data;


				if (accountResult.IsError || accountData == null || string.IsNullOrWhiteSpace(accountData.Username))
				{
					AppLogs.Log("Account data request failed", nameof(loginRequest));
					return accountResult.Map(_ => new UserModel());
				}

				AppUserData.SetLoginData(_services, accountData.Id, accountData.Username);
				return new DataResult<UserModel>(new UserModel
				{
					UserId = accountData.Id,
					Username = accountData.Username
				});
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets profile data by username and user's ID and saves it.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <returns>Task.</returns>
		async Task<DataResult<UserProfileModel>> getProfileData(string username)
		{
			var userProfile = await DataAccess.GetProfileInfo(username);
			AppUserData.SetProfileData(_services, userProfile.Data);
			return userProfile;
		}

		/// <summary>
		/// Validates credentials.
		/// </summary>
		/// <returns><code>true</code> on success, <code>false</code> otherwise.</returns>
		bool checkCredentials()
		{
			if (string.IsNullOrEmpty(Username) ||
				string.IsNullOrEmpty(Password))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Sets loading status.
		/// </summary>
		/// <param name="isLoading">Is loading status.</param>
		/// <param name="message">Message to show.</param>
		void setLoading(bool isLoading, string message = null)
		{
			if (isLoading)
			{
				_services.Dialogs.ShowLoading(message);
			}
			else
			{
				_services.Dialogs.HideLoading();
			}
		}
	}
}

