using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EduCATS.Data.Caching;
using EduCATS.Data.Interfaces;
using EduCATS.Demo;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Json;

namespace EduCATS.Data
{
	/// <summary>
	/// Data Access helper.
	/// </summary>
	/// <typeparam name="T">Type for data.</typeparam>
	public partial class DataAccess<T> : IDataAccess<T> where T : new()
	{
		/// <summary>
		/// Success string response.
		/// </summary>
		const string _nonJsonSuccessResponse = "\"Ok\"";

		/// <summary>
		/// Known server error codes mapped to localization keys.
		/// </summary>
		/// <remarks>
		/// <c>1</c> = account not confirmed by teacher yet.
		/// Add new codes here as the server introduces them.
		/// </remarks>
		static readonly Dictionary<int, string> _serverErrorCodeKeys = new Dictionary<int, string>
		{
			{ 1, "login_user_profile_not_verify" },
			{ 3, "login_invalid_credentials" },
		};

		/// <summary>
		/// Caching key.
		/// </summary>
		readonly string _key;

		/// <summary>
		/// Is caching enabled.
		/// </summary>
		readonly bool _isCaching;

		/// <summary>
		/// Error message.
		/// </summary>
		readonly string _messageForError;

		/// <summary>
		/// Callback to invoke.
		/// </summary>
		static Func<Task<KeyValuePair<string, HttpStatusCode>>> _callback;

		/// <summary>
		/// Is error occurred.
		/// </summary>
		public bool IsError { get; set; }

		/// <summary>
		/// Is network connection issue.
		/// </summary>
		public bool IsConnectionError { get; set; }

		/// <summary>
		/// Is session expired issue.
		/// </summary>
		public bool IsSessionExpiredError { get; set; }

		/// <summary>
		/// Error message localized key.
		/// </summary>
		public string ErrorMessageKey { get; set; }

		/// <summary>
		/// Is <see cref="ErrorMessageKey"/> a ready-to-display message
		/// (e.g. taken directly from server response) rather than
		/// a localization key that needs translating.
		/// </summary>
		public bool IsRawErrorMessage { get; set; }

		/// <summary>
		/// Platform services.
		/// </summary>
		readonly IPlatformServices _services;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="messageForError">Error message.</param>
		/// <param name="callback">Callback to invoke.</param>
		/// <param name="key">Caching key.</param>
		public DataAccess(
			string messageForError,
			Task<object> callback,
			string key = null,
			IPlatformServices services = null)
		{
			_key = key;
			_messageForError = messageForError;
			_services = services ?? new PlatformServices();
			_isCaching = !string.IsNullOrEmpty(_key);
			setCallback(callback);
		}

		/// <summary>
		/// Get single object.
		/// </summary>
		/// <returns>Single object.</returns>
		public async Task<T> GetSingle()
		{
			var singleObject = checkSingleObjectReadyForResponse();

			if (singleObject != null)
			{
				return singleObject;
			}

			var response = await _callback();
			System.Diagnostics.Debug.WriteLine($"=== GetSingle: StatusCode={response.Value}, Body={response.Key}");

			if (response.Value != HttpStatusCode.OK)
			{
				handleErrorResponse(response);
				return new T();
			}

			singleObject = GetAccess(response);

			if (singleObject == null)
			{
				setError(_messageForError);
				return new T();
			}

			return singleObject;
		}

		/// <summary>
		/// Get objects list.
		/// </summary>
		/// <returns>Objects list.</returns>
		public async Task<List<T>> GetList()
		{
			var list = checkListReadyForResponse();

			if (list != null)
			{
				return list;
			}

			var response = await _callback();

			if (response.Value != HttpStatusCode.OK)
			{
				handleErrorResponse(response);
				return new List<T>();
			}

			list = GetListAccess(response);

			if (list == null)
			{
				setError(_messageForError);
				return new List<T>();
			}

			return list;
		}

		/// <summary>
		/// Get data from cache
		/// and set connection error.
		/// </summary>
		/// <returns>Data object.</returns>
		T checkSingleObjectReadyForResponse()
		{
			if (CheckConnectionEstablished())
			{
				return default;
			}

			var data = getCacheAndSetConnectionError();
			return JsonController<T>.ConvertJsonToObject(data) ?? new T();
		}

		/// <summary>
		/// Get data list from cache
		/// and set connection error.
		/// </summary>
		/// <returns>List of data.</returns>
		List<T> checkListReadyForResponse()
		{
			if (CheckConnectionEstablished())
			{
				return null;
			}

			var data = getCacheAndSetConnectionError();
			var list = JsonController<List<T>>.ConvertJsonToObject(data);
			return list ?? new List<T>();
		}

		/// <summary>
		/// Get cache and set connection error.
		/// </summary>
		/// <returns>Data in string format (<c>json</c>).</returns>
		string getCacheAndSetConnectionError()
		{
			setError("base_connection_error", true);
			return _key == null ? null : getDataFromCache(_key);
		}

