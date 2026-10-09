using System;
using System.IO;
using System.Threading.Tasks;
using EduCATS.Helpers.Logs;
using Microsoft.Maui.Storage;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Downloaded chat attachments kept in the cache directory,
	/// so a file is downloaded only once.
	/// </summary>
	public static class ChatFileCache
	{
		const string _directoryName = "chat_files";

		/// <summary>
		/// Local path of a cached attachment (the file may not exist).
		/// </summary>
		public static string GetPath(int chatId, bool isGroup, string fileName) =>
			Path.Combine(
				FileSystem.CacheDirectory,
				_directoryName,
				$"{(isGroup ? "g" : "p")}{chatId}",
				Path.GetFileName(fileName));

		/// <summary>
		/// Get the cached file or download it.
		/// </summary>
		/// <returns>Local path, or <c>null</c> if downloading failed.</returns>
		public static async Task<string> GetOrDownloadAsync(int chatId, bool isGroup, string fileName)
		{
			var path = GetPath(chatId, isGroup, fileName);

			if (File.Exists(path) && new FileInfo(path).Length > 0)
			{
				return path;
			}

			var bytes = await ChatApiService.DownloadFile(chatId, fileName);

			if (bytes == null)
			{
				return null;
			}

			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));

				// Write to a temp file first: an interrupted write must not leave a broken cache entry.
				var tempPath = $"{path}.download";
				await File.WriteAllBytesAsync(tempPath, bytes);
				File.Move(tempPath, path, overwrite: true);
				return path;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return null;
			}
		}

		/// <summary>
		/// Delete all cached attachments (logout).
		/// </summary>
		public static void Clear()
		{
			try
			{
				var directory = Path.Combine(FileSystem.CacheDirectory, _directoryName);

				if (Directory.Exists(directory))
				{
					Directory.Delete(directory, recursive: true);
				}
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		}
	}
}
