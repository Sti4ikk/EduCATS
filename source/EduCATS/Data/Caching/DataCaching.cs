using System;
using System.Threading.Tasks;
using EduCATS.Constants;
using MonkeyCache.FileStore;

namespace EduCATS.Data.Caching
{
	/// <summary>
	/// Data caching with <c>MonkeyCache</c>.
	/// </summary>
	/// <typeparam name="T">Type to cache.</typeparam>
	public static class DataCaching<T>
	{
		/// <summary>
		/// Delete all cache.
		/// </summary>
		public static void RemoveCache()
		{
			Barrel.Current.EmptyAll();
		}

		/// <summary>
		/// Save data cache for specified key.
		/// </summary>
		/// <param name="key">Key for data.</param>
		/// <param name="data">Data to cache.</param>
		public static void Save(string key, T data)
		{
			Barrel.Current.Add(key, data, TimeSpan.FromDays(GlobalConsts.CacheExpirationInDays));
		}

		/// <summary>
		/// Get data cahce for key.
		/// </summary>
		/// <param name="key">Key for data.</param>
		/// <returns>Cached data if key exists.</returns>
		/// <remarks>
		/// Expired data isn't returned (it's deleted). Only this key is checked:
		/// cleaning the whole cache on every read walked through all of it.
		/// </remarks>
		public static T Get(string key)
		{
			DataCachingCleanup.RunOnce();

			if (!Barrel.Current.Exists(key))
			{
				return default;
			}

			if (Barrel.Current.IsExpired(key))
			{
				Barrel.Current.Empty(key);
				return default;
			}

			return Barrel.Current.Get<T>(key);
		}
	}

	/// <summary>
	/// Removes expired cache entries once per app run, in the background.
	/// </summary>
	/// <remarks>
	/// Expiration time is specified in
	/// <see cref="GlobalConsts.CacheExpirationInDays"/> constant.
	/// </remarks>
	static class DataCachingCleanup
	{
		static int _isStarted;

		public static void RunOnce()
		{
			if (System.Threading.Interlocked.Exchange(ref _isStarted, 1) == 1)
			{
				return;
			}

			Task.Run(() =>
			{
				try
				{
					Barrel.Current.EmptyExpired();
				}
				catch
				{
					// Cleanup is best effort: the next run retries.
				}
			});
		}
	}
}
