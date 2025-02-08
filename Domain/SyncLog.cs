using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class SyncLog
    {
        public int LogID { get; set; }
        public DateTime SyncTimestamp { get; set; }
        public string Operation { get; set; }
        public string Details { get; set; }
        public virtual ICollection<ChangeTracker> ChangeTrackers { get; set; } = new List<ChangeTracker>();
    }
}
