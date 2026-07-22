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

		// Кэшируем готовые ImageSource по их хэш-коду
		private static readonly Dictionary<int, ImageSource> _imageCache = new Dictionary<int, ImageSource>();
		private static readonly object _cacheLock = new object();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not string base64Image || string.IsNullOrWhiteSpace(base64Image))
			{
				return null;
			}

			// Быстрый расчет хэша строки вместо SHA256
			int cacheKey = base64Image.GetHashCode();

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
					_imageCache[cacheKey] = imageSource;
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