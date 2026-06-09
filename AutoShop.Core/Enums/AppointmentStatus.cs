using System;
using System.Collections.Generic;
using System.Text;

namespace AutoShop.Core.Enums
{
    public enum AppointmentStatus
    {
        Scheduled = 0,
        Confirmed = 1,
        CheckedIn = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        NoShow = 6
    }
}
