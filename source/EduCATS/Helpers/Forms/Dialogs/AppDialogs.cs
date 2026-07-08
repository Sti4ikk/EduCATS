using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Controls.UserDialogs.Maui;
using ActionSheetConfig = Controls.UserDialogs.Maui.ActionSheetConfig;


namespace EduCATS.Helpers.Forms.Dialogs
{
	/// <summary>
	/// <see cref="IDialogs"/> implementation.
	/// </summary>
	public class AppDialogs : IDialogs
	{
		/// <summary>
		/// Localized "OK" text.
		/// </summary>
		static string _baseOK => CrossLocalization.Translate("base_ok");

		/// <summary>
		/// Localized "No" text.
		/// </summary>
		static string _baseNo => CrossLocalization.Translate("base_no");

		/// <summary>
		/// Localized "Yes" text.
		/// </summary>
		static string _baseYes => CrossLocalization.Translate("base_yes");

		/// <summary>
		/// Localized "Error" text.
		/// </summary>
		static string _baseError => CrossLocalization.Translate("base_error");

		/// <summary>
		/// Localized "Cancel" text.
		/// </summary>
		static string _baseCancel => CrossLocalization.Translate("base_cancel");

		/// <summary>
		/// Localized "Loading" text.
		/// </summary>
		static string _baseLoading => CrossLocalization.Translate("base_loading");

		/// <summary>
		/// Number of currently active <see cref="ShowLoading()"/> calls
		/// across the whole app.
		/// </summary>
		/// <remarks>
		/// <see cref="UserDialogs.Instance"/> is a single app-wide HUD, not
		/// scoped per page or per ViewModel. Multiple screens/ViewModels can
		/// legitimately call ShowLoading/HideLoading concurrently (e.g. one
		/// tab's load is still in flight when another tab starts its own),
		/// and without a reference count, whichever finishes first hides
		/// the HUD out from under the other, or leaves it stuck depending
		/// on timing. The HUD is now only actually shown on 0-&gt;1 and only
		/// actually hidden on 1-&gt;0.
		/// </remarks>
		static int _loadingCount;

		/// <summary>
		/// Property for getting <see cref="Application.Current.Windows[0].Page"/>.
		/// </summary>
		Page mainPage =>
			Application.Current.Windows[0].Page;

		/// <summary>
		/// Show error dialog.
		/// </summary>
		/// <param name="message">Dialog description.</param>
		public void ShowError(string message) =>
			mainPage.DisplayAlertAsync(_baseError, message, _baseOK);

		/// <summary>
		/// Show message dialog.
		/// </summary>
		/// <param name="title">Dialog title.</param>
		/// <param name="message">Dialog description.</param>
		public void ShowMessage(string title, string message) =>
			mainPage.DisplayAlertAsync(title, message, _baseOK);

		/// <summary>
		/// Show message dialog.
		/// </summary>
		/// <param name="title">Dialog title.</param>
		/// <param name="message">Dialog description.</param>
		public Task<bool> ShowMessageUpdate(string title, string message, string linkButton, string cancelButton) =>
			mainPage.DisplayAlertAsync(title, message, linkButton, cancelButton);

		/// <summary>
		/// Show loading dialog.
		/// </summary>
		public void ShowLoading() => ShowLoading(_baseLoading);

		/// <summary>
		/// Show loading dialog.
		/// </summary>
		/// <param name="message">Dialog description.</param>
		public void ShowLoading(string message)
		{
			Interlocked.Increment(ref _loadingCount);
			UserDialogs.Instance.ShowLoading(message);
		}

		/// <summary>
		/// Hide loading dialog.
		/// </summary>
		public void HideLoading()
		{
			var remaining = Interlocked.Decrement(ref _loadingCount);

			if (remaining < 0)
			{
				// Defensive: more Hide calls than Show calls happened
				// (shouldn't occur, but avoid the counter going negative
				// and permanently breaking future Show/Hide pairs).
				Interlocked.Exchange(ref _loadingCount, 0);
			}

			if (remaining <= 0)
			{
				UserDialogs.Instance.HideHud();
			}
		}

		/// <summary>
		/// Show progress dialog.
		/// </summary>
		/// <param name="message">Dialog message.</param>
		/// <param name="cancelText">Cancel button text.</param>
		/// <param name="onCancel">Action on cancel.</param>
		/// <returns>Progress dialog.</returns>
		public object ShowProgress(string message, string cancelText, Action onCancel)
		{
			return UserDialogs.Instance.Progress(
				message: message,
				cancelText: cancelText
			//onCancel: onCancel
			);
		}

		/// <summary>
		/// Update progress dialog with percent.
		/// </summary>
		/// <param name="dialog">
		/// Progress dialog instance
		/// (retrieved from <see cref="ShowProgress(string, string, Action)"/>).
		/// </param>
		/// <param name="percent">Percent to apply.</param>
		public void UpdateProgress(object dialog, int percent)
		{
			var progressDialog = getProgressDialog(dialog);

			if (progressDialog == null)
			{
				return;
			}

			progressDialog.PercentComplete = percent;
		}

		/// <summary>
		/// Hide progress dialog.
		/// </summary>
		/// <param name="dialog">
		/// Progress dialog instance
		/// (retrieved from <see cref="ShowProgress(string, string, Action)"/>).
		/// </param>
		public void HideProgress(object dialog) =>
			getProgressDialog(dialog)?.Hide();

		/// <summary>
		/// Show alert sheet.
		/// </summary>
		/// <param name="title">Dialog title.</param>
		/// <param name="buttons">Dialog buttons (id and name).</param>
		/// <param name="command">Command to execute on button click.</param>
		/// <returns>Chosen button name.</returns>
		public void ShowSheet(string title, Dictionary<int, string> buttonList, ICommand command)
		{
			var config = new ActionSheetConfig().SetTitle(title);

			foreach (var button in buttonList)
			{
				config.Add(button.Value, () => command.Execute(button.Key));
			}

			config.SetCancel(_baseCancel, () => command.Execute(-1));
			UserDialogs.Instance.ActionSheet(config);
		}

		/// <summary>
		/// Show confirmation dialog.
		/// </summary>
		/// <param name="title">Dialog title.</param>
		/// <param name="message">Dialog description.</param>
		/// <returns>Dialog result.</returns>
		public async Task<bool> ShowConfirmationMessage(string title, string message) =>
			await mainPage.DisplayAlertAsync(title, message, _baseYes, _baseNo);

		/// <summary>
		/// Show confirmation dialog with button OK.
		/// </summary>
		/// <param name="title">Dialog title.</param>
		/// <param name="message">Dialog description.</param>
		public async Task ShowConfirmation(string title, string message) =>
			await mainPage.DisplayAlertAsync(title, message, _baseOK);

		/// <summary>
		/// Convert object to <see cref="IProgressDialog"/>.
		/// </summary>
		/// <param name="dialog">Dialog object.</param>
		/// <returns>Progress dialog.</returns>
		IProgressDialog getProgressDialog(object dialog)
		{
			if (dialog == null || !(dialog is IProgressDialog))
			{
				return null;
			}

			var progressDialog = dialog as IProgressDialog;
			return progressDialog;
		}
	}
}