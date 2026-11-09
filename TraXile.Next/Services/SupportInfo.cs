using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TraXile.Next.Services
{
    /// <summary>What the support text reports about the running app (collected on the UI thread).</summary>
    public sealed class SupportContext
    {
        public DateTime Now { get; set; } = DateTime.Now;
        public string AppVersion { get; set; } = "";
        public string CoreVersion { get; set; } = "";
        public string Mode { get; set; } = "";
        public string AppDir { get; set; } = "";
        public string AppDataPath { get; set; } = "";
        public string ClientTxtPath { get; set; }
        public string WindowSize { get; set; } = "";
        public string WindowLocation { get; set; } = "";
        public string WindowState { get; set; } = "";
        public IEnumerable<KeyValuePair<string, string>> Settings { get; set; } = Array.Empty<KeyValuePair<string, string>>();
    }

    /// <summary>
    /// Support information like the classic DebugInfo window (Main.GetSupportInfoText / BuildSupportBundle):
    /// a text to paste into an issue, and a zip with the TraXile data folder, Client.txt and that text.
    /// </summary>
    public static class SupportInfo
    {
        public const string FolderName = "Support";

        /// <summary>Folders of the data folder that are not put into the bundle (the bundles themselves, backups).</summary>
        private static readonly string[] SkippedFolders = { FolderName, "backups" };

        /// <summary>Settings whose value is replaced by "***" in the text (tokens of the backend sync etc.).</summary>
        private static readonly string[] SecretWords = { "token", "secret", "password", "apikey", "api_key" };

        public static string Folder(string appDataPath) => Path.Combine(appDataPath, FolderName);

        public static string BuildText(SupportContext c)
        {
            var sb = new StringBuilder();
            void Line(string key, string value) => sb.AppendLine($"{key}: {value}");

            Line("Date and time", c.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            Line("Timezone", TimeZoneInfo.Local.StandardName);
            Line("Calendar", CultureInfo.CurrentCulture.Calendar.ToString());
            Line("Application Version", c.AppVersion);
            Line("Core Version", c.CoreVersion);
            Line("Mode", c.Mode);
            Line("Startup Path", c.AppDir);
            Line("AppData Path", c.AppDataPath);
            Line("Window Size", c.WindowSize);
            Line("Window Location", c.WindowLocation);
            Line("Window State", c.WindowState);
            Line("Logfile Path", c.ClientTxtPath ?? "(not set)");
            if (!string.IsNullOrEmpty(c.ClientTxtPath) && File.Exists(c.ClientTxtPath))
            {
                var fi = new FileInfo(c.ClientTxtPath);
                Line("Logfile Last Write", fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                Line("Logfile Size", $"{fi.Length} bytes");
            }
            else
            {
                Line("Logfile Last Write", "(file not found)");
            }
            Line("Operating System", Environment.OSVersion.ToString());
            Line("64-bit OS", Environment.Is64BitOperatingSystem.ToString());
            Line("OS Language", CultureInfo.InstalledUICulture.Name);
            Line(".NET Runtime", Environment.Version.ToString());

            foreach (KeyValuePair<string, string> kvp in c.Settings.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                Line($"Setting: {kvp.Key}", IsSecret(kvp.Key) && !string.IsNullOrEmpty(kvp.Value) ? "***" : kvp.Value);
            }
            return sb.ToString();
        }

        private static bool IsSecret(string key) =>
            SecretWords.Any(w => key.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0);

        /// <summary>
        /// Creates <c>Support\TraXile_Support_Bundle_&lt;date&gt;.zip</c> in the data folder: <c>APP_DATA</c> (database, config,
        /// metadata, logs; without backups and older bundles), <c>Client.txt</c>, <c>debug_info.txt</c> and
        /// <c>app_files.txt</c> (the files of the app folder with size and version). Older bundles are removed.
        /// Returns the path of the zip.
        /// </summary>
        public static string CreateBundle(string appDataPath, string appDir, string clientTxtPath, string infoText, DateTime now)
        {
            string folder = Folder(appDataPath);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
            string temp = Path.Combine(folder, "temp");
            Directory.CreateDirectory(temp);
            try
            {
                CopyDataFolder(appDataPath, Path.Combine(temp, "APP_DATA"));
                if (!string.IsNullOrEmpty(clientTxtPath) && File.Exists(clientTxtPath))
                {
                    CopyShared(clientTxtPath, Path.Combine(temp, "Client.txt"));
                }
                File.WriteAllText(Path.Combine(temp, "debug_info.txt"), infoText, Encoding.UTF8);
                File.WriteAllText(Path.Combine(temp, "app_files.txt"), ListAppFiles(appDir), Encoding.UTF8);

                string zip = Path.Combine(folder, $"TraXile_Support_Bundle_{now:yyyy-MM-dd-HH-mm-ss}.zip");
                ZipFile.CreateFromDirectory(temp, zip);
                return zip;
            }
            finally
            {
                if (Directory.Exists(temp)) Directory.Delete(temp, recursive: true);
            }
        }

        private static void CopyDataFolder(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                if (file.EndsWith(".msi", StringComparison.OrdinalIgnoreCase)) continue; // installer packages of the updater
                CopyShared(file, Path.Combine(target, Path.GetFileName(file)));
            }
            foreach (string dir in Directory.GetDirectories(source))
            {
                string name = Path.GetFileName(dir);
                if (SkippedFolders.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase))) continue;
                CopyDataFolder(dir, Path.Combine(target, name));
            }
        }

        /// <summary>Copies a file that another process may have open (database of the core, Client.txt of the game).</summary>
        private static void CopyShared(string source, string target)
        {
            try
            {
                using (var from = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var to = File.Create(target))
                {
                    from.CopyTo(to);
                }
            }
            catch (IOException)
            {
                // locked exclusively: leave it out, the bundle is still useful
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static string ListAppFiles(string appDir)
        {
            var sb = new StringBuilder();
            if (!Directory.Exists(appDir)) return sb.ToString();
            foreach (string file in Directory.GetFiles(appDir, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string rel = file.Substring(appDir.Length).TrimStart('\\', '/');
                string version = "";
                if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    try { version = FileVersionInfo.GetVersionInfo(file).FileVersion ?? ""; } catch (Exception) { }
                }
                sb.AppendLine($"{rel}\t{new FileInfo(file).Length}\t{version}".TrimEnd('\t'));
            }
            return sb.ToString();
        }
    }
}
