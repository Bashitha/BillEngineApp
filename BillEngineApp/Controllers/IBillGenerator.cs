using System.Collections.Generic;
using BillEngineApp.Models;
using System;

namespace BillEngineApp.Controllers

{
    public interface IBillGenerator
    {
        bool IsPhoneNumber(string phoneNumber);
        bool AreLocalPhoneNumbers(string callerParty, string calledParty);
        List<CDR> GetCDRSForCallerPhoneNumber(string callerPhoneNumber, string filePath);
        Caller GetCustomerDetailsForCallerPhoneNumber(string callerPhoneNumber, string filePath);
        double CalculateTotalCallCharges(String callerPhoneNumber, Package package, List<CDR> cdrList);
        List<BillReport> GenerateBill(List<String> callerPhoneNumbersList,string cdrFilePath, string customerFilePath,string packageFilePath);
        int GetThePerMinuteChargeForTheCurrentMinute(Package package, TimeSpan peakStartTime, TimeSpan peakOffTime, TimeSpan callStartTime, TimeSpan currentTime, bool areCallingAndCalledPartiesLocal);
        Package GetPackageSubscribedByTheCustomer(string packageName, string packagesFilePath);
        List<TimeSpan> GetPeakStartTimeAndPeakOffTime(string packageName);
    }
}