using System;
using EduCATS.Helpers.Files;

namespace EduCATS.Helpers.Logs
{
	/// <summary>
	/// Persistent application log.
	/// </summary>
	public static class AppLogs
	{
		const string _logsFileName = "app_logs.txt";
		const double _maximumFileSizeMb = 1;

		/// <summary>
		/// File manager interface implementation.
		/// </summary>
		public static IFileManager FileManager = new FileManager();

		/// <summary>
		/// Logs file path.
		/// </summary>
		public static string LogsFilePath { get; private set; }

		/// <summary>
		/// Initialize application logs class.
		/// </summary>
		public static void Initialize(string directoryForLogs)
		{
			FileManager ??= new FileManager();
			LogsFilePath = $"{directoryForLogs}/{_logsFileName}";

			if (FileManager.Exists(LogsFilePath))
			{
				if (FileManager.GetFileSize(LogsFilePath) >= _maximumFileSizeMb)
				{
					FileManager.Delete(LogsFilePath);
					FileManager.Create(LogsFilePath);
				}

				return;
			}

			FileManager.Create(LogsFilePath);
		}

		/// <summary>
		/// Log exception.
		/// </summary>
		public static void Log(Exception ex, string caller = "")
		{
			if (ex == null)
			{
				return;
			}

			Log(ex.ToString(), caller);
		}

		/// <summary>
		/// Log message.
		/// </summary>
		public static void Log(string message, string caller = "")
		{
			if (message == null)
			{
				return;
			}

			var source = string.IsNullOrWhiteSpace(caller) ? string.Empty : $" [{caller}]";
			FileManager.Append(LogsFilePath, $"{DateTime.UtcNow:O}{source} {message}{Environment.NewLine}");
		}

		/// <summary>
		/// Get logs file contents.
		/// </summary>
		public static string ReadLog()
		{
			if (FileManager == null)
			{
				throw new Exception("Application logs are not initialized.");
			}

			return FileManager.Read(LogsFilePath);
		}

		/// <summary>
		/// Delete logs file.
		/// </summary>
		public static void DeleteLog()
		{
			if (FileManager.Exists(LogsFilePath))
			{
				FileManager.Delete(LogsFilePath);
			}
		}
	}
}