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

		[JsonProperty("align")]
		public string Align { get; set; }

		[JsonProperty("chatId")]
		public int ChatId { get; set; }

		[JsonIgnore]
		public bool IsMine { get; set; }

		[JsonIgnore]
		public bool IsImageMessage => IsImage == true;

		[JsonIgnore]
		public bool IsFileMessage => IsFile == true;


		[JsonIgnore]
		public DateTime LocalTime =>
			DateTime.SpecifyKind(Time, DateTimeKind.Utc).ToLocalTime();

		/// <summary>
		/// True когда сообщение - обычный текст, без вложений.
		/// Используется для показа/скрытия textLabel в ячейке чата.
		/// </summary>
		[JsonIgnore]
		public bool IsPlainText => !IsImageMessage && !IsFileMessage;

		/// <summary>
		/// "имя_файла.pdf (123.4 KB)" для чипа файла в UI.
		/// </summary>
		[JsonIgnore]
		public string FileDisplayText
		{
			get
			{
				var fileName = !string.IsNullOrEmpty(FileContent) ? FileContent : Text;
				return string.IsNullOrEmpty(FileSize) ? fileName : $"{fileName} ({FileSize})";
			}
		}
	}
}