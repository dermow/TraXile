using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Support text and support bundle (classic DebugInfo window).</summary>
    public class SupportTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "traxile-support-" + Guid.NewGuid().ToString("N"));
        private string AppData => Path.Combine(_root, "appdata");
        private string AppDir => Path.Combine(_root, "app");
        private string ClientTxt => Path.Combine(_root, "poe", "Client.txt");

        public SupportTests()
        {
            Directory.CreateDirectory(Path.Combine(AppData, "metadata"));
            Directory.CreateDirectory(Path.Combine(AppData, "backups", "2026-11-01"));
            Directory.CreateDirectory(AppDir);
            Directory.CreateDirectory(Path.GetDirectoryName(ClientTxt));
            File.WriteAllText(Path.Combine(AppData, "config.xml"), "<settings />");
            File.WriteAllText(Path.Combine(AppData, "data.db"), "db");
            File.WriteAllText(Path.Combine(AppData, "metadata", "metadata.xml"), "<metadata />");
            File.WriteAllText(Path.Combine(AppData, "backups", "2026-11-01", "data.db"), "old db");
            File.WriteAllText(Path.Combine(AppData, "Setup_3.0.1.msi"), "msi");
            File.WriteAllText(Path.Combine(AppDir, "TraXile.exe"), "exe");
            File.WriteAllText(ClientTxt, "2026/11/09 10:00:00 log line");
        }

        public void Dispose()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }

        private SupportContext Context() => new SupportContext
        {
            Now = new DateTime(2026, 11, 9, 10, 30, 0),
            AppVersion = "3.0.0",
            CoreVersion = "2.2.6",
            Mode = "Live",
            AppDir = AppDir,
            AppDataPath = AppData,
            ClientTxtPath = ClientTxt,
            WindowSize = "1440x940",
            WindowState = "Normal",
            Settings = new Dictionary<string, string>
            {
                ["theme"] = "Dark",
                ["backend_sync_token"] = "abc123",
                ["poe_logfile_path"] = ClientTxt,
            },
        };

        [Fact]
        public void Text_has_the_classic_lines_and_all_settings()
        {
            string text = SupportInfo.BuildText(Context());

            Assert.Contains("Date and time: 2026-11-09 10:30:00", text);
            Assert.Contains("Application Version: 3.0.0", text);
            Assert.Contains("Core Version: 2.2.6", text);
            Assert.Contains($"Logfile Path: {ClientTxt}", text);
            Assert.Contains("Logfile Size: 28 bytes", text);
            Assert.Contains("Window Size: 1440x940", text);
            Assert.Contains("Setting: theme: Dark", text);
        }

        [Fact]
        public void Secrets_are_masked()
        {
            string text = SupportInfo.BuildText(Context());
            Assert.Contains("Setting: backend_sync_token: ***", text);
            Assert.DoesNotContain("abc123", text);
        }

        [Fact]
        public void Missing_log_is_reported_not_thrown()
        {
            SupportContext c = Context();
            c.ClientTxtPath = null;
            Assert.Contains("Logfile Path: (not set)", SupportInfo.BuildText(c));
        }

        [Fact]
        public void Bundle_contains_data_log_and_info_but_no_backups_or_installers()
        {
            string zip = SupportInfo.CreateBundle(AppData, AppDir, ClientTxt, "info", new DateTime(2026, 11, 9, 10, 30, 5));

            Assert.Equal(Path.Combine(AppData, "Support", "TraXile_Support_Bundle_2026-11-09-10-30-05.zip"), zip);
            using (ZipArchive archive = ZipFile.OpenRead(zip))
            {
                string[] entries = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).ToArray();
                Assert.Contains("APP_DATA/config.xml", entries);
                Assert.Contains("APP_DATA/data.db", entries);
                Assert.Contains("APP_DATA/metadata/metadata.xml", entries);
                Assert.Contains("Client.txt", entries);
                Assert.Contains("debug_info.txt", entries);
                Assert.Contains("app_files.txt", entries);
                Assert.DoesNotContain(entries, e => e.StartsWith("APP_DATA/backups", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entries, e => e.StartsWith("APP_DATA/Support", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entries, e => e.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

                using (var reader = new StreamReader(archive.GetEntry("app_files.txt").Open()))
                {
                    Assert.Contains("TraXile.exe", reader.ReadToEnd());
                }
            }
            Assert.False(Directory.Exists(Path.Combine(AppData, "Support", "temp")));
        }

        [Fact]
        public void A_new_bundle_replaces_the_old_one()
        {
            string first = SupportInfo.CreateBundle(AppData, AppDir, ClientTxt, "info", new DateTime(2026, 11, 9, 10, 0, 0));
            string second = SupportInfo.CreateBundle(AppData, AppDir, ClientTxt, "info", new DateTime(2026, 11, 9, 11, 0, 0));

            Assert.False(File.Exists(first));
            Assert.Equal(new[] { second }, Directory.GetFiles(Path.Combine(AppData, "Support")));
        }

        [Fact]
        public void Files_open_by_the_game_are_copied()
        {
            using (new FileStream(ClientTxt, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                string zip = SupportInfo.CreateBundle(AppData, AppDir, ClientTxt, "info", DateTime.Now);
                using (ZipArchive archive = ZipFile.OpenRead(zip))
                {
                    Assert.NotNull(archive.GetEntry("Client.txt"));
                }
            }
        }

        [Fact]
        public void View_model_copies_and_creates_the_bundle()
        {
            string copied = null;
            var vm = new SupportViewModel(AppData, AppDir, Context, t => copied = t);

            vm.CopyToClipboard();
            Assert.Contains("Application Version: 3.0.0", copied);
            Assert.Equal("Copied to the clipboard.", vm.Status);
            Assert.False(vm.StatusIsError);

            Assert.True(vm.CreateBundleAsync(new DateTime(2026, 11, 9, 12, 0, 0)).GetAwaiter().GetResult());
            Assert.True(File.Exists(vm.LastBundle));
            Assert.StartsWith("Created TraXile_Support_Bundle_2026-11-09-12-00-00.zip", vm.Status);
            Assert.True(vm.IsIdle);
        }

        [Fact]
        public async Task Failures_are_shown_as_status()
        {
            var vm = new SupportViewModel(AppData, AppDir, Context, t => throw new System.Runtime.InteropServices.COMException("clipboard busy"));
            vm.CopyToClipboard();
            Assert.True(vm.StatusIsError);

            string blocked = Path.Combine(_root, "blocked");
            Directory.CreateDirectory(blocked);
            File.WriteAllText(Path.Combine(blocked, SupportInfo.FolderName), "a file where the folder should be");
            var broken = new SupportViewModel(blocked, AppDir, Context, t => { });
            Assert.False(await broken.CreateBundleAsync(DateTime.Now));
            Assert.True(broken.StatusIsError);
        }
    }
}
