using EduCATS.Helpers.Forms.Pages;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace EduCATS.MAUI.Platforms.iOS
{
	/// <summary>
	/// Navigation page renderer that hides the tab bar for
	/// <see cref="IHidesTabBar"/> pages the native way: the bar slides away
	/// with the push animation and comes back on the way back
	/// (including the swipe-back gesture).
	/// </summary>
	public class TabBarAwareNavigationRenderer : NavigationRenderer
	{
		public override void PushViewController(UIViewController viewController, bool animated)
		{
			if (hidesTabBar(viewController))
			{
				viewController.HidesBottomBarWhenPushed = true;
			}

			base.PushViewController(viewController, animated);
		}

		/// <summary>
		/// The pushed controller wraps the page's own controller.
		/// </summary>
		static bool hidesTabBar(UIViewController viewController)
		{
			if (viewController is ContainerViewController { CurrentView: IHidesTabBar })
			{
				return true;
			}

			foreach (var child in viewController.ChildViewControllers)
			{
				if (child is ContainerViewController { CurrentView: IHidesTabBar })
				{
					return true;
				}
			}

			return false;
		}
	}
}
