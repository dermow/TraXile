using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>The dashboard on the global filter; its range buttons set the filter's time range.</summary>
    public class DashboardFilterTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter(LeagueCatalog.Demo(DateTime.Now));
        private readonly DashboardViewModel _vm;

        public DashboardFilterTests() => _vm = new DashboardViewModel(_source, filter: _filter);

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        private int OverviewCount => _vm.Overview.Where(r => !r.IsHideout).Sum(r => r.Count);

        [Theory]
        [InlineData(TimeRange.Today, FilterTime.Today)]
        [InlineData(TimeRange.Week, FilterTime.Last7Days)]
        [InlineData(TimeRange.Month, FilterTime.Last30Days)]
        [InlineData(TimeRange.All, FilterTime.All)]
        public void Range_buttons_set_the_filter_time(TimeRange range, FilterTime expected)
        {
            _vm.Range = range;
            Assert.Equal(expected, _filter.Time);
            Assert.Equal(range, _vm.Range);
        }

        [Fact]
        public void Other_filter_times_show_no_button_and_name_the_range()
        {
            _filter.Time = FilterTime.League;
            _filter.League = _filter.Leagues.First(l => !l.IsCurrent);

            Assert.Equal(TimeRange.Other, _vm.Range);
            Assert.Equal($"League {_filter.League.Name}", _vm.RangeLabel);
        }

        [Fact]
        public void Current_league_button_sets_the_league_filter()
        {
            LeagueInfo current = LeagueCatalog.Current(_filter.Leagues);
            Assert.True(_vm.HasCurrentLeague);

            _vm.Range = TimeRange.League;

            Assert.Equal(FilterTime.League, _filter.Time);
            Assert.Same(current, _filter.League);
            Assert.Equal(TimeRange.League, _vm.Range);
            Assert.Equal($"League {current.Name}", _vm.RangeLabel);
            Assert.Equal(_source.GetHistory().Count(a => a.Started >= current.Start && a.Started <= current.End), OverviewCount);
        }

        [Fact]
        public void Current_league_button_is_disabled_without_leagues()
        {
            var filter = new GlobalFilter();
            using (var vm = new DashboardViewModel(_source, filter: filter))
            {
                Assert.False(vm.HasCurrentLeague);
                vm.Range = TimeRange.League;
                Assert.Equal(FilterTime.All, filter.Time);
            }
        }

        [Fact]
        public void Starts_with_all_time_like_the_other_pages()
        {
            Assert.Equal(TimeRange.All, _vm.Range);
            Assert.Equal(_source.GetHistory().Count, OverviewCount);
        }

        [Fact]
        public void Type_and_tag_filter_apply_to_kpis_and_overview()
        {
            _filter.Type = ACTIVITY_TYPES.MAP;
            _filter.SetTag("juiced", true);

            int expected = _source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.MAP && a.Tags.Any(t => t.Id == "juiced"));
            Assert.Equal(expected, OverviewCount);
            // the type filter is maps only, so "Maps completed" counts all of them
            Assert.Equal(expected.ToString("N0"), _vm.Kpis.Single(k => k.Title == "Maps completed").Value);
        }

        [Fact]
        public void Recent_activities_ignore_the_filter_and_show_the_latest_five()
        {
            _filter.Type = ACTIVITY_TYPES.MAP;
            _filter.SetTag("juiced", true);
            _filter.Time = FilterTime.Last24Hours;

            Assert.Equal(_source.GetHistory().Take(5).Select(a => a.Id), _vm.Recent.Select(a => a.Id));
        }

        [Fact]
        public void Time_range_of_the_filter_applies()
        {
            _filter.Time = FilterTime.Last24Hours;

            DateTime from = DateTime.Now.AddHours(-24).AddMinutes(-1);
            Assert.Equal(_source.GetHistory().Count(a => a.Started >= from), OverviewCount);
            Assert.Equal("Activity by hour", _vm.DailyTitle);
        }

        private static readonly LeagueInfo[] Leagues =
        {
            new LeagueInfo { Name = "Old", Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 4, 1) },
            new LeagueInfo { Name = "Previous", Start = new DateTime(2026, 4, 10), End = new DateTime(2026, 7, 1) },
            new LeagueInfo { Name = "Current", Start = new DateTime(2026, 7, 10), End = new DateTime(2026, 11, 1) },
        };

        [Fact]
        public void League_compares_with_the_same_span_of_the_previous_league()
        {
            // day 20 of the current league vs. the first 20 days of the previous one
            (DateTime from, DateTime to) = DashboardViewModel.ComparisonWindow(
                Leagues[2].Start, Leagues[2].Start.AddDays(20), Leagues[2], Leagues);

            Assert.Equal(new DateTime(2026, 4, 10), from);
            Assert.Equal(new DateTime(2026, 4, 30), to);
        }

        [Fact]
        public void League_comparison_ends_with_the_previous_league()
        {
            // the previous league ran 82 days; the current one is at day 100
            (DateTime from, DateTime to) = DashboardViewModel.ComparisonWindow(
                Leagues[2].Start, Leagues[2].Start.AddDays(100), Leagues[2], Leagues);

            Assert.Equal(Leagues[1].Start, from);
            Assert.Equal(Leagues[1].End, to);
        }

        [Fact]
        public void Other_ranges_and_the_first_league_compare_with_the_period_right_before()
        {
            DateTime start = new DateTime(2026, 1, 1);
            Assert.Equal((start.AddDays(-7), start), DashboardViewModel.ComparisonWindow(start, start.AddDays(7), null, Leagues));
            Assert.Equal((start.AddDays(-7), start), DashboardViewModel.ComparisonWindow(start, start.AddDays(7), Leagues[0], Leagues));
        }

        [Fact]
        public void Live_card_is_not_filtered()
        {
            _filter.Type = ACTIVITY_TYPES.HEIST; // the demo's current activity is a map
            Assert.True(_vm.Live.IsTracking);
            Assert.Equal(ACTIVITY_TYPES.MAP, _vm.Live.Activity.Type);
        }
    }
}
