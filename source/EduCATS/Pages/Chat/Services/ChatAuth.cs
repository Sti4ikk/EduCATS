using System;
using System.Net.Http;
using System.Net.Http.Headers;
using EduCATS.Helpers.Forms;

namespace EduCATS.Pages.Chat.Services
{
	/// <summary>
	/// Access token for the chat service.
	/// </summary>
	/// <remarks>
	/// The chat server currently trusts the <c>userId</c> passed in requests.
	/// The token is sent so that the server can verify the user; until the
	/// server checks it, anonymous endpoints simply ignore it.
	/// Never send it to other hosts (e.g. link previews).
	/// </remarks>
	public static class ChatAuth
	{
		const string _bearerPrefix = "Bearer ";

		/// <summary>
		/// Raw token (without the "Bearer" scheme), <c>null</c> if not logged in.
		/// </summary>
		public static string GetToken()
		{
			var token = PlatformServices.Current.Preferences.AccessToken?.Trim();

			if (string.IsNullOrEmpty(token))
			{
				return null;
			}

			return token.StartsWith(_bearerPrefix, StringComparison.OrdinalIgnoreCase) ?
				token.Substring(_bearerPrefix.Length).Trim() :
				token;
		}

		/// <summary>
		/// Add the Authorization header to a chat service request.
		/// </summary>
		public static void Apply(HttpRequestMessage request)
		{
			var token = GetToken();

			if (token != null)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}
	}
}
