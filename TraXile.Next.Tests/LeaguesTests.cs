using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class LeaguesTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter(LeagueCatalog.Demo(DateTime.Now));

        public void Dispose() => _source.Dispose();

        private static ActivityInfo Act(ACTIVITY_TYPES type, DateTime started, int seconds, int level = 0, int deaths = 0) =>
            new ActivityInfo { Id = Guid.NewGuid().ToString(), Type = type, Area = "x", Started = started, TotalSeconds = seconds, AreaLevel = level, Deaths = deaths };

        [Fact]
        public void League_figures_like_the_classic_dashboard()
        {
            var league = new LeagueInfo { Name = "Test", Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 4, 1) };
            var history = new[]
            {
                Act(ACTIVITY_TYPES.MAP, new DateTime(2026, 1, 2), 300, level: 83, deaths: 1),
                Act(ACTIVITY_TYPES.MAP, new DateTime(2026, 1, 3), 9000, level: 80),   // capped at 3600
                Act(ACTIVITY_TYPES.CAMPAIGN, new DateTime(2026, 1, 1, 12, 0, 0), 1800),
                Act(ACTIVITY_TYPES.MAP, new DateTime(2025, 12, 31), 100, level: 83),  // before the league
            };

            LeagueStats s = LeaguesViewModel.Compute(league, history, t => 3600);

            Assert.Equal(3, s.Activities);
            Assert.Equal(300 + 3600 + 1800, s.PlaySeconds);
            Assert.Equal(2, s.Maps);
            Assert.Equal(1, s.MapsT16);
            Assert.Equal((300 + 3600) / 2.0, s.AverageMapSeconds);
            Assert.Equal(300, s.AverageMapT16Seconds);
            Assert.Equal(1, s.Deaths);
            Assert.Equal(1800, s.CampaignSeconds);
        }

        [Fact]
        public void Compares_the_last_n_leagues_oldest_first_in_eight_charts()
        {
            using (var vm = new LeaguesViewModel(_source, _filter))
            {
                Assert.Equal(5, vm.LeagueCount);
                string[] newest = _filter.Leagues.OrderByDescending(l => l.Start).Take(5).Reverse().Select(l => l.Name).ToArray();
                Assert.Equal(newest, vm.Stats.Select(s => s.League.Name));
                Assert.Equal(8, vm.Charts.Count);

                vm.LeagueCount = 3;
                Assert.Equal(3, vm.Stats.Count);

                vm.LeagueCount = 20; // more than there are
                Assert.Equal(_filter.Leagues.Count, vm.Stats.Count);
            }
        }

        [Fact]
        public void Filter_applies_except_its_time_range()
        {
            using (var vm = new LeaguesViewModel(_source, _filter))
            {
                LeagueInfo current = vm.Stats.Last().League;
                int all = vm.Stats.Last().Activities;
                Assert.True(all > 0);

                _filter.Time = FilterTime.Today; // ignored: the league keeps its play period
                Assert.Equal(all, vm.Stats.Last().Activities);

                _filter.Type = ACTIVITY_TYPES.HEIST; // applies
                int heists = _source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.HEIST && a.Started >= current.Start && a.Started <= current.End);
                Assert.Equal(heists, vm.Stats.Last().Activities);
                Assert.Equal(0, vm.Stats.Last().Maps);
            }
        }

        [Fact]
        public void No_leagues_without_metadata()
        {
            using (var vm = new LeaguesViewModel(_source, new GlobalFilter()))
            {
                Assert.False(vm.HasLeagues);
                Assert.Empty(vm.Stats);
            }
        }
    }
}
