using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Data.Caching;
using EduCATS.Data.Interfaces;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Data
{
	/// <summary>
	/// Helper partial class for <see cref="DataAccess"/>.
	/// </summary>
	public static partial class DataAccess
	{
		/// <summary>
		/// Delete data cache.
		/// </summary>
		public static void ResetData()
		{
			DataCaching<object>.RemoveCache();
		}

		/// <summary>
		/// Get single object with error details.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="dataAccess">Data Access instance.</param>
		/// <returns>Object with error details.</returns>
		public async static Task<DataResult<T>> GetSingleData<T>(IDataAccess<T> dataAccess)
		{
			var data = await dataAccess.GetSingle();
			return CreateResult(data, dataAccess);
		}

		/// <summary>
		/// Get objects list with error details.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="dataAccess">Data Access instance.</param>
		/// <returns>Objects list with error details.</returns>
		public async static Task<DataResult<List<T>>> GetListData<T>(IDataAccess<T> dataAccess)
		{
			var data = await dataAccess.GetList();
			return CreateResult(data, dataAccess);
		}

		/// <summary>
		/// Get complex key with identifiers.
		/// </summary>
		/// <param name="key">Basic key.</param>
		/// <param name="firstId">First ID.</param>
		/// <param name="secondId">Second ID.</param>
		/// <returns></returns>
		public static string GetKey(string key, object firstId, object secondId)
		{
			return $"{GetKey(key, firstId)}/{secondId}";
		}

		/// <summary>
		/// Get complex key with identifier.
		/// </summary>
		/// <param name="key">Basic key.</param>
		/// <param name="id"><ID./param>
		/// <returns></returns>
		public static string GetKey(string key, object id)
		{
			return $"{key}/{id}";
		}

		/// <summary>
		/// Create result with error details of the finished request.
		/// </summary>
		/// <typeparam name="TData">Data type.</typeparam>
		/// <typeparam name="TItem">Data Access type.</typeparam>
		/// <param name="data">Data.</param>
		/// <param name="dataAccess">Finished Data Access instance.</param>
		/// <remarks>
		/// <see cref="IDataAccess{T}.ErrorMessageKey"/> is used as-is if it's
		/// a raw message (e.g. text received directly from the server),
		/// otherwise it's passed through <see cref="CrossLocalization.Translate"/>.
		/// </remarks>
		/// <returns>Result.</returns>
		public static DataResult<TData> CreateResult<TData, TItem>(TData data, IDataAccess<TItem> dataAccess)
		{
			var messageKey = dataAccess.ErrorMessageKey;

			if (messageKey == null)
			{
				return new DataResult<TData>(data);
			}

			var message = dataAccess.IsRawErrorMessage ?
				messageKey : CrossLocalization.Translate(messageKey);

			return new DataResult<TData>(
				data,
				message,
				dataAccess.IsConnectionError,
				dataAccess.IsSessionExpiredError);
		}
	}
}
