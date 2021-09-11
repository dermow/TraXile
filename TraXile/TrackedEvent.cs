using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    class TrackedEvent
    {
#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
        private EVENT_TYPES evType;
#pragma warning restore IDE0044 // Modifizierer "readonly" hinzufügen
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
