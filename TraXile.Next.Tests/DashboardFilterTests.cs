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
            _filter.League = LeagueCatalog.Current(_filter.Leagues);

            Assert.Equal(TimeRange.Other, _vm.Range);
            Assert.Equal($"League {_filter.League.Name}", _vm.RangeLabel);
        }

        [Fact]
        public void Starts_with_all_time_like_the_other_pages()
        {
            Assert.Equal(TimeRange.All, _vm.Range);
            Assert.Equal(_source.GetHistory().Count, OverviewCount);
        }

        [Fact]
        public void Type_and_tag_filter_apply_to_kpis_overview_and_recent()
        {
            _filter.Type = ACTIVITY_TYPES.MAP;
            _filter.SetTag("juiced", true);

            int expected = _source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.MAP && a.Tags.Any(t => t.Id == "juiced"));
            Assert.Equal(expected, OverviewCount);
            Assert.Equal(expected.ToString("N0"), _vm.Kpis.Single(k => k.Title == "Activities").Value);
            Assert.All(_vm.Recent, a => Assert.Contains(a.Tags, t => t.Id == "juiced"));
        }

        [Fact]
        public void Time_range_of_the_filter_applies()
        {
            _filter.Time = FilterTime.Last24Hours;

            DateTime from = DateTime.Now.AddHours(-24).AddMinutes(-1);
            Assert.Equal(_source.GetHistory().Count(a => a.Started >= from), OverviewCount);
            Assert.Equal("Activity by hour", _vm.DailyTitle);
            Assert.All(_vm.Recent, a => Assert.True(a.Started >= from));
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
