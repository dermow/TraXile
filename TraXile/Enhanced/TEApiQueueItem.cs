using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile.Enhanced
{
    enum TEApiQueueItemPostType
    {
        SINGLE,
        BULK
    }

    internal class TEApiQueueItem
    {
        public List<TrX_TrackedActivity> Items { get; set; }
        public TrX_TrackedActivity Item { get; set; }
        public TEApiQueueItemPostType Type { get; set; }

        public TEApiQueueItem() 
        {
            Items = new List<TrX_TrackedActivity>();
            Type = TEApiQueueItemPostType.SINGLE;
        }
    }
}
