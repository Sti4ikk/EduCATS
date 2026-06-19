using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Charts;
using EduCATS.Pages.Statistics.Base.Models;
using EduCATS.Themes;

namespace EduCATS.Helpers.Forms.Converters
{
	public class DoubleListToRadarChartConverter : IValueConverter
	{
		// В Syncfusion цвета задаются через Brush
		static Color _lineColor = Colors.Gray;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;

			var metrics = value as List<StatsChartEntryModel>;
			if (metrics == null || metrics.Count == 0) return null;

			// Создаём серию данных (Radar/Polar Line)
			var series = new PolarLineSeries
			{
				ItemsSource = metrics,
				XBindingPath = "Type", // Укажите правильное поле для подписей (например, название предмета)
				YBindingPath = "Value",
				Fill = new SolidColorBrush(_lineColor),
				StrokeWidth = 2,
				ShowMarkers = true,
				MarkerSettings = new ChartMarkerSettings { Width = 10, Height = 10 }
			};

			// SfPolarChart.Series ожидает коллекцию серий, а не сам график
			return new ChartSeriesCollection { series };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}