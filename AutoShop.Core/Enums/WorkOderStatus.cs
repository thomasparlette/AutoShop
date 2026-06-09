using System;
using System.Collections.Generic;
using System.Text;

namespace AutoShop.Core.Enums
{
    public enum WorkOrderStatus
    {
        Draft = 0,
        Open = 1,
        InProgress = 2,
        WaitingApproval = 3,
        Completed = 4,
        Paid = 5,
        Closed = 6,
        Cancelled = 7
    }
}
