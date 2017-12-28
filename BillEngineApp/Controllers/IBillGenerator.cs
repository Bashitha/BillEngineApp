using System.Collections.Generic;
using BillEngineApp.Models;

namespace BillEngineApp.Controllers

{
    public interface IBillGenerator
    {
        bool IsPhoneNumber(string phoneNumber);
        bool AreLocalPhoneNumbers(string callerParty, string calledParty);
        List<CDR> GetCDRSForCallerPhoneNumber(string callerPhoneNumber, string filePath);
        Caller GetCustomerDetailsForCallerPhoneNumber(string callerPhoneNumber, string filePath);
        double CalculateTotalCallCharges(string callerPhoneNumber, List<CDR> cdrList);
        BillReport GenerateBill(string callerPhoneNumber,string cdrFilePath, string customerFilePath);
    }
}