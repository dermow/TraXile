using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class StatisticsTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();

        public void Dispose() => _source.Dispose();

        // ------------------------------------------------------------------
        // Math
        // ------------------------------------------------------------------

        [Theory]
        [InlineData(1, 10, 10)]  // classic: a minimum of 1 is the first increment, it counts
        [InlineData(5, 10, 5)]
        [InlineData(0, 0, 0)]
        public void Increase_follows_the_classic_rule(int min, int max, int expected)
        {
            Assert.Equal(expected, StatMath.Increase(min, max));
        }

        [Fact]
        public void Daily_increase_uses_the_value_before_the_window_as_start()
        {
            var day = new DateTime(2026, 11, 1);
            var entries = new[]
            {
                new StatEntry(day.AddDays(-3), 40),          // before the window: start value
                new StatEntry(day.AddHours(10), 41),
                new StatEntry(day.AddHours(11), 43),
                new StatEntry(day.AddDays(2).AddHours(9), 44),
            };

            IReadOnlyList<KeyValuePair<DateTime, int>> daily = StatMath.DailyIncrease(entries, day, day.AddDays(3).AddSeconds(-1));

            Assert.Equal(new[] { day, day.AddDays(1), day.AddDays(2) }, daily.Select(d => d.Key));
            Assert.Equal(new[] { 3, 0, 1 }, daily.Select(d => d.Value));
        }

        [Fact]
        public void Daily_increase_treats_a_drop_as_a_counter_reset()
        {
            var day = new DateTime(2026, 11, 1);
            var entries = new[] { new StatEntry(day.AddHours(1), 10), new StatEntry(day.AddHours(2), 2) };

            var daily = StatMath.DailyIncrease(entries, day, day.AddDays(1).AddSeconds(-1));

            Assert.Equal(12, daily.Single().Value);
        }

        // ------------------------------------------------------------------
        // Demo source
        // ------------------------------------------------------------------

        [Fact]
        public void Demo_stats_follow_the_demo_history()
        {
            DateTime from = DateTime.Now.AddYears(-1), to = DateTime.Now.AddDays(1);
            StatInfo maps = _source.GetStats(from, to).Single(s => s.Key == "TotalMapsDone");

            Assert.Equal(_source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.MAP), maps.Value);
            Assert.Equal("Total maps done", maps.Name);
        }

        // ------------------------------------------------------------------
        // View model
        // ------------------------------------------------------------------

        private async Task<StatisticsViewModel> NewVm(GlobalFilter filter = null)
        {
            var vm = new StatisticsViewModel(_source, filter ?? new GlobalFilter());
            await vm.WhenIdle();
            return vm;
        }

        [Fact]
        public async Task Lists_all_stats_by_name_and_selects_one_with_a_value()
        {
            using (StatisticsViewModel vm = await NewVm())
            {
                Assert.True(vm.Items.Count >= 10);
                string[] names = vm.Items.Select(s => s.Name).ToArray();
                Assert.Equal(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase), names);
                Assert.NotNull(vm.SelectedStat);
                Assert.True(vm.SelectedStat.Value > 0);
                Assert.NotEmpty(vm.Series);
            }
        }

        [Fact]
        public async Task Search_matches_name_and_key()
        {
            using (StatisticsViewModel vm = await NewVm())
            {
                vm.SearchText = "maps";
                Assert.Equal(new[] { "TotalMapsDone" }, vm.Items.Select(s => s.Key));

                vm.SearchText = "HideoutTimeSec";
                Assert.Equal("Hideout time", vm.Items.Single().Name);
            }
        }

        [Fact]
        public async Task Chart_shows_the_daily_increase_of_the_selected_stat()
        {
            using (StatisticsViewModel vm = await NewVm())
            {
                vm.SelectedStat = vm.Items.Single(s => s.Key == "TotalMapsDone");
                await vm.WhenIdle();

                Assert.Equal(vm.SelectedStat.Value, vm.Daily.Sum(d => d.Value));
                Assert.Equal(vm.Window().from.Date, vm.Daily.First().Key);
            }
        }

        [Fact]
        public async Task Time_range_of_the_filter_applies_in_whole_days()
        {
            var filter = new GlobalFilter();
            using (StatisticsViewModel vm = await NewVm(filter))
            {
                int all = vm.Items.Single(s => s.Key == "TotalMapsDone").Value;

                filter.Time = FilterTime.Last7Days;
                await vm.WhenIdle();

                (DateTime from, DateTime to) = vm.Window();
                Assert.Equal(DateTime.Today.AddDays(-7), from);
                Assert.Equal(DateTime.Today.AddDays(1).AddSeconds(-1), to);
                int week = vm.Items.Single(s => s.Key == "TotalMapsDone").Value;
                Assert.True(week > 0 && week < all);
            }
        }

        [Fact]
        public async Task Selection_survives_a_reload()
        {
            var filter = new GlobalFilter();
            using (StatisticsViewModel vm = await NewVm(filter))
            {
                vm.SelectedStat = vm.Items.Single(s => s.Key == "HideoutTimeSec");
                await vm.WhenIdle();

                filter.Time = FilterTime.Last30Days;
                await vm.WhenIdle();

                Assert.Equal("HideoutTimeSec", vm.SelectedStat.Key);
                Assert.True(vm.SelectedStat.IsDuration);
            }
        }
    }
}
