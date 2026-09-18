using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class FieldDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IODevice IODevice { get; set; }
        public int IODeviceId { get; set; }
        public int? RunFBId { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
