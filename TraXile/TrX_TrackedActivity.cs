using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Json.Net;
using Newtonsoft.Json;

namespace TraXile
{
    public class TrX_TrackedActivity
    {
        // Start of the pause time
        private DateTime _pauseTimeStart;

        // Is paused?
        private bool _paused;

        // Sub-Activity: Zana-Map
        private TrX_TrackedActivity _zanaMap;
        public TrX_TrackedActivity SideArea_ZanaMap
        {
            get { return _zanaMap; }
            set { _zanaMap = value; }
        }

        // Sub-Activity: Logbook Side
        private TrX_TrackedActivity _logbbookSide;
        public TrX_TrackedActivity SideArea_LogbookSide
        {
            get { return _logbbookSide; }
            set { _logbbookSide = value; }
        }

        // Sub-Activity: Vaal-Sidearea
        private TrX_TrackedActivity _vaalArea;
        public TrX_TrackedActivity SideArea_VaalArea
        {
            get { return _vaalArea; }
            set { _vaalArea = value; }
        }

        // Sub-Activity: Sanctum
        private TrX_TrackedActivity _sanctumArea;
        public TrX_TrackedActivity SideArea_Sanctum
        {
            get { return _sanctumArea; }
            set { _sanctumArea = value; }
        }

        // Sub-Activity: Abyssal Dephts
        private TrX_TrackedActivity _abyssArea;
        public TrX_TrackedActivity SideArea_AbyssArea
        {
            get { return _abyssArea; }
            set { _abyssArea = value; }
        }

        // Sub-Activity: Lab trial
        private TrX_TrackedActivity _labTrial;
        public TrX_TrackedActivity SideArea_LabTrial
        {
            get { return _labTrial; }
            set { _labTrial = value; }
        }

        // Activity finished successful?
        private bool _success;
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        // Stopwatch
        private readonly Stopwatch _stopWatch;
        public TimeSpan StopWatchTimeSpan => _stopWatch.Elapsed;

        // Custom stopwatch value
        private string _customStopWatchValue;
        public string CustomStopWatchValue
        {
            get { return _customStopWatchValue; }
            set { _customStopWatchValue = value; }
        }

        // Stopwatch Value
        public string StopWatchValue
        {
            get
            {
                if (CustomStopWatchValue != null)
                {
                    return _customStopWatchValue;
                }
                else
                {
                    TimeSpan ts = _stopWatch.Elapsed;
                    return String.Format("{0:00}:{1:00}:{2:00}",
                        ts.Hours, ts.Minutes, ts.Seconds);
                }
            }
        }

        // Total Seconds
        private int _totalSeconds;
        public int TotalSeconds
        {
            get { return _totalSeconds; }
            set { _totalSeconds = value; }
        }

       

        // Activity Type
        private ACTIVITY_TYPES _activityType;
        public ACTIVITY_TYPES Type
        {
            get { return _activityType; }
            set { _activityType = value; }
        }

        // Name of the current area
        private string _areaName;
        public string Area
        {
            get { return _areaName.Replace("'", ""); }
            set { _areaName = value; }
        }

        // Original area name (if overwritten)
        private string _origAreaName = "";
        public string AreaOrig
        {
            get { return _origAreaName.Replace("'", ""); }
            set { _origAreaName = value; }
        }

        // Is finished?
        private bool _finished;
        public bool IsFinished
        {
            get { return _finished; }
            set { _finished = value; }
        }

        private int _trialCount;
        public int TrialCount
        {
            get { return _trialCount; }
            set { _trialCount = value; }
        }

        // Is Zana-Map?
        private bool _isZana;
        public bool IsZana
        {
            get { return _isZana; }
            set { _isZana = value; }
        }

        // Time activity was paused in seconds
        private double _pausedTime;
        public double PausedTime
        {
            get { return _pausedTime; }
            set { _pausedTime = value; }
        }

        // Number of Deaths
        private int _deathCounter;
        public int DeathCounter
        {
            get { return _deathCounter; }
            set { _deathCounter = value; }
        }

        // Area level
        private int _areaLevel;
        public int AreaLevel
        {
            get { return _areaLevel; }
            set { _areaLevel = value; }
        }

        // Area Seed
        private long _areaSeed;
        public long AreaSeed
        {
            get { return _areaSeed; }
            set { _areaSeed = value; }
        }

        // Number of portals used
        private int _portalsUsed;
        public int PortalsUsed
        {
            get { return _portalsUsed; }
            set { _portalsUsed = value; }
        }

