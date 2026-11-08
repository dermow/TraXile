using System;
using System.Collections.Generic;
using System.IO;

namespace TraXile.Next.Services
{
    /// <summary>Outcome of applying a prepared backup restore at startup.</summary>
    public sealed class RestoreResult
    {
        public static readonly RestoreResult Nothing = new RestoreResult();

        /// <summary>Files that were replaced by their staged ".restore" copy.</summary>
        public IList<string> RestoredFiles { get; } = new List<string>();

        /// <summary>Set if (part of) the restore failed.</summary>
        public string Error { get; set; }

        public bool HasRestored => RestoredFiles.Count > 0;
        public bool HasFailed => Error != null;
    }

    /// <summary>
    /// A backup restore is prepared by staging copies next to the live files (data.db.restore,
    /// config.xml.restore, labdata.xml.restore, _Client.txt.restore) and restarting. This applies them before
    /// the core opens the database - the same file layout and order as the classic app
    /// (Main.DoBackupRestoreIfPrepared), so a restore prepared by either app is applied by both.
    /// </summary>
    public static class BackupRestore
    {
        public const string Suffix = ".restore";

        /// <summary>Staged copy of Client.txt, next to the configured Client.txt.</summary>
        public static string StagedClientTxt(string clientTxtPath) =>
            Path.Combine(Path.GetDirectoryName(clientTxtPath), "_Client.txt" + Suffix);

        public static bool IsPrepared(AppSetup setup) =>
            File.Exists(setup.DbPath + Suffix) ||
            File.Exists(setup.ConfigPath + Suffix) ||
            File.Exists(Path.Combine(setup.AppDataPath, "labdata.xml") + Suffix);

        public static RestoreResult ApplyIfPrepared(AppSetup setup)
        {
            var result = new RestoreResult();
            try
            {
                Replace(setup.DbPath, result);
                // config.xml first: the Client.txt below is restored to the path of the restored config
                Replace(setup.ConfigPath, result);
                Replace(Path.Combine(setup.AppDataPath, "labdata.xml"), result);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                result.Error = ex.Message;
                return result;
            }

            try
            {
                string clientTxt = setup.ClientTxtPath;
                if (!string.IsNullOrEmpty(clientTxt) && File.Exists(StagedClientTxt(clientTxt)))
                {
                    if (File.Exists(clientTxt)) File.Delete(clientTxt);
                    File.Move(StagedClientTxt(clientTxt), clientTxt);
                    result.RestoredFiles.Add(clientTxt);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                result.Error = "Could not restore Client.txt, please make sure that Path of Exile is not running. " + ex.Message;
            }

            return result;
        }

        private static void Replace(string target, RestoreResult result)
        {
            string staged = target + Suffix;
            if (!File.Exists(staged)) return;

            if (File.Exists(target)) File.Delete(target);
            File.Move(staged, target);
            result.RestoredFiles.Add(target);
        }
    }
}
