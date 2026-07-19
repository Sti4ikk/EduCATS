using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Mirrors server-side Entities.CTO.MessageCto - the payload sent
	/// to MessageHub.SendMessage as a JSON string.
	/// </summary>
	public class MessageSendModel
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("imageContent")]
		public string ImageContent { get; set; }

		[JsonProperty("isimage")]
		public bool? IsImage { get; set; }

		[JsonProperty("isfile")]
		public bool? IsFile { get; set; }

		[JsonProperty("fileContent")]
		public string FileContent { get; set; }

		[JsonProperty("fileSize")]
		public string FileSize { get; set; }

		[JsonProperty("chatId")]
		public int ChatId { get; set; }

		[JsonProperty("userId")]
		public int UserId { get; set; }
	}
}