using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Key figures of one league (classic Main.RenderLeagueDashboard).</summary>
    public sealed class LeagueStats
    {
        public LeagueInfo League { get; set; }
        public int Activities { get; set; }
        /// <summary>Capped seconds of all activities.</summary>
        public double PlaySeconds { get; set; }
        public int Maps { get; set; }
        /// <summary>Maps with area level 83 or higher (classic "T16").</summary>
        public int MapsT16 { get; set; }
        public double AverageMapSeconds { get; set; }
        public double AverageMapT16Seconds { get; set; }
        public int Deaths { get; set; }
        public double CampaignSeconds { get; set; }
    }

    /// <summary>One of the eight comparison charts.</summary>
    public sealed class LeagueChart
    {
        public string Title { get; set; }
        /// <summary>How the figures are computed, if the title does not say it; null for none.</summary>
        public string Note { get; set; }
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
    }

    /// <summary>
    /// Leagues page: compares the last 3/5/10/15/20 leagues in eight charts (classic Main.RenderLeagueDashboard).
    /// Uses the global filter except its time range: every league uses its whole play period. Times are capped at the time cap of
    /// their activity type, like the classic dashboard.
    /// </summary>
    public sealed class LeaguesViewModel : AnalyticsViewModel
    {
        public static readonly int[] CountOptions = { 3, 5, 10, 15, 20 };

        public LeaguesViewModel(ITrackingSource source, GlobalFilter filter) : base(source, filter)
        {
            Filter.LeaguesChanged += OnLeaguesChanged;
            Refresh();
        }

        private void OnLeaguesChanged(object sender, EventArgs e) => Refresh();

        public override void Dispose()
        {
            Filter.LeaguesChanged -= OnLeaguesChanged;
            base.Dispose();
        }

        public IReadOnlyList<int> LeagueCounts => CountOptions;

        private int _leagueCount = 5;
        /// <summary>How many of the newest leagues to compare (classic default 5, not stored).</summary>
        public int LeagueCount
        {
            get => _leagueCount;
            set { if (SetProperty(ref _leagueCount, value)) Refresh(); }
        }

        public bool HasLeagues => Filter.Leagues.Count > 0;

        private IReadOnlyList<LeagueStats> _stats = Array.Empty<LeagueStats>();
        /// <summary>The compared leagues, oldest first.</summary>
        public IReadOnlyList<LeagueStats> Stats { get => _stats; private set => SetProperty(ref _stats, value); }

        private IReadOnlyList<LeagueChart> _charts = Array.Empty<LeagueChart>();
        public IReadOnlyList<LeagueChart> Charts { get => _charts; private set => SetProperty(ref _charts, value); }

        /// <summary>Figures of one league from all activities that started in its play period.</summary>
        public static LeagueStats Compute(LeagueInfo league, IEnumerable<ActivityInfo> history, Func<ACTIVITY_TYPES, int> timeCap)
        {
            List<ActivityInfo> acts = history.Where(a => a.Started >= league.Start && a.Started <= league.End).ToList();
            double Capped(ActivityInfo a)
            {
                int cap = timeCap(a.Type);
                return cap > 0 && a.TotalSeconds > cap ? cap : a.TotalSeconds;
            }
            List<ActivityInfo> maps = acts.Where(a => a.Type == ACTIVITY_TYPES.MAP).ToList();
            List<ActivityInfo> t16 = maps.Where(a => a.AreaLevel >= 83).ToList();
            return new LeagueStats
            {
                League = league,
                Activities = acts.Count,
                PlaySeconds = acts.Sum(Capped),
                Maps = maps.Count,
                MapsT16 = t16.Count,
                AverageMapSeconds = maps.Count > 0 ? maps.Average(Capped) : 0,
                AverageMapT16Seconds = t16.Count > 0 ? t16.Average(Capped) : 0,
                Deaths = acts.Sum(a => a.Deaths),
                CampaignSeconds = acts.Where(a => a.Type == ACTIVITY_TYPES.CAMPAIGN).Sum(Capped)
            };
        }

        public override void Refresh()
        {
            // type, area, level and tags of the global filter apply; its time range does not (each league has its own)
            List<ActivityInfo> history = Source.GetHistory().Where(Filter.MatchesIgnoringTime).ToList();
            Stats = Filter.Leagues.OrderByDescending(l => l.Start).Take(LeagueCount).Reverse()
                .Select(l => Compute(l, history, Source.GetTimeCap))
                .ToList();
            OnPropertyChanged(nameof(HasLeagues));
            BuildCharts();
        }

        protected override void OnThemeChanged() => BuildCharts();

        private void BuildCharts()
        {
            // every chart is a single series compared across leagues, so one color for all keeps the grid calm
            SKColor color = AccentColor;
            Func<double, string> count = v => v.ToString("N0");
            Func<double, string> hours = v => $"{v:0.#} h";
            Func<double, string> minutes = v => $"{v:0.#} min";

            LeagueChart Chart(string title, string note, Func<LeagueStats, double> value, Func<double, string> format) =>
                new LeagueChart
                {
                    Title = title,
                    Note = note,
                    Series = new[] { Columns(title, Stats.Select(s => Math.Round(value(s), 1)), color, format) },
                    XAxes = new[] { CategoryAxis(Stats.Select(s => s.League.Name).ToList()) },
                    YAxes = new[] { ValueAxis(format == count ? null : format) } // counts: whole steps
                };

            Charts = new[]
            {
                Chart("Maps", null, s => s.Maps, count),
                Chart("T16 maps", "Maps of area level 83 and above.", s => s.MapsT16, count),
                Chart("Activities", null, s => s.Activities, count),
                Chart("Playtime", "Capped at the time caps.", s => s.PlaySeconds / 3600, hours),
                Chart("Average map time", "Capped at the map time cap.", s => s.AverageMapSeconds / 60, minutes),
                Chart("Average T16 map time", "Maps of area level 83 and above, capped at the map time cap.", s => s.AverageMapT16Seconds / 60, minutes),
                Chart("Deaths", null, s => s.Deaths, count),
                Chart("Campaign", null, s => s.CampaignSeconds / 3600, hours),
            };
        }
    }
}
