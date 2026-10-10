using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;

namespace EduCATS.Pages
{
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Start an async operation without awaiting it
		/// (from a constructor or a property setter).
		/// </summary>
		/// <remarks>
		/// Runs on the main thread: the operation updates bound properties and
		/// collections, which must not be changed from a background thread
		/// (network calls are async anyway). Errors are logged instead of being lost.
		/// </remarks>
		/// <param name="services">Platform services.</param>
		/// <param name="operation">Operation.</param>
		protected static void RunOnMainThread(IPlatformServices services, Func<Task> operation)
		{
			services.Device.MainThread(async () =>
			{
				try
				{
					await operation();
				}
				catch (Exception ex)
				{
					AppLogs.Log(ex);
				}
			});
		}
	}
}

