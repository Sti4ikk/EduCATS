using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Converters
{
	/// <summary>
	/// Checks whether an HTML string contains content that Label's
	/// <c>TextType.Html</c> can't render correctly: an embedded &lt;img&gt;
	/// tag, or a MathML &lt;math&gt; formula.
	/// </summary>
	/// <remarks>
	/// Used to decide whether a question description needs to be rendered
	/// in a WebView (supports images and, via MathJax, MathML - but has
	/// flaky auto-height behavior inside a ListView Header) or can stay in
	/// a plain Label with TextType.Html (stable sizing, but can't render
	/// images or MathML formulas - both get silently flattened to plain
	/// text by the platform's native HTML renderer).
	/// Pass converter parameter "Invert" to get the opposite result.
	/// </remarks>
	public class HasEmbeddedImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var html = value as string;
			var needsWebView = !string.IsNullOrEmpty(html) &&
				(html.Contains("<img", StringComparison.OrdinalIgnoreCase) ||
				html.Contains("<math", StringComparison.OrdinalIgnoreCase));

			var invert = "Invert".Equals(parameter as string, StringComparison.OrdinalIgnoreCase);
			return invert ? !needsWebView : needsWebView;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}