        // Tier of thet current area
        public int MapTier
        {
            get
            {
                switch (_areaLevel)
                {
                    case 68:
                        return 1;
                    case 69:
                        return 2;
                    case 70:
                        return 3;
                    case 71:
                        return 4;
                    case 72:
                        return 5;
                    case 73:
                        return 6;
                    case 74:
                        return 7;
                    case 75:
                        return 8;
                    case 76:
                        return 9;
                    case 77:
                        return 10;
                    case 78:
                        return 11;
                    case 79:
                        return 12;
                    case 80:
                        return 13;
                    case 81:
                        return 14;
                    case 82:
                        return 15;
                    case 83:
                        return 16;
                    case 84:
                        return 17;
                    case 85:
                        return 18;
                    case 86:
                        return 19;
                    case 87:
                        return 20;
                    default:
                        return 0;
                }
            }
        }

        // Unique ID of this activity
        public string UniqueID
        {
            get { return string.Format("{0}_{1}", _activityTimeStamp, _areaName.Replace(" ", "_")); }
        }

        // Is the activity manually paused?
        private bool _isManuallyPaused;
        public bool ManuallyPaused
        {
            get { return _isManuallyPaused; }
            set { _isManuallyPaused = value; }
        }

        // Instance Endpoint of this activity
        private string _instanceEndpoint;
        public string InstanceEndpoint
        {
            get { return _instanceEndpoint; }
            set { _instanceEndpoint = value; }
        }

        // Time the activity has been started
        private DateTime _startTime;
        public DateTime Started
        {
            get { return _startTime; }
            set 
            { 
                _startTime = value;
                _lastTimeEntered = value;
            }
        }

        // Time the activity was last ended
        private DateTime _lastEndTime;
        public DateTime LastEnded
        {
            get { return _lastEndTime; }
            set { _lastEndTime = value; }
        }

        // Count of Trialmaster Rounds
        private int _trialMasterCount;
        public int TrialMasterCount
        {
            get
            {
                return _trialMasterCount;
            }

            set { _trialMasterCount = value; }
        }

        // Trial master finished successful`?
        private bool _trialMasterSuccess;
        public bool TrialMasterSuccess
        {
            get { return _trialMasterSuccess; }
            set { _trialMasterSuccess = value; }
        }

        // All Trialmaster rounds finished?
        private bool _trialMasterFull;
        public bool TrialMasterFullFinished
        {
            get { return _trialMasterFull; }
            set { _trialMasterFull = value; }
        }

        // Unix timesatmp of activity start
        private long _activityTimeStamp;
        public long TimeStamp
        {
            get { return _activityTimeStamp; }
            set { _activityTimeStamp = value; }
        }

        // List of tags for this activity
        private readonly List<string> _tagIDs;
        public List<string> Tags => _tagIDs;

        // Rouge Count
        private int _rougeCount;
        public int RougeCont
        {
            get { return _rougeCount; }
            set { _rougeCount = value; }
        }

        // number of pauses
        private int _pauseCount;
        public int PauseCount => _pauseCount;

        // Last time entered
        private DateTime _lastTimeEntered;
        public DateTime LastTimeEntered
        {
            get {  return _lastTimeEntered; }
            set { _lastTimeEntered = value; }
        }

        private bool _queuedForAPI;
        public bool QueuedForAPISync
        {
            get { return _queuedForAPI; }
            set { _queuedForAPI = value; }
        }

