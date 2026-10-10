using System;
using System.Globalization;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// A non-message item inserted into the conversation list to show
	/// "Today" / "12.07.2026" style date dividers between messages
	/// from different days.
	/// </summary>
	public class DateSeparatorModel
	{
		public DateSeparatorModel(DateTime date)
		{
			Date = date.Date;
		}

		public DateTime Date { get; }

		public string DisplayText
		{
			get
			{
				var today = DateTime.Now.Date;

				if (Date == today)
				{
					return CrossLocalization.Translate("chat_today");
				}

				if (Date == today.AddDays(-1))
				{
					return CrossLocalization.Translate("chat_yesterday");
				}

				return Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
			}
		}
	}
}