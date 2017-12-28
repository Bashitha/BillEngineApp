using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillEngineApp.Models
{
    public class CallDetails
    {
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public String DestinationNumber { get; set; }
        public double Charge { get; set; }
    }
}
