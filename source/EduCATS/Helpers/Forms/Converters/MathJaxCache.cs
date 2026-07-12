using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace EduCATS.Helpers.Forms.Converters
{
	/// <summary>
	/// Loads and caches the MathJax script bundled as a local asset,
	/// so question descriptions with MathML formulas render inside the
	/// WebView without depending on a CDN network request (which can be
	/// unreliable/blocked on some Android emulators/devices).
	/// </summary>
	public static class MathJaxCache
	{
		/// <summary>
		/// Path of the bundled script inside Resources/Raw.
		/// </summary>
		const string _assetPath = "mathjax/mml-svg.js";

		/// <summary>
		/// Cached script contents once loaded.
		/// </summary>
		public static string Script { get; private set; }

		/// <summary>
		/// Load the script from the app package into memory.
		/// Safe to call multiple times - only loads once.
		/// </summary>
		public static async Task LoadAsync()
		{
			if (!string.IsNullOrEmpty(Script))
			{
				return;
			}

			try
			{
				using var stream = await FileSystem.OpenAppPackageFileAsync(_assetPath);
				using var reader = new StreamReader(stream);
				Script = await reader.ReadToEndAsync();
			}
			catch (Exception)
			{
				// If the asset is missing/unreadable, Script stays null
				// and the converter will fall back to the CDN <script src>.
			}
		}
	}
}