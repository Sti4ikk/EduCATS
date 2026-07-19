using Newtonsoft.Json;
using System;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Mirrors server-side Entities.DTO.MessageDto.
	/// </summary>
	public class MessageItemModel
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("time")]
		public System.DateTime Time { get; set; }

		[JsonProperty("imageContent")]
		public string[] ImageContent { get; set; }

		[JsonProperty("isimage")]
		public bool? IsImage { get; set; }

		[JsonProperty("isfile")]
		public bool? IsFile { get; set; }

		[JsonProperty("fileContent")]
		public string FileContent { get; set; }

		[JsonProperty("fileSize")]
		public string FileSize { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("profile")]
		public string Profile { get; set; }

		/// <summary>
		/// "left" or "right" - who the message belongs to.
		/// Null in some server responses; computed client-side as fallback.
		/// </summary>
		[JsonProperty("align")]
		public string Align { get; set; }

		[JsonProperty("chatId")]
		public int ChatId { get; set; }

		/// <summary>
		/// Not part of server DTO - set locally by comparing
		/// Name/sender to the current user, used for bubble alignment
		/// when Align comes back null (server clears it for broadcast
		/// - see MessageHub.SendMessage: msg.Align = null before
		/// sending to the group).
		/// </summary>
		[JsonIgnore]
		public bool IsMine { get; set; }

		/// <summary>
		/// Local (device timezone) version of <see cref="Time"/>.
		/// Server sends message timestamps in UTC without an explicit
		/// offset, so DateTimeKind comes back Unspecified - we treat it
		/// as UTC explicitly and convert to local time for display.
		/// </summary>
		[JsonIgnore]
		public DateTime LocalTime =>
			DateTime.SpecifyKind(Time, DateTimeKind.Utc).ToLocalTime();
	}
}