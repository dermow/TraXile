using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Heist page (classic Main.RenderHeistDashboard) on the global filter: heists and average time by area level,
    /// count by area and count by tag. Times are capped at the heist time cap like in the classic dashboard.
    /// </summary>
    public sealed class HeistViewModel : AnalyticsViewModel
    {
        /// <summary>Area levels on the charts (classic: 67-83).</summary>
        public const int MinLevel = 67, MaxLevel = 83;

        public HeistViewModel(ITrackingSource source, GlobalFilter filter) : base(source, filter)
        {
            Refresh();
        }

        private int _heistCount;
        public int HeistCount { get => _heistCount; private set => SetProperty(ref _heistCount, value); }

        public bool HasData => HeistCount > 0;

        private string _summary = "";
        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        /// <summary>Heists per area level, index 0 = <see cref="MinLevel"/>.</summary>
        public int[] CountByLevel { get; private set; } = new int[MaxLevel - MinLevel + 1];

        /// <summary>Average (capped) seconds per area level, index 0 = <see cref="MinLevel"/>.</summary>
        public double[] AverageByLevel { get; private set; } = new double[MaxLevel - MinLevel + 1];

        private IReadOnlyList<CountRow> _byArea = Array.Empty<CountRow>();
        public IReadOnlyList<CountRow> ByArea { get => _byArea; private set => SetProperty(ref _byArea, value); }

        private IReadOnlyList<CountRow> _byTag = Array.Empty<CountRow>();
        public IReadOnlyList<CountRow> ByTag { get => _byTag; private set => SetProperty(ref _byTag, value); }

        private ISeries[] _countSeries = Array.Empty<ISeries>();
        public ISeries[] CountSeries { get => _countSeries; private set => SetProperty(ref _countSeries, value); }

        private ISeries[] _timeSeries = Array.Empty<ISeries>();
        public ISeries[] TimeSeries { get => _timeSeries; private set => SetProperty(ref _timeSeries, value); }

        private Axis[] _countXAxes = { new Axis() };
        public Axis[] CountXAxes { get => _countXAxes; private set => SetProperty(ref _countXAxes, value); }

        private Axis[] _timeXAxes = { new Axis() };
        public Axis[] TimeXAxes { get => _timeXAxes; private set => SetProperty(ref _timeXAxes, value); }

        private Axis[] _countYAxes = { new Axis() };
        public Axis[] CountYAxes { get => _countYAxes; private set => SetProperty(ref _countYAxes, value); }

        private Axis[] _timeYAxes = { new Axis() };
        public Axis[] TimeYAxes { get => _timeYAxes; private set => SetProperty(ref _timeYAxes, value); }

        public override void Refresh()
        {
            List<ActivityInfo> heists = FilteredHistory().Where(a => a.Type == ACTIVITY_TYPES.HEIST).ToList();
            int cap = Source.GetTimeCap(ACTIVITY_TYPES.HEIST);
            int Capped(ActivityInfo a) => cap > 0 && a.TotalSeconds > cap ? cap : a.TotalSeconds;

            int n = MaxLevel - MinLevel + 1;
            var counts = new int[n];
            var averages = new double[n];
            for (int i = 0; i < n; i++)
            {
                List<ActivityInfo> ofLevel = heists.Where(a => a.AreaLevel == MinLevel + i).ToList();
                counts[i] = ofLevel.Count;
                averages[i] = ofLevel.Count > 0 ? ofLevel.Average(Capped) : 0;
            }
            CountByLevel = counts;
            AverageByLevel = averages;

            ByArea = CountRow.Build(heists.GroupBy(a => a.Area, StringComparer.OrdinalIgnoreCase)
                .Select(g => new CountRow
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Detail = ActivityInfo.FormatDuration((int)g.Average(Capped)) + " avg."
                }));
            ByTag = CountRow.ByTag(heists);

            HeistCount = heists.Count;
            Summary = heists.Count == 0 ? "No heists in the filtered activities."
                : $"{heists.Count:N0} heists · average {ActivityInfo.FormatDuration((int)heists.Average(Capped))}";
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(CountByLevel));
            OnPropertyChanged(nameof(AverageByLevel));

            BuildCharts();
        }

        private void BuildCharts()
        {
            SkiaSharp.SKColor color = ActivityPalette.Sk(ACTIVITY_TYPES.HEIST);
            string[] levels = Enumerable.Range(MinLevel, MaxLevel - MinLevel + 1).Select(l => l.ToString()).ToArray();
            CountSeries = new[] { Columns("Heists", CountByLevel.Select(c => (double)c), color, v => $"{v:N0} heists") };
            TimeSeries = new[] { Columns("Average time", AverageByLevel.Select(s => Math.Round(s / 60.0, 1)), color, v => $"{v:0.0} min") };
            CountXAxes = new[] { CategoryAxis(levels) };
            TimeXAxes = new[] { CategoryAxis(levels) };
            CountYAxes = new[] { ValueAxis() };
            TimeYAxes = new[] { ValueAxis(v => $"{v:0.#} min") };
        }

        protected override void OnThemeChanged() => BuildCharts();
    }
}
