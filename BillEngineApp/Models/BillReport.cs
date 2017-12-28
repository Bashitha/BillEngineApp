using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillEngineApp.Models
{
    public class BillReport
    {
        public String PhoneNumber { get; set; }
        public String BillingAddress { get; set; }
        public double TotalCallCharges { get; set; }
        public double TotalDiscount { get; set; }
        public double Tax { get; set; }
        public double Rental { get; set; }
        public double BillAmount { get; set; }
        public List<CallDetails> ListOfCallDetails { get; set; }
    }
}
