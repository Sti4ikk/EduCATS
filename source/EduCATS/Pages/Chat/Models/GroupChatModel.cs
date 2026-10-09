using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Mirrors server-side Entities.DTO.GroupChatDto - a single group
	/// chat inside a subject (usually per student sub-group).
	/// </summary>
	public class GroupChatModel : ChatListItemModel
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("img")]
		public string Img { get; set; }

		[JsonProperty("groupId")]
		public int GroupId { get; set; }

		[JsonProperty("isActiveOnCurrentGroup")]
		public bool IsActiveOnCurrentGroup { get; set; }

		[JsonProperty("isCompletedForUser")]
		public bool IsCompletedForUser { get; set; }

		/// <summary>
		/// Not part of the server DTO - filled in client-side from the
		/// parent <see cref="SubjectChatsModel"/> when flattening the
		/// two-level subject/group hierarchy into a single list, so the
		/// row can show "Subject — Group name".
		/// </summary>
		[JsonIgnore]
		public string SubjectName { get; set; }

		[JsonIgnore]
		public string DisplayName => string.IsNullOrEmpty(SubjectName)
			? Name
			: $"{SubjectName} — {Name}";

		[JsonIgnore]
		public override bool IsGroupChat => true;
	}
}
