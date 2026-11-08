using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Statistics page: all stat counters of the core with their value in the filter's time range, and the
    /// selected counter as a zoomable time series (classic "All stats" tab, Main.RenderAllStatsDashboard).
    /// Only the time range of the global filter applies: the counters are not linked to activities.
    /// </summary>
    public sealed class StatisticsViewModel : AnalyticsViewModel
    {
        private readonly IStatsSource _stats;
        private readonly Func<DateTime> _now;
        private IReadOnlyList<StatInfo> _all = Array.Empty<StatInfo>();
        private int _version;

        public StatisticsViewModel(ITrackingSource source, GlobalFilter filter, Func<DateTime> now = null) : base(source, filter)
        {
            _stats = source as IStatsSource;
            _now = now ?? (() => DateTime.Now);
            Refresh();
        }

        /// <summary>The source has no stat counters (should not happen with the real core or demo data).</summary>
        public bool IsAvailable => _stats != null;

        // ------------------------------------------------------------------
        // Window
        // ------------------------------------------------------------------

        private DateTime _from, _to;

        /// <summary>
        /// The filter's time range widened to whole days, like the classic stats list; "All" starts at the oldest
        /// stored stat value.
        /// </summary>
        public (DateTime from, DateTime to) Window()
        {
            TimeWindow w = Filter.Window;
            DateTime now = _now();
            DateTime from = w.From ?? _stats?.OldestStatTime ?? now;
            DateTime to = w.To ?? now;
            return (from.Date, to.Date.AddDays(1).AddSeconds(-1));
        }

        public string RangeText => $"{_from:yyyy-MM-dd} – {_to:yyyy-MM-dd}";

        // ------------------------------------------------------------------
        // List
        // ------------------------------------------------------------------

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value ?? "")) ApplySearch(); }
        }

        private IReadOnlyList<StatInfo> _items = Array.Empty<StatInfo>();
        /// <summary>Stats matching the search, by name.</summary>
        public IReadOnlyList<StatInfo> Items { get => _items; private set => SetProperty(ref _items, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

        public bool IsEmpty => !IsLoading && Items.Count == 0;

        private void ApplySearch()
        {
            string[] terms = SearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Items = _all
                .Where(s => terms.All(t => s.Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                           s.Key.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            OnPropertyChanged(nameof(IsEmpty));
        }

        public override void Refresh() => _reloadTask = ReloadAsync();

        private Task _reloadTask = Task.CompletedTask;
        private Task _seriesTask = Task.CompletedTask;

        /// <summary>Completes when the running reload and chart load are done (tests, screenshots).</summary>
        public async Task WhenIdle()
        {
            Task r, s;
            do
            {
                r = _reloadTask;
                s = _seriesTask;
                await Task.WhenAll(r, s);
            } while (r != _reloadTask || s != _seriesTask);
        }

        /// <summary>Reads the values in the background (the queries can take a moment on big databases).</summary>
        public async Task ReloadAsync()
        {
            if (_stats == null) return;
            int version = ++_version;
            (DateTime from, DateTime to) = Window();
            _from = from;
            _to = to;
            OnPropertyChanged(nameof(RangeText));
            IsLoading = true;

            IReadOnlyList<StatInfo> stats = await Task.Run(() => _stats.GetStats(from, to));
            if (version != _version) return; // a newer reload is running

            _all = stats;
            IsLoading = false;
            ApplySearch();

            string key = SelectedStat?.Key ?? _pendingKey;
            SelectedStat = _all.FirstOrDefault(s => s.Key == key) ?? Items.FirstOrDefault(s => s.Value > 0) ?? Items.FirstOrDefault();
            _seriesTask = LoadSeriesAsync();
            await _seriesTask;
        }

        private string _pendingKey;

        // ------------------------------------------------------------------
        // Chart of the selected stat
        // ------------------------------------------------------------------

        private StatInfo _selectedStat;
        public StatInfo SelectedStat
        {
            get => _selectedStat;
            set
            {
                if (!SetProperty(ref _selectedStat, value)) return;
                _pendingKey = value?.Key;
                OnPropertyChanged(nameof(HasSelection));
                _seriesTask = LoadSeriesAsync();
            }
        }

        public bool HasSelection => SelectedStat != null;

        private IReadOnlyList<KeyValuePair<DateTime, int>> _daily = Array.Empty<KeyValuePair<DateTime, int>>();
        /// <summary>Increase per day of the selected stat in the window.</summary>
        public IReadOnlyList<KeyValuePair<DateTime, int>> Daily { get => _daily; private set => SetProperty(ref _daily, value); }

        private ISeries[] _series = Array.Empty<ISeries>();
        public ISeries[] Series { get => _series; private set => SetProperty(ref _series, value); }

        private Axis[] _xAxes = { new Axis() };
        public Axis[] XAxes { get => _xAxes; private set => SetProperty(ref _xAxes, value); }

        private Axis[] _yAxes = { new Axis() };
        public Axis[] YAxes { get => _yAxes; private set => SetProperty(ref _yAxes, value); }

        private int _seriesVersion;

        private async Task LoadSeriesAsync()
        {
            StatInfo stat = SelectedStat;
            int version = ++_seriesVersion;
            if (stat == null || _stats == null)
            {
                Daily = Array.Empty<KeyValuePair<DateTime, int>>();
                Series = Array.Empty<ISeries>();
                return;
            }

            DateTime from = _from, to = _to;
            IReadOnlyList<StatEntry> entries = await Task.Run(() => _stats.GetStatSeries(stat.Key, from, to));
            if (version != _seriesVersion) return;

            Daily = StatMath.DailyIncrease(entries, from, to);
            BuildChart(stat);
        }

        protected override void OnThemeChanged()
        {
            if (SelectedStat != null) BuildChart(SelectedStat);
        }

        private void BuildChart(StatInfo stat)
        {
            bool duration = stat.IsDuration;
            Series = new ISeries[]
            {
                new ColumnSeries<DateTimePoint>
                {
                    Name = stat.Name,
                    Values = Daily.Select(d => new DateTimePoint(d.Key, duration ? Math.Round(d.Value / 3600.0, 2) : d.Value)).ToArray(),
                    Fill = new SolidColorPaint(AccentColor),
                    Stroke = null,
                    Rx = 2,
                    Ry = 2,
                    MaxBarWidth = 26,
                    Padding = 2,
                    YToolTipLabelFormatter = p => duration ? $"{p.Coordinate.PrimaryValue:0.0} h" : $"{p.Coordinate.PrimaryValue:N0}",
                }
            };
            XAxes = new Axis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM d"))
                {
                    LabelsPaint = new SolidColorPaint(TextMuted),
                    TextSize = 11,
                    SeparatorsPaint = null,
                    TicksPaint = null,
                }
            };
            YAxes = new[] { ValueAxis(duration ? (Func<double, string>)(v => $"{v:0.#} h") : v => v.ToString("N0")) };
        }
    }
}
