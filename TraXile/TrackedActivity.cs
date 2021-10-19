using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TraXile
{
    public class TrackedActivity
    {
        private readonly Stopwatch _stopWatch;
        private TrackedActivity _zanaMap;
        private List<string> _tagIDs;
        private DateTime _startTime, _lastEndTime, _pauseTimeStart, _pauseTimeEnd;
        private string _instanceEndpoint;
        private string _areaName;
        private ACTIVITY_TYPES _activityType;
        private bool _isZana;
        private int _pauseCount;
        private bool _isManuallyPaused;
        private bool _trialMasterSuccess;
        private bool _trialMasterFull;
        private bool _finished;
        private bool _success;
        private int _trialMasterCount;
        private int _deathCounter;
        private int _areaLevel;
        private int _portalsUsed;
        private int _trialCount;
        private double _pausedTime;
        private bool _paused;
        private int _totalSeconds;
        private long _activityTimeStamp;
        private string _customStopWatchValue;

        public TrackedActivity()
        {
            _stopWatch = new Stopwatch();
            _zanaMap = null;
            _tagIDs = new List<string>();
        }

        public bool HasTag(string s_id)
        {
            foreach(string tag in _tagIDs)
            {
                if (tag == s_id)
                    return true;
            }
            return false;
        }

        public void StartPauseTime(DateTime dt)
        {
            if(!_paused)
            {
                _paused = true;
                _pauseTimeStart = dt;
            }
        }

        public void EndPauseTime(DateTime dt)
        {
            if(_paused)
            {
                _paused = false;
                _pauseTimeEnd = dt;
                _pauseCount++;
                _pausedTime += (dt - _pauseTimeStart).TotalSeconds;
            }
            _paused = false;
        }

        public void RemoveTag(string s_id)
        {
            if(HasTag(s_id))
               _tagIDs.Remove(s_id);
        }

        public void AddTag(string s_id)
        {
            if(!_tagIDs.Contains(s_id))
            {
                _tagIDs.Add(s_id);
            }
        }

        public int PauseCount
        {
            get { return _pauseCount; }
        }

        public double PausedTime
        {
            get { return _pausedTime; }
            set { _pausedTime = value; }
        }

        public void Pause()
        {
              _isManuallyPaused = true;
              if(_stopWatch.IsRunning)
                _stopWatch.Stop();
        }

        public void Resume()
        {
            _isManuallyPaused = false;
            if(!_stopWatch.IsRunning)
                 _stopWatch.Start();
        }

        public void StartStopWatch()
        {
            _stopWatch.Start();
        }

        public void StopStopWatch()
        {
            _stopWatch.Stop();
        }

        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }


        public TimeSpan StopWatchTimeSpan
        {
            get { return _stopWatch.Elapsed; }
        }

        public string CustomStopWatchValue
        {
            get { return _customStopWatchValue; }
            set { _customStopWatchValue = value; }
        }

        public string StopWatchValue
        {
            get
            {
                if(CustomStopWatchValue != null)
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

        public int TotalSeconds
        {
            get { return _totalSeconds; }
            set { _totalSeconds = value; }
        }

        public int TrialCount
        {
            get { return _trialCount; }
            set { _trialCount = value; }
        }

        public ACTIVITY_TYPES Type
        {
            get { return _activityType; }
            set { _activityType = value; }
        }
        public string Area
        {
            get { return _areaName.Replace("'", ""); }
            set { _areaName = value; }
        }

        public bool IsFinished
        {
            get { return _finished; }
            set { _finished = value; }
        }

        public bool IsZana
        {
            get { return _isZana; }
            set { _isZana = value; }
        }

        public int DeathCounter
        {
            get { return _deathCounter; }
            set { _deathCounter = value; }
        }

        public int AreaLevel
        {
            get { return _areaLevel; }
            set { _areaLevel = value; }
        }

        public int PortalsUsed
        {
            get { return _portalsUsed; }
            set { _portalsUsed = value; }
        }


        public int MapTier
        {
            get
            {
                switch(_areaLevel)
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
                    default:
                        return 0;
                }
            }
        }

        public TrackedActivity ZanaMap
        {
            get { return _zanaMap; }
            set { _zanaMap = value; }
        }

        public bool ManuallyPaused
        {
            get { return _isManuallyPaused; }
            set { _isManuallyPaused = value; }
        }

        public string InstanceEndpoint
        {
            get { return _instanceEndpoint; }
            set { _instanceEndpoint = value; }
        }

        public DateTime Started
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime LastEnded
        {
            get { return _lastEndTime; }
            set { _lastEndTime = value; }
        }

        public int TrialMasterCount
        {
            get
            {
                return _trialMasterCount;
            }

            set { _trialMasterCount = value; }
        }

        public bool TrialMasterSuccess
        {
            get { return _trialMasterSuccess; }
            set { _trialMasterSuccess = value; }
        }

        public bool TrialMasterFullFinished
        {
            get { return _trialMasterFull; }
            set { _trialMasterFull = value; }
        }

        public long TimeStamp
        {
            get { return _activityTimeStamp; }
            set { _activityTimeStamp = value; }
        }

        public List<string> Tags
        {
            get { return _tagIDs; }
        }

    }
}
