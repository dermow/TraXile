using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Mapping page (classic Main.RenderMappingDashboard) on the global filter: maps per tier, average time per tier,
    /// count by map and count by tag. Map times are capped at the map time cap like in the classic dashboard.
    /// </summary>
    public sealed class MappingViewModel : AnalyticsViewModel
    {
        /// <summary>Tiers shown on the charts (T17 maps exist since 3.24).</summary>
        public const int MaxTier = 17;

        public MappingViewModel(ITrackingSource source, GlobalFilter filter) : base(source, filter)
        {
            Refresh();
        }

        private int _mapCount;
        public int MapCount { get => _mapCount; private set => SetProperty(ref _mapCount, value); }

        public bool HasData => MapCount > 0;

        private string _summary = "";
        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        /// <summary>Number of maps per tier, index 0 = T1.</summary>
        public int[] CountByTier { get; private set; } = new int[MaxTier];

        /// <summary>Average (capped) seconds per tier, index 0 = T1; 0 if no map of that tier.</summary>
        public double[] AverageByTier { get; private set; } = new double[MaxTier];

        private IReadOnlyList<CountRow> _byMap = Array.Empty<CountRow>();
        public IReadOnlyList<CountRow> ByMap { get => _byMap; private set => SetProperty(ref _byMap, value); }

        private IReadOnlyList<CountRow> _byTag = Array.Empty<CountRow>();
        public IReadOnlyList<CountRow> ByTag { get => _byTag; private set => SetProperty(ref _byTag, value); }

        private ISeries[] _tierCountSeries = Array.Empty<ISeries>();
        public ISeries[] TierCountSeries { get => _tierCountSeries; private set => SetProperty(ref _tierCountSeries, value); }

        private ISeries[] _tierTimeSeries = Array.Empty<ISeries>();
        public ISeries[] TierTimeSeries { get => _tierTimeSeries; private set => SetProperty(ref _tierTimeSeries, value); }

        private Axis[] _tierXAxes = { new Axis() };
        public Axis[] TierXAxes { get => _tierXAxes; private set => SetProperty(ref _tierXAxes, value); }

        // Axes must not be shared between charts
        private Axis[] _tierXAxes2 = { new Axis() };
        public Axis[] TierXAxes2 { get => _tierXAxes2; private set => SetProperty(ref _tierXAxes2, value); }

        private Axis[] _countYAxes = { new Axis() };
        public Axis[] CountYAxes { get => _countYAxes; private set => SetProperty(ref _countYAxes, value); }

        private Axis[] _timeYAxes = { new Axis() };
        public Axis[] TimeYAxes { get => _timeYAxes; private set => SetProperty(ref _timeYAxes, value); }

        public override void Refresh()
        {
            List<ActivityInfo> maps = FilteredHistory().Where(a => a.Type == ACTIVITY_TYPES.MAP).ToList();
            int cap = Source.GetTimeCap(ACTIVITY_TYPES.MAP);
            int Capped(ActivityInfo a) => cap > 0 && a.TotalSeconds > cap ? cap : a.TotalSeconds;

            var counts = new int[MaxTier];
            var averages = new double[MaxTier];
            for (int t = 1; t <= MaxTier; t++)
            {
                List<ActivityInfo> ofTier = maps.Where(a => a.MapTier == t).ToList();
                counts[t - 1] = ofTier.Count;
                averages[t - 1] = ofTier.Count > 0 ? ofTier.Average(Capped) : 0;
            }
            CountByTier = counts;
            AverageByTier = averages;

            ByMap = CountRow.Build(maps.GroupBy(a => a.Area, StringComparer.OrdinalIgnoreCase)
                .Select(g => new CountRow
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Detail = ActivityInfo.FormatDuration((int)g.Average(Capped)) + " avg."
                }));
            ByTag = CountRow.ByTag(maps);

            MapCount = maps.Count;
            Summary = maps.Count == 0 ? "No maps in the filtered activities."
                : $"{maps.Count:N0} maps · {ByMap.Count:N0} different · average {ActivityInfo.FormatDuration((int)maps.Average(Capped))}";
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(CountByTier));
            OnPropertyChanged(nameof(AverageByTier));

            BuildCharts();
        }

        private SkiaSharp.SKColor MapColor => ActivityPalette.Sk(ActivityPalette.ColorOf(ACTIVITY_TYPES.MAP));

        private void BuildCharts()
        {
            string[] tiers = Enumerable.Range(1, MaxTier).Select(t => $"T{t}").ToArray();
            TierCountSeries = new[] { Columns("Maps", CountByTier.Select(c => (double)c), MapColor, v => $"{v:N0} maps") };
            TierTimeSeries = new[]
            {
                Columns("Average time", AverageByTier.Select(s => Math.Round(s / 60.0, 1)), MapColor, v => $"{v:0.0} min")
            };
            TierXAxes = new[] { CategoryAxis(tiers) };
            TierXAxes2 = new[] { CategoryAxis(tiers) };
            CountYAxes = new[] { ValueAxis() };
            TimeYAxes = new[] { ValueAxis(v => $"{v:0.#} min") };
        }

        protected override void OnThemeChanged() => BuildCharts();
    }
}
