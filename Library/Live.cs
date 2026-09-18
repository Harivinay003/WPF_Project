using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    /// <summary>
    /// Represents a live data value from the Live table.
    /// Maps directly to the database schema: (DevId, SerDevId, ParamId)
    /// </summary>
    public class Live
    {
        // Primary Key Components
        public int DevId { get; set; }
        public int SerDevId { get; set; }
        public int ParamId { get; set; }

        // Data columns
        public DateTime Time { get; set; }
        public double? Value { get; set; }
        
    }
}
