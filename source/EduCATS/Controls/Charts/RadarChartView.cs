using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using EduCATS.Pages.Statistics.Base.Models;

namespace EduCATS.Controls.Charts
{
	public class RadarChartView : GraphicsView, IDrawable
	{
		const float _markerRadius = 5;
		const float _haloPadding = 14;
		const int _defaultRingCount = 1;

		public static readonly BindableProperty EntriesProperty = BindableProperty.Create(
			nameof(Entries),
			typeof(List<StatsChartEntryModel>),
			typeof(RadarChartView),
			null,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty MetricColorsProperty = BindableProperty.Create(
			nameof(MetricColors),
			typeof(Dictionary<StatsChartMetricType, Color>),
			typeof(RadarChartView),
			null,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty AutoScaleProperty = BindableProperty.Create(
			nameof(AutoScale),
			typeof(bool),
			typeof(RadarChartView),
			true,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty RingCountProperty = BindableProperty.Create(
			nameof(RingCount),
			typeof(int),
			typeof(RadarChartView),
			_defaultRingCount,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
			nameof(MaxValue),
			typeof(double),
			typeof(RadarChartView),
			100d,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty GridStepProperty = BindableProperty.Create(
			nameof(GridStep),
			typeof(double),
			typeof(RadarChartView),
			20d,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
			nameof(LineColor),
			typeof(Color),
			typeof(RadarChartView),
			Colors.Gray,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
			nameof(FillColor),
			typeof(Color),
			typeof(RadarChartView),
			Color.FromRgba(128, 128, 128, 60),
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty GridColorProperty = BindableProperty.Create(
			nameof(GridColor),
			typeof(Color),
			typeof(RadarChartView),
			Colors.LightGray,
			propertyChanged: onRedrawPropertyChanged);

		public static readonly BindableProperty ShowHaloProperty = BindableProperty.Create(
			nameof(ShowHalo),
			typeof(bool),
			typeof(RadarChartView),
			true,
			propertyChanged: onRedrawPropertyChanged);

		public RadarChartView()
		{
			Drawable = this;
			SizeChanged += (sender, e) => Invalidate();
		}

		public List<StatsChartEntryModel> Entries
		{
			get => (List<StatsChartEntryModel>)GetValue(EntriesProperty);
			set => SetValue(EntriesProperty, value);
		}

		public Dictionary<StatsChartMetricType, Color> MetricColors
		{
			get => (Dictionary<StatsChartMetricType, Color>)GetValue(MetricColorsProperty);
			set => SetValue(MetricColorsProperty, value);
		}

		public bool AutoScale
		{
			get => (bool)GetValue(AutoScaleProperty);
			set => SetValue(AutoScaleProperty, value);
		}

		public int RingCount
		{
			get => (int)GetValue(RingCountProperty);
			set => SetValue(RingCountProperty, value);
		}

		public double MaxValue
		{
			get => (double)GetValue(MaxValueProperty);
			set => SetValue(MaxValueProperty, value);
		}

		public double GridStep
		{
			get => (double)GetValue(GridStepProperty);
			set => SetValue(GridStepProperty, value);
		}

		public Color LineColor
		{
			get => (Color)GetValue(LineColorProperty);
			set => SetValue(LineColorProperty, value);
		}

		public Color FillColor
		{
			get => (Color)GetValue(FillColorProperty);
			set => SetValue(FillColorProperty, value);
		}

		public Color GridColor
		{
			get => (Color)GetValue(GridColorProperty);
			set => SetValue(GridColorProperty, value);
		}

		public bool ShowHalo
		{
			get => (bool)GetValue(ShowHaloProperty);
			set => SetValue(ShowHaloProperty, value);
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			var entries = Entries;

			if (entries == null || entries.Count < 3)
			{
				return;
			}

			var centerX = dirtyRect.Width / 2;
			var centerY = dirtyRect.Height / 2;
			var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - _haloPadding;

			if (radius <= 0)
			{
				return;
			}

			var count = entries.Count;
			var angleStep = 2 * Math.PI / count;
			var startAngle = -Math.PI / 2;

			double effectiveMax;
			int ringCount;

			if (AutoScale)
			{
				var maxEntry = entries.Max(e => e.Value);
				effectiveMax = niceCeiling(maxEntry);
				ringCount = Math.Max(1, RingCount);
			}
			else
			{
				effectiveMax = MaxValue;
				ringCount = Math.Max(1, (int)Math.Round(MaxValue / GridStep));
			}

			var points = computePoints(centerX, centerY, radius, entries, angleStep, startAngle, effectiveMax);
			var colors = resolvePointColors(entries);

			canvas.SaveState();
			drawGrid(canvas, centerX, centerY, radius, ringCount);
			drawAxes(canvas, centerX, centerY, radius, count, angleStep, startAngle);

			if (ShowHalo)
			{
				drawHalos(canvas, centerX, centerY, points, colors);
			}

			drawData(canvas, points, colors);
			canvas.RestoreState();
		}

		List<PointF> computePoints(float centerX, float centerY, float radius, List<StatsChartEntryModel> entries, double angleStep, double startAngle, double effectiveMax)
		{
			var safeMax = effectiveMax <= 0 ? 1 : effectiveMax;
			var points = new List<PointF>(entries.Count);

			for (var i = 0; i < entries.Count; i++)
			{
				var value = Math.Max(0d, Math.Min(entries[i].Value, safeMax));
				var pointRadius = radius * (float)(value / safeMax);
				var angle = startAngle + angleStep * i;
				var x = centerX + pointRadius * (float)Math.Cos(angle);
				var y = centerY + pointRadius * (float)Math.Sin(angle);
				points.Add(new PointF(x, y));
			}

			return points;
		}

		List<Color> resolvePointColors(List<StatsChartEntryModel> entries)
		{
			var metricColors = MetricColors;

			if (metricColors != null)
			{
				return entries
					.Select(e => metricColors.TryGetValue(e.Type, out var color) ? color : LineColor)
					.ToList();
			}

			return entries.Select(e => LineColor).ToList();
		}

		void drawGrid(ICanvas canvas, float centerX, float centerY, float radius, int ringCount)
		{
			canvas.StrokeColor = GridColor;
			canvas.StrokeSize = 1;

			for (var ring = 1; ring <= ringCount; ring++)
			{
				var ringRadius = radius * ring / ringCount;
				canvas.DrawEllipse(centerX - ringRadius, centerY - ringRadius, ringRadius * 2, ringRadius * 2);
			}
		}

		void drawAxes(ICanvas canvas, float centerX, float centerY, float radius, int count, double angleStep, double startAngle)
		{
			canvas.StrokeColor = GridColor;
			canvas.StrokeSize = 1;

			for (var i = 0; i < count; i++)
			{
				var angle = startAngle + angleStep * i;
				var x = centerX + radius * (float)Math.Cos(angle);
				var y = centerY + radius * (float)Math.Sin(angle);
				canvas.DrawLine(centerX, centerY, x, y);
			}
		}

		void drawHalos(ICanvas canvas, float centerX, float centerY, List<PointF> points, List<Color> colors)
		{
			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = new float[] { 2, 3 };

			for (var i = 0; i < points.Count; i++)
			{
				var distance = (float)Math.Sqrt(Math.Pow(points[i].X - centerX, 2) + Math.Pow(points[i].Y - centerY, 2));

				if (distance <= 0)
				{
					continue;
				}

				canvas.StrokeColor = colors[i];
				canvas.DrawEllipse(centerX - distance, centerY - distance, distance * 2, distance * 2);
			}

			canvas.StrokeDashPattern = null;
		}

		void drawData(ICanvas canvas, List<PointF> points, List<Color> colors)
		{
			var path = new PathF();

			for (var i = 0; i < points.Count; i++)
			{
				if (i == 0)
				{
					path.MoveTo(points[i].X, points[i].Y);
				}
				else
				{
					path.LineTo(points[i].X, points[i].Y);
				}
			}

			path.Close();

			canvas.FillColor = blendAll(colors).WithAlpha(0.25f);
			canvas.FillPath(path);

			canvas.StrokeSize = 2;

			for (var i = 0; i < points.Count; i++)
			{
				var next = (i + 1) % points.Count;
				canvas.StrokeColor = blend(colors[i], colors[next]);
				canvas.DrawLine(points[i].X, points[i].Y, points[next].X, points[next].Y);
			}

			for (var i = 0; i < points.Count; i++)
			{
				canvas.FillColor = colors[i];
				canvas.FillCircle(points[i].X, points[i].Y, _markerRadius);
			}
		}

		static Color blend(Color a, Color b)
		{
			return new Color(
				(a.Red + b.Red) / 2,
				(a.Green + b.Green) / 2,
				(a.Blue + b.Blue) / 2,
				(a.Alpha + b.Alpha) / 2);
		}

		static Color blendAll(List<Color> colors)
		{
			if (colors == null || colors.Count == 0)
			{
				return Colors.Gray;
			}

			var r = colors.Average(c => c.Red);
			var g = colors.Average(c => c.Green);
			var b = colors.Average(c => c.Blue);
			var a = colors.Average(c => c.Alpha);

			return new Color(r, g, b, a);
		}

		static double niceCeiling(double value)
		{
			if (value <= 0)
			{
				return 1;
			}

			var exponent = Math.Floor(Math.Log10(value));
			var magnitude = Math.Pow(10, exponent);
			var normalized = value / magnitude;

			double niceNormalized;

			if (normalized <= 1)
			{
				niceNormalized = 1;
			}
			else if (normalized <= 2)
			{
				niceNormalized = 2;
			}
			else if (normalized <= 5)
			{
				niceNormalized = 5;
			}
			else
			{
				niceNormalized = 10;
			}

			return niceNormalized * magnitude;
		}

		static void onRedrawPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((RadarChartView)bindable).Invalidate();
		}
	}
}