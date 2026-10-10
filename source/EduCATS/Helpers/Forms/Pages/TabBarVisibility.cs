using System;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Forms.Pages
{
	/// <summary>
	/// Page that takes the whole screen: the bottom tab bar is hidden
	/// while it's open (conversations, calls).
	/// </summary>
	public interface IHidesTabBar
	{
	}

	/// <summary>
	/// Bottom tab bar visibility.
	/// </summary>
	/// <remarks>
	/// <see cref="TabbedPage"/> has no API for that. iOS hides the bar natively
	/// (hidesBottomBarWhenPushed, set by the app's navigation renderer),
	/// other platforms set <see cref="PlatformSetVisible"/>.
	/// </remarks>
	public static class TabBarVisibility
	{
		/// <summary>
		/// Platform implementation: show (<c>true</c>) or hide the tab bar.
		/// </summary>
		public static Action<TabbedPage, bool> PlatformSetVisible { get; set; }

		/// <summary>
		/// Should the tab bar be visible for the page on top of the tab's stack.
		/// </summary>
		public static bool IsVisibleFor(Page page) =>
			(page is NavigationPage navigationPage ? navigationPage.CurrentPage : page) is not IHidesTabBar;

		/// <summary>
		/// Apply visibility for the currently opened page of the tabbed page.
		/// </summary>
		public static void Update(TabbedPage tabbedPage)
		{
			if (tabbedPage?.CurrentPage == null)
			{
				return;
			}

			PlatformSetVisible?.Invoke(tabbedPage, IsVisibleFor(tabbedPage.CurrentPage));
		}
	}
}
