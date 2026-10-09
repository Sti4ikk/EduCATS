using Newtonsoft.Json;
using System;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// A single personal chat entry, as returned by
	/// <c>ChatApi/Chat/GetAllChats</c>.
	/// </summary>
	/// <remarks>
	/// Mirrors the server-side <c>ChatDto</c> (Entities/DTO/ChatDto.cs
	/// in the ChatServer repository). ASP.NET Core's default
	/// System.Text.Json serializer outputs camelCase property names,
	/// which is what the JsonProperty attributes below assume - verify
	/// via a quick DevTools check on the real response if any field
	/// comes back empty/null.
	/// </remarks>
	public class ChatItemModel : ChatListItemModel
	{
		/// <summary>
		/// Id of the other participant.
		/// </summary>
		[JsonProperty("userId")]
		public int UserId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Avatar, typically a base64 data URI
		/// (e.g. "data:image/png;base64,...").
		/// </summary>
		[JsonProperty("img")]
		public string Img { get; set; }

		bool? _isOnline;

		/// <summary>
		/// Is the other participant online (updated live by the "Status" hub event).
		/// </summary>
		[JsonProperty("isOnline")]
		public bool? IsOnline
		{
			get => _isOnline;
			set
			{
				if (SetProperty(ref _isOnline, value))
				{
					OnPropertyChanged(nameof(IsOnlineVisible));
				}
			}
		}

		[JsonIgnore]
		public bool IsOnlineVisible => IsOnline == true;

		[JsonProperty("lastMessageDate")] // Замените на реальное название ключа из вашего API
		public DateTime? LastMessageDate { get; set; }

		[JsonIgnore]
		public override bool IsGroupChat => false;
	}
}
