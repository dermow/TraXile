using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace TraXile.Next.Services
{
    /// <summary>A Path of Exile league with its play period (from the TraXile metadata).</summary>
    public sealed class LeagueInfo
    {
        public string Name { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        /// <summary>The league that is running now (flag in the metadata).</summary>
        public bool IsCurrent { get; set; }

        public string Version => $"{Major}.{Minor}";
        public override string ToString() => $"{Name} ({Version})";
    }

    /// <summary>
    /// Reads the leagues from the metadata file the classic app downloads
    /// (<c>%APPDATA%\TraXile\metadata\metadata.xml</c>, same parsing as the classic <c>InitLeagueInfo</c>).
    /// </summary>
    public static class LeagueCatalog
    {
        /// <summary>Leagues in file order (oldest first); empty if the file is missing or broken.</summary>
        public static IReadOnlyList<LeagueInfo> Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return Array.Empty<LeagueInfo>();
                var xml = new XmlDocument();
                xml.Load(path);
                return Parse(xml);
            }
            catch (Exception)
            {
                return Array.Empty<LeagueInfo>();
            }
        }

        public static IReadOnlyList<LeagueInfo> Parse(XmlDocument xml)
        {
            var result = new List<LeagueInfo>();
            XmlNode root = xml.SelectSingleNode("/metadata/leagues");
            if (root == null) return result;

            foreach (XmlNode node in root.SelectNodes("league"))
            {
                try
                {
                    result.Add(new LeagueInfo
                    {
                        Name = node.Attributes["name"].Value,
                        Major = int.Parse(node.Attributes["major"].Value, CultureInfo.InvariantCulture),
                        Minor = int.Parse(node.Attributes["minor"].Value, CultureInfo.InvariantCulture),
                        Start = ParseDate(node.Attributes["start"].Value),
                        End = ParseDate(node.Attributes["end"].Value),
                        IsCurrent = node.Attributes["current"] != null && bool.TryParse(node.Attributes["current"].Value, out bool c) && c
                    });
                }
                catch (Exception)
                {
                    // skip broken entries, keep the others
                }
            }
            return result;
        }

        /// <summary>"2017-08-04 20:00:00 +0" (UTC offset) -> local time, like the classic DateTime.Parse.</summary>
        private static DateTime ParseDate(string s) => DateTime.Parse(s, CultureInfo.InvariantCulture);

        /// <summary>The running league: the one flagged current, otherwise the newest one.</summary>
        public static LeagueInfo Current(IEnumerable<LeagueInfo> leagues) =>
            leagues.FirstOrDefault(l => l.IsCurrent) ?? leagues.OrderBy(l => l.Start).LastOrDefault();

        /// <summary>Sample leagues for the demo data: the last one covers the demo history.</summary>
        public static IReadOnlyList<LeagueInfo> Demo(DateTime now)
        {
            DateTime start = now.Date.AddDays(-20);
            var list = new List<LeagueInfo>();
            string[] names = { "Necropolis", "Settlers", "Phrecia", "Mercenaries", "Keepers", "Ascendancy" };
            for (int i = 0; i < names.Length; i++)
            {
                int age = names.Length - 1 - i; // 0 = newest
                DateTime s = start.AddDays(-age * 120);
                list.Add(new LeagueInfo
                {
                    Name = names[i],
                    Major = 3,
                    Minor = 24 + i,
                    Start = s,
                    End = age == 0 ? s.AddDays(120) : s.AddDays(110),
                    IsCurrent = age == 0
                });
            }
            return list;
        }
    }
}
