using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class ActivityLogViewModelTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly ActivityLogViewModel _vm;

        public ActivityLogViewModelTests()
        {
            _vm = new ActivityLogViewModel(_source);
        }

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        /// <summary>The search box is debounced; Refresh() applies the current filters immediately.</summary>
        private void Search(string text)
        {
            _vm.SearchText = text;
            _vm.Refresh();
        }

        [Fact]
        public void Shows_whole_history_newest_first_without_filters()
        {
            Assert.Equal(_source.GetHistory().Count, _vm.Items.Count);
            Assert.False(_vm.HasActiveFilters);
            Assert.True(_vm.Items.Zip(_vm.Items.Skip(1), (a, b) => a.Started >= b.Started).All(x => x));
        }

        [Fact]
        public void Global_type_filter_keeps_only_that_type()
        {
            _vm.Filter.Type = ACTIVITY_TYPES.HEIST;

            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => Assert.Equal(ACTIVITY_TYPES.HEIST, a.Type));
            Assert.Equal(_source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.HEIST), _vm.Items.Count);
            Assert.True(_vm.HasActiveFilters);
        }

        [Fact]
        public void Global_tag_filter_keeps_only_tagged_activities()
        {
            _vm.Filter.SetTag("expedition", true);

            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => Assert.Contains(a.Tags, t => t.Id == "expedition"));
        }

        [Fact]
        public void Global_time_filter_cuts_off_older_activities()
        {
            _vm.Filter.Time = FilterTime.Last7Days;

            DateTime from = DateTime.Now.AddDays(-7).AddMinutes(-1);
            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => Assert.True(a.Started >= from));
            Assert.True(_vm.Items.Count < _source.GetHistory().Count);
        }

        [Fact]
        public void Search_applies_on_top_of_the_global_filter()
        {
            _vm.Filter.Type = ACTIVITY_TYPES.MAP;
            Search("strand");

            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => { Assert.Equal(ACTIVITY_TYPES.MAP, a.Type); Assert.Equal("Strand", a.Area); });
        }

        [Fact]
        public void Search_is_case_insensitive_and_matches_area_type_level_and_tags()
        {
            Search("crimson");
            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => Assert.Equal("Crimson Temple", a.Area));

            Search("HEIST");
            Assert.All(_vm.Items, a => Assert.Equal(ACTIVITY_TYPES.HEIST, a.Type));

            Search("t16");
            Assert.All(_vm.Items, a => Assert.Equal(16, a.MapTier));

            Search("blight");
            Assert.NotEmpty(_vm.Items);
            Assert.All(_vm.Items, a => Assert.Contains(a.Tags, t => t.Name == "Blight"));
        }

        [Fact]
        public void Search_terms_are_combined_with_and()
        {
            Search("strand blight");

            Assert.All(_vm.Items, a =>
            {
                Assert.Equal("Strand", a.Area);
                Assert.Contains(a.Tags, t => t.Id == "blight");
            });
        }

        [Fact]
        public void Search_without_match_shows_empty_state()
        {
            Search("does-not-exist");

            Assert.True(_vm.IsEmpty);
            Assert.Equal("No activities match your filters.", _vm.EmptyText);
        }

        [Fact]
        public void Clear_filters_resets_everything()
        {
            _vm.Filter.Time = FilterTime.Last7Days;
            _vm.Filter.Type = ACTIVITY_TYPES.MAP;
            Search("temple");

            _vm.ClearFiltersCommand.Execute(null);

            Assert.False(_vm.HasActiveFilters);
            Assert.False(_vm.Filter.IsActive);
            Assert.Equal("", _vm.SearchText);
            Assert.Equal(_source.GetHistory().Count, _vm.Items.Count);
        }

        [Fact]
        public void Filters_survive_a_refresh()
        {
            _vm.Filter.Type = ACTIVITY_TYPES.MAP;

            _vm.Refresh();

            Assert.Equal(ACTIVITY_TYPES.MAP, _vm.Filter.Type);
            Assert.All(_vm.Items, a => Assert.Equal(ACTIVITY_TYPES.MAP, a.Type));
        }

        [Fact]
        public void Single_selection_exposes_the_activity()
        {
            ActivityInfo first = _vm.Items[0];

            _vm.SetSelection(new[] { first });

            Assert.Same(first, _vm.SelectedActivity);
            Assert.True(_vm.HasSingleSelection);
            Assert.False(_vm.HasMultiSelection);
        }

        [Fact]
        public void Selection_is_dropped_when_filtered_out()
        {
            ActivityInfo map = _vm.Items.First(a => a.Type == ACTIVITY_TYPES.MAP);
            _vm.SetSelection(new[] { map });

            _vm.Filter.Type = ACTIVITY_TYPES.HEIST;

            Assert.False(_vm.HasSelection);
        }

        [Fact]
        public void Available_tags_are_custom_tags_missing_on_the_selection()
        {
            ActivityInfo untagged = _vm.Items.First(a => a.Tags.All(t => t.IsDefault));
            _vm.SetSelection(new[] { untagged });

            Assert.NotEmpty(_vm.AvailableTags);
            Assert.All(_vm.AvailableTags, t => Assert.False(t.IsDefault));
        }

        [Fact]
        public void Add_tag_to_single_activity()
        {
            ActivityInfo target = _vm.Items.First(a => a.Tags.All(t => t.Id != "bricked"));
            _vm.SetSelection(new[] { target });
            TagInfo bricked = _vm.AvailableTags.Single(t => t.Id == "bricked");

            _vm.AddTagCommand.Execute(bricked);

            Assert.Contains(Find(target.Id).Tags, t => t.Id == "bricked");
            Assert.DoesNotContain(_vm.AvailableTags, t => t.Id == "bricked");
        }

        [Fact]
        public void Bulk_tagging_adds_the_tag_to_every_selected_activity_once()
        {
            ActivityInfo[] targets = _vm.Items.Take(25).ToArray();
            ActivityInfo alreadyTagged = targets.FirstOrDefault(a => a.Tags.Any(t => t.Id == "juiced"));
            _vm.SetSelection(targets);
            TagInfo juiced = _vm.AvailableTags.Single(t => t.Id == "juiced");

            _vm.AddTagCommand.Execute(juiced);

            Assert.All(targets, a => Assert.Single(Find(a.Id).Tags, t => t.Id == "juiced"));
            if (alreadyTagged != null) Assert.Single(Find(alreadyTagged.Id).Tags, t => t.Id == "juiced");
        }

        [Fact]
        public void Default_tags_cannot_be_added_or_removed()
        {
            ActivityInfo withDefault = _vm.Items.First(a => a.Tags.Any(t => t.Id == "expedition"));
            TagInfo expedition = withDefault.Tags.First(t => t.Id == "expedition");
            _vm.SetSelection(new[] { withDefault });

            _vm.RemoveTagCommand.Execute(expedition);

            Assert.Contains(Find(withDefault.Id).Tags, t => t.Id == "expedition");
            Assert.DoesNotContain(_vm.AvailableTags, t => t.IsDefault);
        }

        [Fact]
        public void Remove_custom_tag_from_selected_activity()
        {
            ActivityInfo target = _vm.Items[0];
            _vm.SetSelection(new[] { target });
            _vm.AddTagCommand.Execute(_vm.AvailableTags.Single(t => t.Id == "party"));
            TagInfo party = Find(target.Id).Tags.Single(t => t.Id == "party");

            _vm.RemoveTagCommand.Execute(party);

            Assert.DoesNotContain(Find(target.Id).Tags, t => t.Id == "party");
        }

        [Fact]
        public void Delete_removes_selected_activities_from_list_and_source()
        {
            ActivityInfo[] victims = _vm.Items.Take(3).ToArray();
            int before = _vm.Items.Count;
            _vm.SetSelection(victims);

            _vm.DeleteSelected();

            Assert.Equal(before - 3, _vm.Items.Count);
            Assert.False(_vm.HasSelection);
            Assert.All(victims, v => Assert.DoesNotContain(_source.GetHistory(), a => a.Id == v.Id));
        }

        [Fact]
        public void Csv_export_matches_the_classic_format()
        {
            ActivityInfo[] activities = _vm.Items.Where(a => a.Tags.Count > 1).Take(3).ToArray();
            string path = Path.Combine(Path.GetTempPath(), $"traxile-test-{Guid.NewGuid():N}.csv");
            try
            {
                ActivityLogViewModel.ExportCsv(path, activities);

                byte[] bytes = File.ReadAllBytes(path);
                Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3)); // UTF-8 BOM

                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                Assert.Equal("time; type; area; area_level; stopwatch (seconds); death_counter; tags", lines[0]);
                Assert.Equal(activities.Length + 1, lines.Length);

                ActivityInfo a = activities[0];
                string expected = string.Join(";",
                    a.Started.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), a.Type, a.Area, a.AreaLevel, a.TotalSeconds, a.Deaths,
                    string.Join(",", a.Tags.Select(t => t.Id)));
                Assert.Equal(expected, lines[1]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private ActivityInfo Find(string id) => _source.GetHistory().Single(a => a.Id == id);
    }
}
