using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Deterministic sample data so the UI can be explored (and screenshotted)
    /// without Path of Exile or an existing TraXile database.
    /// </summary>
    public sealed class DemoTrackingSource : ITrackingSource
    {
        private static readonly string[] MapNames =
        {
            "Crimson Temple", "Strand", "Tower", "Dunes", "Cemetery", "Defiled Cathedral",
            "Toxic Sewer", "Jungle Valley", "City Square", "Glacier", "Burial Chambers", "Lava Lake"
        };

        private readonly List<ActivityInfo> _history = new List<ActivityInfo>();
        private readonly Stopwatch _current = Stopwatch.StartNew();
        private readonly TimeSpan _currentOffset = TimeSpan.FromSeconds(274);
        private readonly ActivityInfo _currentActivity;
        private bool _paused;

        public bool IsDemo => true;
        public string Notice { get; }
        public event EventHandler HistoryChanged;

        public DemoTrackingSource(string notice = null) : this(notice, 0)
        {
        }

        /// <summary>
        /// Creates a demo source with exactly <paramref name="activityCount"/> activities going back from now
        /// (one every ~8 minutes of play time). Used for performance tests with large histories.
        /// </summary>
        public static DemoTrackingSource WithActivityCount(int activityCount) => new DemoTrackingSource(null, activityCount);

        private DemoTrackingSource(string notice, int activityCount)
        {
            Notice = notice ?? "You are looking at generated sample data — no Path of Exile log is being read.";

            var rnd = new Random(42);
            DateTime now = DateTime.Now;

            if (activityCount > 0)
            {
                DateTime t = now.AddMinutes(-6);
                for (int i = 0; i < activityCount; i++)
                {
                    ActivityInfo a = RandomActivity(rnd, t);
                    a.Started = t.AddSeconds(-a.TotalSeconds);
                    _history.Add(a); // newest first
                    t = a.Started.AddSeconds(-rnd.Next(40, 240));
                }
            }

            for (int day = 29; day >= 0 && activityCount == 0; day--)
            {
                // Some days off, weekends busier
                DateTime date = now.Date.AddDays(-day);
                bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                if (rnd.NextDouble() < 0.15 && day > 0) continue;

                int sessions = weekend ? rnd.Next(25, 45) : rnd.Next(8, 25);
                DateTime t = date.AddHours(weekend ? 13 : 18).AddMinutes(rnd.Next(0, 60));
                if (day == 0) t = now.AddHours(-3);

                for (int i = 0; i < sessions && t < now.AddMinutes(-6); i++)
                {
                    ActivityInfo a = RandomActivity(rnd, t);
                    _history.Add(a);
                    t = t.AddSeconds(a.TotalSeconds + rnd.Next(40, 240)); // + hideout time
                }
            }

            if (activityCount == 0) _history.Reverse(); // newest first, like the core

            _currentActivity = new ActivityInfo
            {
                Id = "demo-current",
                Type = ACTIVITY_TYPES.MAP,
                Area = "Crimson Temple",
                MapTier = 16,
                AreaLevel = 83,
                Started = now - _currentOffset,
                Tags = new[] { Tag("delirium", "Delirium", 0xE5E7EB, 0x111827), Tag("expedition", "Expedition", 0x2DD4BF, 0x042F2E) }
            };
        }

        private static ActivityInfo RandomActivity(Random rnd, DateTime start)
        {
            double r = rnd.NextDouble();
            ACTIVITY_TYPES type =
                r < 0.62 ? ACTIVITY_TYPES.MAP :
                r < 0.72 ? ACTIVITY_TYPES.HEIST :
                r < 0.79 ? ACTIVITY_TYPES.DELVE :
                r < 0.84 ? ACTIVITY_TYPES.SANCTUM :
                r < 0.88 ? ACTIVITY_TYPES.LABYRINTH :
                r < 0.91 ? ACTIVITY_TYPES.SIMULACRUM :
                r < 0.94 ? ACTIVITY_TYPES.LOGBOOK :
                r < 0.96 ? ACTIVITY_TYPES.SEARING_EXARCH_FIGHT :
                r < 0.98 ? ACTIVITY_TYPES.MAVEN_FIGHT : ACTIVITY_TYPES.BREACHSTONE;

            int seconds;
            string area;
            int tier = 0, level = 0;
            var tags = new List<TagInfo>();
            var subs = new List<ActivityInfo>();

            switch (type)
            {
                case ACTIVITY_TYPES.MAP:
                    area = MapNames[rnd.Next(MapNames.Length)];
                    tier = rnd.NextDouble() < 0.7 ? 16 : rnd.Next(14, 17);
                    level = 67 + tier;
                    seconds = rnd.Next(150, 480);
                    if (rnd.NextDouble() < 0.30) tags.Add(Tag("expedition", "Expedition", 0x2DD4BF, 0x042F2E));
                    if (rnd.NextDouble() < 0.20) tags.Add(Tag("blight", "Blight", 0x86EFAC, 0x052E16));
                    if (rnd.NextDouble() < 0.15) tags.Add(Tag("harvest", "Harvest", 0x818CF8, 0x1E1B4B));
                    if (rnd.NextDouble() < 0.10) tags.Add(Tag("syndicate", "Syndicate", 0xFBBF24, 0x422006));
                    if (rnd.NextDouble() < 0.12) tags.Add(Juiced);
                    if (rnd.NextDouble() < 0.04) tags.Add(Party);
                    if (rnd.NextDouble() < 0.06)
                    {
                        subs.Add(new ActivityInfo
                        {
                            Id = Guid.NewGuid().ToString(), Type = ACTIVITY_TYPES.VAAL_SIDEAREA, Area = "Vaal Temple",
                            AreaLevel = level, Started = start.AddSeconds(90), TotalSeconds = rnd.Next(40, 120)
                        });
                    }
                    if (rnd.NextDouble() < 0.05)
                    {
                        subs.Add(new ActivityInfo
                        {
                            Id = Guid.NewGuid().ToString(), Type = ACTIVITY_TYPES.ABYSSAL_DEPTHS, Area = "Abyssal Depths",
                            AreaLevel = level, Started = start.AddSeconds(150), TotalSeconds = rnd.Next(60, 150)
                        });
                    }
                    break;
                case ACTIVITY_TYPES.HEIST:
                    area = "Bunker"; level = 83; seconds = rnd.Next(200, 600); break;
                case ACTIVITY_TYPES.DELVE:
                    area = "Azurite Mine"; level = 83; seconds = rnd.Next(300, 900); break;
                case ACTIVITY_TYPES.SANCTUM:
                    area = "Forbidden Sanctum"; level = 83; seconds = rnd.Next(600, 1500); break;
                case ACTIVITY_TYPES.LABYRINTH:
                    area = "Eternal Labyrinth"; level = 75; seconds = rnd.Next(420, 720); break;
                case ACTIVITY_TYPES.SIMULACRUM:
                    area = "Bridge Enraptured"; level = 83; seconds = rnd.Next(900, 1500); break;
                case ACTIVITY_TYPES.LOGBOOK:
                    area = "Expedition Logbook"; level = 83; seconds = rnd.Next(300, 600); break;
                case ACTIVITY_TYPES.SEARING_EXARCH_FIGHT:
                    area = "Absence of Patience and Wisdom"; level = 85; seconds = rnd.Next(90, 240); break;
                case ACTIVITY_TYPES.MAVEN_FIGHT:
                    area = "Absence of Mercy and Empathy"; level = 85; seconds = rnd.Next(150, 360); break;
                default:
                    area = "Breachstone"; level = 83; seconds = rnd.Next(120, 240); break;
            }

            return new ActivityInfo
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Area = area,
                AreaLevel = level,
                MapTier = tier,
                Started = start,
                TotalSeconds = seconds,
                Deaths = rnd.NextDouble() < 0.08 ? rnd.Next(1, 3) : 0,
                Portals = rnd.Next(1, 4),
                PausedSeconds = rnd.NextDouble() < 0.05 ? rnd.Next(30, 300) : 0,
                Tags = tags,
                SubActivities = subs
            };
        }

        // One shared instance per tag, like the core's tag list
        private static readonly Dictionary<string, TagInfo> TagCatalog = new Dictionary<string, TagInfo>();

        private static TagInfo Tag(string id, string name, int back, int fore, bool isDefault = true)
        {
            if (!TagCatalog.TryGetValue(id, out TagInfo tag))
            {
                tag = new TagInfo
                {
                    Id = id,
                    Name = name,
                    Background = ActivityPalette.BrushOf(Hex(back)),
                    Foreground = ActivityPalette.BrushOf(Hex(fore)),
                    IsDefault = isDefault
                };
                TagCatalog[id] = tag;
            }
            return tag;
        }

        // Custom (user) tags can be added/removed manually
        private static TagInfo Juiced => Tag("juiced", "Juiced", 0xF5B942, 0x1F1300, isDefault: false);
        private static TagInfo Party => Tag("party", "Party", 0x60A5FA, 0x0B1A33, isDefault: false);
        private static TagInfo Bricked => Tag("bricked", "Bricked", 0xFB7185, 0x3B0712, isDefault: false);

        private static Color Hex(int rgb) =>
            Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));

        public void Start() { }

        public IReadOnlyList<ActivityInfo> GetHistory() => _history.ToList(); // snapshot, the list can be edited

        public LiveState GetLiveState()
        {
            TimeSpan elapsed = _currentOffset + _current.Elapsed;
            _currentActivity.TotalSeconds = (int)elapsed.TotalSeconds;
            _currentActivity.Deaths = 0;

            return new LiveState
            {
                IsTracking = true,
                IsPaused = _paused,
                CurrentArea = _currentActivity.Area,
                Activity = _currentActivity,
                Elapsed = elapsed,
                ActiveTags = _currentActivity.Tags
            };
        }

        public double GetHideoutSeconds(DateTime from, DateTime to)
        {
            // ~12% of activity time is spent in hideout
            return _history.Where(a => a.Started >= from && a.Started <= to).Sum(a => a.TotalSeconds) * 0.12;
        }

        public int GetTimeCap(ACTIVITY_TYPES type) => 3600;

        public void TogglePause()
        {
            _paused = !_paused;
            if (_paused) _current.Stop(); else _current.Start();
        }

        public IReadOnlyList<TagInfo> GetTags()
        {
            // make sure the custom tags exist even if no activity uses them yet
            _ = Juiced; _ = Party; _ = Bricked;
            return TagCatalog.Values.ToList();
        }

        public void AddTag(string activityId, string tagId)
        {
            ActivityInfo act = _history.FirstOrDefault(a => a.Id == activityId);
            if (act == null || !TagCatalog.TryGetValue(tagId, out TagInfo tag) || tag.IsDefault) return;
            if (act.Tags.Any(t => t.Id == tagId)) return;

            act.Tags = act.Tags.Concat(new[] { tag }).ToList();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveTag(string activityId, string tagId)
        {
            ActivityInfo act = _history.FirstOrDefault(a => a.Id == activityId);
            if (act == null || !TagCatalog.TryGetValue(tagId, out TagInfo tag) || tag.IsDefault) return;

            act.Tags = act.Tags.Where(t => t.Id != tagId).ToList();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteActivities(IReadOnlyCollection<string> activityIds)
        {
            var ids = new HashSet<string>(activityIds);
            if (_history.RemoveAll(a => ids.Contains(a.Id)) > 0)
            {
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose() { }
    }
}
