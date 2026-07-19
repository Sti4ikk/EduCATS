namespace EduCATS.Networking
{
	/// <summary>
	/// Chat microservice API links.
	/// </summary>
	/// <remarks>
	/// The chat service is a separate .NET 8 microservice (ChatServer),
	/// proxied on the same domain under <c>/catService/</c> (REST) and
	/// <c>/chatSignalR/</c> (real-time hub), based on the reverse-proxy
	/// rewrite rules in the CATSdesigner web frontend
	/// (<c>server/src/app.ts</c>): <c>^/catService -> /ChatApi</c>.
	/// Unlike the main LMPlatform API, these endpoints currently have
	/// no authentication/authorization on the server side.
	/// </remarks>
	public static class ChatLinks
	{
		/// <summary>
		/// Get all personal chats for a user.
		/// </summary>
		public static string GetAllChats => $"{Servers.Current}/catService/Chat/GetAllChats";

		/// <summary>
		/// Get all subject/group chats for a user.
		/// </summary>
		public static string GetAllGroups => $"{Servers.Current}/catService/Chat/GetAllGroups";

		/// <summary>
		/// Get personal chat message history.
		/// </summary>
		public static string GetChatMsgs => $"{Servers.Current}/catService/Message/GetChatMsgs";

		/// <summary>
		/// Get group chat message history.
		/// </summary>
		public static string GetGroupMsgs => $"{Servers.Current}/catService/Message/GetGroupMsgs";

		/// <summary>
		/// Get basic user info (name, avatar, online status).
		/// </summary>
		public static string GetUserInfo => $"{Servers.Current}/catService/Chat/GetUserInfo";

		/// <summary>
		/// Create (or get existing) a personal chat between two users.
		/// </summary>
		public static string CreateChat => $"{Servers.Current}/catService/Chat/CreateChat";

		/// <summary>
		/// SignalR hub for real-time chat messaging.
		/// </summary>
		public static string ChatHub => $"{Servers.Current}/chatSignalR";

		public static string UpdateReadChat => $"{Servers.Current}/catService/Chat/UpdateReadChat";
	}
}