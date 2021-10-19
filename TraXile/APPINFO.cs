using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TraXile
{
    static class APPINFO
    {
        public static string VERSION = "0.5.3";
        public static string RELEASE_TAG = "beta1";
        public static string NAME = "TraXile";
        public static string ISSUE_URL = "https://github.com/dermow/TraXile/issues";
        public static string RELEASE_URL = "https://github.com/dermow/TraXile/releases/latest";
        public static string WIKI_URL = "https://github.com/dermow/TraXile/wiki";

        public static long BUILDTIME
        {
            get { return Assembly.GetExecutingAssembly().GetLinkerTime(); }
        }

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
