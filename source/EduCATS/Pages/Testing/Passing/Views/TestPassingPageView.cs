using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Pages.Testing.Passing.ViewModels;
using EduCATS.Pages.Testing.Passing.Views.ViewCells;
using EduCATS.Themes;
using Nyxbull.Plugins.CrossLocalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;


namespace EduCATS.Pages.Testing.Passing.Views
{
	public class TestPassingPageView : ContentPage
	{
		const double _spacing = 1;
		const double _buttonHeight = 50;
		const double _buttonGridHeight = 100;
		const double _descriptionMinHeight = 1;
		const int _mathJaxReadyMaxAttempts = 20;
		const int _mathJaxReadyPollDelayMs = 150;
		const int _stabilityCheckMaxAttempts = 10;
		const int _stabilityCheckDelayMs = 100;

		static Thickness _buttonGridPadding = new Thickness(20);
		static Thickness _titleLayoutPadding = new Thickness(20);

		public TestPassingPageView(int testId, bool forSelfStudy)
		{
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			BindingContext = new TestPassingPageViewModel(new PlatformServices(), testId, forSelfStudy);
			this.SetBinding(TitleProperty, "Title");
			setToolbar();
			createViews();
		}

		void setToolbar()
		{
			var speechToolbar = new ToolbarItem();

			speechToolbar.SetBinding(
				MenuItem.IconImageSourceProperty,
				"HeadphonesIcon",
				converter: new StringToImageSourceConverter());

			speechToolbar.SetBinding(MenuItem.CommandProperty, "SpeechCommand");
			ToolbarItems.Add(speechToolbar);

			var closeToolbarItem = new ToolbarItem
			{
				IconImageSource = ImageSource.FromFile(Theme.Current.BaseCloseIcon)
			};

			closeToolbarItem.SetBinding(MenuItem.CommandProperty, "CloseCommand");
			ToolbarItems.Add(closeToolbarItem);
		}

		void createViews()
		{
			var listView = createQuestionList();
			var buttonLayout = createButtonLayout();

			var mainLayout = new StackLayout
			{
				Spacing = _spacing,
				Children = {
					listView, buttonLayout
				}
			};

			mainLayout.SetBinding(IsEnabledProperty, "IsNotLoading");
			Content = mainLayout;
		}

		Grid createButtonLayout()
		{
			var acceptButton = createButton(
				CrossLocalization.Translate("test_passing_answer"),
				"AnswerCommand");

			var skipButton = createButton(
				CrossLocalization.Translate("test_passing_skip"),
				"SkipCommand");

			var buttonGridLayout = new Grid
			{
				HeightRequest = _buttonGridHeight,
				VerticalOptions = LayoutOptions.End,
				HorizontalOptions = LayoutOptions.Fill,
				Padding = _buttonGridPadding,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				ColumnDefinitions =
		{
			new ColumnDefinition { Width = GridLength.Star },
			new ColumnDefinition { Width = GridLength.Star }
		}
			};

			buttonGridLayout.Add(skipButton, 0, 0);
			buttonGridLayout.Add(acceptButton, 1, 0);

			buttonGridLayout.ColumnSpacing = 10;

			return buttonGridLayout;
		}

		Button createButton(string text, string commandString)
		{
			var button = new Button
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = _buttonHeight,
				CornerRadius = (int)_buttonHeight / 2,
				BackgroundColor = Color.FromArgb(Theme.Current.AppStatusBarBackgroundColor),
				TextColor = Color.FromArgb(Theme.Current.TestPassingButtonTextColor),
				Style = AppStyles.GetButtonStyle(),
				Text = text
			};

			button.SetBinding(Button.CommandProperty, commandString);
			return button;
		}

		ListView createQuestionList()
		{
			var titleLayout = createTitleLayout();

			var listView = new ListView
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Header = titleLayout,
				HasUnevenRows = true,
				ItemTemplate = new TestAnswerDataTemplateSelector
				{
					SingleTemplate = new DataTemplate(typeof(TestSingleAnswerViewCell)),
					EditableTemplate = new DataTemplate(typeof(TestEditableAnswerViewCell)),
					MultipleTemplate = new DataTemplate(typeof(TestMultipleAnswerViewCell)),
					MovableTemplate = new DataTemplate(typeof(TestMovableAnswerViewCell))
				},
				SeparatorColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				SeparatorVisibility = SeparatorVisibility.None
			};

