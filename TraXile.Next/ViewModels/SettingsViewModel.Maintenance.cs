using System;
using System.IO;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Maintenance actions of the classic Settings tab. All of them end with a restart.</summary>
    public sealed partial class SettingsViewModel
    {
        /// <summary>Maintenance needs the live core, write access and a configured Client.txt.</summary>
        public bool CanMaintain => CanWrite && Core != null && AppSetup.IsValidClientTxt(ClientTxtPath);

        /// <summary>
        /// Reads Client.txt again from the start (classic: reload logfile). Activities are rebuilt;
        /// tags and pauses added by hand are lost.
        /// </summary>
        public bool ReloadLogfile(out string error)
        {
            error = null;
            if (!CanMaintain) { error = "Not available right now."; return false; }

            Core.StopAndClearHistory();
            _restart();
            return true;
        }

        /// <summary>
        /// Keeps a copy of Client.txt and empties it, so TraXile starts fast again (classic: shrink logfile).
        /// The tracked history stays.
        /// </summary>
        public bool ShrinkLogfile(DateTime now, out string archive, out string error)
        {
            archive = null;
            error = null;
            if (!CanMaintain) { error = "Not available right now."; return false; }

            try
            {
                archive = Maintenance.ShrinkClientTxt(ClientTxtPath, now);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                error = ex.Message;
                return false;
            }
            _restart();
            return true;
        }

        /// <summary>
        /// Deletes all data: optional backup first, then Client.txt is emptied and the history cleared
        /// (classic: full reset).
        /// </summary>
        public bool FullReset(bool backupFirst, DateTime now, out string backupDir, out string error)
        {
            backupDir = null;
            error = null;
            if (!CanMaintain) { error = "Not available right now."; return false; }

            try
            {
                if (backupFirst) backupDir = Maintenance.CreateBackup(_setup, Maintenance.AutoBackupName, now);
                Maintenance.TruncateClientTxt(ClientTxtPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                error = ex.Message;
                return false;
            }

            Core.StopAndClearHistory();
            _restart();
            return true;
        }
    }
}
