using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    class TrackedEvent
    {
        private EVENT_TYPES evType;
        private string sLogLine;
        private DateTime dtEvStart;

        public TrackedEvent(EVENT_TYPES evType)
        {
            this.evType = evType;
        }
        public string LogLine
        {
            get
            {
                return this.sLogLine;
            }
            set
            {
                this.sLogLine = value;
            }
        }

        public override string ToString()
        {
            return "TrackedEvent -> Time: " + dtEvStart + ", Type: " + evType.ToString() + ", MatchedLine: '" + sLogLine + "'";
        }

        public DateTime EventTime
        {
            get
            {
                return this.dtEvStart;
            }
            set
            {
                this.dtEvStart = value;
            }
        }

        public EVENT_TYPES EventType
        {
            get
            {
                return this.evType;
            }
        }
    }
}
