using System;
using System.IO;

namespace TraXile.Next.Services
{
    /// <summary>
    /// File operations behind the maintenance actions of the settings page, with the same file layout as the
    /// classic app (backups in &lt;data folder&gt;\backups\&lt;name&gt;\&lt;yyyy-MM-dd_HHmmss&gt;, rolled logs as
    /// Client.&lt;yyyy-MM-dd-H-m-s&gt;.txt next to Client.txt).
    /// </summary>
    public static class Maintenance
    {
        public const string AutoBackupName = "Auto_Backup";

        /// <summary>Copies Client.txt, data.db, config.xml, labdata.xml and stats.cache into a new backup folder.</summary>
        public static string CreateBackup(AppSetup setup, string name, DateTime now)
        {
            string dir = Path.Combine(setup.AppDataPath, "backups", name, now.ToString("yyyy-MM-dd_HHmmss"));
            Directory.CreateDirectory(dir);

            CopyIfExists(setup.ClientTxtPath, Path.Combine(dir, "Client.txt"));
            CopyIfExists(Path.Combine(setup.AppDataPath, "stats.cache"), Path.Combine(dir, "stats.cache"));
            CopyIfExists(setup.DbPath, Path.Combine(dir, "data.db"));
            CopyIfExists(setup.ConfigPath, Path.Combine(dir, "config.xml"));
            CopyIfExists(Path.Combine(setup.AppDataPath, "labdata.xml"), Path.Combine(dir, "labdata.xml"));
            return dir;
        }

        /// <summary>
        /// Keeps a copy of Client.txt as Client.&lt;date&gt;.txt and empties the original, which makes the next
        /// start fast again. Returns the path of the copy.
        /// </summary>
        public static string ShrinkClientTxt(string clientTxtPath, DateTime now)
        {
            string archive = Path.Combine(Path.GetDirectoryName(clientTxtPath), $"Client.{now:yyyy-MM-dd-H-m-s}.txt");
            File.Copy(clientTxtPath, archive);
            TruncateClientTxt(clientTxtPath);
            return archive;
        }

        /// <summary>Empties Client.txt (Path of Exile may keep it open, so share read/write).</summary>
        public static void TruncateClientTxt(string clientTxtPath)
        {
            using (new FileStream(clientTxtPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { }
        }

        private static void CopyIfExists(string source, string target)
        {
            if (!string.IsNullOrEmpty(source) && File.Exists(source)) File.Copy(source, target);
        }
    }
}
