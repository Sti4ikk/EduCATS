using System;
using EduCATS.Helpers.Forms.Devices;
using EduCATS.Helpers.Forms.Dialogs;
using EduCATS.Helpers.Forms.Pages;
using EduCATS.Helpers.Forms.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace EduCATS.Helpers.Forms
{
	/// <summary>
	/// Platform services.
	/// </summary>
	public class PlatformServices : IPlatformServices
	{
		static IServiceProvider _serviceProvider;
		static readonly Lazy<IPlatformServices> _fallback = new Lazy<IPlatformServices>(() => new PlatformServices());

		/// <summary>
		/// Device.
		/// </summary>
		public IDevice Device { get; set; }

		/// <summary>
		/// Dialogs.
		/// </summary>
		public IDialogs Dialogs { get; set; }

		/// <summary>
		/// Navigation.
		/// </summary>
		public IPages Navigation { get; set; }

		/// <summary>
		/// Preferences.
		/// </summary>
		public IPreferences Preferences { get; set; }

		public PlatformServices() {
			Device = new AppDevice();
			Dialogs = new AppDialogs();
			Navigation = new AppPages();
			Preferences = new AppPrefs();
		}

		/// <summary>
		/// Shared instance registered in the DI container (<c>MauiProgram</c>).
		/// </summary>
		/// <remarks>
		/// For code that isn't created by the container (pages are created
		/// with constructor arguments by <see cref="AppPages"/>).
		/// Falls back to a single local instance when there is no container (unit tests).
		/// </remarks>
		public static IPlatformServices Current =>
			_serviceProvider?.GetService<IPlatformServices>() ?? _fallback.Value;

		/// <summary>
		/// Set the DI container (called once by <c>MauiProgram</c>).
		/// </summary>
		/// <param name="serviceProvider">Service provider.</param>
		public static void SetServiceProvider(IServiceProvider serviceProvider) =>
			_serviceProvider = serviceProvider;

		/// <summary>
		/// Register platform services in a DI container.
		/// </summary>
		/// <param name="services">Service collection.</param>
		/// <returns>Service collection.</returns>
		public static IServiceCollection Register(IServiceCollection services) =>
			services.AddSingleton<IPlatformServices, PlatformServices>();
	}
}
