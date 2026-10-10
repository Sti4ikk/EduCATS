using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using EduCATS.Helpers.Extensions;
using EduCATS.Helpers.Files;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Logs;
using Moq;
using NUnit.Framework;

namespace EduCATS.UnitTests
{
	[TestFixture]
	public class PerformanceHelpersTests
	{
		[Test]
		public void InsertRangeRaisesOneNotificationTest()
		{
			var collection = new RangeObservableCollection<int>(new[] { 4, 5 });
			var events = new List<NotifyCollectionChangedEventArgs>();
			collection.CollectionChanged += (sender, e) => events.Add(e);

			collection.InsertRange(0, new[] { 1, 2, 3 });

			Assert.That(collection, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
			Assert.That(events.Count, Is.EqualTo(1));
			Assert.That(events[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
			Assert.That(events[0].NewStartingIndex, Is.EqualTo(0));
			Assert.That(events[0].NewItems.Count, Is.EqualTo(3));
		}

		[Test]
		public void InsertEmptyRangeDoesNothingTest()
		{
			var collection = new RangeObservableCollection<int>();
			var raised = false;
			collection.CollectionChanged += (sender, e) => raised = true;

			collection.InsertRange(0, new int[0]);

			Assert.That(raised, Is.False);
		}

		[Test]
		public void LogsAreWrittenInBatchesWithoutLossTest()
		{
			var written = new StringBuilder();
			var appendCalls = 0;
			var mock = new Mock<IFileManager>();
			mock.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
			mock.Setup(m => m.GetFileSize(It.IsAny<string>())).Returns(0);
			mock.Setup(m => m.Append(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((path, data) =>
				{
					lock (written)
					{
						appendCalls++;
						written.Append(data);
					}
				});

			AppLogs.FileManager = mock.Object;
			AppLogs.Initialize(string.Empty);

			for (var index = 0; index < 50; index++)
			{
				AppLogs.Log($"line {index};");
			}

			AppLogs.Flush();

			lock (written)
			{
				for (var index = 0; index < 50; index++)
				{
					Assert.That(written.ToString(), Does.Contain($"line {index};"));
				}

				Assert.That(appendCalls, Is.LessThan(50));
			}
		}

		[Test]
		public void Base64ImagesAreCachedTest()
		{
			var converter = new Base64ToImageSourceConverter();
			var base64 = System.Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });

			var first = converter.Convert(base64, null, null, null);
			var second = converter.Convert(base64, null, null, null);

			Assert.That(first, Is.Not.Null);
			Assert.That(second, Is.SameAs(first));
		}
	}
}
