using System;
using System.Collections.Generic;
using System.Text;

namespace AutoShop.Core.Entities
{
    public class WorkOrderLineItem
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal LaborHours { get; set; }
        public decimal LaborRate { get; set; }

        public decimal PartsCost { get; set; }
        public decimal LineTotal { get; set; }

        public bool IsPart { get; set; }
    }
}
