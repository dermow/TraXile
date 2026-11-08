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
    public sealed class LiveTrackingSource : ITrackingSource, ICoreMaintenance
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
            _logic.Start();
        }

        private void RaiseHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);

        public bool IsLoading => _logic != null && !_logic.EventQueueInitialized;

        public double LoadProgress =>
            _logic == null || _logic.LogLinesTotal <= 0 ? 0 : Math.Min(1.0, _logic.LogLinesRead / _logic.LogLinesTotal);

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
            _logic.Database.DoNonQuery("UPDATE tx_kvstore SET value = '0' WHERE key = 'last_hash'");
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

        private static TagInfo ToTagInfo(TrX_ActivityTag tag) => new TagInfo
        {
            Id = tag.ID,
            Name = tag.DisplayName,
            Background = ToBrush(tag.BackColor),
            Foreground = ToBrush(tag.ForeColor),
            IsDefault = tag.IsDefault
        };

        private ActivityInfo ToInfo(TrX_TrackedActivity a) => ToInfo(a, includeSubs: true);

        private ActivityInfo ToInfo(TrX_TrackedActivity a, bool includeSubs)
        {
            var tags = new List<TagInfo>();
            foreach (string id in a.Tags.ToArray())
            {
                TrX_ActivityTag tag = _logic.GetTagByID(id);
                if (tag == null) continue;
                tags.Add(ToTagInfo(tag));
            }

            IReadOnlyList<ActivityInfo> subs = includeSubs
                ? a.GetSubActivities().Where(s => s != null).Select(s => ToInfo(s, includeSubs: false)).ToList()
                : (IReadOnlyList<ActivityInfo>)Array.Empty<ActivityInfo>();

            return new ActivityInfo
            {
                Id = a.UniqueID,
                Type = a.Type,
                Area = a.Area,
                AreaLevel = a.AreaLevel,
                MapTier = a.MapTier,
                Started = a.Started,
                TotalSeconds = a.TotalSeconds,
                Deaths = a.DeathCounter,
                Portals = a.PortalsUsed,
                PausedSeconds = a.PausedTime,
                Tags = tags,
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
