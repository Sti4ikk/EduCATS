using System;

namespace EduCATS.Pages.Chat.Models
{
	public class IncomingCallModel
	{
		public int ChatId { get; set; }

		public string CallerUserId { get; set; }
	}

	public class CallCandidateModel
	{
		public object Candidate { get; set; }

		public string ConnectionId { get; set; }
	}
}