using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class TODTarrif
    {
        public int Id { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public Single? Cost { get; set; }
        public TodType TodType { get; set; }

    }
    public enum TodType
    {
        Peak,
        OffPeak,
        Normal
    }
}
