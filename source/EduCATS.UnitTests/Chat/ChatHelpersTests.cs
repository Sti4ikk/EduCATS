using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EduCATS.Helpers.Forms.Pages;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.Controls;
using NUnit.Framework;
using SkiaSharp;

namespace EduCATS.UnitTests.Chat
{
	[TestFixture]
	public class ChatHelpersTests
	{
		[Test]
		public void SplitFindsLinksAndKeepsTextTest()
		{
			var parts = MessageLinkParser.Split("Смотри https://educats.by/news?id=5, и www.bntu.by.");

			Assert.That(parts.Select(p => p.Text), Is.EqualTo(new[]
			{
				"Смотри ", "https://educats.by/news?id=5", ", и ", "www.bntu.by", "."
			}));
			Assert.That(parts[1].Url, Is.EqualTo("https://educats.by/news?id=5"));
			Assert.That(parts[3].Url, Is.EqualTo("https://www.bntu.by"));
			Assert.That(parts.Count(p => p.IsLink), Is.EqualTo(2));
		}

		[Test]
		public void SplitWithoutLinksReturnsTextTest()
		{
			var parts = MessageLinkParser.Split("Просто текст");

			Assert.That(parts.Count, Is.EqualTo(1));
			Assert.That(parts[0].IsLink, Is.False);
			Assert.That(MessageLinkParser.GetFirstUrl("Просто текст"), Is.Null);
			Assert.That(MessageLinkParser.Split(null), Is.Empty);
		}

		[Test]
		public void ParseReadsOpenGraphTest()
		{
			var html = @"<html><head>
				<meta content=""It's a &quot;test&quot; page"" property=""og:title"">
				<meta property='og:description' content='Описание страницы'>
				<meta property=""og:image"" content=""/img/preview.png"">
				<title>Fallback</title></head></html>";

			var preview = LinkPreviewService.Parse(html, new Uri("https://educats.by/news/1"));

			Assert.That(preview.Title, Is.EqualTo("It's a \"test\" page"));
			Assert.That(preview.Description, Is.EqualTo("Описание страницы"));
			Assert.That(preview.ImageUrl, Is.EqualTo("https://educats.by/img/preview.png"));
			Assert.That(preview.Host, Is.EqualTo("educats.by"));
		}

		[Test]
		public void ParseFallsBackToTitleTagTest()
		{
			var preview = LinkPreviewService.Parse("<head><title> Заголовок </title></head>", new Uri("https://bntu.by"));

			Assert.That(preview.Title, Is.EqualTo("Заголовок"));
			Assert.That(preview.HasImage, Is.False);
			Assert.That(LinkPreviewService.Parse("<p>no title</p>", new Uri("https://bntu.by")), Is.Null);
		}

		[Test]
		public void GetScaledSizeKeepsAspectRatioTest()
		{
			Assert.That(ChatAttachmentService.GetScaledSize(4000, 3000, 1920), Is.EqualTo((1920, 1440)));
			Assert.That(ChatAttachmentService.GetScaledSize(3000, 4000, 1920), Is.EqualTo((1440, 1920)));
			Assert.That(ChatAttachmentService.GetScaledSize(800, 600, 1920), Is.EqualTo((800, 600)));
		}

		[Test]
		public void FormatSizeTest()
		{
			Assert.That(ChatAttachmentService.FormatSize(0), Is.EqualTo("0 Bytes"));
			Assert.That(ChatAttachmentService.FormatSize(512), Is.EqualTo("512 Bytes"));
			Assert.That(ChatAttachmentService.FormatSize(1536), Is.EqualTo("1.5 KB"));
			Assert.That(ChatAttachmentService.FormatSize(5 * 1024 * 1024), Is.EqualTo("5 MB"));
		}

		[Test]
		public void IsImageFileTest()
		{
			Assert.That(ChatAttachmentService.IsImageFile("photo.JPG"), Is.True);
			Assert.That(ChatAttachmentService.IsImageFile("lab1.pdf"), Is.False);
		}

		[Test]
		public void TryCompressImageScalesDownTest()
		{
			var directory = Path.Combine(Path.GetTempPath(), "educats_tests_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);

			try
			{
				var source = Path.Combine(directory, "big.png");

				using (var bitmap = new SKBitmap(3000, 1500))
				using (var image = SKImage.FromBitmap(bitmap))
				using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
				using (var file = File.Create(source))
				{
					data.SaveTo(file);
				}

				var target = Path.Combine(directory, "small.jpg");

				Assert.That(ChatAttachmentService.TryCompressImage(source, target, 1920, 80), Is.True);

				using var result = SKBitmap.Decode(target);
				Assert.That(result.Width, Is.EqualTo(1920));
				Assert.That(result.Height, Is.EqualTo(960));
			}
			finally
			{
				Directory.Delete(directory, recursive: true);
			}
		}

		[Test]
		public void TryCompressImageRejectsNonImageTest()
		{
			var path = Path.GetTempFileName();

			try
			{
				File.WriteAllText(path, "not an image");
				Assert.That(ChatAttachmentService.TryCompressImage(path, path + ".jpg", 1920, 80), Is.False);
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Test]
		public void MessageStatusRaisesDependentPropertiesTest()
		{
			var message = new MessageItemModel { IsFile = true };
			var changed = new List<string>();
			message.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

			message.Status = MessageSendStatus.Sending;
			Assert.That(message.IsUploading, Is.True);

			message.Status = MessageSendStatus.Failed;
			Assert.That(message.IsFailed, Is.True);
			Assert.That(changed, Does.Contain(nameof(MessageItemModel.IsFailed)));
			Assert.That(changed, Does.Contain(nameof(MessageItemModel.IsUploading)));
		}

		[Test]
		public void FileNameOfUploadedFileTest()
		{
			var message = new MessageItemModel { IsFile = true, FileContent = "report.pdf", FileSize = "12 KB" };

			Assert.That(message.HasInlineFile, Is.False);
			Assert.That(message.FileName, Is.EqualTo("report.pdf"));
			Assert.That(message.FileDisplayText, Is.EqualTo("report.pdf (12 KB)"));
		}

		[Test]
		public void FileNameOfInlineFileTest()
		{
			// Old messages: the file itself (base64) and its name in the text.
			var base64 = Convert.ToBase64String(new byte[1024]);
			var message = new MessageItemModel { IsFile = true, Text = "lab1.docx", FileContent = base64, FileSize = "1 KB" };

			Assert.That(message.HasInlineFile, Is.True);
			Assert.That(message.FileName, Is.EqualTo("lab1.docx"));
			Assert.That(message.FileDisplayText, Is.EqualTo("lab1.docx (1 KB)"));

			message.Text = null;
			Assert.That(message.FileName, Does.Not.Contain(base64));
		}

		[Test]
		public void TabBarIsHiddenForFullScreenPagesTest()
		{
			Assert.That(TabBarVisibility.IsVisibleFor(new ContentPage()), Is.True);
			Assert.That(TabBarVisibility.IsVisibleFor(new FullScreenPage()), Is.False);
		}

		class FullScreenPage : ContentPage, IHidesTabBar
		{
		}
	}
}
