using System;
using System.Threading.Tasks;
using EduCATS.Pages.Chat.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace EduCATS.Pages.Chat.Views
{
	public partial class CallPage : ContentPage
	{
		readonly int _chatId;

		public CallPage(int chatId, string title)
		{
			InitializeComponent();
			_chatId = chatId;
			Title = "Звонок";
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
					StatusLabel.Text = "Вызов...";
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = true;
					EndButton.IsVisible = false;
					RejectButton.Text = "Отменить";
					break;

				case CallState.Incoming:
					StatusLabel.Text = "Входящий звонок";
					AcceptButton.IsVisible = true;
					RejectButton.IsVisible = true;
					EndButton.IsVisible = false;
					break;

				case CallState.Connecting:
					StatusLabel.Text = "Подключение...";
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = false;
					EndButton.IsVisible = true;
					break;

				case CallState.Connected:
					StatusLabel.Text = "На связи";
					AcceptButton.IsVisible = false;
					RejectButton.IsVisible = false;
					EndButton.IsVisible = true;
					break;

				case CallState.Rejected:
				case CallState.Ended:
					StatusLabel.Text = state == CallState.Rejected ? "Звонок отклонён" : "Звонок завершён";
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