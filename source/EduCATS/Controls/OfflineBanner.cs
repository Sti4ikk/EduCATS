using System;
using EduCATS.Helpers.Logs;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Networking;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Controls
{
	/// <summary>
	/// "No connection" strip shown while the device is offline.
	/// </summary>
	/// <remarks>
	/// Replaces error dialogs on connection loss: pages keep showing
	/// cached data, the strip tells the user it may be outdated.
	/// </remarks>
	public class OfflineBanner : ContentView
	{
		static readonly Color _backgroundColor = Color.FromArgb("#5F6368");

		public OfflineBanner()
		{
			var label = new Label
			{
				Text = CrossLocalization.Translate("base_offline_banner"),
				TextColor = Colors.White,
				FontSize = 13,
				HorizontalTextAlignment = TextAlignment.Center
			};

			Content = new Border
			{
				StrokeThickness = 0,
				BackgroundColor = _backgroundColor,
				Padding = new Thickness(12, 6),
				Content = label
			};

			SemanticProperties.SetDescription(this, label.Text);
			IsVisible = !isOnline();

			Loaded += (sender, e) => Connectivity.ConnectivityChanged += onConnectivityChanged;
			Unloaded += (sender, e) => Connectivity.ConnectivityChanged -= onConnectivityChanged;
		}

		/// <summary>
		/// Put the banner above page content.
		/// </summary>
		/// <param name="content">Page content.</param>
		/// <returns>Content with the banner.</returns>
		public static View Wrap(View content)
		{
			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			grid.Add(new OfflineBanner(), 0, 0);
			grid.Add(content, 0, 1);
			return grid;
		}

		void onConnectivityChanged(object sender, ConnectivityChangedEventArgs e) =>
			MainThread.BeginInvokeOnMainThread(() => IsVisible = e.NetworkAccess != NetworkAccess.Internet);

		static bool isOnline()
		{
			try
			{
				return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				return true;
			}
		}
	}
}
