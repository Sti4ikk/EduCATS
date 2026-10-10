using Android.Views;
using EduCATS.Helpers.Forms.Pages;
using Google.Android.Material.BottomNavigation;

namespace EduCATS.MAUI.Platforms.Android
{
	/// <summary>
	/// Bottom tab bar visibility (<see cref="TabBarVisibility"/>).
	/// </summary>
	/// <remarks>
	/// MAUI puts the tabs (BottomNavigationView) into the window layout
	/// next to the page container, not inside the TabbedPage view.
	/// With the bar gone the container takes the freed space.
	/// </remarks>
	public static class AndroidTabBar
	{
		public static void SetVisible(TabbedPage tabbedPage, bool isVisible)
		{
			var root = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView as ViewGroup;
			var bar = find(root);

			if (bar == null)
			{
				return;
			}

			var visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;

			if (bar.Visibility != visibility)
			{
				bar.Visibility = visibility;
			}
		}

		static BottomNavigationView find(ViewGroup group)
		{
			if (group == null)
			{
				return null;
			}

			for (var i = 0; i < group.ChildCount; i++)
			{
				var child = group.GetChildAt(i);

				if (child is BottomNavigationView bar)
				{
					return bar;
				}

				var nested = find(child as ViewGroup);

				if (nested != null)
				{
					return nested;
				}
			}

			return null;
		}
	}
}
