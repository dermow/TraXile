using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    public enum TimeRange { Today, Week, Month, All }

    public sealed class KpiViewModel : ObservableObject
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string Delta { get; set; }
        public bool DeltaPositive { get; set; }
        public bool HasDelta => !string.IsNullOrEmpty(Delta);
        public Brush AccentBrush { get; set; }
        public ISeries[] Sparkline { get; set; }

        // Axes must not be shared between chart instances -> one set per KPI
        public Axis[] SparkXAxes { get; } = { new Axis { IsVisible = false } };
        public Axis[] SparkYAxes { get; } = { new Axis { IsVisible = false, MinLimit = 0 } };
        public Margin SparkMargin { get; } = new Margin(0);
    }

    /// <summary>A tag on the live card: custom tags can be toggled, default tags are only shown while active.</summary>
    public sealed class LiveTagChip
    {
        public TagInfo Tag { get; set; }
        public bool IsActive { get; set; }
        public bool CanToggle => !Tag.IsDefault;
        public string Name => Tag.Name;
        public Brush Background => Tag.Background;
        public Brush Foreground => Tag.Foreground;
        public string ToolTip => CanToggle
            ? (IsActive ? $"Remove \"{Tag.Name}\" from the current activity" : $"Add \"{Tag.Name}\" to the current activity")
            : "Set automatically";
    }

    public sealed class NoticeItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public bool IsError { get; set; }
    }

    public sealed class LegendItem
    {
        public string Name { get; set; }
        public Brush Brush { get; set; }
        public string Hours { get; set; }
        public string Percent { get; set; }
        public double Fraction { get; set; }
    }

    public sealed class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ITrackingSource _source;
        private readonly DispatcherTimer _clock;
        private readonly DispatcherTimer _refreshDebounce;
        private IReadOnlyList<ActivityInfo> _history = Array.Empty<ActivityInfo>();

        // Axis / grid paints depend on the theme
        private bool _isDark = true;

        public DashboardViewModel(ITrackingSource source)
        {
            _source = source;

            TogglePauseCommand = new RelayCommand(() => { _source.TogglePause(); UpdateLive(); });
            StopCommand = new RelayCommand(StopCurrentActivity, () => Live.IsTracking);
            ToggleTagCommand = new RelayCommand<LiveTagChip>(ToggleTag);
            SetRangeCommand = new RelayCommand<string>(r =>
            {
                if (Enum.TryParse(r, out TimeRange tr)) Range = tr;
            });

            _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clock.Tick += (s, e) => UpdateLive();
            _clock.Start();

            // Coalesce bursts of core events (e.g. during initial log parsing)
            _refreshDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _refreshDebounce.Tick += (s, e) => { _refreshDebounce.Stop(); Refresh(); };

            _source.HistoryChanged += OnHistoryChanged;
            if (!string.IsNullOrEmpty(_source.Notice)) AddNotice(_source.IsDemo ? "Demo mode" : "Note", _source.Notice);

            Refresh();
            UpdateLive();
        }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            _refreshDebounce.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshDebounce.Stop();
                _refreshDebounce.Start();
            }));
        }

        // ------------------------------------------------------------------
        // Commands & general state
        // ------------------------------------------------------------------

        public IRelayCommand TogglePauseCommand { get; }
        public IRelayCommand<string> SetRangeCommand { get; }

        /// <summary>Finishes the current activity (classic: Stop button on the tracking panel).</summary>
        public IRelayCommand StopCommand { get; }

        private void StopCurrentActivity()
        {
            _source.StopCurrentActivity();
            UpdateLive();
            Refresh(); // the finished activity shows up in the history right away
        }

        /// <summary>Adds or removes a custom tag on the current activity (classic: tag labels on the tracking panel).</summary>
        public IRelayCommand<LiveTagChip> ToggleTagCommand { get; }

        private void ToggleTag(LiveTagChip chip)
        {
            if (chip == null || !chip.CanToggle || !Live.IsTracking) return;
            _source.ToggleCurrentActivityTag(chip.Tag.Id);
            UpdateLive();
        }

        public bool IsDemo => _source.IsDemo;
        /// <summary>Messages shown above the live card (demo mode, restored backup, ...).</summary>
        public ObservableCollection<NoticeItem> Notices { get; } = new ObservableCollection<NoticeItem>();

        public void AddNotice(string title, string text, bool isError = false) =>
            Notices.Add(new NoticeItem { Title = title, Text = text, IsError = isError });

        private TimeRange _range = TimeRange.Week;
        public TimeRange Range
        {
            get => _range;
            set
            {
                if (SetProperty(ref _range, value))
                {
                    OnPropertyChanged(nameof(RangeLabel));
                    Refresh();
                }
            }
        }

        public string RangeLabel
        {
            get
            {
                switch (Range)
                {
                    case TimeRange.Today: return "Today";
                    case TimeRange.Week: return "Last 7 days";
                    case TimeRange.Month: return "Last 30 days";
                    default: return "All time";
                }
            }
        }

        public void SetTheme(bool dark)
        {
            _isDark = dark;
            ChartTextPaint = new SolidColorPaint(Token("Text"));
            Refresh();
        }

        // ------------------------------------------------------------------
        // Live tracking card
        // ------------------------------------------------------------------

        private LiveState _live = new LiveState();
        public LiveState Live
        {
            get => _live;
            private set
            {
                bool wasTracking = _live.IsTracking;
                if (SetProperty(ref _live, value) && wasTracking != value.IsTracking) StopCommand?.NotifyCanExecuteChanged();
            }
        }

        private string _elapsedText = "00:00";
        public string ElapsedText { get => _elapsedText; private set => SetProperty(ref _elapsedText, value); }

        private string _liveTitle = "Waiting for activity";
        public string LiveTitle { get => _liveTitle; private set => SetProperty(ref _liveTitle, value); }

        private string _liveSubtitle = "Enter a map, heist or boss fight to start tracking";
        public string LiveSubtitle { get => _liveSubtitle; private set => SetProperty(ref _liveSubtitle, value); }

        private Brush _liveBrush = ActivityPalette.BrushOf(ActivityPalette.Other);
        public Brush LiveBrush { get => _liveBrush; private set => SetProperty(ref _liveBrush, value); }

        private double _liveProgress;
        /// <summary>Progress of current run vs. the average for this activity type (0..1).</summary>
        public double LiveProgress { get => _liveProgress; private set => SetProperty(ref _liveProgress, value); }

        private string _liveCompare = "";
        public string LiveCompare { get => _liveCompare; private set => SetProperty(ref _liveCompare, value); }

        /// <summary>Active default tags (read only) followed by all custom tags (toggles).</summary>
        public ObservableCollection<LiveTagChip> LiveTags { get; } = new ObservableCollection<LiveTagChip>();

        private string _liveTagsKey = "";

        private void UpdateLiveTags(LiveState s)
        {
            var active = new HashSet<string>(s.ActiveTags.Select(t => t.Id));
            var chips = new List<LiveTagChip>();
            if (s.IsTracking)
            {
                chips.AddRange(s.ActiveTags.Where(t => t.IsDefault).Select(t => new LiveTagChip { Tag = t, IsActive = true }));
                chips.AddRange(_source.GetTags().Where(t => !t.IsDefault)
                    .Select(t => new LiveTagChip { Tag = t, IsActive = active.Contains(t.Id) }));
            }

            // Polled every second: only rebuild when something changed (keeps hover and focus)
            string key = string.Join("|", chips.Select(c => $"{c.Tag.Id}:{c.Name}:{c.IsActive}"));
            if (key == _liveTagsKey) return;
            _liveTagsKey = key;

            LiveTags.Clear();
            foreach (LiveTagChip c in chips) LiveTags.Add(c);
        }

        private void UpdateLive()
        {
            LiveState s = _source.GetLiveState();
            Live = s;
            UpdateLiveTags(s);

            if (s.IsTracking && s.Activity != null)
            {
                ActivityInfo a = s.Activity;
                ElapsedText = ActivityInfo.FormatDuration((int)s.Elapsed.TotalSeconds);
                LiveTitle = a.Area;
                LiveSubtitle = string.Join("  ·  ", new[] { a.TypeName, a.LevelText, a.Deaths > 0 ? $"{a.Deaths} deaths" : null }
                    .Where(x => !string.IsNullOrEmpty(x)));
                LiveBrush = a.TypeBrush;

                // Compare against the average of the same type
                double avg = _history.Where(h => h.Type == a.Type && h.TotalSeconds > 0)
                                     .Take(200).Select(h => (double)h.TotalSeconds).DefaultIfEmpty(0).Average();
                if (avg > 0)
                {
                    LiveProgress = Math.Min(1.0, s.Elapsed.TotalSeconds / avg);
                    double diff = s.Elapsed.TotalSeconds - avg;
                    LiveCompare = diff <= 0
                        ? $"{ActivityInfo.FormatDuration((int)-diff)} under your average ({ActivityInfo.FormatDuration((int)avg)})"
                        : $"{ActivityInfo.FormatDuration((int)diff)} over your average ({ActivityInfo.FormatDuration((int)avg)})";
                }
                else
                {
                    LiveProgress = 0;
                    LiveCompare = "First run of this type";
                }
            }
            else
            {
                ElapsedText = "00:00";
                LiveTitle = "Waiting for activity";
                LiveSubtitle = s.CurrentArea != "—" ? $"Currently in {s.CurrentArea}" : "Enter a map, heist or boss fight to start tracking";
                LiveBrush = ActivityPalette.BrushOf(ActivityPalette.Other);
                LiveProgress = 0;
                LiveCompare = "";
            }
        }

        // ------------------------------------------------------------------
        // KPIs, charts, recent list
        // ------------------------------------------------------------------

        public ObservableCollection<KpiViewModel> Kpis { get; } = new ObservableCollection<KpiViewModel>();
        public ObservableCollection<LegendItem> TimeLegend { get; } = new ObservableCollection<LegendItem>();
        public ObservableCollection<ActivityInfo> Recent { get; } = new ObservableCollection<ActivityInfo>();

        private ISeries[] _timeSeries = Array.Empty<ISeries>();
        public ISeries[] TimeSeries { get => _timeSeries; private set => SetProperty(ref _timeSeries, value); }

        private string _totalHours = "0";
        public string TotalHours { get => _totalHours; private set => SetProperty(ref _totalHours, value); }

        private ISeries[] _dailySeries = Array.Empty<ISeries>();
        public ISeries[] DailySeries { get => _dailySeries; private set => SetProperty(ref _dailySeries, value); }

        private Axis[] _dailyXAxes = { new Axis() };
        public Axis[] DailyXAxes { get => _dailyXAxes; private set => SetProperty(ref _dailyXAxes, value); }

        private Axis[] _dailyYAxes = { new Axis() };
        public Axis[] DailyYAxes { get => _dailyYAxes; private set => SetProperty(ref _dailyYAxes, value); }

        public string DailyTitle => Range == TimeRange.Today ? "Activity by hour" : "Activity by day";

        private SolidColorPaint _chartTextPaint = new SolidColorPaint(ActivityPalette.Sk(DesignTokens.Get("Text").Dark));
        public SolidColorPaint ChartTextPaint { get => _chartTextPaint; private set => SetProperty(ref _chartTextPaint, value); }

        /// <summary>Chart colors come from the design tokens of the current theme.</summary>
        private SKColor Token(string key) => ActivityPalette.Sk(DesignTokens.Get(key).For(_isDark));
        private SKColor TextMuted => Token("TextMuted");
        private SKColor GridColor => Token("Border");

        private (DateTime from, DateTime to) GetRange(TimeRange r)
        {
            DateTime now = DateTime.Now;
            switch (r)
            {
                case TimeRange.Today: return (now.Date, now);
                case TimeRange.Week: return (now.Date.AddDays(-6), now);
                case TimeRange.Month: return (now.Date.AddDays(-29), now);
                default:
                    DateTime oldest = _history.Count > 0 ? _history.Min(a => a.Started) : now.Date;
                    return (oldest.Date, now);
            }
        }

        public void Refresh()
        {
            _history = _source.GetHistory();
            (DateTime from, DateTime to) = GetRange(Range);

            List<ActivityInfo> inRange = _history.Where(a => a.Started >= from && a.Started <= to).ToList();

            // Previous period of the same length, for deltas
            TimeSpan len = to - from;
            List<ActivityInfo> prev = Range == TimeRange.All
                ? new List<ActivityInfo>()
                : _history.Where(a => a.Started >= from - len && a.Started < from).ToList();

            BuildKpis(inRange, prev);
            BuildTimeDistribution(inRange, from, to);
            BuildDaily(inRange, from, to);
            BuildTrend(from, to);

            Recent.Clear();
            foreach (ActivityInfo a in _history.Take(12)) Recent.Add(a);

            OnPropertyChanged(nameof(DailyTitle));
        }

        private int Capped(ActivityInfo a) => Math.Min(a.TotalSeconds, _source.GetTimeCap(a.Type));

        private void BuildKpis(List<ActivityInfo> cur, List<ActivityInfo> prev)
        {
            Kpis.Clear();

            var maps = cur.Where(a => a.Type == ACTIVITY_TYPES.MAP).ToList();
            var prevMaps = prev.Where(a => a.Type == ACTIVITY_TYPES.MAP).ToList();

            double avgMap = maps.Count > 0 ? maps.Average(a => a.TotalSeconds) : 0;
            double prevAvgMap = prevMaps.Count > 0 ? prevMaps.Average(a => a.TotalSeconds) : 0;

            double hours = cur.Sum(Capped) / 3600.0;
            double prevHours = prev.Sum(Capped) / 3600.0;

            int deaths = cur.Sum(a => a.Deaths);
            int prevDeaths = prev.Sum(a => a.Deaths);

            Kpis.Add(Kpi("Activities", cur.Count.ToString("N0"), "",
                Delta(cur.Count, prev.Count, higherIsBetter: true), DesignTokens.Get("Accent").For(_isDark),
                Daily(a => 1)));

            Kpis.Add(Kpi("Maps completed", maps.Count.ToString("N0"), "",
                Delta(maps.Count, prevMaps.Count, higherIsBetter: true), ActivityPalette.ColorOf(ActivityPalette.Green),
                Daily(a => a.Type == ACTIVITY_TYPES.MAP ? 1 : 0)));

            Kpis.Add(Kpi("Avg. map time", avgMap > 0 ? ActivityInfo.FormatDuration((int)avgMap) : "—", "",
                Delta(avgMap, prevAvgMap, higherIsBetter: false), ActivityPalette.ColorOf(ActivityPalette.Green),
                Daily(a => a.Type == ACTIVITY_TYPES.MAP ? a.TotalSeconds / 60.0 : double.NaN, average: true)));

            Kpis.Add(Kpi("Time played", hours.ToString("0.0"), "h",
                Delta(hours, prevHours, higherIsBetter: true), ActivityPalette.ColorOf(ActivityPalette.Blue),
                Daily(a => Capped(a) / 3600.0)));

            Kpis.Add(Kpi("Deaths", deaths.ToString("N0"), "",
                Delta(deaths, prevDeaths, higherIsBetter: false), DesignTokens.Get("Brass").For(_isDark),
                Daily(a => a.Deaths)));
        }

        /// <summary>
        /// Relative change vs. the previous period. The arrow shows the direction of the value,
        /// "good" decides the color (e.g. a lower map time is good).
        /// </summary>
        private static (string text, bool good) Delta(double current, double previous, bool higherIsBetter)
        {
            if (previous <= 0 || current <= 0) return (null, true);
            double pct = (current - previous) / previous * 100.0;
            if (Math.Abs(pct) < 0.5) return ("±0%", true);
            bool good = higherIsBetter ? pct > 0 : pct < 0;
            return ($"{(pct > 0 ? "▲" : "▼")} {Math.Abs(pct):0}%", good);
        }

        private KpiViewModel Kpi(string title, string value, string unit, (string text, bool good) delta,
            Color accent, double[] spark)
        {
            SKColor c = ActivityPalette.Sk(accent);
            return new KpiViewModel
            {
                Title = title,
                Value = value,
                Unit = unit,
                Delta = delta.text,
                DeltaPositive = delta.good,
                AccentBrush = ActivityPalette.BrushOf(accent),
                Sparkline = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = spark,
                        GeometrySize = 0,
                        LineSmoothness = 0.7,
                        Stroke = new SolidColorPaint(c, 2),
                        Fill = new LinearGradientPaint(
                            new[] { c.WithAlpha(90), c.WithAlpha(0) },
                            new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                        IsHoverable = false,
                    }
                }
            };
        }

        /// <summary>Per-day values for the last 14 days (sparklines).</summary>
        private double[] Daily(Func<ActivityInfo, double> selector, bool average = false)
        {
            DateTime start = DateTime.Now.Date.AddDays(-13);
            var result = new double[14];
            for (int i = 0; i < 14; i++)
            {
                DateTime d = start.AddDays(i);
                var vals = _history.Where(a => a.Started.Date == d).Select(selector).Where(v => !double.IsNaN(v)).ToList();
                result[i] = vals.Count == 0 ? 0 : (average ? vals.Average() : vals.Sum());
            }
            return result;
        }

        private void BuildTimeDistribution(List<ActivityInfo> cur, DateTime from, DateTime to)
        {
            var byType = cur.GroupBy(a => a.Type)
                            .Select(g => new { Type = g.Key, Seconds = (double)g.Sum(Capped) })
                            .OrderByDescending(x => x.Seconds)
                            .ToList();

            double hideout = _source.GetHideoutSeconds(from, to);
            double total = byType.Sum(x => x.Seconds) + hideout;

            var slices = new List<(string name, Color color, double seconds)>();
            double other = 0;
            foreach (var t in byType)
            {
                if (total > 0 && t.Seconds / total >= 0.04) slices.Add((ActivityPalette.DisplayName(t.Type), ActivityPalette.ColorOf(t.Type), t.Seconds));
                else other += t.Seconds;
            }
            if (other > 0) slices.Add(("Other", ActivityPalette.ColorOf(ActivityPalette.Other), other));
            if (hideout > 0) slices.Add(("Hideout", ActivityPalette.ColorOf(ActivityPalette.Hideout), hideout));

            TotalHours = (total / 3600.0).ToString(total >= 36000 ? "0" : "0.0");

            TimeLegend.Clear();
            var series = new List<ISeries>();
            foreach (var s in slices)
            {
                double hours = s.seconds / 3600.0;
                double frac = total > 0 ? s.seconds / total : 0;

                series.Add(new PieSeries<double>
                {
                    Name = s.name,
                    Values = new[] { Math.Round(hours, 2) },
                    InnerRadius = 78,
                    CornerRadius = 2,
                    Fill = new SolidColorPaint(ActivityPalette.Sk(s.color)),
                    Stroke = new SolidColorPaint(Token("Surface"), 2), // surface ring between slices
                    HoverPushout = 6,
                    ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:0.0} h ({frac:P0})",
                });

                TimeLegend.Add(new LegendItem
                {
                    Name = s.name,
                    Brush = ActivityPalette.BrushOf(s.color),
                    Hours = $"{hours:0.0} h",
                    Percent = $"{frac:P0}",
                    Fraction = frac
                });
            }
            TimeSeries = series.ToArray();
        }

        private void BuildDaily(List<ActivityInfo> cur, DateTime from, DateTime to)
        {
            bool hourly = Range == TimeRange.Today;

            // Buckets
            var labels = new List<string>();
            var bucketStarts = new List<DateTime>();
            if (hourly)
            {
                for (int h = 0; h < 24; h++) { bucketStarts.Add(from.Date.AddHours(h)); labels.Add($"{h:00}"); }
            }
            else
            {
                int days = Math.Max(7, (int)Math.Ceiling((to.Date - from.Date).TotalDays) + 1);
                days = Math.Min(days, 60); // keep "All time" readable: last 60 days
                DateTime start = to.Date.AddDays(-(days - 1));
                for (int i = 0; i < days; i++)
                {
                    DateTime d = start.AddDays(i);
                    bucketStarts.Add(d);
                    labels.Add(days <= 7 ? d.ToString("ddd") : d.ToString("dd.MM"));
                }
                cur = _history.Where(a => a.Started >= start && a.Started <= to).ToList();
            }

            int Bucket(DateTime t) => hourly ? t.Hour : (int)(t.Date - bucketStarts[0]).TotalDays;

            // Top 4 types are stacked individually, the rest is "Other"
            var top = cur.GroupBy(a => a.Type).OrderByDescending(g => g.Sum(Capped)).Take(4).Select(g => g.Key)
                // stack in palette slot order: neighbouring slots are validated to stay distinct
                .OrderBy(t => ActivityPalette.SlotOf(t).Index).ToList();

            var series = new List<ISeries>();
            foreach (ACTIVITY_TYPES type in top)
            {
                series.Add(Stacked(ActivityPalette.DisplayName(type), ActivityPalette.ColorOf(type),
                    Values(cur.Where(a => a.Type == type), bucketStarts.Count, Bucket)));
            }
            var rest = cur.Where(a => !top.Contains(a.Type)).ToList();
            if (rest.Count > 0)
            {
                series.Add(Stacked("Other", ActivityPalette.ColorOf(ActivityPalette.Other), Values(rest, bucketStarts.Count, Bucket)));
            }

            DailySeries = series.ToArray();

            DailyXAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(TextMuted),
                    TextSize = 11,
                    SeparatorsPaint = null,
                    TicksPaint = null,
                    MinStep = 1,
                    ForceStepToMin = labels.Count <= 31,
                }
            };
            DailyYAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    LabelsPaint = new SolidColorPaint(TextMuted),
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(GridColor, 1),
                    Labeler = v => hourly ? $"{v:0}m" : $"{v:0.#}h",
                }
            };

            double[] Values(IEnumerable<ActivityInfo> items, int count, Func<DateTime, int> bucket)
            {
                var v = new double[count];
                foreach (ActivityInfo a in items)
                {
                    int i = bucket(a.Started);
                    if (i >= 0 && i < count) v[i] += hourly ? Capped(a) / 60.0 : Capped(a) / 3600.0;
                }
                return v.Select(x => Math.Round(x, 2)).ToArray();
            }
        }

        private ISeries Stacked(string name, Color color, double[] values) => new StackedColumnSeries<double>
        {
            Name = name,
            Values = values,
            Fill = new SolidColorPaint(ActivityPalette.Sk(color)),
            // 2 px surface gap between stacked segments
            Stroke = new SolidColorPaint(Token("Surface"), 2),
            Rx = 2,
            Ry = 2,
            MaxBarWidth = 26,
            Padding = 3,
        };

        // ------------------------------------------------------------------
        // Trend (live card): cumulative activities in the range vs. the previous period,
        // drawn like the TraXile chart on exile-tools.com
        // ------------------------------------------------------------------

        private ISeries[] _trendSeries = Array.Empty<ISeries>();
        public ISeries[] TrendSeries { get => _trendSeries; private set => SetProperty(ref _trendSeries, value); }

        private Axis[] _trendXAxes = { new Axis { IsVisible = false } };
        public Axis[] TrendXAxes { get => _trendXAxes; private set => SetProperty(ref _trendXAxes, value); }

        private Axis[] _trendYAxes = { new Axis { IsVisible = false } };
        public Axis[] TrendYAxes { get => _trendYAxes; private set => SetProperty(ref _trendYAxes, value); }

        public Margin TrendMargin { get; } = new Margin(6, 12, 6, 6);

        private string _trendTitle = "";
        public string TrendTitle { get => _trendTitle; private set => SetProperty(ref _trendTitle, value); }

        private bool _hasPreviousTrend;
        public bool HasPreviousTrend { get => _hasPreviousTrend; private set => SetProperty(ref _hasPreviousTrend, value); }

        /// <summary>Cumulative activity count per bucket for a period, and the deaths in each bucket.</summary>
        internal static (double[] cumulative, int[] deaths) Cumulate(IEnumerable<ActivityInfo> items, DateTime start, TimeSpan bucket, int count)
        {
            var perBucket = new double[count];
            var deaths = new int[count];
            foreach (ActivityInfo a in items)
            {
                int i = (int)((a.Started - start).Ticks / bucket.Ticks);
                if (i < 0 || i >= count) continue;
                perBucket[i]++;
                deaths[i] += a.Deaths;
            }
            var cumulative = new double[count];
            double sum = 0;
            for (int i = 0; i < count; i++) { sum += perBucket[i]; cumulative[i] = sum; }
            return (cumulative, deaths);
        }

        private void BuildTrend(DateTime from, DateTime to)
        {
            // Fine buckets, so play sessions show up as steps of the curve
            bool hourly = Range == TimeRange.Today;
            TimeSpan bucket;
            DateTime start;
            int count;
            switch (Range)
            {
                case TimeRange.Today: bucket = TimeSpan.FromHours(1); start = from.Date; count = 24; break;
                case TimeRange.Week: bucket = TimeSpan.FromHours(2); start = from.Date; count = 7 * 12; break;
                case TimeRange.Month: bucket = TimeSpan.FromHours(6); start = from.Date; count = 30 * 4; break;
                default: bucket = TimeSpan.FromDays(1); count = 60; start = to.Date.AddDays(-(count - 1)); break;
            }
            int elapsed = Math.Min(count, (int)((to - start).Ticks / bucket.Ticks) + 1);

            (double[] cur, int[] deaths) = Cumulate(_history, start, bucket, count);
            double[] current = cur.Take(elapsed).ToArray(); // the curve stops at "now"

            TimeSpan length = TimeSpan.FromTicks(bucket.Ticks * count);
            double[] previous = Range == TimeRange.All
                ? Array.Empty<double>()
                : Cumulate(_history, start - length, bucket, count).cumulative;
            HasPreviousTrend = previous.Length > 0;

            SKColor curve = Token("Accent");
            var series = new List<ISeries>();

            if (previous.Length > 0)
            {
                series.Add(new LineSeries<double>
                {
                    Name = "Previous period",
                    Values = previous,
                    GeometrySize = 0,
                    LineSmoothness = 0.35,
                    Fill = null,
                    Stroke = new SolidColorPaint(Token("TextMuted").WithAlpha(150), 1.5f)
                    {
                        PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 5, 5 })
                    },
                    IsHoverable = false,
                });
            }

            series.Add(new LineSeries<double>
            {
                Name = "This period",
                Values = current,
                GeometrySize = 0,
                LineSmoothness = 0.35,
                Stroke = new SolidColorPaint(curve, 2.5f),
                Fill = new LinearGradientPaint(
                    new[] { curve.WithAlpha(66), curve.WithAlpha(18), curve.WithAlpha(0) },
                    new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                YToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:N0} activities",
            });

            // Brass markers where deaths happened, sitting on the curve
            var markers = Enumerable.Range(0, current.Length)
                .Where(i => deaths[i] > 0)
                .Select(i => new LiveChartsCore.Defaults.ObservablePoint(i, current[i]))
                .ToArray();
            if (markers.Length > 0)
            {
                series.Add(new ScatterSeries<LiveChartsCore.Defaults.ObservablePoint>
                {
                    Name = "Deaths",
                    Values = markers,
                    GeometrySize = 9,
                    Fill = new SolidColorPaint(Token("Brass")),
                    Stroke = new SolidColorPaint(Token("Surface"), 2),
                    YToolTipLabelFormatter = p => $"{deaths[(int)p.Coordinate.SecondaryValue]} deaths",
                });
            }

            TrendSeries = series.ToArray();
            double max = Math.Max(current.DefaultIfEmpty(0).Max(), previous.DefaultIfEmpty(0).Max());
            TrendXAxes = new[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = count - 1 } };
            TrendYAxes = new[] { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = Math.Max(1, max * 1.15) } };

            TrendTitle = (hourly ? "Activities today" : Range == TimeRange.All ? "Activities, last 60 days" : $"Activities, {RangeLabel.ToLowerInvariant()}")
                + $" · {current.LastOrDefault():N0}";
        }

        public void Dispose()
        {
            _clock.Stop();
            _refreshDebounce.Stop();
            _source.HistoryChanged -= OnHistoryChanged;
        }
    }
}
