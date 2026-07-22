using System.Collections.Generic;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Mirrors server-side Entities.DTO.SubjectChatsDto - a subject
	/// with its list of group chats.
	/// </summary>
	public class SubjectChatsModel
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("shortName")]
		public string ShortName { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		[JsonProperty("unread")]
		public int Unread { get; set; }

		[JsonProperty("groups")]
		public List<GroupChatModel> Groups { get; set; }

		[JsonProperty("isArchived")]
		public bool IsArchived { get; set; }

		[JsonProperty("isCompletedForUser")]
		public bool IsCompletedForUser { get; set; }
	}
}