using System;
using EduCATS.Helpers.Forms.Effects;
using EduCATS.Pages.Learning.Views;
using EduCATS.Pages.Settings.Base.Views;
using EduCATS.Pages.Statistics.Base.Views;
using EduCATS.Pages.Today.Base.Views;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace EduCATS.Pages.Main
{
	public class MainPageView : TabbedPage
	{
		public MainPageView()
		{
			setAndroidConfiguration();
			setPageDetails();
			setPages();
			setCurrentTitle();
			CurrentPageChanged += pageChanged;
		}

		void setPageDetails()
		{
			BarBackgroundColor = Color.FromArgb(Theme.Current.AppNavigationBarBackgroundColor);
			SelectedTabColor = Color.FromArgb(Theme.Current.MainSelectedTabColor);
			UnselectedTabColor = Color.FromArgb(Theme.Current.MainUnselectedTabColor);
		}

		void setPages()
		{
			Children.Add(
				createPage(new TodayPageView(),
				CrossLocalization.Translate("main_today"),
				Theme.Current.MainTodayIcon));
			Children.Add(
				createPage(new LearningPageView(),
				CrossLocalization.Translate("main_learning"),
				Theme.Current.MainLearningIcon));
			Children.Add(
				createPage(new StatsPageView(),
				CrossLocalization.Translate("main_statistics"),
				Theme.Current.MainStatisticsIcon));
			Children.Add(
				createPage(new SettingsPageView(),
				CrossLocalization.Translate("main_settings"),
				Theme.Current.MainSettingsIcon));
		}

		NavigationPage createPage(Page page, string title, string icon)
		{
			return new NavigationPage(page)
			{
				Title = title,
				IconImageSource = ImageSource.FromFile(icon),
				BackgroundColor = Color.FromArgb(Theme.Current.AppNavigationBarBackgroundColor) // FromHex → FromArgb
			};
		}

		void setAndroidConfiguration()
		{
			if (DeviceInfo.Platform != DevicePlatform.Android)
			{ // DeviceInfo.Platform → DeviceInfo.Platform
				return;
			}
			On<Android>().DisableSwipePaging();
			On<Android>().DisableSmoothScroll();
			On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
			Effects.Add(new DisabledShiftEffect());
		}

		void pageChanged(object sender, EventArgs e)
		{
			setCurrentTitle();
		}

		void setCurrentTitle()
		{
			Title = CurrentPage.Title;
		}
	}
}
