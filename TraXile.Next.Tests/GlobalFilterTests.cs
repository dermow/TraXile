using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class GlobalFilterTests : IDisposable
    {
        private static readonly DateTime Now = new DateTime(2026, 11, 8, 15, 30, 0);
        private readonly DemoTrackingSource _source = new DemoTrackingSource();

        public void Dispose() => _source.Dispose();

        private static ActivityInfo Act(DateTime started, ACTIVITY_TYPES type = ACTIVITY_TYPES.MAP, string area = "Strand",
            int level = 83, params string[] tags) => new ActivityInfo
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Area = area,
            AreaLevel = level,
            Started = started,
            TotalSeconds = 120,
            Tags = tags.Select(t => new TagInfo { Id = t, Name = t }).ToList()
        };

        private static GlobalFilter NewFilter(IReadOnlyList<LeagueInfo> leagues = null) => new GlobalFilter(leagues, () => Now);

        // ------------------------------------------------------------------
        // Time
        // ------------------------------------------------------------------

        [Theory]
        [InlineData(FilterTime.Today, 15.5)]          // since midnight
        [InlineData(FilterTime.Last24Hours, 24)]
        [InlineData(FilterTime.Last7Days, 7 * 24)]
        [InlineData(FilterTime.Last30Days, 30 * 24)]
        [InlineData(FilterTime.LastYear, 365 * 24)]
        public void Time_options_cut_off_older_activities(FilterTime time, double hours)
        {
            GlobalFilter f = NewFilter();
            f.Time = time;

            Assert.True(f.Matches(Act(Now.AddHours(-hours).AddMinutes(1))));
            Assert.False(f.Matches(Act(Now.AddHours(-hours).AddMinutes(-1))));
        }

        [Fact]
        public void League_option_uses_the_league_period()
        {
            var league = new LeagueInfo { Name = "Settlers", Major = 3, Minor = 25, Start = new DateTime(2024, 7, 26), End = new DateTime(2025, 2, 1) };
            GlobalFilter f = NewFilter(new[] { league });
            f.Time = FilterTime.League;
            f.League = league;

            Assert.True(f.Matches(Act(new DateTime(2024, 8, 1))));
            Assert.False(f.Matches(Act(new DateTime(2024, 7, 25))));
            Assert.False(f.Matches(Act(new DateTime(2025, 2, 2))));
            Assert.Equal("League Settlers", f.Description);
        }

        [Fact]
        public void Custom_range_includes_both_ends_and_accepts_swapped_bounds()
        {
            GlobalFilter f = NewFilter();
            f.Time = FilterTime.Custom;
            f.CustomFrom = new DateTime(2026, 11, 5, 20, 0, 0);
            f.CustomTo = new DateTime(2026, 11, 1, 8, 0, 0);

            Assert.True(f.Matches(Act(new DateTime(2026, 11, 1, 8, 0, 0))));
            Assert.True(f.Matches(Act(new DateTime(2026, 11, 5, 20, 0, 0))));
            Assert.False(f.Matches(Act(new DateTime(2026, 11, 5, 20, 0, 1))));
        }

        // ------------------------------------------------------------------
        // Type, area, level
        // ------------------------------------------------------------------

        [Fact]
        public void Type_and_area_filter()
        {
            GlobalFilter f = NewFilter();
            f.Type = ACTIVITY_TYPES.HEIST;
            f.Area = "bunker"; // case-insensitive

            Assert.True(f.Matches(Act(Now, ACTIVITY_TYPES.HEIST, "Bunker")));
            Assert.False(f.Matches(Act(Now, ACTIVITY_TYPES.MAP, "Bunker")));
            Assert.False(f.Matches(Act(Now, ACTIVITY_TYPES.HEIST, "Laboratory")));
        }

        [Theory]
        [InlineData(LevelOperator.Equal, 83, true, false, false)]
        [InlineData(LevelOperator.Greater, 83, false, true, false)]
        [InlineData(LevelOperator.GreaterOrEqual, 83, true, true, false)]
        [InlineData(LevelOperator.Less, 83, false, false, true)]
        [InlineData(LevelOperator.LessOrEqual, 83, true, false, true)]
        public void Area_level_operators(LevelOperator op, int level, bool at, bool above, bool below)
        {
            GlobalFilter f = NewFilter();
            f.LevelOperator = op;
            f.AreaLevel = level;

            Assert.Equal(at, f.Matches(Act(Now, level: level)));
            Assert.Equal(above, f.Matches(Act(Now, level: level + 1)));
            Assert.Equal(below, f.Matches(Act(Now, level: level - 1)));
        }

        // ------------------------------------------------------------------
        // Tags
        // ------------------------------------------------------------------

        [Theory]
        [InlineData(TagMatch.Any, true, true, false)]
        [InlineData(TagMatch.All, true, false, false)]
        [InlineData(TagMatch.None, false, false, true)]
        public void Tag_matching(TagMatch match, bool both, bool one, bool none)
        {
            GlobalFilter f = NewFilter();
            f.Tags = new[] { "blight", "juiced" };
            f.TagMatch = match;

            Assert.Equal(both, f.Matches(Act(Now, tags: new[] { "blight", "juiced" })));
            Assert.Equal(one, f.Matches(Act(Now, tags: new[] { "blight" })));
            Assert.Equal(none, f.Matches(Act(Now, tags: new[] { "harvest" })));
        }

        [Fact]
        public void Set_tag_adds_and_removes_once()
        {
            GlobalFilter f = NewFilter();
            f.SetTag("a", true);
            f.SetTag("a", true);
            f.SetTag("b", true);
            f.SetTag("a", false);

            Assert.Equal(new[] { "b" }, f.Tags);
        }

        // ------------------------------------------------------------------
        // State
        // ------------------------------------------------------------------

        [Fact]
        public void Reset_clears_everything_and_raises_changed_once()
        {
            GlobalFilter f = NewFilter();
            f.Time = FilterTime.Today;
            f.Type = ACTIVITY_TYPES.MAP;
            f.Area = "Strand";
            f.AreaLevel = 83;
            f.SetTag("blight", true);
            Assert.True(f.IsActive);
            int changes = 0;
            f.Changed += (s, e) => changes++;

            f.Reset();

            Assert.False(f.IsActive);
            Assert.Equal(1, changes);
            Assert.Equal("No filter", f.Description);
        }

        [Fact]
        public void Description_lists_the_active_parts()
        {
            GlobalFilter f = NewFilter();
            f.Batch(() =>
            {
                f.Time = FilterTime.Last7Days;
                f.Type = ACTIVITY_TYPES.MAP;
                f.LevelOperator = LevelOperator.GreaterOrEqual;
                f.AreaLevel = 83;
                f.Tags = new[] { "juiced", "party" };
                f.TagMatch = TagMatch.All;
            });

            Assert.Equal("Last 7 days · Map · Level ≥ 83 · juiced and party", f.Description);
        }

        [Fact]
        public void Filter_set_on_one_page_applies_on_all_pages()
        {
            var filter = new GlobalFilter();
            using (var logA = new ActivityLogViewModel(_source, filter))
            using (var logB = new ActivityLogViewModel(_source, filter))
            {
                filter.Type = ACTIVITY_TYPES.HEIST;

                Assert.All(logA.Items, a => Assert.Equal(ACTIVITY_TYPES.HEIST, a.Type));
                Assert.All(logB.Items, a => Assert.Equal(ACTIVITY_TYPES.HEIST, a.Type));
                Assert.Equal(logA.Items.Count, logB.Items.Count);
            }
        }

        // ------------------------------------------------------------------
        // Filter bar
        // ------------------------------------------------------------------

        [Fact]
        public void Bar_type_options_are_ordered_by_frequency_and_set_the_filter()
        {
            var filter = new GlobalFilter();
            using (var bar = new FilterBarViewModel(filter, _source))
            {
                int[] counts = bar.TypeOptions.Skip(1).Select(o => o.Count).ToArray();
                Assert.Equal(counts.OrderByDescending(c => c), counts);

                bar.SelectedType = bar.TypeOptions.Single(o => Equals(o.Value, ACTIVITY_TYPES.HEIST));
                Assert.Equal(ACTIVITY_TYPES.HEIST, filter.Type);

                bar.SelectedType = bar.TypeOptions[0];
                Assert.Null(filter.Type);
            }
        }

        [Fact]
        public void Bar_lists_leagues_newest_first_after_the_fixed_ranges()
        {
            var filter = new GlobalFilter(LeagueCatalog.Demo(Now));
            using (var bar = new FilterBarViewModel(filter, _source))
            {
                TimeOption[] leagues = bar.TimeOptions.Where(o => o.Time == FilterTime.League).ToArray();
                Assert.Equal(filter.Leagues.Count, leagues.Length);
                Assert.True(leagues[0].League.IsCurrent);
                Assert.StartsWith("Current league:", leagues[0].Label);

                bar.SelectedTime = leagues[1];
                Assert.Equal(FilterTime.League, filter.Time);
                Assert.Same(leagues[1].League, filter.League);
                Assert.Same(leagues[1], bar.SelectedTime);
            }
        }

        [Fact]
        public void Bar_level_text_sets_the_level_and_flags_invalid_input()
        {
            var filter = new GlobalFilter();
            using (var bar = new FilterBarViewModel(filter, _source))
            {
                bar.LevelText = "83";
                Assert.Equal(83, filter.AreaLevel);
                Assert.False(bar.LevelInvalid);

                bar.LevelText = "8x";
                Assert.True(bar.LevelInvalid);
                Assert.Equal(83, filter.AreaLevel); // keeps the last valid value

                bar.LevelText = "";
                Assert.Null(filter.AreaLevel);

                bar.LevelText = "abc";
                bar.Reset();
                Assert.False(bar.LevelInvalid);
                Assert.Equal("", bar.LevelText);
            }
        }

        [Fact]
        public void Bar_custom_time_texts_set_the_range()
        {
            var filter = new GlobalFilter();
            using (var bar = new FilterBarViewModel(filter, _source))
            {
                bar.SelectedTime = bar.TimeOptions.Single(o => o.Time == FilterTime.Custom);
                Assert.True(bar.IsCustomTime);

                bar.CustomFromDate = new DateTime(2026, 1, 2);
                bar.CustomFromTime = "08:15";
                bar.CustomToDate = new DateTime(2026, 1, 3);
                bar.CustomToTime = "23:59:59";

                Assert.Equal(new DateTime(2026, 1, 2, 8, 15, 0), filter.CustomFrom);
                Assert.Equal(new DateTime(2026, 1, 3, 23, 59, 59), filter.CustomTo);

                bar.CustomToTime = "25:00";
                Assert.True(bar.CustomToTimeInvalid);
                Assert.Equal(new DateTime(2026, 1, 3, 23, 59, 59), filter.CustomTo);
            }
        }

        [Fact]
        public void Bar_tag_choices_toggle_filter_tags()
        {
            var filter = new GlobalFilter();
            using (var bar = new FilterBarViewModel(filter, _source))
            {
                TagChoice blight = bar.TagChoices.Single(t => t.Tag.Id == "blight");
                blight.IsChecked = true;

                Assert.Equal(new[] { "blight" }, filter.Tags);
                Assert.True(bar.HasTagFilter);
                Assert.Equal("Blight", bar.TagButtonText);
                Assert.Equal("Blight", filter.Description);
            }
        }

        // ------------------------------------------------------------------
        // Leagues
        // ------------------------------------------------------------------

        [Fact]
        public void League_metadata_is_parsed_like_the_classic_app()
        {
            var xml = new XmlDocument();
            xml.LoadXml(@"<metadata><leagues>
                <league name=""Harbinger"" major=""3"" minor=""0"" start=""2017-08-04 20:00:00 +0"" end=""2017-12-04 21:00:00 +0"" current=""false"" />
                <league name=""Broken"" major=""x"" minor=""1"" start=""?"" end=""?"" current=""false"" />
                <league name=""Keepers"" major=""3"" minor=""26"" start=""2025-10-31 20:00:00 +0"" end=""2026-03-01 21:00:00 +0"" current=""true"" />
            </leagues></metadata>");

            IReadOnlyList<LeagueInfo> leagues = LeagueCatalog.Parse(xml);

            Assert.Equal(new[] { "Harbinger", "Keepers" }, leagues.Select(l => l.Name));
            Assert.Equal("3.0", leagues[0].Version);
            Assert.Equal(new DateTime(2017, 8, 4, 20, 0, 0, DateTimeKind.Utc).ToLocalTime(), leagues[0].Start);
            Assert.Same(leagues[1], LeagueCatalog.Current(leagues));
        }

        [Fact]
        public void Missing_metadata_gives_no_leagues()
        {
            Assert.Empty(LeagueCatalog.Load(@"C:\does\not\exist\metadata.xml"));
        }
    }
}
