using EduCATS.Helpers.Forms.Converters;
using EduCATS.Helpers.Forms.Styles;
using EduCATS.Themes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace EduCATS.Pages.Learning.Views.ViewCells
{
	public class LearningPageViewCell : StackLayout
	{
		const float _cornerRadius = 10;
		const double _cardHeight = 250;
		static Thickness _padding = new Thickness(10);
		static Thickness _titleMargin = new Thickness(10);

		public LearningPageViewCell()
		{
			BackgroundColor = Color.FromArgb(Theme.Current.AppBackgroundColor);
			Padding = _padding;
			HeightRequest = _cardHeight;
			VerticalOptions = LayoutOptions.Start;

			var image = new Image
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Aspect = Aspect.AspectFill,
				InputTransparent = true
			};
			image.SetBinding(Image.SourceProperty, "Image",
				converter: new StringToImageSourceConverter());

			var title = new Label
			{
				TextColor = Color.FromArgb(Theme.Current.LearningCardTextColor),
				FontAttributes = FontAttributes.Bold,
				Margin = _titleMargin,
				InputTransparent = true,
				Style = AppStyles.GetLabelStyle(bold: true)
			};
			title.SetBinding(Label.TextProperty, "Title");

			var contentGrid = new Grid
			{
				HeightRequest = _cardHeight,
				Children = { image, title }
			};

			// Клипуем сам контент напрямую — обход бага Border/Frame на Android
			contentGrid.SizeChanged += (s, e) =>
			{
				if (contentGrid.Width > 0 && contentGrid.Height > 0)
				{
					contentGrid.Clip = new RoundRectangleGeometry
					{
						CornerRadius = new CornerRadius(_cornerRadius),
						Rect = new Rect(0, 0, contentGrid.Width, contentGrid.Height)
					};
				}
			};

			var border = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(_cornerRadius) },
				HeightRequest = _cardHeight,
				VerticalOptions = LayoutOptions.Start,
				Padding = 0,
				BackgroundColor = Color.FromArgb(Theme.Current.BaseBlockColor),
				Content = contentGrid
			};

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += onCardTapped;
			border.GestureRecognizers.Add(tapGesture);

			Children.Add(border);
		}

		void onCardTapped(object sender, TappedEventArgs e)
		{
			var tappedBorder = (Border)sender;
			var item = tappedBorder.BindingContext;

			Element parent = tappedBorder.Parent;
			while (parent != null && parent is not CollectionView)
			{
				parent = parent.Parent;
			}

			if (parent is CollectionView collectionView)
			{
				collectionView.SelectedItem = null;
				collectionView.SelectedItem = item;
			}
		}
	}
}