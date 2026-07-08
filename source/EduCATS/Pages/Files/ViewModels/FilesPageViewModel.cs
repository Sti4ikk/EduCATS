using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EduCATS.Data;
using EduCATS.Demo;
using EduCATS.Helpers;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Networking;
using EduCATS.Pages.Files.Models;
using EduCATS.Pages.Pickers;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Files.ViewModels
{
	/// <summary>
	/// Files page view model.
	/// </summary>
	public class FilesPageViewModel : SubjectsViewModel
	{
		const string _filenameKey = "filename";
		const string _filepathKey = "filepath";

		double bytesIn;
		double totalBytes;

		object _lastSelectedObject;

		WebClient _client;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dialogs">App dialogs.</param>
		/// <param name="device">App device.</param>
		public FilesPageViewModel(IPlatformServices services) : base(services)
		{
			Task.Run(async () => await update(true));
			SubjectChanged += async (s, e) => await update(true);
		}

		List<FilesPageModel> fileList;

		/// <summary>
		/// File list.
		/// </summary>
		public List<FilesPageModel> FileList
		{
			get { return fileList; }
			set { SetProperty(ref fileList, value); }
		}

		bool _isLoading;

		/// <summary>
		/// Is loading.
		/// </summary>
		public bool IsLoading
		{
			get { return _isLoading; }
			set { SetProperty(ref _isLoading, value); }
		}

		bool _isDownloading;

		/// <summary>
		/// Is a file currently being downloaded.
		/// </summary>
		/// <remarks>
		/// Bind this in the View to show/hide a custom overlay with a
		/// progress indicator. This replaces the previous approach of
		/// using a native Acr/Controls.UserDialogs progress dialog,
		/// which is tied to the hosting Activity and becomes unusable
		/// (stuck at 0% / never hides) if Android recreates the
		/// Activity while an external viewer (e.g. a PDF viewer) is on
		/// top of the app.
		/// </remarks>
		public bool IsDownloading
		{
			get { return _isDownloading; }
			set { SetProperty(ref _isDownloading, value); }
		}

		int _downloadPercentage;

		/// <summary>
		/// Current download percentage (0-100).
		/// </summary>
		public int DownloadPercentage
		{
			get { return _downloadPercentage; }
			set { SetProperty(ref _downloadPercentage, value); }
		}

		string _downloadingFileName;

		/// <summary>
		/// Name of the file currently being downloaded.
		/// </summary>
		public string DownloadingFileName
		{
			get { return _downloadingFileName; }
			set { SetProperty(ref _downloadingFileName, value); }
		}

		object _selectedItem;

		/// <summary>
		/// Selected item.
		/// </summary>
		public object SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				SetProperty(ref _selectedItem, value);
				Task.Run(async () => await openFile(_selectedItem));
			}
		}

		Command _refreshCommand;

		/// <summary>
		/// Refresh command.
		/// </summary>
		public Command RefreshCommand
		{
			get
			{
				return _refreshCommand ?? (_refreshCommand = new Command(
					async () => await update(false)));
			}
		}

		Command _cancelDownloadCommand;

		/// <summary>
		/// Cancel current download command.
		/// </summary>
		/// <remarks>Bind this to a "Cancel" button in the download overlay.</remarks>
		public Command CancelDownloadCommand
		{
			get
			{
				return _cancelDownloadCommand ?? (_cancelDownloadCommand = new Command(abortDownload));
			}
		}

		/// <summary>
		/// Update with dialog or pull-to-refresh.
		/// </summary>
		/// <param name="dialog">Is dialog.</param>
		/// <returns>Task.</returns>
		async Task update(bool dialog)
		{
			if (dialog)
			{
				PlatformServices.Dialogs.ShowLoading();
			}
			else
			{
				IsLoading = true;
			}

			await update();

			if (dialog)
			{
				PlatformServices.Dialogs.HideLoading();
			}
			else
			{
				IsLoading = false;
			}
		}

		/// <summary>
		/// Refresh data.
		/// </summary>
		/// <returns>Task.</returns>
		async Task update()
		{
			try
			{
				await SetupSubjects();
				await getFiles();
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Get file list.
		/// </summary>
		/// <returns>Task.</returns>
		async Task getFiles()
		{
			var appDataDirectory = PlatformServices.Device.GetAppDataDirectory();

			var filesModel = await DataAccess.GetFilesTest(CurrentSubject.Id);
			if (DataAccess.IsError && !DataAccess.IsConnectionError)
			{
				PlatformServices.Dialogs.ShowError(DataAccess.ErrorMessage);
			}

			var files = filesModel?.Files?.Select(f =>
			{
				var file = Path.Combine(appDataDirectory, f.Name);
				var exists = File.Exists(file);
				return new FilesPageModel(f, exists);
			}).ToList();

			if (files == null || files.Count == 0)
			{
				FileList = new List<FilesPageModel>();
				return;
			}

			var valuesForDetails = files
				.Select(file => $"{file.Name}/{file.Id}/{file.PathName}/{file.FileName}")
				.ToList();

			var filesDetails = await DataAccess.GetDetailsFilesTest(valuesForDetails);
			if (filesDetails != null)
			{
				files.ForEach(file =>
				{
					var detail = filesDetails.FirstOrDefault(item => item.Id == file.Id);
					if (detail == null)
					{
						return;
					}

					if (long.TryParse(detail.Size, out var size))
					{
						file.Size = ConverterSize.FormatSize(size);
					}

					if (!string.IsNullOrEmpty(detail.Url))
					{
						file.Url = detail.Url;
					}
				});
			}

			FileList = new List<FilesPageModel>(files);
		}

		/// <summary>
		/// Open file.
		/// </summary>
		/// <param name="selectedObject">Selected object.</param>
		async Task openFile(object selectedObject)
		{
			try
			{
				if (AppDemo.Instance.IsDemoAccount)
				{
					PlatformServices.Device.MainThread(
						() => PlatformServices.Dialogs.ShowError(
							CrossLocalization.Translate("demo_files_download_error")));
					return;
				}

				if (selectedObject == null || !(selectedObject is FilesPageModel))
				{
					return;
				}

				_lastSelectedObject = selectedObject;
				SelectedItem = null;

				var file = selectedObject as FilesPageModel;
				var storageFilePath = Path.Combine(PlatformServices.Device.GetAppDataDirectory(), file.Name);

				if (File.Exists(storageFilePath) && new FileInfo(storageFilePath).Length != 0)
				{
					// File already downloaded - open immediately, no overlay needed.
					PlatformServices.Device.MainThread(() => PlatformServices.Device.LaunchFile(storageFilePath));
					return;
				}

				var fileUri = getFileUri(file);
				if (fileUri == null)
				{
					PlatformServices.Device.MainThread(
						() => PlatformServices.Dialogs.ShowError(
							CrossLocalization.Translate("files_downloading_error")));
					return;
				}

				startDownloadUiState(file.Name);

				totalBytes = bytesIn = 0;
				_client = new WebClient();
				_client.DownloadProgressChanged += downloadProgressChanged;
				_client.DownloadFileCompleted += downloadCompleted;
				if (!string.IsNullOrEmpty(PlatformServices.Preferences.AccessToken))
				{
					_client.Headers[HttpRequestHeader.Authorization] =
						PlatformServices.Preferences.AccessToken;
				}
				_client.QueryString.Add(_filenameKey, file.Name);
				_client.QueryString.Add(_filepathKey, storageFilePath);
				_client.DownloadFileAsync(fileUri, storageFilePath);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				stopDownloadUiState();
				PlatformServices.Device.MainThread(
					() => PlatformServices.Dialogs.ShowError(
						CrossLocalization.Translate("files_downloading_error")));
			}
		}

		/// <summary>
		/// Download completed.
		/// </summary>
		/// <param name="sender">Web client.</param>
		/// <param name="e">Event arguments.</param>
		private void downloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (sender == null)
			{
				return;
			}

			var client = sender as WebClient;
			var fileName = client.QueryString[_filenameKey];
			var pathForFile = client.QueryString[_filepathKey];

			if (e.Cancelled)
			{
				stopDownloadUiState();
				safeDeleteFile(pathForFile);
				return;
			}

			var lengthKnown = totalBytes > 0;
			var succeeded = !lengthKnown || totalBytes == bytesIn;

			if (!succeeded)
			{
				stopDownloadUiState();
				safeDeleteFile(pathForFile);
				PlatformServices.Device.MainThread(() => PlatformServices.Dialogs.ShowError(
					CrossLocalization.Translate("files_downloading_error")));
				return;
			}

			completeDownload(fileName, pathForFile);
		}

		/// <summary>
		/// Delete file ignoring any IO errors (e.g. file locked/missing).
		/// </summary>
		/// <param name="path">File path.</param>
		void safeDeleteFile(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}

		/// <summary>
		/// Complete download.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="pathForFile">Path for file.</param>
		void completeDownload(string fileName, string pathForFile)
		{
			stopDownloadUiState();
			updateDownloadedList();
			PlatformServices.Device.MainThread(() => PlatformServices.Device.LaunchFile(pathForFile));
		}

		/// <summary>
		/// Update downloaded files in list.
		/// </summary>
		void updateDownloadedList()
		{
			if (_lastSelectedObject != null && _lastSelectedObject is FilesPageModel)
			{
				var fileModel = _lastSelectedObject as FilesPageModel;
				var fileList = FileList.Select(
					f => {
						if (f == fileModel)
						{
							f.IsDownloaded = true;
						}

						return f;
					}).ToList();

				FileList = new List<FilesPageModel>(fileList);
			}
		}

		/// <summary>
		/// Download progress changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			bytesIn = e.BytesReceived;
			totalBytes = e.TotalBytesToReceive;

			// TotalBytesToReceive is -1 when the server doesn't send a
			// Content-Length header - percentage can't be computed then,
			// so we keep whatever was last known instead of showing
			// garbage/negative values.
			if (totalBytes <= 0)
			{
				return;
			}

			var percentage = (int)(bytesIn / totalBytes * 100);
			PlatformServices.Device.MainThread(() => DownloadPercentage = percentage);
		}

		/// <summary>
		/// Show the custom download overlay and reset its state.
		/// </summary>
		/// <param name="fileName">Name of the file being downloaded.</param>
		void startDownloadUiState(string fileName)
		{
			PlatformServices.Device.MainThread(() => {
				DownloadingFileName = fileName;
				DownloadPercentage = 0;
				IsDownloading = true;
			});
		}

		/// <summary>
		/// Hide the custom download overlay.
		/// </summary>
		/// <remarks>
		/// Unlike a native progress dialog, these are plain bindable
		/// properties on the ViewModel, so they keep working correctly
		/// even if Android recreates the hosting Activity while an
		/// external app (e.g. a PDF viewer) was on top.
		/// </remarks>
		void stopDownloadUiState()
		{
			PlatformServices.Device.MainThread(() => {
				IsDownloading = false;
				DownloadPercentage = 0;
				DownloadingFileName = null;
			});
		}

		/// <summary>
		/// Abort downloading process.
		/// </summary>
		void abortDownload()
		{
			if (_client == null || !_client.IsBusy)
			{
				return;
			}

			_client.CancelAsync();
		}

		/// <summary>
		/// Build file download URI.
		/// </summary>
		/// <param name="file">Selected file.</param>
		/// <returns>File URI.</returns>
		Uri getFileUri(FilesPageModel file)
		{
			if (file == null)
			{
				return null;
			}

			if (!string.IsNullOrEmpty(file.Url))
			{
				if (Uri.TryCreate(file.Url, UriKind.Absolute, out var fullUrl) &&
					(fullUrl.Scheme == Uri.UriSchemeHttp || fullUrl.Scheme == Uri.UriSchemeHttps))
				{
					return fullUrl;
				}

				var relativeUrl = file.Url.StartsWith("/") ? file.Url : $"/{file.Url}";
				var metadataUrl = new Uri($"{Servers.Current}{relativeUrl}");
				return metadataUrl;
			}

			if (string.IsNullOrEmpty(file.PathName) || string.IsNullOrEmpty(file.FileName))
			{
				return null;
			}

			var fileNameParam = $"{file.PathName}//{file.FileName}";
			var fallbackUrl = new Uri($"{Links.GetFile}?filename={fileNameParam}");
			return fallbackUrl;
		}
	}
}