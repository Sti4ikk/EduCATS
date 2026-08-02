using System.Linq;
using Newtonsoft.Json;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Одна запись в ростере группы (попап "Список студентов").
	/// </summary>
	public class StudentItemModel
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("firstName")]
		public string FirstName { get; set; }

		[JsonProperty("lastName")]
		public string LastName { get; set; }

		[JsonProperty("patronymic")]
		public string Patronymic { get; set; }

		[JsonProperty("avatarUrl")]
		public string AvatarUrl { get; set; }

		/// <summary>
		/// "Фамилия Имя Отчество" для отображения в списке.
		/// </summary>
		public string FullName
		{
			get
			{
				return string.Join(" ", new[] { LastName, FirstName, Patronymic }
					.Where(part => !string.IsNullOrWhiteSpace(part)));
			}
		}

		/// <summary>
		/// Инициалы для заглушки-аватара ("Б" + "О" для "Берестнева Ольга ...").
		/// </summary>
		public string Initials
		{
			get
			{
				var first = string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString();
				var second = string.IsNullOrEmpty(FirstName) ? "" : FirstName[0].ToString();
				return (first + second).ToUpperInvariant();
			}
		}

		public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

		public bool NoAvatar => !HasAvatar;
	}
}