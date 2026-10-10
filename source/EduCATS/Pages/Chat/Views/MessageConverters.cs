using System;
using System.Globalization;
using System.IO;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Message text with clickable links.
	/// </summary>
	public class MessageTextToFormattedStringConverter : IValueConverter
	{
		static readonly Color _linkColor = Color.FromArgb("#1E88E5");

		static readonly Command _openLinkCommand = new Command(async url =>
		{
			try
			{
				await Launcher.Default.OpenAsync(new Uri((string)url));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
			}
		});

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var parts = MessageLinkParser.Split(value as string);

			// Text without links is shown by a plain label.
			if (!parts.Exists(p => p.IsLink))
			{
				return null;
			}

			var formatted = new FormattedString();

			foreach (var part in parts)
			{
				var span = new Span { Text = part.Text };

				if (part.IsLink)
				{
					span.TextColor = _linkColor;
					span.TextDecorations = TextDecorations.Underline;
					span.GestureRecognizers.Add(new TapGestureRecognizer
					{
						Command = _openLinkCommand,
						CommandParameter = part.Url
					});
				}

				formatted.Spans.Add(span);
			}

			return formatted;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}

	/// <summary>
	/// Image of a message: local file being sent, embedded base64
	/// (old messages) or the file on the server (cached).
	/// </summary>
	public class MessageImageSourceConverter : IValueConverter
	{
		static readonly Base64ToImageSourceConverter _base64Converter = new Base64ToImageSourceConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			Create(value as MessageItemModel);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();

		/// <summary>
		/// Create image source of a message.
		/// </summary>
		public static ImageSource Create(MessageItemModel message)
		{
			if (message == null || !message.IsImageMessage)
			{
				return null;
			}

			if (!string.IsNullOrEmpty(message.LocalFilePath) && File.Exists(message.LocalFilePath))
			{
				return ImageSource.FromFile(message.LocalFilePath);
			}

			var base64 = message.ImageContent?.Length > 0 ? message.ImageContent[0] : null;

			if (!string.IsNullOrEmpty(base64))
			{
				return _base64Converter.Convert(base64, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource;
			}

			if (string.IsNullOrEmpty(message.FileContent))
			{
				return null;
			}

			return new UriImageSource
			{
				Uri = new Uri(ChatApiService.GetFileUrl(message.ChatId, message.FileContent)),
				CachingEnabled = true,
				CacheValidity = TimeSpan.FromDays(30)
			};
		}
	}

	/// <summary>
	/// Sending status glyph: 🕓 sending, ✓ sent, ⚠ failed.
	/// </summary>
	public class MessageStatusToGlyphConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is MessageSendStatus status ?
				status switch
				{
					MessageSendStatus.Sending => "🕓",
					MessageSendStatus.Sent => "✓",
					MessageSendStatus.Failed => "⚠",
					_ => string.Empty
				} :
				string.Empty;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
