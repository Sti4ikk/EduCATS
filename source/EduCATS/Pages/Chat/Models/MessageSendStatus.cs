namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Sending status of an own message.
	/// </summary>
	/// <remarks>
	/// "Read" is not available: the server doesn't report read receipts.
	/// </remarks>
	public enum MessageSendStatus
	{
		/// <summary>
		/// Not an own message.
		/// </summary>
		None,

		/// <summary>
		/// Being uploaded / handed to the server.
		/// </summary>
		Sending,

		/// <summary>
		/// Accepted by the server.
		/// </summary>
		Sent,

		/// <summary>
		/// Not sent, can be retried.
		/// </summary>
		Failed
	}
}
