using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Converters
{
	/// <summary>
	/// Wraps a raw HTML fragment (question description, may contain
	/// data-URI images) into a full HTML document and converts it to
	/// <see cref="HtmlWebViewSource"/> for use with <see cref="WebView"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="Label"/> with <c>TextType.Html</c> only supports a small
	/// subset of tags (b, i, font, br, a) via the platform's native HTML
	/// rendering and cannot display &lt;img&gt;/&lt;figure&gt; content -
	/// unsupported markup is dumped as raw text instead of being parsed.
	/// A WebView renders full HTML, including embedded base64 images.
	/// </remarks>
	public class DescriptionToHtmlSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var html = value as string;

			if (string.IsNullOrEmpty(html))
			{
				return new HtmlWebViewSource { Html = string.Empty };
			}

			var document = $@"
				<html>
				<head>
					<meta charset='utf-8'>
					<meta name='viewport' content='width=device-width, initial-scale=1'>
					<style>
						body {{ margin: 0; padding: 0; font-family: sans-serif; }}
						img {{ max-width: 100%; height: auto; }}
					</style>
				</head>
				<body>
					{html}
				</body>
				</html>";

			return new HtmlWebViewSource { Html = document };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}