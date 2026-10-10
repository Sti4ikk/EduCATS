using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Views
{
	/// <summary>
	/// Full-screen image: pinch or double tap to zoom, drag to move.
	/// </summary>
	public class ImageViewerPage : ContentPage
	{
		const double _maxScale = 5;
		const double _doubleTapScale = 2.5;

		readonly Image _image;

		double _panStartX;
		double _panStartY;

		public ImageViewerPage(ImageSource source)
		{
			BackgroundColor = Colors.Black;

			_image = new Image
			{
				Source = source,
				Aspect = Aspect.AspectFit,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var pinch = new PinchGestureRecognizer();
			pinch.PinchUpdated += onPinchUpdated;

			var pan = new PanGestureRecognizer();
			pan.PanUpdated += onPanUpdated;

			var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
			doubleTap.Tapped += (sender, e) => setScale(_image.Scale > 1 ? 1 : _doubleTapScale);

			var imageContainer = new Grid { IsClippedToBounds = true, Children = { _image } };
			imageContainer.GestureRecognizers.Add(pinch);
			imageContainer.GestureRecognizers.Add(pan);
			imageContainer.GestureRecognizers.Add(doubleTap);

			var closeButton = new Button
			{
				Text = "✕",
				FontSize = 22,
				TextColor = Colors.White,
				BackgroundColor = Color.FromRgba(0, 0, 0, 0.4),
				CornerRadius = 22,
				WidthRequest = 44,
				HeightRequest = 44,
				Padding = 0,
				Margin = new Thickness(16, 40, 16, 16),
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Start
			};
			closeButton.Clicked += async (sender, e) => await Navigation.PopModalAsync();
			SemanticProperties.SetDescription(closeButton, CrossLocalization.Translate("base_close"));

			Content = new Grid { Children = { imageContainer, closeButton } };
		}

		void onPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
		{
			switch (e.Status)
			{
				case GestureStatus.Running:
					_image.Scale = Math.Clamp(_image.Scale * e.Scale, 1, _maxScale);
					break;
				case GestureStatus.Completed:
				case GestureStatus.Canceled:
					if (_image.Scale <= 1.05)
					{
						setScale(1);
					}

					break;
			}
		}

		void onPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			if (_image.Scale <= 1)
			{
				return;
			}

			switch (e.StatusType)
			{
				case GestureStatus.Started:
					_panStartX = _image.TranslationX;
					_panStartY = _image.TranslationY;
					break;
				case GestureStatus.Running:
					// Don't let the image leave the screen.
					var maxX = Width * (_image.Scale - 1) / 2;
					var maxY = Height * (_image.Scale - 1) / 2;
					_image.TranslationX = Math.Clamp(_panStartX + e.TotalX, -maxX, maxX);
					_image.TranslationY = Math.Clamp(_panStartY + e.TotalY, -maxY, maxY);
					break;
			}
		}

		void setScale(double scale)
		{
			_image.Scale = scale;

			if (scale <= 1)
			{
				_image.TranslationX = 0;
				_image.TranslationY = 0;
			}
		}
	}
}
