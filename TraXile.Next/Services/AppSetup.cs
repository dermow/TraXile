using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Where TraXile keeps its data and which Client.txt it reads. Uses the same config.xml (and the same
    /// settings manager) as the classic 2.x app, so both read and write the settings the same way.
    /// </summary>
    public sealed class AppSetup
    {
        public const string ClientTxtKey = "poe_logfile_path";

        public AppSetup(string appDataPath)
        {
            AppDataPath = appDataPath;
            ConfigPath = Path.Combine(appDataPath, "config.xml");
            DbPath = Path.Combine(appDataPath, "data.db");
        }

        /// <summary>The real TraXile data folder (%APPDATA%\TraXile).</summary>
        public static AppSetup Default => new AppSetup(TrX_Static.APPDATA_PATH);

        public string AppDataPath { get; }
        public string ConfigPath { get; }
        public string DbPath { get; }

        /// <summary>Configured Client.txt path, or null.</summary>
        public string ClientTxtPath => ReadSetting(ClientTxtKey);

        /// <summary>True if a Client.txt is configured and exists (live tracking can start).</summary>
        public bool HasValidClientTxt => IsValidClientTxt(ClientTxtPath);

        public static bool IsValidClientTxt(string path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path);

        public string ReadSetting(string key, string defaultValue = null) => LoadSettings().ReadSetting(key, defaultValue);

        /// <summary>Writes a setting into config.xml, keeping every other setting as it is.</summary>
        public void WriteSetting(string key, string value)
        {
            Directory.CreateDirectory(AppDataPath);
            LoadSettings().AddOrUpdateSetting(key, value);
        }

        public void SaveClientTxtPath(string path)
        {
            if (!IsValidClientTxt(path)) throw new FileNotFoundException("Client.txt not found", path);
            WriteSetting(ClientTxtKey, path);
        }

        internal TrX_SettingsManager LoadSettings()
        {
            var settings = new TrX_SettingsManager(ConfigPath);
            settings.LoadFromXml();
            return settings;
        }

        /// <summary>
        /// Client.txt files of Steam and standalone installations found on this PC
        /// (same detection as the classic first-run screen).
        /// </summary>
        public static IReadOnlyList<string> DetectClientTxt()
        {
            try
            {
                return new Trx_PoeLogFileDetector().AutoDiscoverPoeLogs()
                    .Where(IsValidClientTxt)
                    .Select(Path.GetFullPath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception)
            {
                return Array.Empty<string>(); // registry or Steam config not readable
            }
        }
    }
}
