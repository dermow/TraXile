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
        private readonly Stopwatch stopWatch;
        private TrackedActivity zanaMap;
        private List<string> tags;
        private DateTime dtStarted;
        private string sInstanceEndpoint;
        private string sArea;
        private ACTIVITY_TYPES sType;
        private bool bIsZana;
        private bool bIsPaused;
        private bool bTrialmasterSuccess;
        private bool bTrialmasterFull;
        private int iTrialMasterCount;
        private int iDeathCounter;
        private int iAreaLevel;
        private long lTimestamp;
        private string sCustomStopWatchVaule;

        public TrackedActivity()
        {
            stopWatch = new Stopwatch();
            zanaMap = null;
            tags = new List<string>();
        }

        public bool HasTag(string s_id)
        {
            foreach(string tag in tags)
            {
                if (tag == s_id)
                    return true;
            }
            return false;
        }

        public void RemoveTag(string s_id)
        {
            if(HasTag(s_id))
               tags.Remove(s_id);
        }

        public void AddTag(string s_id)
        {
            if(!tags.Contains(s_id))
            {
                tags.Add(s_id);
            }
        }

        public void Pause()
        {
              bIsPaused = true;
              if(stopWatch.IsRunning)
                stopWatch.Stop();
        }

        public void Resume()
        {
            bIsPaused = false;
            if(!stopWatch.IsRunning)
                 stopWatch.Start();
        }

        public void StartStopWatch()
        {
            stopWatch.Start();
        }

        public void StopStopWatch()
        {
            stopWatch.Stop();
        }


        public TimeSpan StopWatchTimeSpan
        {
            get { return stopWatch.Elapsed; }
        }

        public string CustomStopWatchValue
        {
            get { return sCustomStopWatchVaule; }
            set { sCustomStopWatchVaule = value; }
        }

        public string StopWatchValue
        {
            get
            {
                if(CustomStopWatchValue != null)
                {
                    return sCustomStopWatchVaule;
                }
                else
                {
                    TimeSpan ts = stopWatch.Elapsed;
                    return String.Format("{0:00}:{1:00}:{2:00}",
                        ts.Hours, ts.Minutes, ts.Seconds);
                }
            }
        }

        public ACTIVITY_TYPES Type
        {
            get { return sType; }
            set { sType = value; }
        }
        public string Area
        {
            get { return sArea.Replace("'", ""); }
            set { sArea = value; }
        }

        public bool IsZana
        {
            get { return bIsZana; }
            set { bIsZana = value; }
        }

        public int DeathCounter
        {
            get { return iDeathCounter; }
            set { iDeathCounter = value; }
        }

        public int AreaLevel
        {
            get { return iAreaLevel; }
            set { iAreaLevel = value; }
        }

        public int MapTier
        {
            get
            {
                switch(iAreaLevel)
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
            get { return zanaMap; }
            set { zanaMap = value; }
        }

        public bool Paused
        {
            get { return bIsPaused; }
        }

        public string InstanceEndpoint
        {
            get { return sInstanceEndpoint; }
            set { sInstanceEndpoint = value; }
        }

        public DateTime Started
        {
            get { return dtStarted; }
            set { dtStarted = value; }
        }

        public int TrialMasterCount
        {
            get
            {
                return iTrialMasterCount;
            }

            set { iTrialMasterCount = value; }
        }

        public bool TrialMasterSuccess
        {
            get { return bTrialmasterSuccess; }
            set { bTrialmasterSuccess = value; }
        }

        public bool TrialMasterFullFinished
        {
            get { return bTrialmasterFull; }
            set { bTrialmasterFull = value; }
        }

        public long TimeStamp
        {
            get { return lTimestamp; }
            set { lTimestamp = value; }
        }

        public List<string> Tags
        {
            get { return tags; }
        }

    }
}
