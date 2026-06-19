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

			var closeToolbarItem = new ToolbarItem {
				IconImageSource = ImageSource.FromFile(Theme.Current.BaseCloseIcon)
			};

			closeToolbarItem.SetBinding(MenuItem.CommandProperty, "CloseCommand");
			ToolbarItems.Add(closeToolbarItem);
		}

		void createViews()
		{
			var listView = createQuestionList();
			var buttonLayout = createButtonLayout();

			var mainLayout = new StackLayout {
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

			// Можно добавить небольшой отступ между кнопками
			buttonGridLayout.ColumnSpacing = 10;

			return buttonGridLayout;
		}

		Button createButton(string text, string commandString)
		{
			var button = new Button {
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

			var listView = new ListView {
				BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor),
				Header = titleLayout,
				HasUnevenRows = true,
				ItemTemplate = new TestAnswerDataTemplateSelector {
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
			return new StackLayout {
				Padding = _titleLayoutPadding,
				Children = {
					questionLabel,
					descriptionLabel
				}
			};
		}

		Label createQuestionLabel()
		{
			var questionLabel = new Label {
				TextColor = Color.FromArgb(Theme.Current.TestPassingQuestionColor),
				Style = AppStyles.GetLabelStyle(NamedSize.Large)
			};

			questionLabel.SetBinding(Label.TextProperty, "Question");
			return questionLabel;
		}

		Label createDescriptionLabel()
		{
			var descriptionLabel = new Label {
				TextColor = Color.FromArgb(Theme.Current.TestPassingQuestionColor),
				TextType = TextType.Html,
				Style = AppStyles.GetLabelStyle()
			};

			descriptionLabel.SetBinding(Label.TextProperty, "Description");
			return descriptionLabel;
		}
	}
}

