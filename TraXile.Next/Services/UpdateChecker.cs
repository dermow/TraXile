using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace TraXile.Next.Services
{
    /// <summary>Result of an update check: the installed and the latest released version and its changes.</summary>
    public sealed class UpdateInfo
    {
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";

        /// <summary>Changelog entries of <see cref="LatestVersion"/>.</summary>
        public IReadOnlyList<string> Changes { get; set; } = Array.Empty<string>();

        /// <summary>The latest version is newer than the installed one.</summary>
        public bool IsAvailable { get; set; }
    }

    /// <summary>
    /// Update check like the classic Main.CheckForUpdate: reads <c>{METADATA_BASE_URL}/{metadata_updates_branch}/versions.xml</c>
    /// (<c>/version/latest</c>, <c>/version/changelog/chg[@version]</c>) and starts <c>TraXile.Updater.exe &lt;version&gt;</c>,
    /// which downloads the release's Setup.msi, closes TraXile and runs the installer.
    /// </summary>
    public static class UpdateChecker
    {
        public const string BranchKey = "metadata_updates_branch";
        public const string UpdaterFile = "TraXile.Updater.exe";

        public static string Url(string branch) =>
            $"{TrX_Static.METADATA_BASE_URL}/{(string.IsNullOrWhiteSpace(branch) ? TrX_Static.UPDATE_DEFAULT_BRANCH : branch)}/{TrX_Static.UPDATES_XML_FILE}";

        /// <summary>The version of this app as shown to the user (e.g. "3.0.0-preview1").</summary>
        public static string AppVersion
        {
            get
            {
                Assembly asm = typeof(UpdateChecker).Assembly;
                string info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                if (string.IsNullOrEmpty(info)) return asm.GetName().Version.ToString(3);
                int plus = info.IndexOf('+'); // build metadata (commit hash) of the SDK
                return plus >= 0 ? info.Substring(0, plus) : info;
            }
        }

        /// <summary>Parses versions.xml and compares its latest version with <paramref name="currentVersion"/>.</summary>
        public static UpdateInfo Parse(string xmlText, string currentVersion)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlText);
            string latest = xml.SelectSingleNode("/version/latest")?.InnerText?.Trim();
            if (string.IsNullOrEmpty(latest) || !TryParse(latest, out _, out _)) throw new FormatException("versions.xml has no valid latest version");

            var changes = new List<string>();
            foreach (XmlNode node in xml.SelectNodes("/version/changelog/chg"))
            {
                if (node.Attributes?["version"]?.Value?.Trim() == latest) changes.Add(node.InnerText.Trim());
            }

            return new UpdateInfo
            {
                CurrentVersion = currentVersion,
                LatestVersion = latest,
                Changes = changes,
                IsAvailable = IsNewer(latest, currentVersion),
            };
        }

        /// <summary>
        /// Remote "major.minor.build" is newer than the current version. A pre-release ("3.0.0-preview1") is older
        /// than the release with the same number.
        /// </summary>
        public static bool IsNewer(string remote, string current)
        {
            if (!TryParse(remote, out Version r, out bool remotePre)) return false;
            if (!TryParse(current, out Version c, out bool currentPre)) return true;
            int cmp = r.CompareTo(c);
            return cmp > 0 || (cmp == 0 && currentPre && !remotePre);
        }

        private static bool TryParse(string text, out Version version, out bool preRelease)
        {
            version = null;
            preRelease = false;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string s = text.Trim();
            int dash = s.IndexOf('-');
            if (dash >= 0)
            {
                preRelease = true;
                s = s.Substring(0, dash);
            }
            string[] parts = s.Split('.');
            if (parts.Length < 2) return false;
            var numbers = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (i >= parts.Length) break;
                if (!int.TryParse(parts[i], out numbers[i]) || numbers[i] < 0) return false;
            }
            version = new Version(numbers[0], numbers[1], numbers[2]);
            return true;
        }

        /// <summary>Downloads and parses versions.xml; null on any error (offline etc.).</summary>
        public static async Task<UpdateInfo> CheckAsync(string url, string currentVersion, Func<string, Task<string>> download = null)
        {
            try
            {
                download = download ?? Http.GetStringAsync;
                string data = await download(url).ConfigureAwait(false);
                return Parse(data, currentVersion);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Starts the updater for <paramref name="version"/> from <paramref name="appDir"/>; false with a reason if not possible.</summary>
        public static bool StartUpdater(string version, string appDir, out string error, Func<ProcessStartInfo, Process> start = null)
        {
            error = null;
            string path = Path.Combine(appDir, UpdaterFile);
            if (!File.Exists(path))
            {
                error = $"{UpdaterFile} was not found in {appDir}. Download the new version from {TrX_Static.RELEASE_URL}.";
                return false;
            }
            try
            {
                start = start ?? (psi => Process.Start(psi));
                start(new ProcessStartInfo { FileName = path, Arguments = version, WorkingDirectory = appDir, UseShellExecute = false });
                return true;
            }
            catch (Exception ex)
            {
                error = $"The updater could not be started: {ex.Message}";
                return false;
            }
        }

        /// <summary>Deletes installer packages (*.msi) the updater left in <paramref name="dir"/> that are older than a day (classic CleanupMSIFiles).</summary>
        public static int CleanupInstallers(string dir, DateTime now)
        {
            if (!Directory.Exists(dir)) return 0;
            int deleted = 0;
            foreach (string file in Directory.GetFiles(dir, "*.msi"))
            {
                try
                {
                    if ((now - File.GetLastWriteTime(file)).TotalHours > 24)
                    {
                        File.Delete(file);
                        deleted++;
                    }
                }
                catch (Exception)
                {
                    // in use or no access: try again next start
                }
            }
            return deleted;
        }
    }
}
