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
    public sealed class DemoTrackingSource : ITrackingSource, IStatsSource, IDivCardSource
    {
        private static readonly string[] MapNames =
        {
            "Crimson Temple", "Strand", "Tower", "Dunes", "Cemetery", "Defiled Cathedral",
            "Toxic Sewer", "Jungle Valley", "City Square", "Glacier", "Burial Chambers", "Lava Lake"
        };

        private static readonly string[] LabNames =
        {
            "Uber-Lab", "Uber-Lab", "Uber-Lab", "The Merciless Labyrinth", "Advanced Uber-Lab", "The Cruel Labyrinth", "Unknown"
        };

        private static readonly string[] HeistAreas =
        {
            "Bunker", "Laboratory", "Records Office", "Mansion", "Smuggler's Den", "Underbelly", "Prohibited Library", "Tunnels", "Repository"
        };

        private readonly List<ActivityInfo> _history = new List<ActivityInfo>();
        private readonly Stopwatch _current = Stopwatch.StartNew();
        private readonly TimeSpan _currentOffset = TimeSpan.FromSeconds(274);
        private ActivityInfo _currentActivity;
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

            // The custom tags exist even if no activity uses them yet
            _ = Juiced; _ = Party; _ = Bricked;
            Bricked.SoundEnabled = true;
            Bricked.SoundId = "default_3.mp3";

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
                Tags = new[] { Tag("delirium", "Delirium", 0xE5E7EB, 0x111827), Tag("expedition", "Expedition", 0x2DD4BF, 0x042F2E), Juiced }
            };
        }

        private ActivityInfo RandomActivity(Random rnd, DateTime start)
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
                    // area and level derived from the duration: no extra random numbers, the rest of the demo data stays the same
                    seconds = rnd.Next(200, 600);
                    area = HeistAreas[seconds % HeistAreas.Length];
                    level = new[] { 72, 75, 78, 81, 83, 83, 83 }[seconds % 7];
                    break;
                case ACTIVITY_TYPES.DELVE:
                    area = "Azurite Mine"; level = 83; seconds = rnd.Next(300, 900); break;
                case ACTIVITY_TYPES.SANCTUM:
                    area = "Forbidden Sanctum"; level = 83; seconds = rnd.Next(600, 1500); break;
                case ACTIVITY_TYPES.LABYRINTH:
                    // lab name derived from the duration (no extra random numbers); the core names labs by difficulty
                    seconds = rnd.Next(420, 720);
                    area = LabNames[seconds % LabNames.Length];
                    level = 75;
                    break;
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

        // One shared instance per tag, like the core's tag list (per source: the tags can be edited)
        private readonly Dictionary<string, TagInfo> _tagCatalog = new Dictionary<string, TagInfo>();

        private TagInfo Tag(string id, string name, int back, int fore, bool isDefault = true)
        {
            if (!_tagCatalog.TryGetValue(id, out TagInfo tag))
            {
                tag = new TagInfo
                {
                    Id = id,
                    Name = name,
                    Background = ActivityPalette.BrushOf(Hex(back)),
                    Foreground = ActivityPalette.BrushOf(Hex(fore)),
                    IsDefault = isDefault
                };
                _tagCatalog[id] = tag;
            }
            return tag;
        }

        // Custom (user) tags can be added/removed manually
        private TagInfo Juiced => Tag("juiced", "Juiced", 0xF5B942, 0x1F1300, isDefault: false);
        private TagInfo Party => Tag("party", "Party", 0x60A5FA, 0x0B1A33, isDefault: false);
        private TagInfo Bricked => Tag("bricked", "Bricked", 0xFB7185, 0x3B0712, isDefault: false);

        private static Color Hex(int rgb) =>
            Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));

        public void Start() { }

        /// <summary>Pretend the log is still being read (for screenshots of the loading state).</summary>
        public bool SimulateLoading { get; set; }

        public bool IsLoading => SimulateLoading;

        public double LoadProgress => SimulateLoading ? 0.42 : 1.0;

        public IReadOnlyList<ActivityInfo> GetHistory() => _history.ToList(); // snapshot, the list can be edited

        public LiveState GetLiveState()
        {
            if (_currentActivity == null) return new LiveState();

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
                ActiveTags = _currentActivity.Tags,
                PreviousActivity = _history.FirstOrDefault(a => a.Type == _currentActivity.Type)
            };
        }

        public double GetHideoutSeconds(DateTime from, DateTime to)
        {
            // ~12% of activity time is spent in hideout
            return _history.Where(a => a.Started >= from && a.Started <= to).Sum(a => a.TotalSeconds) * 0.12;
        }

        public int GetTimeCap(ACTIVITY_TYPES type) => 3600;

        public void StopCurrentActivity()
        {
            if (_currentActivity == null) return;

            ActivityInfo finished = _currentActivity;
            finished.TotalSeconds = (int)(_currentOffset + _current.Elapsed).TotalSeconds;
            _current.Stop();
            _paused = false;
            _currentActivity = null;

            _history.Insert(0, finished); // newest first
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleCurrentActivityTag(string tagId)
        {
            if (_currentActivity == null || !_tagCatalog.TryGetValue(tagId, out TagInfo tag) || tag.IsDefault) return;

            _currentActivity.Tags = _currentActivity.Tags.Any(t => t.Id == tagId)
                ? _currentActivity.Tags.Where(t => t.Id != tagId).ToList()
                : _currentActivity.Tags.Concat(new[] { tag }).ToList();
        }

        public void TogglePause()
        {
            if (_currentActivity == null) return;
            _paused = !_paused;
            if (_paused) _current.Stop(); else _current.Start();
        }

        // Sorted by display name like the core (SELECT ... ORDER BY tag_display)
        public IReadOnlyList<TagInfo> GetTags() =>
            _tagCatalog.Values.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();

        public void CreateTag(string id, string displayName)
        {
            if (_tagCatalog.ContainsKey(id)) return;
            _tagCatalog[id] = new TagInfo
            {
                Id = id,
                Name = displayName,
                Background = ActivityPalette.BrushOf(Colors.White), // core default colors of new tags
                Foreground = ActivityPalette.BrushOf(Colors.Black),
                IsDefault = false
            };
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateTag(TagInfo changed)
        {
            if (changed == null || !_tagCatalog.TryGetValue(changed.Id, out TagInfo tag)) return;

            // The catalog instance is shared by all activities, so they show the new name right away
            tag.Name = changed.Name;
            tag.ShowInHistory = changed.ShowInHistory;
            tag.SoundEnabled = changed.SoundEnabled;
            tag.SoundId = changed.SoundId;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteTag(string id)
        {
            if (!_tagCatalog.TryGetValue(id, out TagInfo tag) || tag.IsDefault) return;

            _tagCatalog.Remove(id);
            // Like the core: activities keep the ID, but unknown tags are not shown any more
            foreach (ActivityInfo a in _history.Where(a => a.Tags.Any(t => t.Id == id)))
            {
                a.Tags = a.Tags.Where(t => t.Id != id).ToList();
            }
            if (_currentActivity != null) _currentActivity.Tags = _currentActivity.Tags.Where(t => t.Id != id).ToList();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddTag(string activityId, string tagId)
        {
            ActivityInfo act = _history.FirstOrDefault(a => a.Id == activityId);
            if (act == null || !_tagCatalog.TryGetValue(tagId, out TagInfo tag) || tag.IsDefault) return;
            if (act.Tags.Any(t => t.Id == tagId)) return;

            act.Tags = act.Tags.Concat(new[] { tag }).ToList();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveTag(string activityId, string tagId)
        {
            ActivityInfo act = _history.FirstOrDefault(a => a.Id == activityId);
            if (act == null || !_tagCatalog.TryGetValue(tagId, out TagInfo tag) || tag.IsDefault) return;

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

        // ------------------------------------------------------------------
        // Stat counters: derived from the demo history, cumulative like tx_stats
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, string> DemoStatNames = new Dictionary<string, string>
        {
            { "TotalMapsDone", "Total maps done" },
            { "TotalHeistsDone", "Total heists done" },
            { "TotalKilledCount", "Deaths" },
            { "LevelUps", "Level Ups" },
            { "SimulacrumStarted", "Simulacrum started" },
            { "LabsStarted", "Labs started" },
            { "LabsFinished", "Finished labs" },
            { "ExpeditionEncounters", "Expedition encounters" },
            { "HideoutTimeSec", "Hideout time" },
            { "AreaChanges", "AreaChanges" },
            { "Suicides", "Suicides" },
        };

        private List<(string key, StatEntry entry)> _stats;

        private List<(string key, StatEntry entry)> Stats
        {
            get
            {
                if (_stats != null) return _stats;
                var values = DemoStatNames.Keys.ToDictionary(k => k, k => 0);
                var list = new List<(string, StatEntry)>();
                void Inc(string key, DateTime t, int by = 1)
                {
                    values[key] += by;
                    list.Add((key, new StatEntry(t, values[key])));
                }

                int n = 0;
                foreach (ActivityInfo a in _history.OrderBy(a => a.Started))
                {
                    DateTime end = a.Started.AddSeconds(a.TotalSeconds);
                    Inc("AreaChanges", a.Started, 2);
                    if (a.Type == ACTIVITY_TYPES.MAP) Inc("TotalMapsDone", end);
                    if (a.Type == ACTIVITY_TYPES.HEIST) Inc("TotalHeistsDone", end);
                    if (a.Type == ACTIVITY_TYPES.SIMULACRUM) Inc("SimulacrumStarted", a.Started);
                    if (a.Type == ACTIVITY_TYPES.LABYRINTH) { Inc("LabsStarted", a.Started); Inc("LabsFinished", end); }
                    if (a.Tags.Any(t => t.Id == "expedition")) Inc("ExpeditionEncounters", end);
                    Inc("HideoutTimeSec", end.AddSeconds(30), (int)(a.TotalSeconds * 0.12));
                    if (++n % 40 == 0) Inc("LevelUps", end);
                }
                return _stats = list;
            }
        }

        public IReadOnlyList<StatInfo> GetStats(DateTime from, DateTime to)
        {
            return DemoStatNames.Select(kv =>
            {
                if (kv.Key == StatMath.DeathsKey)
                {
                    return new StatInfo { Key = kv.Key, Name = kv.Value, Value = StatMath.ActivityDeaths(_history, from, to) };
                }
                int[] inWindow = Stats.Where(s => s.key == kv.Key && s.entry.Time >= from && s.entry.Time <= to)
                                      .Select(s => s.entry.Value).ToArray();
                return new StatInfo
                {
                    Key = kv.Key,
                    Name = kv.Value,
                    Value = inWindow.Length == 0 ? 0 : StatMath.Increase(inWindow.Min(), inWindow.Max())
                };
            }).ToList();
        }

        public IReadOnlyList<StatEntry> GetStatSeries(string key, DateTime from, DateTime to)
        {
            if (key == StatMath.DeathsKey) return StatMath.ActivityDeathSeries(_history, from, to);
            var ofKey = Stats.Where(s => s.key == key).Select(s => s.entry).ToList();
            var result = ofKey.Where(e => e.Time < from).OrderBy(e => e.Time).Skip(Math.Max(0, ofKey.Count(e => e.Time < from) - 1)).ToList();
            result.AddRange(ofKey.Where(e => e.Time >= from && e.Time <= to).OrderBy(e => e.Time));
            return result;
        }

        public DateTime? OldestStatTime => Stats.Count > 0 ? Stats.Min(s => s.entry.Time) : (DateTime?)null;

        // ------------------------------------------------------------------
        // Divination cards: some maps drop one (derived from the activity, no random numbers)
        // ------------------------------------------------------------------

        private static readonly string[] DemoCards =
        {
            "Rain of Chaos", "The Lover", "Emperor's Luck", "Her Mask", "The Gambler", "Loyalty", "The Scholar",
            "Humility", "Boon of Justice", "The Union", "The Opulent", "Lucky Connections", "The Apothecary"
        };

        public IReadOnlyList<DivCardInfo> GetDivCards(DateTime from, DateTime to)
        {
            var draws = _history
                .Where(a => a.Type == ACTIVITY_TYPES.MAP && a.TotalSeconds % 3 == 0)
                // mostly the first (common) cards, now and then any card
                .Select(a => (card: DemoCards[(a.TotalSeconds / 3) % (a.TotalSeconds % 7 == 0 ? DemoCards.Length : 6)],
                              time: a.Started.AddSeconds(a.TotalSeconds / 2)))
                .Where(d => d.time >= from && d.time <= to);
            return draws.GroupBy(d => d.card)
                .Select(g => new DivCardInfo { Name = g.Key, Count = g.Count(), FirstDraw = g.Min(d => d.time), LastDraw = g.Max(d => d.time) })
                .OrderByDescending(c => c.Count).ThenBy(c => c.Name, StringComparer.Ordinal)
                .ToList();
        }

        public void Dispose() { }
    }
}
