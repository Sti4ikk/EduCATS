using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Statistics.Base.Models
{
	public enum StatsChartMetricType
	{
		Pract,
		Labs,
		Tests,
		Course,
		Rating
	}

	public class StatsChartEntryModel
	{
		public StatsChartEntryModel(StatsChartMetricType type, double value)
		{
			Type = type;
			Value = value;
			Label = getLabel(type);
		}

		public StatsChartMetricType Type { get; }
		public double Value { get; }
		public string Label { get; }

		static string getLabel(StatsChartMetricType type)
		{
			return type switch
			{
				StatsChartMetricType.Pract => CrossLocalization.Translate("stats_chart_average_pract"),
				StatsChartMetricType.Labs => CrossLocalization.Translate("stats_chart_average_labs"),
				StatsChartMetricType.Tests => CrossLocalization.Translate("stats_chart_average_tests"),
				StatsChartMetricType.Course => CrossLocalization.Translate("stats_chart_average_course"),
				StatsChartMetricType.Rating => CrossLocalization.Translate("stats_chart_rating"),
				_ => type.ToString()
			};
		}
	}
}