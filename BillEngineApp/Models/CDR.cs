using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillEngineApp.Models
{
    public class CDR
    {
        public String CallingPhoneNo { get; set; }
        public String CalledPhoneNumber { get; set; }
        public DateTime Starting_Time { get; set; }
        public int Duration { get; set; }
        
    }
}
