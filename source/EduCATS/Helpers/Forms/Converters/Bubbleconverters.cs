using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace EduCATS.Helpers.Forms.Converters
{
	/// <summary>
	/// Picks the message bubble background color depending on whether
	/// the message belongs to the current user ("mine") or not.
	/// </summary>
	public class BoolToBubbleColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool mine && mine
				? Microsoft.Maui.Graphics.Color.FromArgb("#DCF8C6")
				: Microsoft.Maui.Graphics.Color.FromArgb("#FFFFFF");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Aligns the message bubble to the right for the current user's
	/// own messages, and to the left for everyone else's.
	/// </summary>
	public class BoolToAlignmentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool mine && mine
				? LayoutOptions.End
				: LayoutOptions.Start;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}