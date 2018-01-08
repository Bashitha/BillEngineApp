using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillEngineApp.Models
{
    public class Package
    {
        public string PackageName { get; set; }
        public int MonthlyRental { get; set; }
        public string BillingType { get; set; }
        public int PeakHourLocalCallsPerMinuteCharge { get; set; }
        public int PeakHourLongCallsPerMinuteCharge { get; set; }
        public int OffPeakHourLocalCallsPerMinuteCharge { get; set; }
        public int OffPeakHourLongCallsPerMinuteCharge { get; set; }

    }
}
