using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class Page
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public List<PageItem> PageItems { get; set; } = new List<PageItem>();
        public string Group { get; set; }
    }


    public class PageItem
    {
        public int Id { get; set; }
        public Page Page { get; set; }
        public int PageId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public List<PageItemTag> PageItemTags { get; set; } = new List<PageItemTag>();
        public List<PageItemParameter> PageItemParameters { get; set; } = new List<PageItemParameter>();
        public string TagIds =>
          PageItemTags == null || PageItemTags.Count == 0
              ? ""
              : string.Join(", ", PageItemTags.Select(t => t.TagId));

        public string SerialDeviceIds =>
            PageItemParameters == null || PageItemParameters.Count == 0
                ? ""
                : string.Join(", ", PageItemParameters.Select(p => p.SerialDeviceId));

        public string ParameterIds =>
            PageItemParameters == null || PageItemParameters.Count == 0
                ? ""
                : string.Join(", ", PageItemParameters.Select(p => p.SerialDeviceParameterId));

        [NotMapped]
        public string TagNames { get; set; }

        [NotMapped]
        public string SerialDeviceNames { get; set; }

        [NotMapped]
        public string ParameterNames { get; set; }


        [NotMapped]
        public string DeviceName { get; set; }

        [NotMapped]
        public string ParameterList { get; set; }
    }
    public class PageItemTag
    {
        public int Id { get; set; }
        public int PageItemId { get; set; }
        public PageItem PageItem { get; set; }
        public Tag Tag { get; set; }
        public int TagId { get; set; }
        public string? Formula { get; set; }
    }
    public class PageItemParameter
    {
        public int Id { get; set; }
        public int PageItemId { get; set; }
        public PageItem PageItem { get; set; }
        public int SerialDeviceId { get; set; }
        public SerialDevice SerialDevice { get; set; }
        public int SerialDeviceParameterId { get; set; }
        public SerialDeviceParameter SerialDeviceParameter { get; set; }
        public string? Formula { get; set; }
    }
}
