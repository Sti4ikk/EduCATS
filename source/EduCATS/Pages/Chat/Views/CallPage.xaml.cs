using System;
using System.Threading.Tasks;
using EduCATS.Helpers.Forms.Pages;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	public partial class CallPage : ContentPage, IHidesTabBar
	{
		readonly int _chatId;

		public CallPage(int chatId, string title)
		{
			InitializeComponent();
			_chatId = chatId;
			Title = CrossLocalization.Translate("chat_call_title");
			AcceptButton.Text = CrossLocalization.Translate("chat_call_accept");
			EndButton.Text = CrossLocalization.Translate("chat_call_end");
			ContactLabel.Text = title;

			CallService.StateChanged += OnCallStateChanged;
			UpdateState(CallService.State);
		}

		protected override void OnDisappearing()
		{
			CallService.StateChanged -= OnCallStateChanged;
			base.OnDisappearing();
		}

		void OnCallStateChanged(CallState state)
		{
			MainThread.BeginInvokeOnMainThread(() => UpdateState(state));
		}

		void UpdateState(CallState state)
		{
			switch (state)
			{
				case CallState.Calling:
					StatusLabel.Text = CrossLocalization.Translate("chat_call_calling");
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = true;
					EndButton.IsVisible = false;
					RejectButton.Text = CrossLocalization.Translate("base_cancel");
					break;

				case CallState.Incoming:
					StatusLabel.Text = CrossLocalization.Translate("chat_incoming_call");
					RejectButton.Text = CrossLocalization.Translate("chat_call_reject");
					AcceptButton.IsVisible = true;
					RejectButton.IsVisible = true;
					EndButton.IsVisible = false;
					break;

				case CallState.Connecting:
					StatusLabel.Text = CrossLocalization.Translate("chat_call_connecting");
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = false;
					EndButton.IsVisible = true;
					break;

				case CallState.Connected:
					StatusLabel.Text = CrossLocalization.Translate("chat_call_connected");
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = false;
					EndButton.IsVisible = true;
					break;

				case CallState.Rejected:
				case CallState.Ended:
					StatusLabel.Text = CrossLocalization.Translate(state == CallState.Rejected ? "chat_call_rejected" : "chat_call_ended");
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = false;
					EndButton.IsVisible = false;

					// Автоматическое закрытие окна после завершения
					Task.Delay(1200).ContinueWith(_ =>
					{
						MainThread.BeginInvokeOnMainThread(async () => await ClosePage());
					});
					break;
			}
		}

		async void OnAcceptClicked(object sender, EventArgs e)
		{
			await CallService.AcceptCall();
		}

		async void OnRejectClicked(object sender, EventArgs e)
		{
			await CallService.RejectCall();
			await ClosePage();
		}

		async void OnEndClicked(object sender, EventArgs e)
		{
			await CallService.EndCall();
			await ClosePage();
		}

		async Task ClosePage()
		{
			if (Navigation.ModalStack.Count > 0)
			{
				await Navigation.PopModalAsync();
			}
			else if (Navigation.NavigationStack.Count > 1)
			{
				await Navigation.PopAsync();
			}
		}
	}
}