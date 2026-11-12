using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    /// <summary>Dashboard range buttons; <see cref="Other"/> = another time range of the global filter.</summary>
    public enum TimeRange { Today, Week, Month, League, All, Other }

    public class KpiViewModel : ObservableObject
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string Delta { get; set; }
        /// <summary>Explanation shown when hovering the tile; null for none.</summary>
        public string ToolTip { get; set; }
        public bool DeltaPositive { get; set; }
        /// <summary>The change is neither good nor bad: the delta is not colored.</summary>
        public bool DeltaNeutral { get; set; }
        public bool HasDelta => !string.IsNullOrEmpty(Delta);
        public Brush AccentBrush { get; set; }
        public ISeries[] Sparkline { get; set; }

        // Axes must not be shared between chart instances -> one set per KPI
        public Axis[] SparkXAxes { get; } = { new Axis { IsVisible = false } };
        public Axis[] SparkYAxes { get; } = { new Axis { IsVisible = false, MinLimit = 0 } };
        public Margin SparkMargin { get; } = new Margin(0);
    }

    /// <summary>
    /// The rotating KPI tile: one Statistic at a time from a shuffled deck. A click opens the Statistic on the
    /// Statistics page, the skip button moves on to the next one.
    /// </summary>
    public sealed class StatSpotlightViewModel : KpiViewModel
    {
        /// <summary>Key of the shown Statistic; null while none is eligible.</summary>
        public string StatKey { get; set; }
        public bool HasStat => StatKey != null;
        public IRelayCommand OpenCommand { get; set; }
        public IRelayCommand SkipCommand { get; set; }
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

    /// <summary>
    /// Row of the overview table under the time distribution (classic Statistics > Overview): one per activity type
    /// with its own donut slice, one "Other" row for the small types the donut groups, plus hideout time.
    /// </summary>
    public sealed class OverviewRow
    {
        public string Name { get; set; }
        /// <summary>Null for the "Other" row and for hideout time.</summary>
        public ACTIVITY_TYPES? Type { get; set; }
        public Brush Brush { get; set; }
        public int Count { get; set; }
        /// <summary>"–" for hideout time (no activities).</summary>
        public string CountText => IsHideout ? "–" : Count.ToString("N0");
        public double Seconds { get; set; }
        public string Hours => $"{Seconds / 3600.0:0.0} h";
        /// <summary>Share of the total time (activities plus, if shown, hideout), 0..1.</summary>
        public double Fraction { get; set; }
        public string Percent => $"{Fraction * 100:0.#} %";
        public bool IsHideout { get; set; }
        public bool IsOther => Members.Count > 0;
        /// <summary>The activity types grouped into the "Other" row, by time descending; empty otherwise.</summary>
        public IReadOnlyList<OverviewRow> Members { get; set; } = Array.Empty<OverviewRow>();
        /// <summary>A type grouped into "Other", shown as an indented sub-row while "Other" is expanded.</summary>
        public bool IsMember { get; set; }
        /// <summary>The "Other" row shows its members as sub-rows (chevron down); set before the table is rebuilt.</summary>
        public bool IsExpanded { get; set; }
    }

    public sealed class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ITrackingSource _source;
        private readonly DispatcherTimer _clock;
        private readonly DispatcherTimer _refreshDebounce;
        private IReadOnlyList<ActivityInfo> _history = Array.Empty<ActivityInfo>();

        // Axis / grid paints depend on the theme
        private bool _isDark = true;

        /// <summary>Classic setting: count hideout time in the time distribution and the overview.</summary>
        public const string ShowHideoutKey = "pie_chart_show_hideout";

        private readonly AppSetup _settings;
        private readonly bool _canWriteSettings;

        /// <param name="settings">config.xml for <see cref="ShowHideoutKey"/>; classic default (true) if null.</param>
        /// <param name="canWriteSettings">False while the classic app owns the data: changes are not stored.</param>
        /// <param name="filter">The global filter; a private, empty one if null (tests).</param>
        /// <param name="random">Shuffles the rotating Statistic tile; a new one if null.</param>
        public DashboardViewModel(ITrackingSource source, AppSetup settings = null, bool canWriteSettings = false, GlobalFilter filter = null,
            Random random = null)
        {
            _source = source;
            _stats = source as IStatsSource;
            _deck = new StatDeck(random);
            Filter = filter ?? new GlobalFilter();
            Filter.Changed += OnFilterChanged;
            Filter.LeaguesChanged += OnLeaguesChanged;
            _settings = settings;
            _canWriteSettings = canWriteSettings;
            _showHideout = !bool.TryParse(settings?.ReadSetting(ShowHideoutKey, "true"), out bool show) || show;

            TogglePauseCommand = new RelayCommand(() => { _source.TogglePause(); UpdateLive(); });
            StopCommand = new RelayCommand(StopCurrentActivity, () => Live.IsTracking);
            ToggleTagCommand = new RelayCommand<LiveTagChip>(ToggleTag);
            ToggleOtherCommand = new RelayCommand(ToggleOther);
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

            OpenStatisticCommand = new RelayCommand(() =>
            {
                if (Spotlight.StatKey != null) StatisticRequested?.Invoke(this, Spotlight.StatKey);
            });
            SkipStatisticCommand = new RelayCommand(SkipStatistic, () => _deck.Count > 1);
            _spotlight = SpotlightTile(null, null, null);

            _rotation = new DispatcherTimer { Interval = SpotlightInterval };
            _rotation.Tick += (s, e) => NextStatistic();
            _rotation.Start();

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

        /// <summary>The global filter; the dashboard shows the filtered activities (the live card is never filtered).</summary>
        public GlobalFilter Filter { get; }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Range));
            OnPropertyChanged(nameof(RangeLabel));
            Refresh();
        }

        private void OnLeaguesChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentLeague));
            OnPropertyChanged(nameof(HasCurrentLeague));
            OnPropertyChanged(nameof(Range));
        }

        /// <summary>The running league (null while no league metadata is known).</summary>
        public LeagueInfo CurrentLeague => LeagueCatalog.Current(Filter.Leagues);

        /// <summary>Enables the "Current league" range button.</summary>
        public bool HasCurrentLeague => CurrentLeague != null;

        /// <summary>
        /// The range buttons are shortcuts for the time range of the global filter
        /// (Today, last 7 / 30 days, current league, all time). Any other time range of the filter
        /// (an older league, 24 hours, custom, ...) reads as <see cref="TimeRange.Other"/>.
        /// </summary>
        public TimeRange Range
        {
            get
            {
                switch (Filter.Time)
                {
                    case FilterTime.Today: return TimeRange.Today;
                    case FilterTime.Last7Days: return TimeRange.Week;
                    case FilterTime.Last30Days: return TimeRange.Month;
                    case FilterTime.All: return TimeRange.All;
                    case FilterTime.League:
                        return Filter.League != null && Filter.League.Name == CurrentLeague?.Name
                            ? TimeRange.League : TimeRange.Other;
                    default: return TimeRange.Other;
                }
            }
            set
            {
                switch (value)
                {
                    case TimeRange.Today: Filter.Time = FilterTime.Today; break;
                    case TimeRange.Week: Filter.Time = FilterTime.Last7Days; break;
                    case TimeRange.Month: Filter.Time = FilterTime.Last30Days; break;
                    case TimeRange.All: Filter.Time = FilterTime.All; break;
                    case TimeRange.League:
                        LeagueInfo current = CurrentLeague;
                        if (current == null) break;
                        Filter.Batch(() =>
                        {
                            Filter.League = current;
                            Filter.Time = FilterTime.League;
                        });
                        break;
                }
            }
        }

        public string RangeLabel
        {
            get
            {
                switch (Filter.Time)
                {
                    case FilterTime.League: return Filter.League == null ? "League" : $"League {Filter.League.Name}";
                    case FilterTime.Custom:
                        TimeWindow w = Filter.Window;
                        return $"{w.From:yyyy-MM-dd HH:mm} – {w.To:yyyy-MM-dd HH:mm}";
                    default: return GlobalFilter.TimeLabel(Filter.Time);
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

        /// <summary>Raised on the UI thread after each live state poll (once per second and after live card actions).</summary>
        public event EventHandler<LiveState> LiveUpdated;

        private void UpdateLive()
        {
            LiveState s = _source.GetLiveState();
            Live = s;
            UpdateLiveTags(s);
            LiveUpdated?.Invoke(this, s);

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
        /// <summary>Overview table under the donut (classic Statistics > Overview), most time first, hideout last.</summary>
        public ObservableCollection<OverviewRow> Overview { get; } = new ObservableCollection<OverviewRow>();

        private bool _showHideout;
        /// <summary>Count hideout time in the time distribution (classic "show hideout time in pie chart").</summary>
        public bool ShowHideout
        {
            get => _showHideout;
            set
            {
                if (!SetProperty(ref _showHideout, value)) return;
                if (_canWriteSettings) _settings?.WriteSetting(ShowHideoutKey, value.ToString()); // "True"/"False" like the classic app
                Refresh();
            }
        }
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

        public string DailyTitle => _scale == Scale.Hourly ? "Activity by hour" : "Activity by day";

        private SolidColorPaint _chartTextPaint = new SolidColorPaint(ActivityPalette.Sk(DesignTokens.Get("Text").Dark));
        public SolidColorPaint ChartTextPaint { get => _chartTextPaint; private set => SetProperty(ref _chartTextPaint, value); }

        /// <summary>Chart colors come from the design tokens of the current theme.</summary>
        private SKColor Token(string key) => ActivityPalette.Sk(DesignTokens.Get(key).For(_isDark));
        private SKColor TextMuted => Token("TextMuted");
        private SKColor GridColor => Token("Border");

        /// <summary>Activities that pass the filter except its time range (charts that look beyond the range).</summary>
        private List<ActivityInfo> _filtered = new List<ActivityInfo>();

        /// <summary>How the charts bucket the range: by hour, 2 h, 6 h or day.</summary>
        private enum Scale { Hourly, Week, Month, Long }

        private Scale _scale = Scale.Long;

        private static Scale ScaleOf(DateTime from, DateTime to, bool unbounded)
        {
            if (unbounded) return Scale.Long;
            double days = (to - from).TotalDays;
            return days <= 1.05 ? Scale.Hourly : days <= 8 ? Scale.Week : days <= 31 ? Scale.Month : Scale.Long;
        }

        /// <summary>The filter's time range, ending at "now" at the latest; open starts begin at the oldest activity.</summary>
        private (DateTime from, DateTime to) GetRange()
        {
            DateTime now = DateTime.Now;
            TimeWindow w = Filter.Window;
            DateTime to = w.To.HasValue && w.To.Value < now ? w.To.Value : now;
            DateTime from = w.From ?? (_history.Count > 0 ? _history.Min(a => a.Started).Date : now.Date);
            if (from > to) from = to;
            return (from, to);
        }

        /// <summary>
        /// The period the deltas compare against: by default the period of the same length right before
        /// the range. For a league filter it is the previous league instead, over the same span from its
        /// start (day 20 of this league vs. day 20 of the previous one), ending at its end at the latest.
        /// </summary>
        private (DateTime from, DateTime to) ComparisonWindow(DateTime from, DateTime to) =>
            ComparisonWindow(from, to, Filter.Time == FilterTime.League ? Filter.League : null, Filter.Leagues);

        public static (DateTime from, DateTime to) ComparisonWindow(DateTime from, DateTime to,
            LeagueInfo league, IEnumerable<LeagueInfo> leagues)
        {
            TimeSpan len = to - from;
            LeagueInfo previous = PreviousLeague(league, leagues);
            if (previous == null) return (from - len, from);

            DateTime end = previous.Start + len;
            if (end > previous.End) end = previous.End;
            if (end > from) end = from;
            return (previous.Start, end);
        }

        /// <summary>The league that started last before <paramref name="league"/> (null for none).</summary>
        public static LeagueInfo PreviousLeague(LeagueInfo league, IEnumerable<LeagueInfo> leagues) =>
            league == null || leagues == null
                ? null
                : leagues.Where(l => l.Start < league.Start).OrderBy(l => l.Start).LastOrDefault();

        public void Refresh()
        {
            _history = _source.GetHistory();
            _filtered = _history.Where(Filter.MatchesIgnoringTime).ToList();
            (DateTime from, DateTime to) = GetRange();
            bool unbounded = !Filter.Window.From.HasValue;
            _scale = ScaleOf(from, to, unbounded);

            List<ActivityInfo> inRange = _filtered.Where(a => a.Started >= from && a.Started <= to).ToList();

            // Comparison period for the deltas
            (DateTime pFrom, DateTime pTo) = ComparisonWindow(from, to);
            List<ActivityInfo> prev = unbounded
                ? new List<ActivityInfo>()
                : _filtered.Where(a => a.Started >= pFrom && a.Started < pTo).ToList();

            BuildKpis(inRange, prev);
            BuildTimeDistribution(inRange, from, to);
            BuildDaily(inRange, from, to);
            BuildTrend(from, to);

            Recent.Clear();
            foreach (ActivityInfo a in inRange.Take(12)) Recent.Add(a);

            OnPropertyChanged(nameof(DailyTitle));
            _spotlightTask = LoadSpotlightAsync();
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

            // The rotating Statistic; its values load in the background (LoadSpotlightAsync)
            Kpis.Add(_spotlight);

            Kpis.Add(Kpi("Maps completed", maps.Count.ToString("N0"), "",
                Delta(maps.Count, prevMaps.Count, higherIsBetter: true), ActivityPalette.ColorOf(ActivityPalette.Green),
                Daily(a => a.Type == ACTIVITY_TYPES.MAP ? 1 : 0)));

            Kpis.Add(Kpi("Avg. map time", avgMap > 0 ? ActivityInfo.FormatDuration((int)avgMap) : "—", "",
                Delta(avgMap, prevAvgMap, higherIsBetter: false), ActivityPalette.ColorOf(ActivityPalette.Green),
                Daily(a => a.Type == ACTIVITY_TYPES.MAP ? a.TotalSeconds / 60.0 : double.NaN, average: true)));

            Kpis.Add(Kpi("Time played", hours.ToString("0.0"), "h",
                Delta(hours, prevHours, higherIsBetter: true), ActivityPalette.ColorOf(ActivityPalette.Blue),
                Daily(a => Capped(a) / 3600.0)));

            KpiViewModel deathsKpi = Kpi("Deaths", deaths.ToString("N0"), "",
                Delta(deaths, prevDeaths, higherIsBetter: false), DesignTokens.Get("Brass").For(_isDark),
                Daily(a => a.Deaths));
            deathsKpi.ToolTip = "Deaths during the Activities in this filter.";
            Kpis.Add(deathsKpi);
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
            Color accent, double[] spark) =>
            Fill(new KpiViewModel(), title, value, unit, delta, accent, spark);

        private static T Fill<T>(T kpi, string title, string value, string unit, (string text, bool good) delta,
            Color accent, double[] spark) where T : KpiViewModel
        {
            SKColor c = ActivityPalette.Sk(accent);
            kpi.Title = title;
            kpi.Value = value;
            kpi.Unit = unit;
            kpi.Delta = delta.text;
            kpi.DeltaPositive = delta.good;
            kpi.AccentBrush = ActivityPalette.BrushOf(accent);
            kpi.Sparkline = new ISeries[]
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
            };
            return kpi;
        }

        // ------------------------------------------------------------------
        // Rotating Statistic tile
        // ------------------------------------------------------------------

        /// <summary>How long the tile shows one Statistic.</summary>
        public static readonly TimeSpan SpotlightInterval = TimeSpan.FromMinutes(2);

        private readonly IStatsSource _stats;
        private readonly StatDeck _deck;
        private readonly DispatcherTimer _rotation;
        private StatSpotlightViewModel _spotlight;
        private Task _spotlightTask = Task.CompletedTask;
        private int _spotlightVersion;

        /// <summary>The rotating tile (first KPI).</summary>
        public StatSpotlightViewModel Spotlight => _spotlight;

        /// <summary>Opens the shown Statistic on the Statistics page (raises <see cref="StatisticRequested"/>).</summary>
        public IRelayCommand OpenStatisticCommand { get; }

        /// <summary>Moves on to the next Statistic and restarts the rotation timer.</summary>
        public IRelayCommand SkipStatisticCommand { get; }

        /// <summary>The player wants to see this Statistic (key) on the Statistics page.</summary>
        public event EventHandler<string> StatisticRequested;

        /// <summary>Completes when the tile's values are loaded (tests).</summary>
        public async Task WhenSpotlightIdle()
        {
            Task t;
            do { t = _spotlightTask; await t; } while (t != _spotlightTask);
        }

        private void SkipStatistic()
        {
            _rotation.Stop();
            _rotation.Start();
            NextStatistic();
        }

        private void NextStatistic()
        {
            _deck.Next();
            _spotlightTask = LoadSpotlightAsync();
        }

        /// <summary>
        /// Reads the Statistics of the filter's time range in the background, updates the deck's pool (only
        /// Statistics &gt; 0 are eligible) and shows the current one with its delta and last 14 days.
        /// </summary>
        private async Task LoadSpotlightAsync()
        {
            int version = ++_spotlightVersion;
            if (_stats == null)
            {
                ShowSpotlight(null, null, null);
                return;
            }

            TimeWindow w = Filter.Window;
            DateTime now = DateTime.Now;
            bool hasPrevious = w.From.HasValue; // like the other KPIs: no delta for an open range
            LeagueInfo league = Filter.Time == FilterTime.League ? Filter.League : null;
            LeagueInfo previousLeague = PreviousLeague(league, Filter.Leagues);
            (DateTime from, DateTime to, IReadOnlyList<StatInfo> stats) = await Task.Run(() =>
            {
                // the same window as the Statistics page, so a click there shows the same value
                (DateTime f, DateTime t) = StatMath.Window(w.From, w.To, _stats.OldestStatTime, now);
                return (f, t, _stats.GetStats(f, t));
            });
            if (version != _spotlightVersion) return;

            _deck.Update(stats.Where(s => s.Value > 0).Select(s => s.Key));
            SkipStatisticCommand.NotifyCanExecuteChanged();
            StatInfo stat = stats.FirstOrDefault(s => s.Key == _deck.Current);
            if (stat == null)
            {
                ShowSpotlight(null, null, null);
                return;
            }

            DateTime sparkFrom = now.Date.AddDays(-13);
            (int? previous, IReadOnlyList<StatEntry> entries) = await Task.Run(() =>
            {
                int? p = null;
                if (hasPrevious)
                {
                    // a league compares with the previous league (see ComparisonWindow)
                    (DateTime pFrom, DateTime pTo) = previousLeague != null
                        ? ComparisonWindow(from, to, league, new[] { previousLeague })
                        : StatMath.PreviousWindow(from, to);
                    p = _stats.GetStats(pFrom, pTo).FirstOrDefault(s => s.Key == stat.Key)?.Value ?? 0;
                }
                return (p, _stats.GetStatSeries(stat.Key, sparkFrom, now));
            });
            if (version != _spotlightVersion) return;

            double[] spark = StatMath.DailyIncrease(entries, sparkFrom, now)
                .Select(d => stat.IsDuration ? d.Value / 3600.0 : d.Value).ToArray();
            ShowSpotlight(stat, previous, spark);
        }

        private void ShowSpotlight(StatInfo stat, int? previous, double[] spark)
        {
            _spotlight = SpotlightTile(stat, previous, spark);
            int i = Kpis.IndexOf(Kpis.OfType<StatSpotlightViewModel>().FirstOrDefault());
            if (i >= 0) Kpis[i] = _spotlight;
            OnPropertyChanged(nameof(Spotlight));
        }

        private StatSpotlightViewModel SpotlightTile(StatInfo stat, int? previous, double[] spark)
        {
            var tile = new StatSpotlightViewModel
            {
                StatKey = stat?.Key,
                DeltaNeutral = true, // more of a Statistic is neither good nor bad
                OpenCommand = OpenStatisticCommand,
                SkipCommand = SkipStatisticCommand,
            };
            return Fill(tile,
                stat?.Name ?? "No statistics in this range",
                stat?.ValueText ?? "—",
                "",
                stat != null && previous.HasValue ? Delta(stat.Value, previous.Value, higherIsBetter: true) : (null, true),
                DesignTokens.Get("Accent").For(_isDark),
                spark ?? new double[14]);
        }

        /// <summary>Per-day values for the last 14 days (sparklines).</summary>
        private double[] Daily(Func<ActivityInfo, double> selector, bool average = false)
        {
            DateTime start = DateTime.Now.Date.AddDays(-13);
            var result = new double[14];
            for (int i = 0; i < 14; i++)
            {
                DateTime d = start.AddDays(i);
                var vals = _filtered.Where(a => a.Started.Date == d).Select(selector).Where(v => !double.IsNaN(v)).ToList();
                result[i] = vals.Count == 0 ? 0 : (average ? vals.Average() : vals.Sum());
            }
            return result;
        }

        /// <summary>Share of the total time below which an activity type is grouped into "Other".</summary>
        public const double OtherThreshold = 0.04;

        /// <summary>
        /// Rows of the time distribution, shared by the donut and the overview table: types by time descending,
        /// then "Other" (only if at least two types are below <see cref="OtherThreshold"/>), then hideout (if &gt; 0).
        /// </summary>
        public static List<OverviewRow> GroupTimeDistribution(
            IEnumerable<(ACTIVITY_TYPES type, int count, double seconds)> byType, double hideoutSeconds)
        {
            var types = byType.OrderByDescending(t => t.seconds).ToList();
            double total = types.Sum(t => t.seconds) + hideoutSeconds;
            double Frac(double s) => total > 0 ? s / total : 0;

            OverviewRow TypeRow((ACTIVITY_TYPES type, int count, double seconds) t) => new OverviewRow
            {
                Name = ActivityPalette.DisplayName(t.type),
                Type = t.type,
                Brush = ActivityPalette.Brush(t.type),
                Count = t.count,
                Seconds = t.seconds,
                Fraction = Frac(t.seconds)
            };

            var small = types.Where(t => Frac(t.seconds) < OtherThreshold).ToList();
            bool group = small.Count >= 2; // a lone small type keeps its name

            var rows = types.Where(t => !group || Frac(t.seconds) >= OtherThreshold).Select(TypeRow).ToList();
            if (group)
            {
                double seconds = small.Sum(t => t.seconds);
                rows.Add(new OverviewRow
                {
                    Name = "Other",
                    Brush = ActivityPalette.BrushOf(ActivityPalette.ColorOf(ActivityPalette.Other)),
                    Count = small.Sum(t => t.count),
                    Seconds = seconds,
                    Fraction = Frac(seconds),
                    Members = small.Select(t =>
                    {
                        OverviewRow m = TypeRow(t);
                        m.IsMember = true;
                        m.Brush = ActivityPalette.BrushOf(ActivityPalette.ColorOf(ActivityPalette.Other));
                        return m;
                    }).ToList()
                });
            }
            if (hideoutSeconds > 0)
            {
                rows.Add(new OverviewRow
                {
                    Name = "Hideout",
                    Brush = ActivityPalette.BrushOf(ActivityPalette.ColorOf(ActivityPalette.Hideout)),
                    Seconds = hideoutSeconds,
                    Fraction = Frac(hideoutSeconds),
                    IsHideout = true
                });
            }
            return rows;
        }

        private void BuildTimeDistribution(List<ActivityInfo> cur, DateTime from, DateTime to)
        {
            var byType = cur.GroupBy(a => a.Type)
                            .Select(g => (g.Key, g.Count(), (double)g.Sum(Capped)));
            double hideout = ShowHideout ? _source.GetHideoutSeconds(from, to) : 0;

            List<OverviewRow> rows = GroupTimeDistribution(byType, hideout);
            Overview.Clear();
            foreach (OverviewRow r in rows) Overview.Add(r);

            // the expansion survives refreshes as long as there is an "Other" row
            if (!rows.Any(r => r.IsOther)) IsOtherExpanded = false;
            FillOverviewTable();

            double total = rows.Sum(r => r.Seconds);
            TotalHours = (total / 3600.0).ToString(total >= 36000 ? "0" : "0.0");

            // the view shows its own tooltip per slice (RowOfSlice), so the series carry no tooltip text
            var series = new List<ISeries>();
            foreach (OverviewRow r in rows)
            {
                Color color = r.IsHideout ? ActivityPalette.ColorOf(ActivityPalette.Hideout)
                            : r.IsOther ? ActivityPalette.ColorOf(ActivityPalette.Other)
                            : ActivityPalette.ColorOf(r.Type.Value);

                series.Add(new PieSeries<double>
                {
                    Name = r.Name,
                    Values = new[] { Math.Round(r.Seconds / 3600.0, 2) },
                    InnerRadius = 78,
                    CornerRadius = 2,
                    Fill = new SolidColorPaint(ActivityPalette.Sk(color)),
                    Stroke = new SolidColorPaint(Token("Surface"), 2), // surface ring between slices
                    HoverPushout = 6,
                });
            }
            TimeSeries = series.ToArray();
        }

        /// <summary>
        /// Rows shown in the overview table: <see cref="Overview"/>, plus the members of "Other" as sub-rows
        /// right below it while <see cref="IsOtherExpanded"/>.
        /// </summary>
        public ObservableCollection<OverviewRow> OverviewTable { get; } = new ObservableCollection<OverviewRow>();

        private bool _isOtherExpanded;
        /// <summary>The "Other" row shows its grouped types; kept across refreshes, not persisted.</summary>
        public bool IsOtherExpanded { get => _isOtherExpanded; private set => SetProperty(ref _isOtherExpanded, value); }

        /// <summary>Expands / collapses the "Other" row (click on the row or on its donut slice).</summary>
        public IRelayCommand ToggleOtherCommand { get; }

        private void ToggleOther()
        {
            if (!Overview.Any(r => r.IsOther)) return;
            IsOtherExpanded = !IsOtherExpanded;
            FillOverviewTable();
        }

        private void FillOverviewTable()
        {
            OverviewTable.Clear();
            foreach (OverviewRow r in Overview)
            {
                r.IsExpanded = r.IsOther && IsOtherExpanded;
                OverviewTable.Add(r);
                if (r.IsExpanded)
                    foreach (OverviewRow m in r.Members) OverviewTable.Add(m);
            }
        }

        /// <summary>The overview row a donut slice stands for (series and rows share their order); null if unknown.</summary>
        public OverviewRow RowOfSlice(ISeries slice)
        {
            int i = Array.IndexOf(TimeSeries, slice);
            return i >= 0 && i < Overview.Count ? Overview[i] : null;
        }

        private void BuildDaily(List<ActivityInfo> cur, DateTime from, DateTime to)
        {
            bool hourly = _scale == Scale.Hourly;

            // Buckets
            var labels = new List<string>();
            var bucketStarts = new List<DateTime>();
            if (hourly)
            {
                // Whole hours of the range ("Today": 00-23, "Last 24 hours": 25 buckets across midnight)
                DateTime first = new DateTime(from.Year, from.Month, from.Day, from.Hour, 0, 0);
                DateTime end = Filter.Window.To ?? to;
                int hours = Math.Max(1, Math.Min(25, (int)Math.Ceiling((end - first).TotalHours)));
                for (int h = 0; h < hours; h++) { DateTime b = first.AddHours(h); bucketStarts.Add(b); labels.Add($"{b.Hour:00}"); }
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
                    // longer ranges: a label every 3rd / 7th day (counted back from the last day), the rest stay empty
                    int step = days <= 14 ? 1 : days <= 31 ? 3 : 7;
                    bool labelled = (days - 1 - i) % step == 0;
                    labels.Add(!labelled ? "" : days <= 7 ? d.ToString("ddd") : d.ToString("dd.MM"));
                }
                cur = _filtered.Where(a => a.Started >= start && a.Started <= to).ToList();
            }

            int Bucket(DateTime t) => hourly ? (int)Math.Floor((t - bucketStarts[0]).TotalHours) : (int)(t.Date - bucketStarts[0]).TotalDays;

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
                    ForceStepToMin = true,
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
            bool hourly = _scale == Scale.Hourly;
            TimeSpan bucket;
            DateTime start;
            int count;
            // the axis spans the range up to the end of today at most; the curve stops at "now"
            DateTime end = Filter.Window.To.HasValue && Filter.Window.To.Value < to.Date.AddDays(1) ? Filter.Window.To.Value : to.Date.AddDays(1);
            switch (_scale)
            {
                case Scale.Hourly: bucket = TimeSpan.FromHours(1); start = new DateTime(from.Year, from.Month, from.Day, from.Hour, 0, 0); break;
                case Scale.Week: bucket = TimeSpan.FromHours(2); start = from.Date; break;
                case Scale.Month: bucket = TimeSpan.FromHours(6); start = from.Date; break;
                default: bucket = TimeSpan.FromDays(1); start = to.Date.AddDays(-59); end = to.Date.AddDays(1); break;
            }
            count = Math.Max(1, (int)Math.Ceiling((end - start).Ticks / (double)bucket.Ticks));
            int elapsed = Math.Min(count, (int)((to - start).Ticks / bucket.Ticks) + 1);

            (double[] cur, int[] deaths) = Cumulate(_filtered, start, bucket, count);
            double[] current = cur.Take(elapsed).ToArray(); // the curve stops at "now"

            LeagueInfo previousLeague = Filter.Time == FilterTime.League ? PreviousLeague(Filter.League, Filter.Leagues) : null;
            double[] previous;
            if (!Filter.Window.From.HasValue)
            {
                previous = Array.Empty<double>();
            }
            else if (previousLeague != null)
            {
                // the previous league, aligned to its start (see ComparisonWindow); stops at its end
                previous = Cumulate(_filtered.Where(a => a.Started < previousLeague.End),
                    start - (from - previousLeague.Start), bucket, count).cumulative;
            }
            else
            {
                TimeSpan length = TimeSpan.FromTicks(bucket.Ticks * count);
                previous = Cumulate(_filtered, start - length, bucket, count).cumulative;
            }
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

            TrendTitle = (_scale == Scale.Long && !Filter.Window.From.HasValue ? "Activities, last 60 days" : $"Activities, {RangeLabel}")
                + $" · {current.LastOrDefault():N0}";
        }

        public void Dispose()
        {
            _clock.Stop();
            _rotation.Stop();
            _spotlightVersion++; // drop a running load
            Filter.Changed -= OnFilterChanged;
            Filter.LeaguesChanged -= OnLeaguesChanged;
            _refreshDebounce.Stop();
            _source.HistoryChanged -= OnHistoryChanged;
        }
    }
}
