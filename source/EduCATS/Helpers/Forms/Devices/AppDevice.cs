using EduCATS.Helpers.Forms.Speech;
using EduCATS.Helpers.Logs;
using EduCATS.Themes;
using EduCATS.Themes.DependencyServices.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EduCATS.Helpers.Forms.Devices
{
	public class AppDevice : IDevice
	{
		private CancellationTokenSource _speechCancellationSource;

		public async Task OpenUri(string url)
		{
			await Browser.OpenAsync(url, new BrowserLaunchOptions
			{
				LaunchMode = BrowserLaunchMode.SystemPreferred,
				TitleMode = BrowserTitleMode.Show,
				PreferredToolbarColor = Color.FromArgb(Theme.Current.AppNavigationBarBackgroundColor),
				PreferredControlColor = Color.FromArgb(Theme.Current.BaseAppColor)
			});
		}

		public string GetAppDataDirectory() =>
			FileSystem.AppDataDirectory;

		public void MainThread(Action action)
		{
			Application.Current?.Dispatcher.Dispatch(action);
		}

		public void SetTimer(TimeSpan interval, Func<bool> callback)
		{
			Application.Current?.Dispatcher.StartTimer(interval, callback);
		}

		public async Task LaunchFile(string pathForFile)
		{
			await Launcher.Default.OpenAsync(new OpenFileRequest
			{
				File = new ReadOnlyFile(pathForFile)
			});
		}

		public async Task ShareFile(string title, string filePath)
		{
			await Share.RequestAsync(new ShareFileRequest
			{
				Title = title,
				File = new ShareFile(filePath)
			});
		}

		public string GetVersion() =>
			AppInfo.VersionString;

		public string GetBuild() =>
			AppInfo.BuildString;

		public async Task Speak(string text)
		{
			_speechCancellationSource = new CancellationTokenSource();

			var options = await SpeechController.GetSettings();

			await TextToSpeech.Default.SpeakAsync(
				text,
				options,
				_speechCancellationSource.Token);
		}

		public void CancelSpeech()
		{
			if (_speechCancellationSource == null)
				return;

			_speechCancellationSource.Cancel();
			_speechCancellationSource.Dispose();
			_speechCancellationSource = null;
		}

		public bool CheckConnectivity()
		{
			return Connectivity.NetworkAccess == NetworkAccess.Internet;
		}

		public string GetRuntimePlatform()
		{
			return DeviceInfo.Platform.ToString();
		}

		public bool SetNativeTheme(string hexColor)
		{
			try
			{
				var service = DependencyService.Get<IThemeNative>();
				service?.SetColors(hexColor);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public double GetNamedSize(int namedSize, Type type)
		{
			return namedSize switch
			{
				0 => 10,
				1 => 12,
				2 => 14,
				3 => 16,
				4 => 20,
				5 => 24,
				_ => 16
			};
		}

		public async Task<bool> SendEmail(
			string to,
			string title,
			string message,
			string attachmentPath)
		{
			try
			{
				var emailMessage = new EmailMessage
				{
					Subject = title,
					Body = message,
					To = new List<string> { to }
				};

				if (!string.IsNullOrWhiteSpace(attachmentPath))
				{
					emailMessage.Attachments.Add(
						new EmailAttachment(attachmentPath));
				}

				await Email.ComposeAsync(emailMessage);

				return true;
			}
			catch (FeatureNotSupportedException fnsEx)
			{
				AppLogs.Log(fnsEx);
				return false;
			}
		}
	}
}