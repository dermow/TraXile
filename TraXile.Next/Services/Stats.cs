using System;
using System.Collections.Generic;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>A stat counter of the core (tx_stats) with its value in a time window.</summary>
    public sealed class StatInfo
    {
        /// <summary>Key in tx_stats, e.g. "TotalMapsDone".</summary>
        public string Key { get; set; }

        /// <summary>Readable name (core StatNamesLong), the key if there is none.</summary>
        public string Name { get; set; }

        /// <summary>Increase in the window (classic TrX_StatsManager.GetIncrementValue).</summary>
        public int Value { get; set; }

        /// <summary>Hideout time and other "...Sec" stats are durations in seconds.</summary>
        public bool IsDuration => Key != null && Key.EndsWith("Sec", StringComparison.Ordinal);

        public string ValueText => IsDuration ? FormatSeconds(Value) : Value.ToString("N0");

        internal static string FormatSeconds(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1 ? $"{ts.TotalHours:N1} h" : $"{ts.Minutes} min";
        }
    }

    /// <summary>One stored value of a stat counter (the counters are cumulative).</summary>
    public struct StatEntry
    {
        public DateTime Time;
        public int Value;

        public StatEntry(DateTime time, int value)
        {
            Time = time;
            Value = value;
        }
    }

    /// <summary>The stat counters the core writes while parsing (tx_stats). Not part of every tracking source.</summary>
    public interface IStatsSource
    {
        /// <summary>All known stats with their increase in [from, to].</summary>
        IReadOnlyList<StatInfo> GetStats(DateTime from, DateTime to);

        /// <summary>
        /// Stored values of a stat in [from, to], oldest first; the first entry is the last value before
        /// <paramref name="from"/> (if any), so the first increase in the window can be computed.
        /// </summary>
        IReadOnlyList<StatEntry> GetStatSeries(string key, DateTime from, DateTime to);

        /// <summary>Time of the oldest stored stat value (classic "All" range), null if there is none.</summary>
        DateTime? OldestStatTime { get; }
    }

    public static class StatMath
    {
        /// <summary>
        /// The classic increase of a stat in a window: max - min of the stored values, where a minimum of 1 counts
        /// as 0 (the first increment of a counter is part of the window).
        /// </summary>
        public static int Increase(int min, int max) => max - (min == 1 ? 0 : min);

        /// <summary>
        /// Increase per day between <paramref name="from"/> and <paramref name="to"/> (local days). Entries before
        /// <paramref name="from"/> only serve as the starting value; a drop of the counter (reset) counts as a new start.
        /// </summary>
        public static IReadOnlyList<KeyValuePair<DateTime, int>> DailyIncrease(IEnumerable<StatEntry> entries, DateTime from, DateTime to)
        {
            DateTime first = from.Date, last = to.Date;
            if (last < first) return Array.Empty<KeyValuePair<DateTime, int>>();
            int days = (int)(last - first).TotalDays + 1;
            var sums = new int[days];

            int? prev = null;
            foreach (StatEntry e in entries.OrderBy(e => e.Time))
            {
                int delta = prev.HasValue ? (e.Value >= prev.Value ? e.Value - prev.Value : e.Value) : e.Value;
                prev = e.Value;
                if (e.Time < from || e.Time > to) continue;
                sums[(int)(e.Time.Date - first).TotalDays] += delta;
            }
            return Enumerable.Range(0, days).Select(i => new KeyValuePair<DateTime, int>(first.AddDays(i), sums[i])).ToList();
        }
    }
}
