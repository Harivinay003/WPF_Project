using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class SLDPage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public List<SLDPageItem> SLDPageItems { get; set; } = new List<SLDPageItem>();
    }

    public class SLDPageItem
    {
        public int Id { get; set; }
        public SLDPage SLDPage { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public List<SLDPageItemTag> SLDPageItemTags { get; set; } = new List<SLDPageItemTag>();
        public List<SLDPageItemParameter> SLDPageItemParameters { get; set; } = new List<SLDPageItemParameter>();
        public SLDPageItem? Parent { get; set; }
    }

    public class SLDPageItemTag
    {
        public int Id { get; set; }
        public SLDPageItem SLDPageItem { get; set; }
        public Tag Tag { get; set; }
    }

    public class SLDPageItemParameter
    {
        public int Id { get; set; }
        public SLDPageItem SLDPageItem { get; set; }
        public SerialDevice SerialDevice { get; set; }
        public SerialDeviceParameter SerialDeviceParameter { get; set; }
    }
}
