using System;
using System.Threading.Tasks;
using EduCATS.Pages.Chat.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.Services
{
	public static class CallNavigationService
	{
		static bool _initialized;


		public static void Initialize()
		{
			if (_initialized)
			{
				return;
			}


			_initialized =
				true;


			CallService.IncomingCall +=
				OnIncomingCall;
		}


		static async void OnIncomingCall(
			int chatId)
		{
			try
			{
				await MainThread.InvokeOnMainThreadAsync(
					async () =>
					{
						var currentPage =
							GetCurrentPage();


						if (
							currentPage is CallPage)
						{
							return;
						}


						var callPage =
							new CallPage(
								chatId,
								"Входящий звонок");


						await currentPage.Navigation.PushModalAsync(
							callPage);
					});
			}
			catch (
				Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(
					$"Incoming call navigation error: {ex}");
			}
		}


		static Page GetCurrentPage()
		{
			var page =
				Application.Current.MainPage;


			if (
				page is NavigationPage navigationPage)
			{
				return navigationPage.CurrentPage;
			}


			if (
				page is Shell shell)
			{
				return shell.CurrentPage;
			}


			return page;
		}
	}
}