namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Link preview (Open Graph data of a web page).
	/// </summary>
	public class LinkPreviewModel
	{
		public string Url { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string ImageUrl { get; set; }

		public string Host { get; set; }

		public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

		public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
	}
}
