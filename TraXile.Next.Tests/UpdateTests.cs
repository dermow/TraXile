using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Update check (versions.xml), updater start and the update state of the app.</summary>
    public class UpdateTests : IDisposable
    {
        private const string Xml = @"<version>
            <latest>3.0.1</latest>
            <changelog>
                <chg version=""3.0.1"">Faster start</chg>
                <chg version=""3.0.0"">New UI</chg>
                <chg version=""3.0.1"">Fixed the tags overlay</chg>
            </changelog>
        </version>";

        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-update-" + Guid.NewGuid().ToString("N"));

        public UpdateTests() => Directory.CreateDirectory(_dir);

        public void Dispose()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
        }

        [Fact]
        public void Url_like_the_classic_app()
        {
            Assert.Equal("https://traxile-backend.exile-tools.com/main/versions.xml", UpdateChecker.Url(null));
            Assert.Equal("https://traxile-backend.exile-tools.com/dev/versions.xml", UpdateChecker.Url("dev"));
        }

        [Fact]
        public void Changelog_of_the_latest_version_only()
        {
            UpdateInfo info = UpdateChecker.Parse(Xml, "3.0.0");

            Assert.True(info.IsAvailable);
            Assert.Equal("3.0.1", info.LatestVersion);
            Assert.Equal(new[] { "Faster start", "Fixed the tags overlay" }, info.Changes);
        }

        [Theory]
        [InlineData("3.0.1", "3.0.0", true)]
        [InlineData("3.1.0", "3.0.9", true)]
        [InlineData("4.0.0", "3.9.9", true)]
        [InlineData("3.0.0", "3.0.0", false)]
        [InlineData("2.2.7", "3.0.0", false)]     // a 2.x release is no update for 3.0
        [InlineData("3.0.0", "3.0.0-preview1", true)] // the release follows its preview
        [InlineData("3.0.0-preview2", "3.0.0", false)]
        [InlineData("3.0.10", "3.0.9", true)]     // numeric, not text order
        [InlineData("garbage", "3.0.0", false)]
        public void Version_comparison(string remote, string current, bool newer)
        {
            Assert.Equal(newer, UpdateChecker.IsNewer(remote, current));
        }

        [Fact]
        public void App_version_has_no_build_metadata()
        {
            Assert.DoesNotContain("+", UpdateChecker.AppVersion);
            Assert.StartsWith("3.", UpdateChecker.AppVersion);
        }

        [Fact]
        public async Task Failed_check_returns_null()
        {
            Assert.Null(await UpdateChecker.CheckAsync("x", "3.0.0", _ => throw new System.Net.WebException("offline")));
            Assert.Null(await UpdateChecker.CheckAsync("x", "3.0.0", _ => Task.FromResult("<html>502</html>")));
            Assert.NotNull(await UpdateChecker.CheckAsync("x", "3.0.0", _ => Task.FromResult(Xml)));
        }

        [Fact]
        public void Updater_is_started_with_the_version()
        {
            File.WriteAllText(Path.Combine(_dir, UpdateChecker.UpdaterFile), "");
            ProcessStartInfo started = null;

            Assert.True(UpdateChecker.StartUpdater("3.0.1", _dir, out string error, psi => { started = psi; return null; }));
            Assert.Null(error);
            Assert.Equal(Path.Combine(_dir, UpdateChecker.UpdaterFile), started.FileName);
            Assert.Equal("3.0.1", started.Arguments);
        }

        [Fact]
        public void Missing_updater_is_reported()
        {
            Assert.False(UpdateChecker.StartUpdater("3.0.1", _dir, out string error, psi => throw new InvalidOperationException("must not start")));
            Assert.Contains(UpdateChecker.UpdaterFile, error);
        }

        [Fact]
        public void Old_installer_packages_are_cleaned_up()
        {
            var now = new DateTime(2026, 11, 9, 12, 0, 0);
            string old = Path.Combine(_dir, "Setup_3.0.0.msi");
            string fresh = Path.Combine(_dir, "Setup_3.0.1.msi");
            string other = Path.Combine(_dir, "config.xml");
            foreach (string f in new[] { old, fresh, other }) File.WriteAllText(f, "");
            File.SetLastWriteTime(old, now.AddHours(-25));
            File.SetLastWriteTime(fresh, now.AddHours(-2));
            File.SetLastWriteTime(other, now.AddDays(-30));

            Assert.Equal(1, UpdateChecker.CleanupInstallers(_dir, now));
            Assert.False(File.Exists(old));
            Assert.True(File.Exists(fresh));
            Assert.True(File.Exists(other));
        }

        [Fact]
        public async Task View_model_shows_the_hint_and_keeps_it_when_offline()
        {
            UpdateInfo next = UpdateChecker.Parse(Xml, "3.0.0");
            var results = new System.Collections.Generic.Queue<UpdateInfo>(new[] { next, null });
            using (var vm = new UpdateViewModel("3.0.0", () => Task.FromResult(results.Dequeue()), v => null))
            {
                Assert.False(vm.IsUpdateAvailable);
                Assert.Equal("Not checked yet.", vm.StatusText);

                await vm.CheckAsync();
                Assert.True(vm.IsUpdateAvailable);
                Assert.Equal("Update 3.0.1", vm.HintText);
                Assert.Equal("TraXile 3.0.1 is available.", vm.StatusText);

                await vm.CheckAsync(); // offline now
                Assert.True(vm.IsUpdateAvailable);
            }
        }

        [Fact]
        public async Task Install_starts_the_updater_for_the_latest_version()
        {
            string startedFor = null;
            using (var vm = new UpdateViewModel("3.0.0", () => Task.FromResult(UpdateChecker.Parse(Xml, "3.0.0")),
                       v => { startedFor = v; return null; }))
            {
                Assert.NotNull(vm.Install()); // nothing to install before a check
                Assert.Null(startedFor);

                await vm.CheckAsync();
                Assert.Null(vm.Install());
                Assert.Equal("3.0.1", startedFor);
            }
        }

        [Fact]
        public async Task Up_to_date()
        {
            using (var vm = new UpdateViewModel("3.0.1", () => Task.FromResult(UpdateChecker.Parse(Xml, "3.0.1")), v => null))
            {
                await vm.CheckAsync();
                Assert.False(vm.IsUpdateAvailable);
                Assert.Equal("You are up to date.", vm.StatusText);
                Assert.Equal("", vm.HintText);
            }
        }
    }
}
