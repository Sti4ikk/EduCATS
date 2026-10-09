using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Loads link previews (Open Graph title, description and image).
	/// </summary>
	public static class LinkPreviewService
	{
		/// <summary>
		/// Only the beginning of a page is read: meta tags are in &lt;head&gt;.
		/// </summary>
		const int _maxHtmlBytes = 256 * 1024;

		static readonly TimeSpan _timeout = TimeSpan.FromSeconds(6);

		static readonly HttpClient _client = createClient();

		/// <summary>
		/// Previews by URL (<c>null</c> result if the page has no preview).
		/// </summary>
		static readonly ConcurrentDictionary<string, Task<LinkPreviewModel>> _cache =
			new ConcurrentDictionary<string, Task<LinkPreviewModel>>();

		static LinkPreviewService()
		{
			// windows-1251 and other legacy charsets (common on .by sites).
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		/// <summary>
		/// Get a preview of a page.
		/// </summary>
		/// <param name="url">Absolute http(s) URL.</param>
		/// <returns>Preview or <c>null</c>.</returns>
		public static Task<LinkPreviewModel> GetAsync(string url)
		{
			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
				(uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
			{
				return Task.FromResult<LinkPreviewModel>(null);
			}

			return _cache.GetOrAdd(uri.AbsoluteUri, _ => loadAsync(uri));
		}

		/// <summary>
		/// Parse Open Graph data (with &lt;title&gt; as a fallback) from HTML.
		/// </summary>
		/// <param name="html">Page HTML.</param>
		/// <param name="pageUri">Page URL (to resolve relative image URLs).</param>
		/// <returns>Preview, or <c>null</c> if the page has no title.</returns>
		public static LinkPreviewModel Parse(string html, Uri pageUri)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}

			var title = getMeta(html, "og:title") ?? getMeta(html, "twitter:title") ?? getTitleTag(html);

			if (string.IsNullOrWhiteSpace(title))
			{
				return null;
			}

			var description = getMeta(html, "og:description") ?? getMeta(html, "description");
			var image = getMeta(html, "og:image") ?? getMeta(html, "twitter:image");

			if (image != null && Uri.TryCreate(pageUri, image, out var imageUri) &&
				(imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp))
			{
				image = imageUri.AbsoluteUri;
			}
			else
			{
				image = null;
			}

			return new LinkPreviewModel
			{
				Url = pageUri.AbsoluteUri,
				Host = pageUri.Host,
				Title = truncate(title, 120),
				Description = truncate(description, 200),
				ImageUrl = image
			};
		}

		static async Task<LinkPreviewModel> loadAsync(Uri uri)
		{
			try
			{
				using var cancellation = new CancellationTokenSource(_timeout);
				using var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);

				var mediaType = response.Content.Headers.ContentType?.MediaType;

				if (!response.IsSuccessStatusCode ||
					(mediaType != null && !mediaType.Contains("html", StringComparison.OrdinalIgnoreCase)))
				{
					return null;
				}

				var html = await readLimitedAsync(response, cancellation.Token);
				return Parse(html, response.RequestMessage?.RequestUri ?? uri);
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return null;
			}
		}

		static async Task<string> readLimitedAsync(HttpResponseMessage response, CancellationToken token)
		{
			await using var stream = await response.Content.ReadAsStreamAsync(token);
			var buffer = new byte[_maxHtmlBytes];
			var total = 0;
			int read;

			while (total < buffer.Length &&
				(read = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total), token)) > 0)
			{
				total += read;
			}

			var charset = response.Content.Headers.ContentType?.CharSet;
			var encoding = Encoding.UTF8;

			try
			{
				if (!string.IsNullOrEmpty(charset))
				{
					encoding = Encoding.GetEncoding(charset.Trim('"'));
				}
			}
			catch (ArgumentException)
			{
			}

			return encoding.GetString(buffer, 0, total);
		}

		static string getMeta(string html, string name)
		{
			var escapedName = Regex.Escape(name);

			// Attributes may come in any order: property/name before or after content.
			// The value ends with the same quote it starts with (it may contain the other one).
			var match = Regex.Match(html,
				$@"<meta[^>]+(?:property|name)\s*=\s*[""']{escapedName}[""'][^>]*?content\s*=\s*([""'])(?<value>.*?)\1",
				RegexOptions.IgnoreCase | RegexOptions.Singleline);

			if (!match.Success)
			{
				match = Regex.Match(html,
					$@"<meta[^>]+?content\s*=\s*([""'])(?<value>.*?)\1[^>]*(?:property|name)\s*=\s*[""']{escapedName}[""']",
					RegexOptions.IgnoreCase | RegexOptions.Singleline);
			}

			return match.Success ? clean(match.Groups["value"].Value) : null;
		}

		static string getTitleTag(string html)
		{
			var match = Regex.Match(html, @"<title[^>]*>([^<]*)</title>", RegexOptions.IgnoreCase);
			return match.Success ? clean(match.Groups[1].Value) : null;
		}

		static string clean(string value)
		{
			var decoded = WebUtility.HtmlDecode(value ?? string.Empty);
			decoded = Regex.Replace(decoded, @"\s+", " ").Trim();
			return decoded.Length == 0 ? null : decoded;
		}

		static string truncate(string value, int maxLength) =>
			value == null || value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";

		static HttpClient createClient()
		{
			var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
			// Some sites return the preview only to browser-like agents.
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EduCATS link preview)");
			return client;
		}
	}
}
