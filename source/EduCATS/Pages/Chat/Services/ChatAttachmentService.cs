using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EduCATS.Helpers.Logs;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Attachment prepared for sending.
	/// </summary>
	public class ChatAttachment
	{
		/// <summary>
		/// Local copy of the file.
		/// </summary>
		public string FilePath { get; set; }

		/// <summary>
		/// File name on the server.
		/// </summary>
		public string FileName { get; set; }

		public long Size { get; set; }

		public bool IsImage { get; set; }
	}

	/// <summary>
	/// Picking, capturing and compressing chat attachments.
	/// </summary>
	public static class ChatAttachmentService
	{
		/// <summary>
		/// Max attachment size.
		/// </summary>
		public const int MaxFileSizeMegabytes = 25;

		/// <summary>
		/// Photos are scaled down to this size (longest side).
		/// </summary>
		const int _maxImageSide = 1920;

		/// <summary>
		/// JPEG quality of compressed photos.
		/// </summary>
		const int _jpegQuality = 80;

		/// <summary>
		/// Photos smaller than this are sent as is.
		/// </summary>
		const long _compressThresholdBytes = 300 * 1024;

		static readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic" };

		public static bool IsCaptureSupported
		{
			get
			{
				try
				{
					return MediaPicker.Default.IsCaptureSupported;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public static Task<FileResult> PickFileAsync() =>
			pickSafe(() => FilePicker.Default.PickAsync(PickOptions.Default));

		public static Task<FileResult> PickPhotoAsync() =>
			pickSafe(async () =>
			{
				var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
				return photos?.FirstOrDefault();
			});

		public static Task<FileResult> CapturePhotoAsync() =>
			pickSafe(() => MediaPicker.Default.CapturePhotoAsync());

		public static bool IsImageFile(string fileName) =>
			Array.IndexOf(_imageExtensions, Path.GetExtension(fileName)?.ToLowerInvariant()) >= 0;

		/// <summary>
		/// Copy the picked file to the cache (compressing photos).
		/// </summary>
		/// <param name="file">Picked file.</param>
		/// <param name="isCapturedPhoto">Is it a photo from the camera
		/// (gets a unique name instead of the camera's one).</param>
		/// <returns>Prepared attachment.</returns>
		public static async Task<ChatAttachment> PrepareAsync(FileResult file, bool isCapturedPhoto = false)
		{
			var fileName = isCapturedPhoto ?
				$"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg" :
				sanitizeFileName(file.FileName);

			var directory = Path.Combine(FileSystem.CacheDirectory, "chat_outgoing");
			Directory.CreateDirectory(directory);
			var path = Path.Combine(directory, $"{Guid.NewGuid():N}_{fileName}");

			await using (var source = await file.OpenReadAsync())
			await using (var target = File.Create(path))
			{
				await source.CopyToAsync(target);
			}

			var isImage = isCapturedPhoto || IsImageFile(fileName);

			if (isImage && isPhotoFormat(fileName) && new FileInfo(path).Length > _compressThresholdBytes)
			{
				var compressedPath = Path.ChangeExtension(path, ".jpg");

				if (await Task.Run(() => TryCompressImage(path, compressedPath, _maxImageSide, _jpegQuality)))
				{
					if (compressedPath != path)
					{
						File.Delete(path);
					}

					path = compressedPath;
					fileName = Path.ChangeExtension(fileName, ".jpg");
				}
			}

			return new ChatAttachment
			{
				FilePath = path,
				FileName = fileName,
				Size = new FileInfo(path).Length,
				IsImage = isImage
			};
		}

		/// <summary>
		/// Scale an image down and save it as JPEG,
		/// applying the EXIF orientation (camera photos are often rotated).
		/// </summary>
		/// <param name="sourcePath">Source image.</param>
		/// <param name="targetPath">Target JPEG file (may equal the source).</param>
		/// <param name="maxSide">Max size of the longest side.</param>
		/// <param name="quality">JPEG quality.</param>
		/// <returns><c>true</c> if compressed, <c>false</c> if the image can't be decoded.</returns>
		public static bool TryCompressImage(string sourcePath, string targetPath, int maxSide, int quality)
		{
			try
			{
				SKBitmap oriented;

				using (var codec = SKCodec.Create(sourcePath))
				{
					if (codec == null)
					{
						return false;
					}

					using var decoded = SKBitmap.Decode(codec);

					if (decoded == null)
					{
						return false;
					}

					oriented = applyOrientation(decoded, codec.EncodedOrigin);
				}

				using (oriented)
				{
					var size = GetScaledSize(oriented.Width, oriented.Height, maxSide);
					using var resized = size.Width == oriented.Width && size.Height == oriented.Height ?
						oriented.Copy() :
						oriented.Resize(new SKImageInfo(size.Width, size.Height), new SKSamplingOptions(SKCubicResampler.Mitchell));

					using var image = SKImage.FromBitmap(resized);
					using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
					using var output = File.Create(targetPath);
					data.SaveTo(output);
				}

				return true;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return false;
			}
		}

		/// <summary>
		/// Size that fits into <paramref name="maxSide"/> keeping the aspect ratio.
		/// </summary>
		public static (int Width, int Height) GetScaledSize(int width, int height, int maxSide)
		{
			var longest = Math.Max(width, height);

			if (longest <= maxSide || longest == 0)
			{
				return (width, height);
			}

			var scale = (double)maxSide / longest;
			return (Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
		}

		/// <summary>
		/// Human-readable file size (same format as the web client).
		/// </summary>
		public static string FormatSize(long bytes)
		{
			string[] units = { "Bytes", "KB", "MB", "GB" };

			if (bytes <= 0)
			{
				return "0 Bytes";
			}

			var unit = Math.Min(units.Length - 1, (int)Math.Floor(Math.Log(bytes) / Math.Log(1024)));
			var value = bytes / Math.Pow(1024, unit);
			return $"{Math.Round(value, 2).ToString(System.Globalization.CultureInfo.InvariantCulture)} {units[unit]}";
		}

		static SKBitmap applyOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
		{
			switch (origin)
			{
				case SKEncodedOrigin.BottomRight:
					return rotate(bitmap, 180);
				case SKEncodedOrigin.RightTop:
					return rotate(bitmap, 90);
				case SKEncodedOrigin.LeftBottom:
					return rotate(bitmap, 270);
				default:
					return bitmap.Copy();
			}
		}

		static SKBitmap rotate(SKBitmap bitmap, int degrees)
		{
			var isSideways = degrees == 90 || degrees == 270;
			var rotated = new SKBitmap(
				isSideways ? bitmap.Height : bitmap.Width,
				isSideways ? bitmap.Width : bitmap.Height);

			using var canvas = new SKCanvas(rotated);
			canvas.Translate(rotated.Width / 2f, rotated.Height / 2f);
			canvas.RotateDegrees(degrees);
			canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
			canvas.DrawBitmap(bitmap, 0, 0);
			return rotated;
		}

		/// <summary>
		/// Formats that are safe to re-encode as JPEG
		/// (GIF would lose animation, PNG - transparency).
		/// </summary>
		static bool isPhotoFormat(string fileName)
		{
			var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
			return extension is ".jpg" or ".jpeg" or ".heic" or ".webp" or ".bmp";
		}

		static string sanitizeFileName(string fileName)
		{
			var name = Path.GetFileName(fileName ?? string.Empty);

			foreach (var invalid in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(invalid, '_');
			}

			return string.IsNullOrWhiteSpace(name) ? $"file_{DateTime.Now:yyyyMMdd_HHmmss}" : name;
		}

		static async Task<FileResult> pickSafe(Func<Task<FileResult>> pick)
		{
			try
			{
				return await pick();
			}
			catch (Exception ex)
			{
				// Cancelled, no permission or no camera - nothing to send.
				AppLogs.Log(ex);
				return null;
			}
		}
	}
}
