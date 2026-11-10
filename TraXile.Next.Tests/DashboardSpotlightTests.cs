using System;
using System.Linq;
using System.Threading.Tasks;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>The rotating Statistic tile on the dashboard (replaces the former "Activities" KPI).</summary>
    public class DashboardSpotlightTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();

        public void Dispose() => _source.Dispose();

        private async Task<DashboardViewModel> NewVm(TimeRange range = TimeRange.Month, ITrackingSource source = null)
        {
            var vm = new DashboardViewModel(source ?? _source, random: new Random(42)) { Range = range };
            await vm.WhenSpotlightIdle();
            return vm;
        }

        [Fact]
        public async Task First_tile_shows_a_statistic_of_the_range_instead_of_the_activity_count()
        {
            using (DashboardViewModel vm = await NewVm())
            {
                Assert.Same(vm.Spotlight, vm.Kpis[0]);
                Assert.DoesNotContain(vm.Kpis, k => k.Title == "Activities");
                Assert.Equal(5, vm.Kpis.Count);

                // Same value as on the Statistics page for this range
                (DateTime from, DateTime to) = StatMath.Window(vm.Filter.Window.From, vm.Filter.Window.To, _source.OldestStatTime, DateTime.Now);
                StatInfo stat = _source.GetStats(from, to).Single(s => s.Key == vm.Spotlight.StatKey);
                Assert.True(stat.Value > 0);
                Assert.Equal(stat.Name, vm.Spotlight.Title);
                Assert.Equal(stat.ValueText, vm.Spotlight.Value);
                Assert.Equal(14, ((LiveChartsCore.SkiaSharpView.LineSeries<double>)vm.Spotlight.Sparkline[0]).Values.Count());
            }
        }

        [Fact]
        public async Task Delta_is_neutral_and_missing_for_an_open_range()
        {
            using (DashboardViewModel vm = await NewVm(TimeRange.Week))
            {
                Assert.True(vm.Spotlight.DeltaNeutral);
            }
            using (DashboardViewModel vm = await NewVm(TimeRange.All))
            {
                Assert.False(vm.Spotlight.HasDelta);
            }
        }

        [Fact]
        public async Task Skip_shows_another_statistic()
        {
            using (DashboardViewModel vm = await NewVm())
            {
                string first = vm.Spotlight.StatKey;
                Assert.True(vm.SkipStatisticCommand.CanExecute(null));

                vm.SkipStatisticCommand.Execute(null);
                await vm.WhenSpotlightIdle();

                Assert.NotNull(vm.Spotlight.StatKey);
                Assert.NotEqual(first, vm.Spotlight.StatKey);
                Assert.Same(vm.Spotlight, vm.Kpis[0]);
            }
        }

        [Fact]
        public async Task A_refresh_keeps_the_shown_statistic()
        {
            using (DashboardViewModel vm = await NewVm())
            {
                string shown = vm.Spotlight.StatKey;

                vm.Refresh();
                await vm.WhenSpotlightIdle();

                Assert.Equal(shown, vm.Spotlight.StatKey);
            }
        }

        [Fact]
        public async Task Open_requests_the_shown_statistic()
        {
            using (DashboardViewModel vm = await NewVm())
            {
                string requested = null;
                vm.StatisticRequested += (s, key) => requested = key;

                vm.Spotlight.OpenCommand.Execute(null);

                Assert.Equal(vm.Spotlight.StatKey, requested);
            }
        }

        [Fact]
        public async Task Without_statistics_the_tile_shows_a_placeholder()
        {
            using (DashboardViewModel vm = await NewVm())
            {
                // a range long before the demo data
                vm.Filter.Batch(() =>
                {
                    vm.Filter.CustomFrom = new DateTime(2001, 1, 1);
                    vm.Filter.CustomTo = new DateTime(2001, 1, 31);
                    vm.Filter.Time = FilterTime.Custom;
                });
                await vm.WhenSpotlightIdle();

                Assert.Null(vm.Spotlight.StatKey);
                Assert.Equal("—", vm.Spotlight.Value);
                Assert.Equal("No statistics in this range", vm.Spotlight.Title);
                Assert.False(vm.SkipStatisticCommand.CanExecute(null));

                string requested = null;
                vm.StatisticRequested += (s, key) => requested = key;
                vm.Spotlight.OpenCommand.Execute(null);
                Assert.Null(requested);
            }
        }

        [Fact]
        public async Task Statistics_page_selects_the_requested_statistic_and_clears_a_hiding_search()
        {
            using (var stats = new StatisticsViewModel(_source, new GlobalFilter()))
            {
                await stats.WhenIdle();
                stats.SearchText = "zzz-no-match";

                stats.Select("HideoutTimeSec");
                await stats.WhenIdle();

                Assert.Equal("HideoutTimeSec", stats.SelectedStat.Key);
                Assert.Equal("", stats.SearchText);
            }
        }
    }
}
