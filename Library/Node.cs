using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public NodeType Type { get; set; }
        public Node? Parent { get; set; }
        public int? ParentId { get; set; }
        public List<Node> Children { get; set; }
        public SerialDevice? Feeder { get; set; }
        public int? FeederId { get; set; }
        public FieldDevice? FieldDevice { get; set; }
        public int? FieldDeviceId { get; set; }
        public string ParentName => Parent?.Name ?? "(Root)";
        public string FeederName => Feeder?.Name ?? "";
        public string FieldDeviceName => FieldDevice?.Name ?? "";

        public string Formula { get; set; }
    }
    public enum NodeType
    {
        Section,
        Root,
        Feeder,
        FieldDevice
    }
}