		/// <summary>
		/// Parse response and get list of objects.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <returns>List of objects.</returns>
		public List<T> GetListAccess(KeyValuePair<string, HttpStatusCode> response)
		{
			switch (response.Value)
			{
				case HttpStatusCode.OK:
					var data = parseResponse(response, _key, _isCaching);

					if (data.Equals(_nonJsonSuccessResponse))
					{
						return new List<T>();
					}

					if (!JsonController.IsJsonValid(data))
					{
						return default;
					}

					return JsonController<List<T>>.ConvertJsonToObject(data);
				default:
					return default;
			}
		}

		/// <summary>
		/// Parse response and get object.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <returns>Object.</returns>
		public T GetAccess(KeyValuePair<string, HttpStatusCode> response)
		{
			switch (response.Value)
			{
				case HttpStatusCode.OK:
					var data = parseResponse(response, _key, _isCaching);

					if (data.Equals(_nonJsonSuccessResponse))
					{
						return new T();
					}

					if (!JsonController.IsJsonValid(data))
					{
						return default;
					}

					return JsonController<T>.ConvertJsonToObject(data);
				default:
					return default;
			}
		}

		/// <summary>
		/// Determine and set the correct error for a non-<c>200</c> response.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <remarks>
		/// Priority: known server error code (localized) &gt;
		/// raw server description (fallback) &gt; HTTP-status-based default.
		/// </remarks>
		void handleErrorResponse(KeyValuePair<string, HttpStatusCode> response)
		{
			var serverError = tryParseServerError(response.Key);

			if (serverError != null &&
				_serverErrorCodeKeys.TryGetValue(serverError.Error, out var localizationKey))
			{
				setError(localizationKey, sessionExpired: false);
				return;
			}

			if (!string.IsNullOrWhiteSpace(serverError?.Description))
			{
				setError(serverError.Description, sessionExpired: false, isRawMessage: true);
				return;
			}

			if (response.Value == HttpStatusCode.Unauthorized)
			{
				setError("base_session_expired", sessionExpired: true);
				return;
			}

			setError(_messageForError);
		}

		/// <summary>
		/// Try to parse the server error payload from the response body.
		/// </summary>
		/// <param name="rawBody">Raw response body.</param>
		/// <returns>Parsed model, or <c>null</c> if body is not valid JSON.</returns>
		static ServerErrorModel tryParseServerError(string rawBody)
		{
			if (!JsonController.IsJsonValid(rawBody))
			{
				return null;
			}

			return JsonController<ServerErrorModel>.ConvertJsonToObject(rawBody);
		}

		/// <summary>
		/// Server error payload. Can be returned by the server
		/// alongside non-200 status codes (e.g. 400, 401).
		/// </summary>
		class ServerErrorModel
		{
			public int Error { get; set; }

			public string Description { get; set; }
		}

		/// <summary>
		/// Set error details.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="sessionExpired">Session expired error.</param>
		/// <param name="isConnectionError">Is connection error.</param>
		/// <param name="isRawMessage">
		/// Is <paramref name="message"/> a ready-to-display message
		/// (e.g. from server response) rather than a localization key.
		/// </param>
		void setError(
			string message,
			bool sessionExpired = true,
			bool isConnectionError = false,
			bool isRawMessage = false)
		{
			IsError = true;
			ErrorMessageKey = message;
			IsConnectionError = isConnectionError;
			IsSessionExpiredError = sessionExpired;
			IsRawErrorMessage = isRawMessage;
		}

		/// <summary>
		/// Parse response.
		/// </summary>
		/// <param name="responseObject">Response object.</param>
		/// <param name="key">Caching key.</param>
		/// <param name="isCaching">Is caching enabled.</param>
		/// <returns>Json string.</returns>
		string parseResponse(object responseObject, string key = null, bool isCaching = true)
		{
			var response = (KeyValuePair<string, HttpStatusCode>)responseObject;

			if (isCaching && !string.IsNullOrEmpty(key))
			{
				DataCaching<string>.Save(key, response.Key);
			}

			return response.Key;
		}

		/// <summary>
		/// Get data from cache.
		/// </summary>
		/// <param name="key">Caching key.</param>
		/// <returns>Json string.</returns>
		static string getDataFromCache(string key)
		{
			return DataCaching<string>.Get(key);
		}

		/// <summary>
		/// Set callback variable.
		/// </summary>
		/// <param name="callback">Callback object.</param>
		static void setCallback(Task<object> callback)
		{
			if (callback == null)
			{
				_callback = async () => {
					await Task.Run(() => { });
					return new KeyValuePair<string, HttpStatusCode>();
				};
				return;
			}

			_callback = async () => {
				var result = await callback;
				return (KeyValuePair<string, HttpStatusCode>)result;
			};
		}

		/// <summary>
		/// Check network connection.
		/// </summary>
		/// <returns><c>True</c> if established.</returns>
		public virtual bool CheckConnectionEstablished()
		{
			if (AppDemo.Instance.IsDemoAccount)
			{
				return true;
			}

			return _services.Device.CheckConnectivity();
		}
	}
}