using EduCATS.Helpers.Forms;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Chat role of the current user.
	/// </summary>
	public static class ChatRoles
	{
		public const string Lecturer = "lector";
		public const string Student = "student";

		/// <summary>
		/// Role the user joined the chat with, or a guess
		/// (professors have no group) if not joined yet.
		/// </summary>
		/// <param name="services">Platform services.</param>
		/// <returns>Role.</returns>
		public static string Current(IPlatformServices services) =>
			ChatHubService.CurrentRole ??
			(string.IsNullOrEmpty(services.Preferences.GroupName) ? Lecturer : Student);
	}
}
