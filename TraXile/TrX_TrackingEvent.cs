using System;

namespace TraXile
{
    class TrX_TrackingEvent
    {
        private readonly EVENT_TYPES _eventType;
        private string _logLine;
        private DateTime _eventStartTime;

        public TrX_TrackingEvent(EVENT_TYPES evType)
        {
            this._eventType = evType;
        }
        public string LogLine
        {
            get
            {
                return this._logLine;
            }
            set
            {
                this._logLine = value;
            }
        }

        public override string ToString()
        {
            return "TrackedEvent -> Time: " + _eventStartTime + ", Type: " + _eventType.ToString() + ", MatchedLine: '" + _logLine + "'";
        }

        public DateTime EventTime
        {
            get
            {
                return this._eventStartTime;
            }
            set
            {
                this._eventStartTime = value;
            }
        }

        public EVENT_TYPES EventType
        {
            get
            {
                return this._eventType;
            }
        }
    }
}
