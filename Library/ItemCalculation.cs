using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEMS.Library
{
    public class ItemCalculation
    {
        public int Id { get; set; }
        public string ItemTitle { get; set; }
        public string Units { get; set; }
        public int Decimals { get; set; }
        public List<ItemCalculationComponent> Components { get; set; } = new List<ItemCalculationComponent>();
    }

    public class ItemCalculationComponent
    {
        public int Id { get; set; }
        public int ItemCalculationId { get; set; }
        public int SerialDeviceId { get; set; }
        public int ParameterId { get; set; }
        public OperationType Operation { get; set; } // Add or Subtract
        public double Multiplier { get; set; } = 1.0; // For unit conversion if needed
        public double Divider { get; set; } = 1.0; // For unit conversion if needed
    }

    public enum OperationType
    {
        Add = 1,
        Subtract = 2
    }
}
