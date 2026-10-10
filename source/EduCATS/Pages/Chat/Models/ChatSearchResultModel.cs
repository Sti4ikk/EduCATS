using System;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Message found by the search through all chats.
	/// </summary>
	public class ChatSearchResultModel
	{
		public int ChatId { get; set; }

		public bool IsGroupChat { get; set; }

		/// <summary>
		/// Academic group id (group chats only).
		/// </summary>
		public int GroupId { get; set; }

		/// <summary>
		/// Other participant's id (personal chats only).
		/// </summary>
		public int PeerUserId { get; set; }

		public string ChatTitle { get; set; }

		/// <summary>
		/// Author and text of the message.
		/// </summary>
		public string Snippet { get; set; }

		public DateTime Time { get; set; }

		public string DisplayTime => Time.Date == DateTime.Today ?
			Time.ToString("HH:mm") :
			Time.ToString("dd.MM.yyyy");
	}
}
