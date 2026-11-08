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
    public sealed class LiveTrackingSource : ITrackingSource
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
        public static LiveTrackingSource TryCreate(out string reason)
        {
            reason = null;

            if (!File.Exists(TrX_Static.CONFIG_PATH) || !File.Exists(TrX_Static.DB_PATH))
            {
                reason = "No TraXile data found yet. Run the classic TraXile once to select your Client.txt — showing demo data.";
                return null;
            }

            var settings = new TrX_SettingsManager(TrX_Static.CONFIG_PATH);
            settings.LoadFromXml();

            string logPath = settings.ReadSetting("poe_logfile_path");
            if (string.IsNullOrEmpty(logPath) || !File.Exists(logPath))
            {
                reason = "Path of Exile Client.txt is not configured. Set it in the classic TraXile — showing demo data.";
                return null;
            }

            return new LiveTrackingSource(settings);
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

        public IReadOnlyList<ActivityInfo> GetHistory()
        {
            if (_logic == null) return Array.Empty<ActivityInfo>();

            // The core mutates the list on its worker thread (without locking) -> copy defensively.
            // List<T>.ToArray() never throws on concurrent writes; worst case we miss the newest entry
            // until the next refresh.
            TrX_TrackedActivity[] copy = _logic.ActivityHistory.ToArray();
            return copy.Where(a => a != null).Select(ToInfo).ToList();
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

        public void TogglePause()
        {
            TrX_TrackedActivity act = GetEffectiveActivity();
            if (act == null) return;

            if (act.ManuallyPaused) _logic.ResumeCurrentActivityOrSide();
            else _logic.PauseCurrentActivityOrSide();
        }

        private ActivityInfo ToInfo(TrX_TrackedActivity a)
        {
            var tags = new List<TagInfo>();
            foreach (string id in a.Tags.ToArray())
            {
                TrX_ActivityTag tag = _logic.GetTagByID(id);
                if (tag == null) continue;
                tags.Add(new TagInfo
                {
                    Id = tag.ID,
                    Name = tag.DisplayName,
                    Background = ToBrush(tag.BackColor),
                    Foreground = ToBrush(tag.ForeColor)
                });
            }

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
                Tags = tags
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
