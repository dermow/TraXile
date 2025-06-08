using System;
using System.Collections.Generic;

namespace TraXile
{
    public class TrX_BackendSync_ActivityDocument
    {
        public string ActivityType { get; set; }
        public int AreaLevel { get; set; }
        public int DeathCounter { get; set; }
        public string AreaName { get; set; }
        public int DurationSec { get; set; }
        public List<string> Tags { get; set; }
        public string Identifier { get; set; }
        public DateTime StartTime { get; set; }
    }
}
