using System;

namespace TraXile
{
    class TrX_TrackingEvent
    {
        // Matched lin in Client.txt
        private string _logLine;
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

        // Start time of the event
        private DateTime _eventStartTime;
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

        // Type of the event
        private readonly EVENT_TYPES _eventType;
        public EVENT_TYPES EventType => _eventType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="evType"></param>
        public TrX_TrackingEvent(EVENT_TYPES evType)
        {
            this._eventType = evType;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TrackedEvent -> Time: " + _eventStartTime + ", Type: " + _eventType.ToString() + ", MatchedLine: '" + _logLine + "'";
        }


    }
}
