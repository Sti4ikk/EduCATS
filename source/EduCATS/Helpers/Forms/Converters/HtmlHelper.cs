using System.Text.RegularExpressions;

namespace EduCATS.Helpers.Forms
{
	public static class HtmlHelper
	{
		/// <summary>
		/// Strips HTML tags from chat message text (server stores rich
		/// text as HTML - e.g. "&lt;p&gt;text&lt;/p&gt;"), converting
		/// block/line breaks to newlines and decoding basic entities.
		/// </summary>
		public static string StripHtml(string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return html;
			}

			// Most messages are plain text: nothing to replace.
			if (html.IndexOf('<') < 0 && html.IndexOf('&') < 0)
			{
				return html.Trim();
			}

			var text = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "</p>\\s*<p>", "\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "<.*?>", string.Empty);

			text = text
				.Replace("&nbsp;", " ")
				.Replace("&amp;", "&")
				.Replace("&lt;", "<")
				.Replace("&gt;", ">")
				.Replace("&quot;", "\"")
				.Replace("&#39;", "'");

			return text.Trim();
		}
	}
}