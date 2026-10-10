using System;
using System.Collections.Generic;
using System.Linq;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using NUnit.Framework;

namespace EduCATS.UnitTests.Chat
{
	[TestFixture]
	public class ChatActivityTests
	{
		[SetUp]
		public void SetUp()
		{
			ChatActivityService.Clear();
			ChatUnreadService.Clear();
		}

		[Test]
		public void ChatWithNewMessageGoesUpTest()
		{
			var chats = createChats(1, 2, 3);
			ChatActivityService.Touch(3, isGroup: false, DateTime.UtcNow);

			var sorted = ChatActivityService.Sort(chats);

			Assert.That(sorted.Select(c => c.Id), Is.EqualTo(new[] { 3, 1, 2 }));
		}

		[Test]
		public void PinnedChatsStayFirstTest()
		{
			var chats = createChats(1, 2, 3);
			chats[1].IsPinned = true;
			ChatActivityService.Touch(3, isGroup: false, DateTime.UtcNow);

			var sorted = ChatActivityService.Sort(chats);

			Assert.That(sorted.Select(c => c.Id), Is.EqualTo(new[] { 2, 3, 1 }));
		}

		[Test]
		public void NewestMessageFirstTest()
		{
			var chats = createChats(1, 2, 3);
			ChatActivityService.Touch(1, isGroup: false, DateTime.UtcNow.AddMinutes(-5));
			ChatActivityService.Touch(2, isGroup: false, DateTime.UtcNow);

			var sorted = ChatActivityService.Sort(chats);

			Assert.That(sorted.Select(c => c.Id), Is.EqualTo(new[] { 2, 1, 3 }));
		}

		[Test]
		public void OlderTimeDoesNotMoveChatDownTest()
		{
			var now = DateTime.UtcNow;
			ChatActivityService.Touch(1, isGroup: false, now);
			ChatActivityService.Touch(1, isGroup: false, now.AddHours(-1));

			Assert.That(ChatActivityService.Get(1, isGroup: false), Is.EqualTo(now));
		}

		[Test]
		public void PersonalAndGroupChatsAreSeparateTest()
		{
			ChatActivityService.Touch(5, isGroup: true, DateTime.UtcNow);

			Assert.That(ChatActivityService.Get(5, isGroup: false), Is.Null);
			Assert.That(ChatActivityService.Get(5, isGroup: true), Is.Not.Null);
		}

		[Test]
		public void UnreadChatsFirstWithoutTimeTest()
		{
			var chats = createChats(1, 2);
			chats[1].Unread = 3;

			var sorted = ChatActivityService.Sort(chats);

			Assert.That(sorted.Select(c => c.Id), Is.EqualTo(new[] { 2, 1 }));
		}

		[Test]
		public void MoreUnreadMessagesMoveChatUpTest()
		{
			var chats = createChats(1, 2);
			ChatUnreadService.SetPersonal(chats);

			chats[1].Unread = 1;
			ChatUnreadService.SetPersonal(chats);

			Assert.That(ChatActivityService.Get(2, isGroup: false), Is.Not.Null);
			Assert.That(ChatActivityService.Get(1, isGroup: false), Is.Null);
		}

		static List<ChatItemModel> createChats(params int[] ids) =>
			ids.Select(id => new ChatItemModel { Id = id, Name = $"Chat {id}" }).ToList();
	}
}
