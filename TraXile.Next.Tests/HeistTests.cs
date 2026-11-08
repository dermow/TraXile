using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class HeistTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter();
        private readonly HeistViewModel _vm;

        public HeistTests() => _vm = new HeistViewModel(_source, _filter);

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        private ActivityInfo[] Heists => _source.GetHistory().Where(a => a.Type == ACTIVITY_TYPES.HEIST).ToArray();

        [Fact]
        public void Heists_and_capped_average_time_by_area_level()
        {
            int cap = _source.GetTimeCap(ACTIVITY_TYPES.HEIST);
            Assert.Equal(Heists.Length, _vm.HeistCount);
            for (int level = HeistViewModel.MinLevel; level <= HeistViewModel.MaxLevel; level++)
            {
                ActivityInfo[] ofLevel = Heists.Where(a => a.AreaLevel == level).ToArray();
                Assert.Equal(ofLevel.Length, _vm.CountByLevel[level - HeistViewModel.MinLevel]);
                double avg = ofLevel.Length > 0 ? ofLevel.Average(a => Math.Min(a.TotalSeconds, cap)) : 0;
                Assert.Equal(avg, _vm.AverageByLevel[level - HeistViewModel.MinLevel], 6);
            }
            Assert.True(_vm.CountByLevel.Count(c => c > 0) > 1); // demo heists have several levels
        }

        [Fact]
        public void Count_by_area_most_first()
        {
            int[] counts = _vm.ByArea.Select(r => r.Count).ToArray();
            Assert.Equal(Heists.Select(a => a.Area).Distinct().Count(), counts.Length);
            Assert.Equal(counts.OrderByDescending(c => c), counts);
            Assert.Equal(Heists.Length, counts.Sum());
        }

        [Fact]
        public void Count_by_tag_counts_the_tags_of_heists_only()
        {
            ActivityInfo heist = Heists[0];
            _source.AddTag(heist.Id, "party");
            _vm.Refresh();

            Assert.Equal(Heists.Count(a => a.Tags.Any(t => t.Id == "party")), _vm.ByTag.Single(r => r.Tag.Id == "party").Count);
            Assert.DoesNotContain(_vm.ByTag, r => r.Tag.Id == "expedition"); // map tag in the demo data
        }

        [Fact]
        public void Global_filter_applies()
        {
            _filter.LevelOperator = LevelOperator.GreaterOrEqual;
            _filter.AreaLevel = 81;

            Assert.Equal(Heists.Count(a => a.AreaLevel >= 81), _vm.HeistCount);
            Assert.All(_vm.CountByLevel.Take(81 - HeistViewModel.MinLevel), c => Assert.Equal(0, c));
        }
    }
}
