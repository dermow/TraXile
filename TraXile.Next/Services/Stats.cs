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

        /// <summary>Tooltip of the Statistic: its description if it has one, else its key.</summary>
        public string ToolTip => Descriptions.TryGetValue(Key ?? "", out string d) ? d : Key;

        /// <summary>Explanations for Statistics whose meaning is easy to mix up with an Activity-based value.</summary>
        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { StatMath.DeathsKey, "Sum of the death counters of the Activities started in this time range " +
                                  "(the same value as the Deaths on the Dashboard)." }
        };

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
        /// Key of the "Deaths" Statistic. Its value is not read from tx_stats (the core counts every death in the
        /// log there) but derived from the Activities' death counters, so it matches the Deaths on the Dashboard.
        /// </summary>
        public const string DeathsKey = "TotalKilledCount";

        /// <summary>Sum of the death counters of the Activities started in [from, to].</summary>
        public static int ActivityDeaths(IEnumerable<ActivityInfo> history, DateTime from, DateTime to) =>
            history.Where(a => a.Started >= from && a.Started <= to).Sum(a => a.Deaths);

        /// <summary>
        /// The Activities' death counters as a cumulative series like tx_stats (one entry per Activity with deaths,
        /// at its start): the last entry before <paramref name="from"/> (if any), then the entries in [from, to].
        /// </summary>
        public static IReadOnlyList<StatEntry> ActivityDeathSeries(IEnumerable<ActivityInfo> history, DateTime from, DateTime to)
        {
            var result = new List<StatEntry>();
            StatEntry? before = null;
            int total = 0;
            foreach (ActivityInfo a in history.Where(a => a.Deaths > 0 && a.Started <= to).OrderBy(a => a.Started))
            {
                total += a.Deaths;
                var e = new StatEntry(a.Started, total);
                if (a.Started < from) before = e;
                else result.Add(e);
            }
            if (before.HasValue) result.Insert(0, before.Value);
            return result;
        }

        /// <summary>
        /// The classic increase of a stat in a window: max - min of the stored values, where a minimum of 1 counts
        /// as 0 (the first increment of a counter is part of the window).
        /// </summary>
        public static int Increase(int min, int max) => max - (min == 1 ? 0 : min);

        /// <summary>
        /// The window for Statistics: the filter's time range widened to whole days, like the classic stats list;
        /// an open start begins at the oldest stored value (<paramref name="oldest"/>), an open end at <paramref name="now"/>.
        /// </summary>
        public static (DateTime from, DateTime to) Window(DateTime? rangeFrom, DateTime? rangeTo, DateTime? oldest, DateTime now)
        {
            DateTime from = rangeFrom ?? oldest ?? now;
            DateTime to = rangeTo ?? now;
            return (from.Date, to.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>The window of the same length right before [<paramref name="from"/>, <paramref name="to"/>].</summary>
        public static (DateTime from, DateTime to) PreviousWindow(DateTime from, DateTime to)
        {
            TimeSpan length = to - from + TimeSpan.FromSeconds(1);
            return (from - length, from.AddSeconds(-1));
        }

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
