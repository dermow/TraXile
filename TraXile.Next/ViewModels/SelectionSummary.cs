using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>One activity type in the selection summary.</summary>
    public sealed class SummaryTypeRow
    {
        public ACTIVITY_TYPES Type { get; set; }
        public string Name => ActivityPalette.DisplayName(Type);
        public Brush Brush => ActivityPalette.Brush(Type);
        public int Count { get; set; }
        public int TotalSeconds { get; set; }
        public int AverageSeconds => Count == 0 ? 0 : (int)Math.Round((double)TotalSeconds / Count);
        public string Total => SelectionSummary.FormatLong(TotalSeconds);
        public string Average => ActivityInfo.FormatDuration(AverageSeconds);
    }

    /// <summary>One tag in the selection summary.</summary>
    public sealed class SummaryTagRow
    {
        public TagInfo Tag { get; set; }
        public int Count { get; set; }
        /// <summary>Share of the selected activities that have the tag, 0..100 (one decimal).</summary>
        public double Percent { get; set; }
        public string Name => Tag.Name;
        public Brush Background => Tag.Background;
        public Brush Foreground => Tag.Foreground;
        public string Text => $"{Count:N0} · {Percent:0.#} %";
    }

    /// <summary>
    /// Summary of selected activities, like the classic SummaryForm (Main.BuildAndShowSummary): count, total and
    /// average time, a table per activity type and tag counts. Durations are capped at the time cap of their type,
    /// like everywhere in the classic statistics.
    /// </summary>
    public sealed class SelectionSummary
    {
        public static readonly SelectionSummary Empty = new SelectionSummary();

        public int Count { get; private set; }
        public int TotalSeconds { get; private set; }
        public int AverageSeconds => Count == 0 ? 0 : (int)Math.Round((double)TotalSeconds / Count);
        public string Total => FormatLong(TotalSeconds);
        public string Average => ActivityInfo.FormatDuration(AverageSeconds);
        public IReadOnlyList<SummaryTypeRow> Types { get; private set; } = Array.Empty<SummaryTypeRow>();
        public IReadOnlyList<SummaryTagRow> Tags { get; private set; } = Array.Empty<SummaryTagRow>();
        public bool HasTags => Tags.Count > 0;

        /// <param name="timeCap">Time cap in seconds per type (null: no cap).</param>
        public static SelectionSummary Create(IReadOnlyCollection<ActivityInfo> activities, Func<ACTIVITY_TYPES, int> timeCap = null)
        {
            if (activities == null || activities.Count == 0) return Empty;

            int Capped(ActivityInfo a)
            {
                int cap = timeCap?.Invoke(a.Type) ?? 0;
                return cap > 0 && a.TotalSeconds > cap ? cap : a.TotalSeconds;
            }

            var types = activities.GroupBy(a => a.Type)
                .Select(g => new SummaryTypeRow { Type = g.Key, Count = g.Count(), TotalSeconds = g.Sum(Capped) })
                .OrderByDescending(r => r.TotalSeconds).ThenBy(r => r.Name)
                .ToList();

            var tags = activities.SelectMany(a => a.Tags.Where(t => t != null).GroupBy(t => t.Id).Select(g => g.First()))
                .GroupBy(t => t.Id)
                .Select(g => new SummaryTagRow
                {
                    Tag = g.First(),
                    Count = g.Count(),
                    Percent = Math.Round(100.0 * g.Count() / activities.Count, 1)
                })
                .OrderByDescending(r => r.Count).ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new SelectionSummary
            {
                Count = activities.Count,
                TotalSeconds = types.Sum(t => t.TotalSeconds),
                Types = types,
                Tags = tags
            };
        }

        /// <summary>h:mm:ss, or "1d 2h 3m" from one day on (classic: long version for durations over one day).</summary>
        public static string FormatLong(int seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalDays >= 1
                ? $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m"
                : ActivityInfo.FormatDuration(seconds);
        }
    }
}