        // Has been synced to API?
        private bool _syncedToAPI;
        public bool SyncedToAPI
        {
            get { return _syncedToAPI; }
            set { _syncedToAPI = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TrX_TrackedActivity()
        {
            _stopWatch = new Stopwatch();
            _zanaMap = null;
            _vaalArea = null;
            _logbbookSide = null;
            _tagIDs = new List<string>();
            _rougeCount = 0;
        }

        /// <summary>
        /// Get stopwatch value after caps are applied
        /// </summary>
        /// <param name="cap"></param>
        /// <returns></returns>
        public string GetCappedStopwatchValue(int cap)
        {
            TimeSpan ts;
            string s;
            ts = TimeSpan.FromSeconds(_totalSeconds > cap ? cap : _totalSeconds);
            s = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            if(_totalSeconds > cap)
            {
                s += " (cap)";
            }
            return s;
        }
        
        /// <summary>
        ///  Check if this activity has a specific tag
        /// </summary>
        /// <param name="s_id"></param>
        /// <returns></returns>
        public bool HasTag(string s_id)
        {
            foreach (string tag in _tagIDs)
            {
                if (tag == s_id)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Start pause time
        /// </summary>
        /// <param name="dt"></param>
        public void StartPauseTime(DateTime dt)
        {
            if (!_paused)
            {
                _paused = true;
                _pauseTimeStart = dt;
            }
        }

        /// <summary>
        /// End and calculate pause time
        /// </summary>
        /// <param name="dt"></param>
        public void EndPauseTime(DateTime dt)
        {
            if (_paused)
            {
                _paused = false;
                _pauseCount++;
                _pausedTime += (dt - _pauseTimeStart).TotalSeconds;
            }
            _paused = false;
        }

        /// <summary>
        /// Remove a given tag from this activity
        /// </summary>
        /// <param name="s_id"></param>
        public void RemoveTag(string s_id)
        {
            if (HasTag(s_id))
                _tagIDs.Remove(s_id);
        }

        /// <summary>
        /// Add a specific tag to this activity
        /// </summary>
        /// <param name="s_id"></param>
        public void AddTag(string s_id)
        {
            if (!_tagIDs.Contains(s_id))
            {
                _tagIDs.Add(s_id);
            }
        }

        /// <summary>
        /// Return activity as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "{0} -> UID: {1}, " +
                "Type: {2}, " +
                "Timestamp: {3, " +
                "Area: {4}, " +
                "AreaLevel: {5}, " +
                "PausedTime: {6}, " +
                "PausedCount: {7}, " +
                "Instance: {8}, " +
                "Started: {9}," +
                "LastEnded: {10}",
                "StopWatchValue: {11}",
                "CustomStopWatchValue: {12}",
                this.GetType(),
                _activityType,
                _activityTimeStamp,
                _areaName,
                _areaLevel,
                PausedTime,
                PauseCount,
                _instanceEndpoint,
                _startTime,
                _lastEndTime,
                StopWatchValue,
                CustomStopWatchValue
                );
        }

        /// <summary>
        /// Pause this activity
        /// </summary>
        public void Pause()
        {
            _isManuallyPaused = true;
            if (_stopWatch.IsRunning)
                _stopWatch.Stop();
        }

        /// <summary>
        /// Resume this activity
        /// </summary>
        public void Resume()
        {
            _isManuallyPaused = false;
            if (!_stopWatch.IsRunning)
                _stopWatch.Start();
        }

        /// <summary>
        /// Start the Stopwatch
        /// </summary>
        public void StartStopWatch()
        {
            _stopWatch.Start();
        }

        /// <summary>
        /// Stop the stopwatch
        /// </summary>
        public void StopStopWatch()
        {
            _stopWatch.Stop();
        }

        /// <summary>
        /// GetCSV Headline
        /// </summary>
        /// <returns></returns>
        public static string GetCSVHeadline()
        {
            return "time; type; area; area_level; stopwatch (seconds); death_counter; tags";
        }

        /// <summary>
        /// Get Activity Data as CSV
        /// </summary>
        /// <returns></returns>
        public string ToCSVLine()
        {
            string output;
            string tags;
            output = "{0};{1};{2};{3};{4};{5};{6}";
            tags = "";

            for(int i = 0; i < _tagIDs.Count; i++)
            {
                tags += _tagIDs[i];
                if(i < (_tagIDs.Count-1))
                {
                    tags += ",";
                }
            }

            return string.Format(output, _startTime, _activityType, _areaName, _areaLevel, _totalSeconds, _deathCounter, tags);
        }

        /// <summary>
        /// Get all sub activities if any
        /// </summary>
        /// <returns></returns>
        public List<TrX_TrackedActivity> GetSubActivities()
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            if(_logbbookSide != null)
            {
                results.Add(_logbbookSide);
            }

            if(_zanaMap != null)
            {
                results.Add(_zanaMap);
            }

            if(_labTrial != null)
            {
                results.Add(_labTrial);
            }

            if(_vaalArea != null)
            {
                results.Add(_vaalArea);
            }

            if(_abyssArea != null)
            {
                results.Add(_abyssArea);
            }

            if (_sanctumArea != null)
            {
                results.Add(_sanctumArea);
            }

            return results;
        }

        public TrX_BackendSync_ActivityDocument GetSerializableObject(string client_id = "")
        {
            TrX_BackendSync_ActivityDocument serializable = new TrX_BackendSync_ActivityDocument();
            serializable.AreaName = _areaName;
            serializable.AreaLevel = _areaLevel;
            serializable.DurationSec = _totalSeconds;
            serializable.Tags = _tagIDs;
            serializable.ActivityType = _activityType.ToString();
            serializable.Identifier = UniqueID;
            serializable.DeathCounter = _deathCounter;
            serializable.StartTime = _startTime;
            serializable.ClientIdentifier = client_id;

            return serializable;
        }

        public string ToJSON(string client_id = "", Formatting fmt = Formatting.None)
        {
            TrX_BackendSync_ActivityDocument serializable = GetSerializableObject(client_id);
            return JsonConvert.SerializeObject(serializable, fmt);
        }


      
    }
}
