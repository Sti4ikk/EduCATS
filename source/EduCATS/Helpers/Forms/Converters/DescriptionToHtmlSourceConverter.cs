using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Converters
{
	public class DescriptionToHtmlSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var html = value as string;

			if (string.IsNullOrEmpty(html))
			{
				return new HtmlWebViewSource { Html = string.Empty };
			}

			var mathJaxScript = string.IsNullOrEmpty(MathJaxCache.Script)
				? "<script src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/mml-chtml.js'></script>"
				: $"<script>{MathJaxCache.Script}</script>";

			var document = $@"
				<html>
				<head>
					<meta charset='utf-8'>
					<meta name='viewport' content='width=device-width, initial-scale=1'>
					<script>
						window.mathJaxReady = false;

						window.MathJax = {{
							startup: {{
								typeset: true,
								ready: function () {{
									MathJax.startup.defaultReady();
									MathJax.startup.promise.then(function () {{
										window.mathJaxReady = true;
									}});
								}}
							}}
						}};
					</script>
					{mathJaxScript}
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