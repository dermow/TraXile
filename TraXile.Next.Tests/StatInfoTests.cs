using System;
using System.Linq;
using TraXile.Next.Services;
using Xunit;

namespace TraXile.Next.Tests
{
    public class StatInfoTests
    {
        [Fact]
        public void DeathsStatistic_ReferencesDashboardDeaths()
        {
            var stat = new StatInfo { Key = StatMath.DeathsKey, Name = "Deaths" };
            Assert.Contains("Deaths on the Dashboard", stat.ToolTip);
        }

        [Fact]
        public void StatisticWithoutDescription_FallsBackToKey()
        {
            var stat = new StatInfo { Key = "TotalMapsDone", Name = "Total maps done" };
            Assert.Equal("TotalMapsDone", stat.ToolTip);
        }

        private static ActivityInfo Act(DateTime started, int deaths) =>
            new ActivityInfo { Id = Guid.NewGuid().ToString(), Type = ACTIVITY_TYPES.MAP, Area = "x", Started = started, Deaths = deaths };

        private static readonly ActivityInfo[] History =
        {
            Act(new DateTime(2026, 1, 1, 10, 0, 0), 2),
            Act(new DateTime(2026, 1, 2, 10, 0, 0), 0),
            Act(new DateTime(2026, 1, 2, 12, 0, 0), 1),
            Act(new DateTime(2026, 1, 3, 10, 0, 0), 3),
        };

        [Fact]
        public void DeathsStatistic_IsTheSumOfActivityDeathsInTheWindow()
        {
            Assert.Equal(4, StatMath.ActivityDeaths(History, new DateTime(2026, 1, 2), new DateTime(2026, 1, 3, 23, 59, 59)));
            Assert.Equal(6, StatMath.ActivityDeaths(History, DateTime.MinValue, DateTime.MaxValue));
        }

        [Fact]
        public void DeathsSeries_DailyIncreaseMatchesActivityDeaths()
        {
            DateTime from = new DateTime(2026, 1, 2), to = new DateTime(2026, 1, 3, 23, 59, 59);
            var series = StatMath.ActivityDeathSeries(History, from, to);

            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), series[0].Time); // starting value before the window
            int[] daily = StatMath.DailyIncrease(series, from, to).Select(d => d.Value).ToArray();
            Assert.Equal(new[] { 1, 3 }, daily);
        }

        [Fact]
        public void DemoSource_DeathsStatisticMatchesActivityDeaths()
        {
            var source = new DemoTrackingSource();
            DateTime from = DateTime.MinValue, to = DateTime.MaxValue;
            int stat = source.GetStats(from, to).Single(s => s.Key == StatMath.DeathsKey).Value;
            Assert.Equal(source.GetHistory().Sum(a => a.Deaths), stat);
        }
    }
}
