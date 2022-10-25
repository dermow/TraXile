using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile.UI
{
    internal class TrX_HelpDefinitions
    {
        // Mapping Control.Name -> ToolTipText
        public static Dictionary<string, string> ToolTips = new Dictionary<string, string>()
        {
            { "buttonReloadLogfile", "This action resets all data and reloads the current Client.txt"  },

            { "buttonChangeLogReload", "This action changes the path to the Client.txt, resets all data and reloads the logfile with the new path" },

            { "buttonFullReset", "With this action all statistics will be set to 0, all entries in history will be deleted and your current Client.txt will be cleared. "
                + Environment.NewLine + "A backup will be created. Path of Exile needs to be closed first!" },

            { "buttonRollLog", "Roll (rename) your current Client.txt (if it gets too big) so that you can safely delete it. Keeping all data in TraXile. "
                + Environment.NewLine + "Path of Exile needs to be closed first!" },

            { "timeCaps",  "You can set time caps (in seconds) for all activity types. Activities that take longer are capped to this value. This is used to filter out very long idle times." },

            { "labelMinTimeCap", "Set the minimum time for a activity to be valid. Helps to filter out ultra short maps and stuff like that." },

            { "buttonCreateBackup", "Creates a backup of your current database, config and Client.txt" },

            { "buttonRestoreBackup", "Restores TraXile to the state of a previously created backup. Application will be restarted. Path of Exile needs to be closed first!" },

            { "buttonDeleteBackup", "Deletes the selected Backup." },

            { "checkBoxShowGridInAct", "Show grid lines in activity history table." },

            { "checkBoxShowGridInStats", "Show grid lines in statistics table." },

            { "checkBoxMinimizeToTray", "Minimize to tray and hide in taskbar." },
        };
    }
}
