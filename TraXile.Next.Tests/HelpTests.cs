using System;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Help page: the documented chat commands and search examples match what the core and the search do.</summary>
    public class HelpTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly ActivityLogViewModel _log;

        public HelpTests()
        {
            _log = new ActivityLogViewModel(_source) { MaxItems = ActivityLogViewModel.AllItems };
        }

        public void Dispose()
        {
            _log.Dispose();
            _source.Dispose();
        }

        private ActivityInfo[] Search(string text)
        {
            _log.SearchText = text;
            _log.Refresh();
            return _log.Items.ToArray();
        }

        [Fact]
        public void Chat_commands_are_the_ones_the_core_handles()
        {
            // TrX_CoreLogic handles "trax::<command> <args>" for these commands
            Assert.Equal(new[] { "tag", "untag", "pause", "resume", "finish" },
                HelpViewModel.ChatCommands.Select(c => c.Input.Substring("trax::".Length).Split(' ')[0]));
            Assert.All(HelpViewModel.ChatCommands, c => Assert.False(string.IsNullOrWhiteSpace(c.Description)));
        }

        [Fact]
        public void Tier_example_finds_only_maps_of_that_tier()
        {
            ActivityInfo[] t16 = Search("t16");
            Assert.NotEmpty(t16);
            Assert.All(t16, a => Assert.Equal(16, a.MapTier));
        }

        [Fact]
        public void Several_words_must_all_match()
        {
            ActivityInfo[] maps = Search("map");
            ActivityInfo[] both = Search("map blight");
            Assert.True(both.Length < maps.Length);
            Assert.All(both, a => Assert.Contains(a.Tags, t => t.Name.IndexOf("blight", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        [Fact]
        public void Search_ignores_case_and_matches_part_of_a_word()
        {
            ActivityInfo[] lower = Search("strand");
            Assert.NotEmpty(lower);
            Assert.Equal(lower.Length, Search("STRAND").Length);
            Assert.Equal(lower.Length, Search("stra").Length);
        }

        [Fact]
        public void Every_example_finds_something_in_the_demo_data()
        {
            foreach (HelpEntry example in HelpViewModel.SearchExamples)
            {
                Assert.True(Search(example.Input).Length > 0, $"\"{example.Input}\" finds nothing");
            }
        }

        [Fact]
        public void About_shows_the_app_version_and_links()
        {
            using (var update = new UpdateViewModel("3.0.0", () => System.Threading.Tasks.Task.FromResult<UpdateInfo>(null), v => null))
            {
                var help = new HelpViewModel(update, null);
                Assert.Equal(UpdateChecker.AppVersion, help.Version);
                Assert.Same(update, help.Update);
            }
            Assert.Contains(HelpViewModel.Links, l => l.Url == "https://github.com/dermow/TraXile/issues");
            Assert.All(HelpViewModel.Links.Concat(HelpViewModel.Credits), l => Assert.StartsWith("https://", l.Url));
        }
    }
}
