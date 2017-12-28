using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillEngineApp.Models
{
    public class Caller
    {
        public String FullName { get; set; }
        public String BillingAddress { get; set; }
        public String PhoneNumber { get; set; }
        public int PackageCode { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
