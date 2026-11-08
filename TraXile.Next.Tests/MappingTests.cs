using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class MappingTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter();
        private readonly MappingViewModel _vm;

        public MappingTests() => _vm = new MappingViewModel(_source, _filter);

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        private ActivityInfo[] Maps => _source.GetHistory().Where(a => a.Type == ACTIVITY_TYPES.MAP).ToArray();

        [Fact]
        public void Maps_per_tier()
        {
            Assert.Equal(Maps.Length, _vm.MapCount);
            for (int t = 1; t <= MappingViewModel.MaxTier; t++)
            {
                Assert.Equal(Maps.Count(a => a.MapTier == t), _vm.CountByTier[t - 1]);
            }
        }

        [Fact]
        public void Average_time_per_tier_is_capped_at_the_map_time_cap()
        {
            ActivityInfo[] t16 = Maps.Where(a => a.MapTier == 16).ToArray();
            int cap = _source.GetTimeCap(ACTIVITY_TYPES.MAP);

            Assert.Equal(t16.Average(a => Math.Min(a.TotalSeconds, cap)), _vm.AverageByTier[15], 6);
            Assert.Equal(0, _vm.AverageByTier[0]); // no T1 maps
        }

        [Fact]
        public void Count_by_map_most_first_with_bars_relative_to_the_top()
        {
            var expected = Maps.GroupBy(a => a.Area).OrderByDescending(g => g.Count()).First();

            Assert.Equal(Maps.Select(a => a.Area).Distinct().Count(), _vm.ByMap.Count);
            Assert.Equal(expected.Count(), _vm.ByMap[0].Count);
            Assert.Equal(1.0, _vm.ByMap[0].Fraction);
            Assert.All(_vm.ByMap, r => Assert.InRange(r.Fraction, 0.0, 1.0));
            Assert.EndsWith(" avg.", _vm.ByMap[0].Detail);
        }

        [Fact]
        public void Count_by_tag_counts_the_tags_of_maps_only()
        {
            CountRow expedition = _vm.ByTag.Single(r => r.Tag.Id == "expedition");
            Assert.Equal(Maps.Count(a => a.Tags.Any(t => t.Id == "expedition")), expedition.Count);
            Assert.True(expedition.HasTag);
        }

        [Fact]
        public void Global_filter_applies()
        {
            _filter.Area = "Strand";

            Assert.Equal(Maps.Count(a => a.Area == "Strand"), _vm.MapCount);
            Assert.Single(_vm.ByMap);

            _filter.Type = ACTIVITY_TYPES.HEIST;
            Assert.False(_vm.HasData);
            Assert.Empty(_vm.ByMap);
        }
    }
}
