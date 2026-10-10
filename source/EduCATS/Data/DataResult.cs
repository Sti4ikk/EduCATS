using System;

namespace EduCATS.Data
{
	/// <summary>
	/// Result of a data request: fetched data together with
	/// the error details of this particular request.
	/// </summary>
	/// <remarks>
	/// Error details belong to the result rather than to shared static state,
	/// so concurrent requests can't overwrite each other's errors.
	/// </remarks>
	/// <typeparam name="T">Data type.</typeparam>
	public class DataResult<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="errorMessage">
		/// Ready-to-display error message, <c>null</c> if no error occurred.
		/// </param>
		/// <param name="isConnectionError">Is network connection issue.</param>
		/// <param name="isSessionExpiredError">Is session expired issue.</param>
		public DataResult(
			T data,
			string errorMessage = null,
			bool isConnectionError = false,
			bool isSessionExpiredError = false)
		{
			Data = data;

			if (errorMessage == null)
			{
				return;
			}

			IsError = true;
			ErrorMessage = errorMessage;
			IsConnectionError = isConnectionError;
			IsSessionExpiredError = isSessionExpiredError;
		}

		/// <summary>
		/// Data.
		/// </summary>
		/// <remarks>
		/// On error contains cached data (if any) or an empty object.
		/// </remarks>
		public T Data { get; }

		/// <summary>
		/// Is error occurred.
		/// </summary>
		public bool IsError { get; }

		/// <summary>
		/// Is network connection issue.
		/// </summary>
		public bool IsConnectionError { get; }

		/// <summary>
		/// Is session expired issue.
		/// </summary>
		public bool IsSessionExpiredError { get; }

		/// <summary>
		/// Error message.
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// Convert data keeping error details.
		/// </summary>
		/// <typeparam name="TResult">Converted data type.</typeparam>
		/// <param name="map">Conversion function.</param>
		/// <returns>Converted result.</returns>
		public DataResult<TResult> Map<TResult>(Func<T, TResult> map) =>
			new DataResult<TResult>(
				Data == null ? default : map(Data),
				ErrorMessage,
				IsConnectionError,
				IsSessionExpiredError);
	}
}
