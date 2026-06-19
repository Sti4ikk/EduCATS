using Newtonsoft.Json;

namespace EduCATS.Data.Models
{
	/// <summary>
	/// Test answer model.
	/// </summary>
	public class TestAnswerModel
	{
		/// <summary>
		/// Answer title.
		/// </summary>
		[JsonProperty("Content")]
		public string Content { get; set; }

		/// <summary>
		/// Is answer correct.
		/// </summary>
		[JsonProperty("ÑorrectnessIndicator")]
		public int ÑorrectnessIndicator { get; set; }

		/// <summary>
		/// Answer ID.
		/// </summary>
		[JsonProperty("Id")]
		public int Id { get; set; }
	}
}

