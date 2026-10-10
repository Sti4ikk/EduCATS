using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EduCATS.Helpers.Extensions
{
	/// <summary>
	/// Observable collection that inserts many items with one notification.
	/// </summary>
	/// <remarks>
	/// A list re-lays itself out on every notification: inserting a page of
	/// items one by one made it do that dozens of times in a row.
	/// </remarks>
	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		public RangeObservableCollection()
		{
		}

		public RangeObservableCollection(IEnumerable<T> items) : base(items)
		{
		}

		/// <summary>
		/// Insert items starting at the index.
		/// </summary>
		public void InsertRange(int index, IList<T> items)
		{
			if (items == null || items.Count == 0)
			{
				return;
			}

			CheckReentrancy();

			for (var i = 0; i < items.Count; i++)
			{
				Items.Insert(index + i, items[i]);
			}

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add, (IList)new List<T>(items), index));
		}
	}
}
