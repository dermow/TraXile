using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace TraXile.Next.Services
{
    /// <summary>
    /// UI-friendly, immutable snapshot of a tracked activity.
    /// Decouples the views from the mutable core objects that are changed on worker threads.
    /// </summary>
    public sealed class ActivityInfo
    {
        public string Id { get; set; }
        public ACTIVITY_TYPES Type { get; set; }
        public string Area { get; set; }
        public int AreaLevel { get; set; }
        public int MapTier { get; set; }
        public DateTime Started { get; set; }
        public int TotalSeconds { get; set; }
        public int Deaths { get; set; }
        public int Portals { get; set; }
        public double PausedSeconds { get; set; }
        public IReadOnlyList<TagInfo> Tags { get; set; } = Array.Empty<TagInfo>();

        /// <summary>Side areas visited during this activity (Zana, Vaal, Abyss, Sanctum, ...).</summary>
        public IReadOnlyList<ActivityInfo> SubActivities { get; set; } = Array.Empty<ActivityInfo>();

        public string TypeName => ActivityPalette.DisplayName(Type);
        public Brush TypeBrush => ActivityPalette.Brush(Type);
        public string Duration => FormatDuration(TotalSeconds);
        public string StartedText => Started.ToString("ddd HH:mm");
        public string StartedFull => Started.ToString("yyyy-MM-dd HH:mm");
        public string PausedText => PausedSeconds >= 1 ? FormatDuration((int)PausedSeconds) : "—";
        public string TagNames => string.Join(", ", Tags.Select(t => t.Name));
        public bool HasSubActivities => SubActivities.Count > 0;

        public string LevelText =>
            MapTier > 0 ? $"T{MapTier}" :
            AreaLevel > 0 ? $"Lvl {AreaLevel}" : "";

        public static string FormatDuration(int seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}"
                : $"{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }

    public sealed class TagInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }

        /// <summary>Default tags are set automatically by the tracker and cannot be added/removed manually.</summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// What is happening right now (polled by the UI once per second).
    /// </summary>
    public sealed class LiveState
    {
        public bool IsTracking { get; set; }
        public bool IsPaused { get; set; }
        public string CurrentArea { get; set; } = "—";
        public ActivityInfo Activity { get; set; }
        public TimeSpan Elapsed { get; set; }
        public IReadOnlyList<TagInfo> ActiveTags { get; set; } = Array.Empty<TagInfo>();
    }

    /// <summary>
    /// Data source for the UI: either the real TraXile core or demo data.
    /// </summary>
    public interface ITrackingSource : IDisposable
    {
        bool IsDemo { get; }

        /// <summary>Optional message for the user (e.g. why demo mode is active).</summary>
        string Notice { get; }

        /// <summary>Raised (on any thread) when the activity history changed.</summary>
        event EventHandler HistoryChanged;

        void Start();
        IReadOnlyList<ActivityInfo> GetHistory();
        LiveState GetLiveState();
        double GetHideoutSeconds(DateTime from, DateTime to);
        int GetTimeCap(ACTIVITY_TYPES type);
        void TogglePause();

        /// <summary>All known tags (default and custom).</summary>
        IReadOnlyList<TagInfo> GetTags();

        /// <summary>Attach a (non-default) tag to an activity and persist it.</summary>
        void AddTag(string activityId, string tagId);

        /// <summary>Remove a (non-default) tag from an activity and persist it.</summary>
        void RemoveTag(string activityId, string tagId);

        /// <summary>Delete activities from the history and the database.</summary>
        void DeleteActivities(IReadOnlyCollection<string> activityIds);
    }
}
