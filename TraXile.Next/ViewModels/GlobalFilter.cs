using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Time part of the global filter (classic: comboBox_Filter_TimeRange).</summary>
    public enum FilterTime { All, Today, Last24Hours, Last7Days, Last30Days, LastYear, League, Custom }

    /// <summary>Comparison for the area level filter (classic: =, &gt;, &gt;=, &lt;, &lt;=).</summary>
    public enum LevelOperator { Equal, Greater, GreaterOrEqual, Less, LessOrEqual }

    /// <summary>How the selected tags combine (classic: OR / AND / NOT).</summary>
    public enum TagMatch { Any, All, None }

    /// <summary>A resolved time window; null bounds are open.</summary>
    public struct TimeWindow
    {
        public DateTime? From;
        public DateTime? To;

        public bool Contains(DateTime t) => (!From.HasValue || t >= From.Value) && (!To.HasValue || t <= To.Value);
    }

    /// <summary>
    /// The one app-wide filter of all analytics pages (not the dashboard). Every filtered page reads it and
    /// listens to <see cref="Changed"/>, so a filter set on one page applies on all of them.
    /// Kept in memory only, like the classic filter (not persisted across restarts).
    /// </summary>
    public sealed class GlobalFilter : ObservableObject
    {
        private readonly Func<DateTime> _now;
        private int _suspend;
        private bool _changedWhileSuspended;

        public GlobalFilter(IReadOnlyList<LeagueInfo> leagues = null, Func<DateTime> now = null)
        {
            Leagues = leagues ?? Array.Empty<LeagueInfo>();
            _now = now ?? (() => DateTime.Now);
            DateTime today = _now().Date;
            _customFrom = today.AddDays(-6);
            _customTo = today.AddDays(1).AddSeconds(-1);
        }

        /// <summary>Raised after any part of the filter changed.</summary>
        public event EventHandler Changed;

        /// <summary>Known leagues, newest last (for the league time option).</summary>
        public IReadOnlyList<LeagueInfo> Leagues { get; private set; }

        /// <summary>Raised after <see cref="SetLeagues"/> replaced the league list.</summary>
        public event EventHandler LeaguesChanged;

        /// <summary>
        /// Replaces the league list (e.g. after the metadata was downloaded). A selected league is kept by name;
        /// if it is gone, the time filter falls back to "All time".
        /// </summary>
        public void SetLeagues(IReadOnlyList<LeagueInfo> leagues)
        {
            Leagues = leagues ?? Array.Empty<LeagueInfo>();
            LeagueInfo selected = League == null ? null : Leagues.FirstOrDefault(l => l.Name == League.Name);
            Batch(() =>
            {
                if (selected != null) League = selected;
                else if (Time == FilterTime.League) { Time = FilterTime.All; League = null; }
            });
            LeaguesChanged?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------
        // Time
        // ------------------------------------------------------------------

        private FilterTime _time = FilterTime.All;
        public FilterTime Time { get => _time; set => Set(ref _time, value); }

        private LeagueInfo _league;
        /// <summary>League for <see cref="FilterTime.League"/>.</summary>
        public LeagueInfo League { get => _league; set => Set(ref _league, value); }

        private DateTime _customFrom;
        public DateTime CustomFrom { get => _customFrom; set => Set(ref _customFrom, value); }

        private DateTime _customTo;
        public DateTime CustomTo { get => _customTo; set => Set(ref _customTo, value); }

        /// <summary>The time window of the current time option (classic SetFilter).</summary>
        public TimeWindow Window
        {
            get
            {
                DateTime now = _now();
                switch (Time)
                {
                    case FilterTime.Today: return new TimeWindow { From = now.Date, To = now.Date.AddDays(1).AddTicks(-1) };
                    case FilterTime.Last24Hours: return new TimeWindow { From = now.AddHours(-24), To = now };
                    case FilterTime.Last7Days: return new TimeWindow { From = now.AddDays(-7), To = now };
                    case FilterTime.Last30Days: return new TimeWindow { From = now.AddDays(-30), To = now };
                    case FilterTime.LastYear: return new TimeWindow { From = now.AddDays(-365), To = now };
                    case FilterTime.League:
                        return League == null ? new TimeWindow() : new TimeWindow { From = League.Start, To = League.End };
                    case FilterTime.Custom:
                        return CustomFrom <= CustomTo
                            ? new TimeWindow { From = CustomFrom, To = CustomTo }
                            : new TimeWindow { From = CustomTo, To = CustomFrom };
                    default: return new TimeWindow();
                }
            }
        }

        // ------------------------------------------------------------------
        // Activity type, area, area level
        // ------------------------------------------------------------------

        private ACTIVITY_TYPES? _type;
        public ACTIVITY_TYPES? Type { get => _type; set => Set(ref _type, value); }

        private string _area;
        /// <summary>Area name (case-insensitive), null = all areas.</summary>
        public string Area { get => _area; set => Set(ref _area, string.IsNullOrWhiteSpace(value) ? null : value); }

        private int? _areaLevel;
        public int? AreaLevel { get => _areaLevel; set => Set(ref _areaLevel, value); }

        private LevelOperator _levelOperator = LevelOperator.Equal;
        public LevelOperator LevelOperator { get => _levelOperator; set => Set(ref _levelOperator, value); }

        // ------------------------------------------------------------------
        // Tags
        // ------------------------------------------------------------------

        private IReadOnlyList<string> _tags = Array.Empty<string>();
        /// <summary>Tag IDs; empty = no tag filter.</summary>
        public IReadOnlyList<string> Tags
        {
            get => _tags;
            set => Set(ref _tags, (IReadOnlyList<string>)(value ?? Array.Empty<string>()).Distinct().ToList());
        }

        private TagMatch _tagMatch = TagMatch.Any;
        public TagMatch TagMatch { get => _tagMatch; set => Set(ref _tagMatch, value); }

        public void SetTag(string tagId, bool on)
        {
            if (string.IsNullOrEmpty(tagId) || Tags.Contains(tagId) == on) return;
            Tags = on ? Tags.Concat(new[] { tagId }).ToList() : Tags.Where(t => t != tagId).ToList();
        }

        // ------------------------------------------------------------------
        // State
        // ------------------------------------------------------------------

        public bool IsActive =>
            Time != FilterTime.All || Type.HasValue || Area != null || AreaLevel.HasValue || Tags.Count > 0;

        public void Reset()
        {
            Batch(() =>
            {
                Time = FilterTime.All;
                League = null;
                Type = null;
                Area = null;
                AreaLevel = null;
                LevelOperator = LevelOperator.Equal;
                Tags = Array.Empty<string>();
                TagMatch = TagMatch.Any;
            });
        }

        /// <summary>Changes several parts at once and raises <see cref="Changed"/> only once.</summary>
        public void Batch(Action change)
        {
            _suspend++;
            try { change(); }
            finally
            {
                _suspend--;
                if (_suspend == 0 && _changedWhileSuspended)
                {
                    _changedWhileSuspended = false;
                    RaiseChanged();
                }
            }
        }

        private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (!SetProperty(ref field, value, name)) return;
            if (_suspend > 0) _changedWhileSuspended = true;
            else RaiseChanged();
        }

        private void RaiseChanged()
        {
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(Description));
            Changed?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------
        // Applying
        // ------------------------------------------------------------------

        public bool Matches(ActivityInfo a)
        {
            if (a == null) return false;
            if (!Window.Contains(a.Started)) return false;
            return MatchesIgnoringTime(a);
        }

        /// <summary>All parts except the time range (leagues page, previous periods on the dashboard).</summary>
        public bool MatchesIgnoringTime(ActivityInfo a)
        {
            if (Type.HasValue && a.Type != Type.Value) return false;
            if (Area != null && !string.Equals(a.Area, Area, StringComparison.OrdinalIgnoreCase)) return false;
            if (AreaLevel.HasValue && !Compare(a.AreaLevel, AreaLevel.Value, LevelOperator)) return false;

            if (Tags.Count > 0)
            {
                bool Has(string id) => a.Tags.Any(t => t.Id == id);
                switch (TagMatch)
                {
                    case TagMatch.Any: if (!Tags.Any(Has)) return false; break;
                    case TagMatch.All: if (!Tags.All(Has)) return false; break;
                    case TagMatch.None: if (Tags.Any(Has)) return false; break;
                }
            }
            return true;
        }

        /// <summary>The activities that pass the filter, in source order.</summary>
        public List<ActivityInfo> Apply(IEnumerable<ActivityInfo> source)
        {
            TimeWindow w = Window; // resolve "now" once
            return source.Where(a => a != null && w.Contains(a.Started) && MatchesIgnoringTime(a)).ToList();
        }

        private static bool Compare(int value, int level, LevelOperator op)
        {
            switch (op)
            {
                case LevelOperator.Greater: return value > level;
                case LevelOperator.GreaterOrEqual: return value >= level;
                case LevelOperator.Less: return value < level;
                case LevelOperator.LessOrEqual: return value <= level;
                default: return value == level;
            }
        }

        // ------------------------------------------------------------------
        // Text
        // ------------------------------------------------------------------

        public static string OperatorSymbol(LevelOperator op)
        {
            switch (op)
            {
                case LevelOperator.Greater: return ">";
                case LevelOperator.GreaterOrEqual: return "≥";
                case LevelOperator.Less: return "<";
                case LevelOperator.LessOrEqual: return "≤";
                default: return "=";
            }
        }

        public static string TimeLabel(FilterTime time)
        {
            switch (time)
            {
                case FilterTime.Today: return "Today";
                case FilterTime.Last24Hours: return "Last 24 hours";
                case FilterTime.Last7Days: return "Last 7 days";
                case FilterTime.Last30Days: return "Last 30 days";
                case FilterTime.LastYear: return "Last year";
                case FilterTime.League: return "League";
                case FilterTime.Custom: return "Custom range";
                default: return "All time";
            }
        }

        /// <summary>Tag ID -> display name, for <see cref="Description"/> (set by the filter bar).</summary>
        public Func<string, string> TagName { get; set; } = id => id;

        /// <summary>Short text of all active parts, e.g. "Last 7 days · Map · Level ≥ 83 · Juiced or Party".</summary>
        public string Description
        {
            get
            {
                var parts = new List<string>();
                switch (Time)
                {
                    case FilterTime.All: break;
                    case FilterTime.League: parts.Add(League == null ? "League" : $"League {League.Name}"); break;
                    case FilterTime.Custom:
                        TimeWindow w = Window;
                        parts.Add($"{w.From:yyyy-MM-dd HH:mm} – {w.To:yyyy-MM-dd HH:mm}");
                        break;
                    default: parts.Add(TimeLabel(Time)); break;
                }
                if (Type.HasValue) parts.Add(ActivityPalette.DisplayName(Type.Value));
                if (Area != null) parts.Add(Area);
                if (AreaLevel.HasValue) parts.Add($"Level {OperatorSymbol(LevelOperator)} {AreaLevel}");
                if (Tags.Count > 0)
                {
                    string names = string.Join(TagMatch == TagMatch.All ? " and " : " or ", Tags.Select(TagName));
                    parts.Add(TagMatch == TagMatch.None ? $"without {names}" : names);
                }
                return parts.Count == 0 ? "No filter" : string.Join(" · ", parts);
            }
        }
    }
}
