using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Logs;
using EduCATS.Pages.Chat.Services;
using EduCATS.Pages.Chat.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Personal conversation: online status of the other participant and calls.
	/// </summary>
	public class ConversationPageView : ConversationPageViewBase
	{
		static readonly Color _onlineColor = Color.FromArgb("#2E9E44");
		static readonly Color _offlineColor = Color.FromArgb("#8A8A8A");

		readonly int _chatId;

		public ConversationPageView(int chatId, string title, int peerUserId = 0)
			: base(new ConversationPageViewModel(PlatformServices.Current, chatId, title, peerUserId))
		{
			_chatId = chatId;
			InitializeView();
		}

		protected override View CreateSubtitle()
		{
			var statusLabel = new Label { FontSize = 12 };
			statusLabel.SetBinding(Label.TextProperty, nameof(ConversationPageViewModel.PeerStatusText));
			statusLabel.SetBinding(Label.TextColorProperty, new Binding(
				nameof(ConversationPageViewModel.IsPeerOnline),
				converter: new BoolToColorConverter(_onlineColor, _offlineColor)));
			return statusLabel;
		}

		protected override IEnumerable<View> CreateHeaderIcons()
		{
			yield return CreateHeaderIcon("icon_phone.png", 18, CrossLocalization.Translate("a11y_call"), startCall);
		}

		async Task startCall()
		{
			try
			{
				await CallService.StartCall(_chatId);
				await Navigation.PushAsync(new CallPage(_chatId, ViewModel.Title));
			}
			catch (Exception ex)
			{
				AppLogs.Log(ex);
				await DisplayAlertAsync(
					CrossLocalization.Translate("base_error"),
					CrossLocalization.Translate("chat_call_error"),
					"OK");
			}
		}

		/// <summary>
		/// Picks one of two colors by a boolean.
		/// </summary>
		class BoolToColorConverter : IValueConverter
		{
			readonly Color _trueColor;
			readonly Color _falseColor;

			public BoolToColorConverter(Color trueColor, Color falseColor)
			{
				_trueColor = trueColor;
				_falseColor = falseColor;
			}

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
				value is true ? _trueColor : _falseColor;

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
				throw new NotSupportedException();
		}
	}
}
