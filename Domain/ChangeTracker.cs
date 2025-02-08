using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ChangeTracker
    {
        public int ChangeID { get; set; }
        public string TableName { get; set; }
        public int RecordID { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangeTimestamp { get; set; }
    }
}
