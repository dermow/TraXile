using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TraXile
{
    static class TrX_AppInfo
    {
        // Application Version
        public static string VERSION = "0.9.5";

        // Application Name
        public static string NAME = "TraXile";

        // URL to issue reporting
        public static string ISSUE_URL = "https://github.com/dermow/TraXile/issues";

        // URL to releases
        public static string RELEASE_URL = "https://github.com/dermow/TraXile/releases/latest";

        // URL to Wiki
        public static string WIKI_URL = "https://github.com/dermow/TraXile/wiki";

        // URL to settings wiki
        public static string WIKI_URL_SETTINGS = "https://github.com/dermow/TraXile/wiki/Settings";

        // Path to Appdata
        public static string APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + TrX_AppInfo.NAME;

        // Path to cache file
        public static string CACHE_PATH = APPDATA_PATH + @"\stats.cache";

        // Path to config file
        public static string CONFIG_PATH = APPDATA_PATH + @"\config.xml";

        // Path to database file
        public static string DB_PATH = APPDATA_PATH + @"\data.db";

        // Path to version file
        public static string VERSION_FILE_PATH = APPDATA_PATH + @"\VERSION.txt";

        // Time this assembly was build
        public static long BUILDTIME
        {
            get { return Assembly.GetExecutingAssembly().GetLinkerTime(); }
        }

        /// <summary>
        /// Get Build time
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static long GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return ((DateTimeOffset)localTime).ToUnixTimeSeconds();
        }
    }
}
