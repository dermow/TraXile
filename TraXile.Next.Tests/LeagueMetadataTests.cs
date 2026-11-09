using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>League metadata download and refreshing the league options of the global filter.</summary>
    public class LeagueMetadataTests : IDisposable
    {
        private const string Xml = @"<metadata><leagues>
            <league name=""Settlers"" major=""3"" minor=""25"" start=""2024-07-26 20:00:00 +0"" end=""2025-02-01 21:00:00 +0"" current=""false"" />
            <league name=""Keepers"" major=""3"" minor=""26"" start=""2025-10-31 20:00:00 +0"" end=""2026-03-01 21:00:00 +0"" current=""true"" />
        </leagues></metadata>";

        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-meta-" + Guid.NewGuid().ToString("N"));
        private string Target => Path.Combine(_dir, "metadata", "metadata.xml");

        public void Dispose()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
        }

        [Fact]
        public void Url_like_the_classic_app()
        {
            Assert.Equal("https://traxile-backend.exile-tools.com/main/metadata.xml", MetadataDownloader.Url(null));
            Assert.Equal("https://traxile-backend.exile-tools.com/dev/metadata.xml", MetadataDownloader.Url("dev"));
        }

        [Fact]
        public async Task Download_is_stored_and_readable()
        {
            Assert.True(await MetadataDownloader.UpdateAsync("x", Target, _ => Task.FromResult(Xml)));
            Assert.Equal(new[] { "Settlers", "Keepers" }, LeagueCatalog.Load(Target).Select(l => l.Name));
        }

        [Fact]
        public async Task Failed_or_invalid_download_keeps_the_old_file()
        {
            await MetadataDownloader.UpdateAsync("x", Target, _ => Task.FromResult(Xml));

            Assert.False(await MetadataDownloader.UpdateAsync("x", Target, _ => throw new System.Net.WebException("offline")));
            Assert.False(await MetadataDownloader.UpdateAsync("x", Target, _ => Task.FromResult("<html>502</html>")));
            Assert.False(await MetadataDownloader.UpdateAsync("x", Target, _ => Task.FromResult("not xml")));

            Assert.Equal(2, LeagueCatalog.Load(Target).Count);
        }

        [Fact]
        public void New_leagues_keep_the_selected_league_by_name()
        {
            var old = new LeagueInfo { Name = "Keepers", Start = new DateTime(2025, 10, 31), End = new DateTime(2026, 1, 1) };
            var filter = new GlobalFilter(new[] { old });
            filter.Time = FilterTime.League;
            filter.League = old;

            var updated = new LeagueInfo { Name = "Keepers", Start = old.Start, End = new DateTime(2026, 3, 1) };
            filter.SetLeagues(new[] { updated, new LeagueInfo { Name = "Next", Start = new DateTime(2026, 3, 6), End = new DateTime(2026, 7, 1) } });

            Assert.Equal(FilterTime.League, filter.Time);
            Assert.Same(updated, filter.League);
            Assert.Equal(new DateTime(2026, 3, 1), filter.Window.To);
        }

        [Fact]
        public void A_vanished_league_falls_back_to_all_time()
        {
            var old = new LeagueInfo { Name = "Gone", Start = new DateTime(2020, 1, 1), End = new DateTime(2020, 2, 1) };
            var filter = new GlobalFilter(new[] { old }) { Time = FilterTime.League };
            filter.League = old;

            filter.SetLeagues(Array.Empty<LeagueInfo>());

            Assert.Equal(FilterTime.All, filter.Time);
            Assert.Null(filter.League);
        }

        [Fact]
        public void Filter_bar_and_leagues_page_pick_up_new_leagues()
        {
            using (var source = new DemoTrackingSource())
            {
                var filter = new GlobalFilter();
                using (var bar = new FilterBarViewModel(filter, source))
                using (var leagues = new LeaguesViewModel(source, filter))
                {
                    Assert.DoesNotContain(bar.TimeOptions, o => o.Time == FilterTime.League);
                    Assert.False(leagues.HasLeagues);

                    filter.SetLeagues(LeagueCatalog.Demo(DateTime.Now));

                    Assert.Equal(filter.Leagues.Count, bar.TimeOptions.Count(o => o.Time == FilterTime.League));
                    Assert.True(leagues.HasLeagues);
                    Assert.NotEmpty(leagues.Stats);
                }
            }
        }
    }
}
