using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Text fragment: plain text or a link.
	/// </summary>
	public readonly struct MessageTextPart
	{
		public MessageTextPart(string text, string url)
		{
			Text = text;
			Url = url;
		}

		public string Text { get; }

		/// <summary>
		/// Absolute URL, <c>null</c> for plain text.
		/// </summary>
		public string Url { get; }

		public bool IsLink => Url != null;
	}

	/// <summary>
	/// Finds links in message text.
	/// </summary>
	public static class MessageLinkParser
	{
		static readonly Regex _linkRegex = new Regex(
			@"\b(?:https?://|www\.)[^\s<>""]+",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Punctuation that usually ends a sentence rather than a link.
		/// </summary>
		static readonly char[] _trailingPunctuation = { '.', ',', '!', '?', ':', ';', ')', ']', '}', '\'', '"', '»' };

		/// <summary>
		/// Split text into plain text and link parts.
		/// </summary>
		public static List<MessageTextPart> Split(string text)
		{
			var parts = new List<MessageTextPart>();

			if (string.IsNullOrEmpty(text))
			{
				return parts;
			}

			var position = 0;

			foreach (Match match in _linkRegex.Matches(text))
			{
				var linkText = match.Value.TrimEnd(_trailingPunctuation);

				if (linkText.Length == 0)
				{
					continue;
				}

				if (match.Index > position)
				{
					parts.Add(new MessageTextPart(text.Substring(position, match.Index - position), null));
				}

				parts.Add(new MessageTextPart(linkText, toAbsoluteUrl(linkText)));
				position = match.Index + linkText.Length;
			}

			if (position < text.Length)
			{
				parts.Add(new MessageTextPart(text.Substring(position), null));
			}

			return parts;
		}

		/// <summary>
		/// First link of the text.
		/// </summary>
		/// <returns>Absolute URL or <c>null</c>.</returns>
		public static string GetFirstUrl(string text)
		{
			foreach (var part in Split(text))
			{
				if (part.IsLink)
				{
					return part.Url;
				}
			}

			return null;
		}

		static string toAbsoluteUrl(string link) =>
			link.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? $"https://{link}" : link;
	}
}
