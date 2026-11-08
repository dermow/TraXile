using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>One backup folder: &lt;data folder&gt;\backups\&lt;name&gt;\&lt;yyyy-MM-dd_HHmmss&gt;.</summary>
    public sealed class BackupInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Path { get; set; }
        public IReadOnlyList<string> Files { get; set; } = Array.Empty<string>();
        public long SizeBytes { get; set; }

        /// <summary>A backup needs at least the database to be restored.</summary>
        public bool CanRestore => Files.Contains("data.db", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Named backups with the same layout as the classic app (Main.CreateBackup, ReadBackupList,
    /// PrepareBackupRestore, DeleteBackup), so backups of either app can be restored by the other.
    /// A restore is staged as ".restore" files and applied on the next start (<see cref="BackupRestore"/>).
    /// </summary>
    public static class Backups
    {
        public const string DefaultName = "Default";
        public const string TimestampFormat = "yyyy-MM-dd_HHmmss";

        /// <summary>Process names of the Path of Exile client (classic checked the first two).</summary>
        private static readonly string[] GameProcesses = { "PathOfExileSteam", "PathOfExile", "PathOfExile_x64", "PathOfExile_x64Steam" };

        public static string Folder(AppSetup setup) => System.IO.Path.Combine(setup.AppDataPath, "backups");

        /// <summary>True while Path of Exile runs: it keeps Client.txt open, so a restore must wait.</summary>
        public static bool IsGameRunning() => GameProcesses.Any(n => Process.GetProcessesByName(n).Length > 0);

        /// <summary>All backups, newest first.</summary>
        public static IReadOnlyList<BackupInfo> List(AppSetup setup)
        {
            string root = Folder(setup);
            if (!Directory.Exists(root)) return Array.Empty<BackupInfo>();

            var result = new List<BackupInfo>();
            foreach (string nameDir in Directory.GetDirectories(root))
            {
                foreach (string dir in Directory.GetDirectories(nameDir))
                {
                    string stamp = System.IO.Path.GetFileName(dir);
                    DateTime created = DateTime.TryParseExact(stamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime t)
                        ? t : Directory.GetCreationTime(dir);
                    FileInfo[] files = new DirectoryInfo(dir).GetFiles();
                    result.Add(new BackupInfo
                    {
                        Name = System.IO.Path.GetFileName(nameDir),
                        Created = created,
                        Path = dir,
                        Files = files.Select(f => f.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(),
                        SizeBytes = files.Sum(f => f.Length)
                    });
                }
            }
            return result.OrderByDescending(b => b.Created).ToList();
        }

        /// <summary>Checks a backup name: a folder name, no path (classic: no "/" or ".").</summary>
        public static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null; // becomes "Default"
            if (name.Contains(".") || name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                return "Use a simple name without dots, slashes or other special characters.";
            return null;
        }

        /// <summary>Creates a backup of Client.txt, data.db, stats.cache, config.xml and labdata.xml.</summary>
        public static BackupInfo Create(AppSetup setup, string name, DateTime now)
        {
            string error = ValidateName(name);
            if (error != null) throw new ArgumentException(error, nameof(name));
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();

            string dir = Maintenance.CreateBackup(setup, name, now);
            return List(setup).First(b => string.Equals(b.Path, dir, StringComparison.OrdinalIgnoreCase));
        }

        public static void Delete(AppSetup setup, BackupInfo backup)
        {
            // Never delete anything outside the backups folder
            string root = System.IO.Path.GetFullPath(Folder(setup)).TrimEnd('\\') + "\\";
            string target = System.IO.Path.GetFullPath(backup.Path);
            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Not a backup folder: " + backup.Path);

            Directory.Delete(target, recursive: true);

            // Remove the name folder once its last backup is gone (the classic list skips empty ones anyway)
            string nameDir = System.IO.Path.GetDirectoryName(target);
            if (Directory.Exists(nameDir) && !Directory.EnumerateFileSystemEntries(nameDir).Any()) Directory.Delete(nameDir);
        }

        /// <summary>
        /// Stages the files of a backup as ".restore" copies; they replace the live files on the next start,
        /// before the core opens the database (classic: PrepareBackupRestore).
        /// </summary>
        public static void PrepareRestore(AppSetup setup, BackupInfo backup)
        {
            if (!backup.CanRestore) throw new InvalidOperationException("This backup has no data.db.");

            Directory.CreateDirectory(setup.AppDataPath);
            File.Copy(System.IO.Path.Combine(backup.Path, "data.db"), setup.DbPath + BackupRestore.Suffix, overwrite: true);
            CopyIfExists(System.IO.Path.Combine(backup.Path, "config.xml"), setup.ConfigPath + BackupRestore.Suffix);
            CopyIfExists(System.IO.Path.Combine(backup.Path, "labdata.xml"), System.IO.Path.Combine(setup.AppDataPath, "labdata.xml") + BackupRestore.Suffix);

            string clientTxt = System.IO.Path.Combine(backup.Path, "Client.txt");
            string target = ClientTxtAfterRestore(setup, backup);
            if (File.Exists(clientTxt) && target != null)
            {
                File.Copy(clientTxt, BackupRestore.StagedClientTxt(target), overwrite: true);
            }
        }

        /// <summary>
        /// The Client.txt the restore will use: the one in the backup's config.xml (applied first at startup)
        /// if its folder exists, otherwise the current one.
        /// </summary>
        private static string ClientTxtAfterRestore(AppSetup setup, BackupInfo backup)
        {
            string fromBackup = File.Exists(System.IO.Path.Combine(backup.Path, "config.xml"))
                ? new AppSetup(backup.Path).ClientTxtPath : null;
            if (!string.IsNullOrWhiteSpace(fromBackup) && Directory.Exists(System.IO.Path.GetDirectoryName(fromBackup))) return fromBackup;

            string current = setup.ClientTxtPath;
            return !string.IsNullOrWhiteSpace(current) && Directory.Exists(System.IO.Path.GetDirectoryName(current)) ? current : null;
        }

        private static void CopyIfExists(string from, string to)
        {
            if (File.Exists(from)) File.Copy(from, to, overwrite: true);
        }
    }
}
