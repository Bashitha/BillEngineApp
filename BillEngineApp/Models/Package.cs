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
    }
}
