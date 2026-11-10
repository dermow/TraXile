using TraXile.Next.Services;
using Xunit;

namespace TraXile.Next.Tests
{
    public class StatInfoTests
    {
        [Fact]
        public void DeathsStatistic_ExplainsDifferenceToActivityDeaths()
        {
            var stat = new StatInfo { Key = "TotalKilledCount", Name = "Deaths" };
            Assert.Contains("Activity Deaths", stat.ToolTip);
        }

        [Fact]
        public void StatisticWithoutDescription_FallsBackToKey()
        {
            var stat = new StatInfo { Key = "TotalMapsDone", Name = "Total maps done" };
            Assert.Equal("TotalMapsDone", stat.ToolTip);
        }
    }
}
