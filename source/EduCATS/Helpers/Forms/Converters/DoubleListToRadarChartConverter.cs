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
		static Color _lineColor = Colors.Gray;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Syncfusion SfPolarChart требует минимум 2 точки — иначе NullRef в DrawSecondaryAxisGridLine
			var fallback = new ChartSeriesCollection
	{
		new PolarLineSeries
		{
			ItemsSource = new List<StatsChartEntryModel>
			{
				new StatsChartEntryModel(StatsChartMetricType.Labs, 0),
				new StatsChartEntryModel(StatsChartMetricType.Tests, 0),
				new StatsChartEntryModel(StatsChartMetricType.Rating, 0)
			},
			XBindingPath = "Label",
			YBindingPath = "Value",
			Fill = new SolidColorBrush(Colors.Transparent),
		}
	};

			if (value is not List<StatsChartEntryModel> metrics || metrics.Count == 0)
				return fallback;

			// Если пришла всего одна точка — тоже используем fallback
			if (metrics.Count < 2)
				return fallback;

			var series = new PolarLineSeries
			{
				ItemsSource = metrics,
				XBindingPath = "Label",
				YBindingPath = "Value",
				Fill = new SolidColorBrush(_lineColor),
				StrokeWidth = 2,
				ShowMarkers = true,
				MarkerSettings = new ChartMarkerSettings { Width = 10, Height = 10 }
			};

			return new ChartSeriesCollection { series };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> null;
	}
}
