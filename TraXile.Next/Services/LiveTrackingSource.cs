using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Adapter around the existing TraXile core (<see cref="TrX_CoreLogic"/>).
    /// The core parses Client.txt on worker threads and owns the SQLite database;
    /// this class only converts its state into immutable snapshots for the UI.
    /// </summary>
    public sealed class LiveTrackingSource : ITrackingSource, ICoreMaintenance, IStatsSource, IDivCardSource
    {
        private readonly TrX_SettingsManager _settings;
        private readonly Dictionary<ACTIVITY_TYPES, int> _timeCaps = new Dictionary<ACTIVITY_TYPES, int>();
        private TrX_CoreLogic _logic;

        public bool IsDemo => false;
        public string Notice => null;
        public event EventHandler HistoryChanged;

        private LiveTrackingSource(TrX_SettingsManager settings)
        {
            _settings = settings;

            foreach (ACTIVITY_TYPES t in (ACTIVITY_TYPES[])Enum.GetValues(typeof(ACTIVITY_TYPES)))
            {
                string key = $"TimeCap{TrX_Helpers.CapitalFirstLetter(t.ToString())}";
                _timeCaps[t] = int.TryParse(_settings.ReadSetting(key, "3600"), out int cap) ? cap : 3600;
            }
        }

        /// <summary>
        /// Try to create a live source. Returns null and a reason if live tracking is not possible.
        /// </summary>
        public static LiveTrackingSource TryCreate(AppSetup setup, out string reason)
        {
            reason = null;

            if (!setup.HasValidClientTxt)
            {
                reason = "No Path of Exile Client.txt selected — showing demo data.";
                return null;
            }

            // The core creates data.db on its first start
            return new LiveTrackingSource(setup.LoadSettings());
        }

        public void Start()
        {
            Directory.CreateDirectory(TrX_Static.APPDATA_PATH);
            Directory.CreateDirectory(TrX_Static.METADATA_PATH);

            int minCap = int.TryParse(_settings.ReadSetting("TimeCapMinimum", "10"), out int m) ? m : 10;

            _logic = new TrX_CoreLogic(minCap)
            {
                ClientTxtPath = _settings.ReadSetting("poe_logfile_path")
            };

            _logic.OnHistoryInitialized += e => RaiseHistoryChanged();
            _logic.OnActivityFinished += e => RaiseHistoryChanged();
            _logic.OnActivityStarted += e => RaiseHistoryChanged();
            _logic.OnTagsUpdated += e => RaiseHistoryChanged();
            _logic.OnDivCardDrawn += e => DivCardsChanged?.Invoke(this, EventArgs.Empty);
            _logic.Start();
        }

        private void RaiseHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler DivCardsChanged;

        public bool IsLoading => _logic != null && !_logic.EventQueueInitialized;

        public double LoadProgress =>
            _logic == null || _logic.LogBytesTotal <= 0 ? 0 : Math.Min(1.0, _logic.LogBytesRead / _logic.LogBytesTotal);

        public IReadOnlyList<ActivityInfo> GetHistory()
        {
            if (_logic == null) return Array.Empty<ActivityInfo>();

            // The core mutates the list on its worker thread (without locking) -> copy defensively.
            // List<T>.ToArray() never throws on concurrent writes; worst case we miss the newest entry
            // until the next refresh.
            TrX_TrackedActivity[] copy = _logic.ActivityHistory.ToArray();
            lock (_deletedIds)
            {
                return copy.Where(a => a != null && !_deletedIds.Contains(a.UniqueID)).Select(ToInfo).ToList();
            }
        }

        // ------------------------------------------------------------------
        // Editing (tags, delete)
        // ------------------------------------------------------------------

        // Deleted activities are removed from the DB right away, but only hidden from the core's
        // in-memory list: that list is written by the parser thread without locking, so removing
        // entries from the UI thread could corrupt it. After a restart they are gone for good.
        private readonly HashSet<string> _deletedIds = new HashSet<string>();

        private TrX_TrackedActivity FindActivity(string id)
        {
            if (_logic == null || id == null) return null;
            return _logic.ActivityHistory.ToArray().FirstOrDefault(a => a != null && a.UniqueID == id);
        }

        public IReadOnlyList<TagInfo> GetTags()
        {
            if (_logic == null) return Array.Empty<TagInfo>();
            return _logic.Tags.ToArray().Where(t => t != null).Select(ToTagInfo).ToList();
        }

        /// <summary>SQL string literal content (the classic app did not escape, so names with ' broke it).</summary>
        private static string Sql(string s) => (s ?? "").Replace("'", "''");

        // The core keeps its tag list in memory and in tx_tags; same statements as the classic Main.AddTag/UpdateTag/DeleteTag.

        public void CreateTag(string id, string displayName)
        {
            if (_logic == null || string.IsNullOrEmpty(id) || _logic.GetTagByID(id) != null) return;

            var tag = new TrX_ActivityTag(id, false) { DisplayName = displayName };
            _logic.Database.DoNonQuery("INSERT INTO tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type, tag_show_in_lv) VALUES "
                + $"('{Sql(tag.ID)}', '{Sql(tag.DisplayName)}', '{tag.BackColor.ToArgb()}', '{tag.ForeColor.ToArgb()}', 'custom', {(tag.ShowInListView ? 1 : 0)})");
            _logic.Tags.Add(tag);
            RaiseHistoryChanged();
        }

        public void UpdateTag(TagInfo changed)
        {
            TrX_ActivityTag tag = changed == null ? null : _logic?.GetTagByID(changed.Id);
            if (tag == null) return;

            tag.DisplayName = changed.Name;
            tag.ShowInListView = changed.ShowInHistory;
            tag.SoundEnabled = changed.SoundEnabled;
            tag.SoundID = TagSoundLibrary.IsSet(changed.SoundId) ? changed.SoundId : TagSoundLibrary.NoSound;
            _logic.Database.DoNonQuery($"UPDATE tx_tags SET tag_display = '{Sql(tag.DisplayName)}', " +
                $"tag_show_in_lv = {(tag.ShowInListView ? 1 : 0)}, audio_on = {(tag.SoundEnabled ? 1 : 0)}, " +
                $"audio_file_name = '{Sql(tag.SoundID)}' WHERE tag_id = '{Sql(tag.ID)}'");
            RaiseHistoryChanged();
        }

        public void DeleteTag(string id)
        {
            TrX_ActivityTag tag = _logic?.GetTagByID(id);
            if (tag == null || tag.IsDefault) return;

            _logic.Tags.Remove(tag);
            _logic.Database.DoNonQuery($"DELETE FROM tx_tags WHERE tag_id = '{Sql(id)}' AND tag_type != 'default'");
            RaiseHistoryChanged();
        }

        public void AddTag(string activityId, string tagId)
        {
            TrX_TrackedActivity act = FindActivity(activityId);
            TrX_ActivityTag tag = _logic?.GetTagByID(tagId);
            if (act == null || tag == null || tag.IsDefault || act.HasTag(tagId)) return;

            _logic.AddTagAutoCreate(tagId, act);
            RaiseHistoryChanged();
        }

        public void RemoveTag(string activityId, string tagId)
        {
            TrX_TrackedActivity act = FindActivity(activityId);
            if (act == null) return;

            _logic.RemoveTagFromActivity(tagId, act);
            RaiseHistoryChanged();
        }

        public void DeleteActivities(IReadOnlyCollection<string> activityIds)
        {
            if (_logic == null || activityIds == null || activityIds.Count == 0) return;

            var ids = new HashSet<string>(activityIds);
            TrX_TrackedActivity[] victims = _logic.ActivityHistory.ToArray()
                .Where(a => a != null && ids.Contains(a.UniqueID)).ToArray();

            foreach (TrX_TrackedActivity act in victims)
            {
                _logic.Database.DoNonQuery($"DELETE FROM tx_activity_log WHERE timestamp = {act.TimeStamp}");
            }

            lock (_deletedIds)
            {
                foreach (TrX_TrackedActivity act in victims) _deletedIds.Add(act.UniqueID);
            }
            RaiseHistoryChanged();
        }

        public LiveState GetLiveState()
        {
            var state = new LiveState();
            if (_logic == null) return state;

            state.CurrentArea = string.IsNullOrEmpty(_logic.CurrentArea) || _logic.CurrentArea == "None"
                ? "—" : _logic.CurrentArea;

            TrX_TrackedActivity act = GetEffectiveActivity();
            if (act != null)
            {
                state.IsTracking = true;
                state.IsPaused = act.ManuallyPaused;
                state.Activity = ToInfo(act);
                state.Elapsed = act.StopWatchTimeSpan;
                state.ActiveTags = state.Activity.Tags;

                TrX_TrackedActivity prev = _logic.OverlayPrevActivity;
                if (prev != null) state.PreviousActivity = ToInfo(prev, includeSubs: false);
            }
            return state;
        }

        /// <summary>
        /// Same logic as the classic UI: a running side area (Vaal, Zana, Abyss, ...) takes precedence.
        /// </summary>
        private TrX_TrackedActivity GetEffectiveActivity()
        {
            TrX_TrackedActivity cur = _logic.CurrentActivity;
            if (cur == null) return null;

            if (_logic.IsBlackKnightFight && cur.SideArea_BlackKnight != null) return cur.SideArea_BlackKnight;
            if (_logic.IsMapZana && cur.SideArea_ZanaMap != null) return cur.SideArea_ZanaMap;
            if (_logic.IsMapVaalArea && cur.SideArea_VaalArea != null) return cur.SideArea_VaalArea;
            if (_logic.IsMapAbyssArea && cur.SideArea_AbyssArea != null) return cur.SideArea_AbyssArea;
            if (_logic.IsMapLabTrial && cur.SideArea_LabTrial != null) return cur.SideArea_LabTrial;
            if (_logic.IsMapSanctum && cur.SideArea_Sanctum != null) return cur.SideArea_Sanctum;
            if (_logic.IsMapSafeHouseSideArea && cur.SideArea_SafeHouse != null) return cur.SideArea_SafeHouse;
            return cur;
        }

        public double GetHideoutSeconds(DateTime from, DateTime to)
        {
            if (_logic == null) return 0;
            long ts1 = ((DateTimeOffset)from).ToUnixTimeSeconds();
            long ts2 = ((DateTimeOffset)to).ToUnixTimeSeconds();
            return _logic.Stats.GetIncrementValue("HideoutTimeSec", ts1, ts2);
        }

        // ------------------------------------------------------------------
        // Stat counters (tx_stats)
        // ------------------------------------------------------------------

        private static long Unix(DateTime t) => ((DateTimeOffset)t).ToUnixTimeSeconds();

        public IReadOnlyList<StatInfo> GetStats(DateTime from, DateTime to)
        {
            if (_logic == null) return Array.Empty<StatInfo>();

            // One query for all stats; same min/max rule as TrX_StatsManager.GetIncrementValue
            var increase = new Dictionary<string, int>();
            Query($"SELECT stat_name, min(stat_value), max(stat_value) FROM tx_stats WHERE timestamp BETWEEN {Unix(from)} AND {Unix(to)} GROUP BY stat_name",
                r =>
                {
                    if (r.IsDBNull(0) || r.IsDBNull(1) || r.IsDBNull(2)) return;
                    increase[r.GetString(0)] = StatMath.Increase(r.GetInt32(1), r.GetInt32(2));
                });

            string[] keys;
            try { keys = _logic.Stats.NumericStats.Keys.ToArray(); }
            catch (InvalidOperationException) { keys = increase.Keys.ToArray(); } // changed by the parser meanwhile

            Dictionary<string, string> names = _logic.StatNamesLong;
            return keys.Union(increase.Keys)
                .Select(k => new StatInfo
                {
                    Key = k,
                    Name = names != null && names.TryGetValue(k, out string n) ? n : k,
                    Value = k == StatMath.DeathsKey
                        ? StatMath.ActivityDeaths(DeathHistory(), from, to)
                        : increase.TryGetValue(k, out int v) ? v : 0
                })
                .ToList();
        }

        /// <summary>
        /// Start and death counter of the Activities with deaths: all the "Deaths" Statistic needs, without the cost
        /// of converting the whole history (<see cref="GetHistory"/>).
        /// </summary>
        private List<ActivityInfo> DeathHistory()
        {
            TrX_TrackedActivity[] copy = _logic.ActivityHistory.ToArray();
            lock (_deletedIds)
            {
                return copy.Where(a => a != null && a.DeathCounter > 0 && !_deletedIds.Contains(a.UniqueID))
                    .Select(a => new ActivityInfo { Started = a.Started, Deaths = a.DeathCounter })
                    .ToList();
            }
        }

        public IReadOnlyList<StatEntry> GetStatSeries(string key, DateTime from, DateTime to)
        {
            var result = new List<StatEntry>();
            if (_logic == null || string.IsNullOrEmpty(key)) return result;
            if (key == StatMath.DeathsKey) return StatMath.ActivityDeathSeries(DeathHistory(), from, to);

            string name = Sql(key);
            Query($"SELECT timestamp, stat_value FROM tx_stats WHERE stat_name = '{name}' AND timestamp < {Unix(from)} ORDER BY timestamp DESC LIMIT 1",
                r => result.Add(Entry(r.GetInt64(0), r.GetInt32(1))));
            Query($"SELECT timestamp, stat_value FROM tx_stats WHERE stat_name = '{name}' AND timestamp BETWEEN {Unix(from)} AND {Unix(to)} ORDER BY timestamp ASC",
                r => result.Add(Entry(r.GetInt64(0), r.GetInt32(1))));
            return result;
        }

        public IReadOnlyList<DivCardInfo> GetDivCards(DateTime from, DateTime to)
        {
            var result = new List<DivCardInfo>();
            // One query instead of the classic count + first + last query per card
            Query($"SELECT divcard, COUNT(*), MIN(timestamp), MAX(timestamp) FROM tx_divcards WHERE timestamp >= {Unix(from)} AND timestamp <= {Unix(to)} " +
                  "GROUP BY divcard ORDER BY COUNT(*) DESC, divcard ASC",
                r => result.Add(new DivCardInfo
                {
                    Name = r.GetString(0),
                    Count = r.GetInt32(1),
                    FirstDraw = DateTimeOffset.FromUnixTimeSeconds(r.GetInt64(2)).LocalDateTime,
                    LastDraw = DateTimeOffset.FromUnixTimeSeconds(r.GetInt64(3)).LocalDateTime
                }));
            return result;
        }

        /// <summary>Runs a query on the core's connection; the core logs errors and returns no reader then.</summary>
        private void Query(string sql, Action<Microsoft.Data.Sqlite.SqliteDataReader> row)
        {
            using (var reader = _logic?.Database.GetSQLReader(sql))
            {
                if (reader == null) return;
                while (reader.Read()) row(reader);
            }
        }

        private static StatEntry Entry(long unix, int value) =>
            new StatEntry(DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime, value);

        public DateTime? OldestStatTime
        {
            get
            {
                if (_logic == null) return null;
                long ts = _logic.Stats.GetOldestTimeStamp();
                return ts > 0 ? DateTimeOffset.FromUnixTimeSeconds(ts).LocalDateTime : (DateTime?)null;
            }
        }

        public int GetTimeCap(ACTIVITY_TYPES type) => _timeCaps.TryGetValue(type, out int c) ? c : 3600;

        public void SetTimeCap(ACTIVITY_TYPES type, int seconds)
        {
            _timeCaps[type] = seconds;
            RaiseHistoryChanged(); // statistics use the caps
        }

        /// <summary>
        /// Stops the core and empties the parsed history, so the next start reads Client.txt from the beginning
        /// (classic: InitLogFileReload). Tags and pause times stored with activities are lost.
        /// </summary>
        public void StopAndClearHistory()
        {
            if (_logic == null) return;
            _logic.Stop();
            _logic.Database.DoNonQuery("DELETE FROM tx_activity_log");
            _logic.Database.DoNonQuery("DELETE FROM tx_stats");
            _logic.Database.DoNonQuery("UPDATE tx_kvstore SET value = '0' WHERE key IN ('last_hash', 'last_hash_offset')");
            _logic.Database.Close();
            _logic = null;
        }

        public void TogglePause()
        {
            TrX_TrackedActivity act = GetEffectiveActivity();
            if (act == null) return;

            if (act.ManuallyPaused) _logic.ResumeCurrentActivityOrSide();
            else _logic.PauseCurrentActivityOrSide();
        }

        public void StopCurrentActivity()
        {
            // While the log is read for the first time, the "current" activity is a historic one
            if (_logic == null || IsLoading) return;

            TrX_TrackedActivity act = _logic.CurrentActivity;
            if (act == null) return;

            // Same call as the classic Stop button: no next activity, side areas are finished with it
            _logic.FinishActivity(act, null, ACTIVITY_TYPES.MAP, new DateTime());

            // Activities below the minimum time raise no event, but the live state changed anyway
            RaiseHistoryChanged();
        }

        public void ToggleCurrentActivityTag(string tagId)
        {
            if (_logic == null || IsLoading) return;

            // The activity shown on the live card, so a running side area (Vaal, Zana, ...) gets the tag.
            // The classic app only tagged the main activity or the Zana map.
            TrX_TrackedActivity act = GetEffectiveActivity();
            TrX_ActivityTag tag = _logic.GetTagByID(tagId);
            if (act == null || tag == null || tag.IsDefault) return;

            // Not in the database yet: FinishActivity saves the activity with its tags
            if (act.HasTag(tagId)) act.RemoveTag(tagId);
            else act.AddTag(tagId);
        }

        private static TagInfo ToTagInfo(TrX_ActivityTag tag) => new TagInfo
        {
            Id = tag.ID,
            Name = tag.DisplayName,
            Background = ToBrush(tag.BackColor),
            Foreground = ToBrush(tag.ForeColor),
            IsDefault = tag.IsDefault,
            ShowInHistory = tag.ShowInListView,
            SoundEnabled = tag.SoundEnabled,
            SoundId = tag.SoundID
        };

        private ActivityInfo ToInfo(TrX_TrackedActivity a) => ToInfo(a, includeSubs: true);

        private ActivityInfo ToInfo(TrX_TrackedActivity a, bool includeSubs) => Convert(a, _logic.GetTagByID, includeSubs);

        /// <summary>Snapshot of a core activity; tags unknown to <paramref name="tagById"/> are only kept in <see cref="ActivityInfo.TagIds"/>.</summary>
        public static ActivityInfo Convert(TrX_TrackedActivity a, Func<string, TrX_ActivityTag> tagById, bool includeSubs = true)
        {
            var tags = new List<TagInfo>();
            string[] tagIds = a.Tags.ToArray();
            foreach (string id in tagIds)
            {
                TrX_ActivityTag tag = tagById(id);
                if (tag == null) continue;
                tags.Add(ToTagInfo(tag));
            }

            IReadOnlyList<ActivityInfo> subs = includeSubs
                ? a.GetSubActivities().Where(s => s != null).Select(s => Convert(s, tagById, includeSubs: false)).ToList()
                : (IReadOnlyList<ActivityInfo>)Array.Empty<ActivityInfo>();

            return new ActivityInfo
            {
                Id = a.UniqueID,
                Type = a.Type,
                Area = a.Area,
                AreaRaw = a.AreaRaw,
                AreaLevel = a.AreaLevel,
                MapTier = a.MapTier,
                Started = a.Started,
                TotalSeconds = a.TotalSeconds,
                Deaths = a.DeathCounter,
                Portals = a.PortalsUsed,
                PausedSeconds = a.PausedTime,
                Tags = tags,
                TagIds = tagIds,
                SubActivities = subs
            };
        }

        private static Brush ToBrush(System.Drawing.Color c) =>
            ActivityPalette.BrushOf(Color.FromArgb(c.A, c.R, c.G, c.B));

        public void Dispose()
        {
            try { _logic?.Stop(); } catch { /* shutting down */ }
        }
    }
}
