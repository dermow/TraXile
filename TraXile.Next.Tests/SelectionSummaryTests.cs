using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class SelectionSummaryTests
    {
        private static TagInfo Tag(string id) => new TagInfo { Id = id, Name = id.ToUpperInvariant() };

        private static ActivityInfo Act(ACTIVITY_TYPES type, int seconds, params TagInfo[] tags) => new ActivityInfo
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Area = "x",
            Started = DateTime.Now,
            TotalSeconds = seconds,
            Tags = tags
        };

        [Fact]
        public void Count_total_and_average()
        {
            var s = SelectionSummary.Create(new[] { Act(ACTIVITY_TYPES.MAP, 100), Act(ACTIVITY_TYPES.MAP, 201), Act(ACTIVITY_TYPES.HEIST, 300) });

            Assert.Equal(3, s.Count);
            Assert.Equal(601, s.TotalSeconds);
            Assert.Equal(200, s.AverageSeconds); // 200.33 rounded
            Assert.Equal("10:01", s.Total);
            Assert.Equal("03:20", s.Average);
        }

        [Fact]
        public void Durations_are_capped_at_the_time_cap_of_their_type()
        {
            var s = SelectionSummary.Create(
                new[] { Act(ACTIVITY_TYPES.MAP, 5000), Act(ACTIVITY_TYPES.HEIST, 5000) },
                t => t == ACTIVITY_TYPES.MAP ? 3600 : 0); // 0 = no cap

            Assert.Equal(3600 + 5000, s.TotalSeconds);
            Assert.Equal(3600, s.Types.Single(r => r.Type == ACTIVITY_TYPES.MAP).TotalSeconds);
        }

        [Fact]
        public void Table_per_type_with_count_total_and_average()
        {
            var s = SelectionSummary.Create(new[] { Act(ACTIVITY_TYPES.MAP, 100), Act(ACTIVITY_TYPES.MAP, 300), Act(ACTIVITY_TYPES.DELVE, 900) });

            SummaryTypeRow delve = s.Types[0]; // most time first
            Assert.Equal(ACTIVITY_TYPES.DELVE, delve.Type);
            Assert.Equal(1, delve.Count);
            SummaryTypeRow map = s.Types[1];
            Assert.Equal(2, map.Count);
            Assert.Equal(400, map.TotalSeconds);
            Assert.Equal(200, map.AverageSeconds);
        }

        [Fact]
        public void Tag_counts_with_share_of_the_selection()
        {
            TagInfo blight = Tag("blight"), juiced = Tag("juiced");
            var s = SelectionSummary.Create(new[]
            {
                Act(ACTIVITY_TYPES.MAP, 1, blight, juiced),
                Act(ACTIVITY_TYPES.MAP, 1, blight),
                Act(ACTIVITY_TYPES.MAP, 1)
            });

            Assert.Equal(new[] { "blight", "juiced" }, s.Tags.Select(t => t.Tag.Id));
            Assert.Equal(2, s.Tags[0].Count);
            Assert.Equal(66.7, s.Tags[0].Percent);
            Assert.Equal(33.3, s.Tags[1].Percent);
            Assert.Equal($"2 · {66.7:0.#} %", s.Tags[0].Text); // the app formats en-US, tests run in the machine's culture
        }

        [Fact]
        public void Totals_from_one_day_use_the_long_format()
        {
            Assert.Equal("23:59:59", SelectionSummary.FormatLong(86399));
            Assert.Equal("1d 2h 3m", SelectionSummary.FormatLong(86400 + 2 * 3600 + 3 * 60 + 4));
        }

        [Fact]
        public void Empty_selection()
        {
            SelectionSummary s = SelectionSummary.Create(Array.Empty<ActivityInfo>());
            Assert.Equal(0, s.Count);
            Assert.Empty(s.Types);
            Assert.False(s.HasTags);
        }

        [Fact]
        public void Activity_log_summarises_a_multi_selection()
        {
            using (var source = new DemoTrackingSource())
            using (var vm = new ActivityLogViewModel(source))
            {
                ActivityInfo[] picked = vm.Items.Take(10).ToArray();

                vm.SetSelection(picked);

                Assert.Equal(10, vm.SelectionSummary.Count);
                Assert.Equal(picked.Sum(a => a.TotalSeconds), vm.SelectionSummary.TotalSeconds); // demo cap: 1 h
                Assert.Equal(picked.Select(a => a.Type).Distinct().Count(), vm.SelectionSummary.Types.Count);

                vm.SetSelection(picked.Take(1));
                Assert.Equal(0, vm.SelectionSummary.Count);
            }
        }
    }
}
