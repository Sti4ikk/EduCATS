using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EduCATS.Helpers.Converters
{
	/// <summary>
	/// Converts an integer percentage (0-100) to a
	/// <see cref="ProgressBar.Progress"/> value (0.0-1.0).
	/// </summary>
	public class PercentageToProgressConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int percentage)
			{
				return percentage / 100d;
			}

			return 0d;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}