using Newtonsoft.Json;
using EduCATS.Controls.RoundedListView.Enums;
using EduCATS.Controls.RoundedListView.Interfaces;

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
	public class ChatItemModel : IRoundedListType // 1. Добавили интерфейс
	{
		[JsonProperty("id")]
		public int Id { get; set; }

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

		[JsonProperty("unread")]
		public int Unread { get; set; }

		[JsonProperty("isOnline")]
		public bool? IsOnline { get; set; }

		/// <summary>
		/// 2. Реализация метода интерфейса IRoundedListType.
		/// Указывает RoundedListView использовать шаблон навигации (кликов) для этой строки.
		/// </summary>
		public RoundedListTypeEnum GetListType()
		{
			return RoundedListTypeEnum.Navigation;
		}
	}
}