			listView.ItemTapped += (sender, args) => ((ListView)sender).SelectedItem = null;
			listView.SetBinding(ListView.SelectedItemProperty, "SelectedItem");
			listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "Answers");
			return listView;
		}

		StackLayout createTitleLayout()
		{
			var questionLabel = createQuestionLabel();
			var descriptionLabel = createDescriptionLabel();
			var descriptionWebView = createDescriptionWebView();

			return new StackLayout
			{
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Padding = _titleLayoutPadding,
				Children = {
					questionLabel,
					descriptionLabel,
					descriptionWebView
				}
			};
		}

		Label createQuestionLabel()
		{
			var questionLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestPassingQuestionColor),
				FontAttributes = FontAttributes.Bold,
				Style = AppStyles.GetLabelStyle(NamedSize.Large)
			};

			questionLabel.SetBinding(Label.TextProperty, "Question");
			return questionLabel;
		}

		/// <summary>
		/// Create the description WebView, used only for descriptions
		/// that contain an embedded (base64) image.
		/// </summary>
		/// <remarks>
		/// Label with <c>TextType.Html</c> only supports a handful of
		/// basic tags and cannot display embedded images, so WebView is
		/// used as a fallback in that case. WebView doesn't auto-size to
		/// its content by default, so height is measured via JS after
		/// each navigation and applied to <c>HeightRequest</c> - this is
		/// not fully reliable inside a ListView Header (can leave a
		/// stale/incorrect gap on some questions), so it's used only
		/// when strictly necessary, i.e. when there's actually an image
		/// to show. Plain text/HTML descriptions (formulas, formatting,
		/// etc.) stay on the more stable <see cref="createDescriptionLabel"/>.
		/// </remarks>
		/// <returns>Description web view.</returns>
		WebView createDescriptionWebView()
		{
			var webView = new WebView
			{
				HeightRequest = _descriptionMinHeight,
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill
			};

			webView.SetBinding(
				WebView.SourceProperty,
				"Description",
				converter: new DescriptionToHtmlSourceConverter());

			webView.SetBinding(
				IsVisibleProperty,
				"Description",
				converter: new HasEmbeddedImageConverter());

			webView.Navigated += async (sender, args) => await resizeToContent(webView);

			return webView;
		}

		/// <summary>
		/// Resize the WebView to fit its rendered HTML content height.
		/// </summary>
		/// <param name="webView">WebView to resize.</param>
		async System.Threading.Tasks.Task resizeToContent(WebView webView)
		{
			try
			{
				for (var i = 0; i < _mathJaxReadyMaxAttempts; i++)
				{
					var ready = await webView.EvaluateJavaScriptAsync(
						"window.mathJaxReady === true ? '1' : '0'");

					if (ready == "1")
					{
						break;
					}

					await System.Threading.Tasks.Task.Delay(_mathJaxReadyPollDelayMs);
				}

				var lastHeight = -1d;

				for (var i = 0; i < _stabilityCheckMaxAttempts; i++)
				{
					var result = await webView.EvaluateJavaScriptAsync("document.body.scrollHeight");

					if (!double.TryParse(result, out var height) || height <= 0)
					{
						break;
					}

					if (height == lastHeight)
					{
						break;
					}

					lastHeight = height;
					webView.HeightRequest = height;

					await System.Threading.Tasks.Task.Delay(_stabilityCheckDelayMs);
				}
			}
			catch
			{
				// Ignore measurement failures - the WebView simply
				// keeps its previous height in that case.
			}
		}

		/// <summary>
		/// Create the description label used for plain text/HTML
		/// descriptions (formulas, formatting, etc. - no images).
		/// </summary>
		/// <remarks>
		/// This is the default, stable path: Label sizes itself reliably
		/// inside the ListView Header. Only hidden when the description
		/// contains an embedded image, which Label's HTML rendering
		/// can't display - <see cref="createDescriptionWebView"/> is used
		/// instead in that case.
		/// </remarks>
		/// <returns>Description label.</returns>
		Label createDescriptionLabel()
		{
			var descriptionLabel = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.TestPassingQuestionColor),
				TextType = TextType.Html,
				Style = AppStyles.GetLabelStyle()
			};

			descriptionLabel.SetBinding(Label.TextProperty, "Description");
			descriptionLabel.SetBinding(
				IsVisibleProperty,
				new Binding(
					"Description",
					converter: new HasEmbeddedImageConverter(),
					converterParameter: "Invert"));

			return descriptionLabel;
		}
	}
}