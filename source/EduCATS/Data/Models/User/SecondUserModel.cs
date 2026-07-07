using Newtonsoft.Json;
using System.Collections.Generic;

namespace EduCATS.Data.Models.User
{
	public class SecondUserModel
	{
		[JsonProperty("userName")]
		public string Username { get; set; }

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("role")]
		public string role { get; set; }
	}
}