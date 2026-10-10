using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Helpers.Files;

namespace EduCATS.Helpers.Logs
{
	/// <summary>
	/// Persistent application log.
	/// </summary>
	/// <remarks>
	/// Lines are queued and written to the file in batches on a background
	/// thread: logging is called for every request, often from the UI thread,
	/// and must not wait for the disk. <see cref="Flush"/> writes the queue
	/// right away (before reading or sending the log, on a crash).
	/// </remarks>
	public static class AppLogs
	{
		const string _logsFileName = "app_logs.txt";
		const double _maximumFileSizeMb = 1;
		const long _maximumFileSizeBytes = (long)(_maximumFileSizeMb * 1024 * 1024);

		/// <summary>
		/// Lines waiting to be written, each with the file manager it was logged with.
		/// </summary>
		static readonly ConcurrentQueue<(IFileManager manager, string line)> _pending =
			new ConcurrentQueue<(IFileManager manager, string line)>();

		static readonly object _writeSync = new object();
		static int _isFlushScheduled;

		/// <summary>
		/// Approximate size of the file (to rotate it during a long session too).
		/// </summary>
		static long _fileSizeBytes;

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

			lock (_writeSync)
			{
				LogsFilePath = $"{directoryForLogs}/{_logsFileName}";
				prepareFile();
			}

			// Lines logged before initialization.
			if (!_pending.IsEmpty)
			{
				scheduleFlush();
			}
		}

		static void prepareFile()
		{
			_fileSizeBytes = 0;

			if (FileManager.Exists(LogsFilePath))
			{
				var sizeMb = FileManager.GetFileSize(LogsFilePath);

				if (sizeMb >= _maximumFileSizeMb)
				{
					FileManager.Delete(LogsFilePath);
					FileManager.Create(LogsFilePath);
					return;
				}

				_fileSizeBytes = (long)(sizeMb * 1024 * 1024);
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
			_pending.Enqueue((FileManager, $"{DateTime.UtcNow:O}{source} {message}{Environment.NewLine}"));
			scheduleFlush();
		}

		/// <summary>
		/// Write queued lines to the file now.
		/// </summary>
		public static void Flush()
		{
			lock (_writeSync)
			{
				// Lines logged before initialization wait for the file path.
				if (_pending.IsEmpty || LogsFilePath == null)
				{
					return;
				}

				var batch = new StringBuilder();
				IFileManager batchManager = null;

				while (_pending.TryDequeue(out var entry))
				{
					if (batchManager != null && entry.manager != batchManager)
					{
						write(batchManager, batch);
						batch.Clear();
					}

					batchManager = entry.manager;
					batch.Append(entry.line);
				}

				write(batchManager, batch);
			}
		}

		static void write(IFileManager manager, StringBuilder batch)
		{
			if (manager == null || batch.Length == 0)
			{
				return;
			}

			try
			{
				if (_fileSizeBytes + batch.Length > _maximumFileSizeBytes)
				{
					manager.Delete(LogsFilePath);
					manager.Create(LogsFilePath);
					_fileSizeBytes = 0;
				}

				manager.Append(LogsFilePath, batch.ToString());
				_fileSizeBytes += batch.Length;
			}
			catch
			{
				// Logging must never crash the app.
			}
		}

		static void scheduleFlush()
		{
			if (Interlocked.Exchange(ref _isFlushScheduled, 1) == 1)
			{
				return;
			}

			Task.Run(() =>
			{
				// Lines logged while writing are picked up by the next flush.
				Interlocked.Exchange(ref _isFlushScheduled, 0);
				Flush();
			});
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

			Flush();
			return FileManager.Read(LogsFilePath);
		}

		/// <summary>
		/// Delete logs file.
		/// </summary>
		public static void DeleteLog()
		{
			lock (_writeSync)
			{
				_pending.Clear();
				_fileSizeBytes = 0;

				if (FileManager.Exists(LogsFilePath))
				{
					FileManager.Delete(LogsFilePath);
				}
			}
		}
	}
}
