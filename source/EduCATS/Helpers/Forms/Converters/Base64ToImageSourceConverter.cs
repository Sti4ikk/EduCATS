using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Converters
{
	public class Base64ToImageSourceConverter : IValueConverter
	{
		private const string _base64Prefix = "data:image/png;base64,";
		private const string _jpegPrefix = "data:image/jpeg;base64,";

		/// <summary>
		/// Decoded images kept in memory: avatars and chat images are bound
		/// again on every scroll and list refresh.
		/// </summary>
		/// <remarks>
		/// Limited: an unlimited cache kept every image ever shown
		/// for the whole app lifetime.
		/// </remarks>
		private const int _maxCachedImages = 100;

		// Ключ - хэш и длина строки (длина отсекает почти все коллизии хэша).
		private static readonly Dictionary<(int hash, int length), ImageSource> _imageCache =
			new Dictionary<(int hash, int length), ImageSource>();

		private static readonly Queue<(int hash, int length)> _cacheOrder = new Queue<(int hash, int length)>();
		private static readonly object _cacheLock = new object();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not string base64Image || string.IsNullOrWhiteSpace(base64Image))
			{
				return null;
			}

			var cacheKey = (base64Image.GetHashCode(), base64Image.Length);

			lock (_cacheLock)
			{
				if (_imageCache.TryGetValue(cacheKey, out var cachedSource))
				{
					return cachedSource;
				}
			}

			// Очищаем префикс
			if (base64Image.StartsWith(_base64Prefix, StringComparison.OrdinalIgnoreCase))
			{
				base64Image = base64Image.Substring(_base64Prefix.Length);
			}
			else if (base64Image.StartsWith(_jpegPrefix, StringComparison.OrdinalIgnoreCase))
			{
				base64Image = base64Image.Substring(_jpegPrefix.Length);
			}

			try
			{
				var imageBytes = System.Convert.FromBase64String(base64Image);

				// FromStream с сигнатурой () => Stream создает новый поток при каждом запросе рендерера
				var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

				lock (_cacheLock)
				{
					if (_imageCache.TryAdd(cacheKey, imageSource))
					{
						_cacheOrder.Enqueue(cacheKey);

						while (_cacheOrder.Count > _maxCachedImages)
						{
							_imageCache.Remove(_cacheOrder.Dequeue());
						}
					}
				}

				return imageSource;
			}
			catch
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
