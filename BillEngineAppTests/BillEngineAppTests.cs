using BillEngineApp.Controllers;
using BillEngineApp.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BillEngineAppTests
{
    [TestFixture]
    class BillEngineAppTests
    {
        private IBillGenerator _sut;

        //change the file paths here
        private static string cdrListPath = @"C:\Users\LBW\source\repos\BillEngineApp\CDR.csv";
        private static string callerListPath = @"C:\Users\LBW\source\repos\BillEngineApp\Customers.csv";
        private static string packageListPath = @"C:\Users\LBW\source\repos\BillEngineApp\Packages.csv"; 

        [SetUp]
        public void Init()
        {
            _sut = new BillGenerator();

        }
        [Test]
        public void OnIsPhoneNumber_WhenInputCorrectFormat_ShouldReturnTrue()
        {
            String phone_number = "091-5232749";
            var result = _sut.IsPhoneNumber(phone_number);
            Assert.AreEqual(true, result);
        }
        [Test]
        public void OnIsPhoneNumber_WhenInputMoreThanTenDigits_ShouldReturnFalse()
        {
            String phone_number = "091-52327490";
            var result = _sut.IsPhoneNumber(phone_number);
            Assert.AreEqual(false, result);
        }
        [Test]
        public void OnIsPhoneNumber_WhenInputCharacters_ShouldReturnFalse()
        {
            String phone_number = "0wu-52327490";
            var result = _sut.IsPhoneNumber(phone_number);
            Assert.AreEqual(false, result);
        }
        [Test]
        public void OnIsPhoneNumber_WhenInputMoreThanThreeDigitsBeforeHiphen_ShouldReturnFalse()
        {
            String phone_number = "0413-52327490";
            var result = _sut.IsPhoneNumber(phone_number);
            Assert.AreEqual(false, result);
        }
        [Test]
        public void OnIsLocalPhoneNumbers_WhenInputTwoLocalPhoneNumbers_ShouldReturnTrue()
        {
            String callerParty = "091-2243533";
            String calledParty = "091-5232749";
            var result = _sut.AreLocalPhoneNumbers(callerParty,calledParty);
            Assert.AreEqual(true,result);
        }
        [Test]
        public void OnIsLocalPhoneNumbers_WhenInputTwoNotLocalPhoneNumbers_ShouldReturnFalse()
        {
            String callerParty = "091-2243533";
            String calledParty = "081-5232749";
            var result = _sut.AreLocalPhoneNumbers(callerParty, calledParty);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void OnGetCDRSForCallerPhoneNumber_WhenInputExistingCallerPhoneNumber_ShouldReturnListofCDRSForParticularCallerPhoneNumber()
        {
            String callerPhoneNumber = "091-5232749";
            List<CDR> cdrsForCallerPhoneNumber = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            DateTime.TryParse("11/12/2017 5:10:00", out DateTime dateTime);
            CDR cdr1 = new CDR
            {
                CallingPhoneNo = "091-5232749",
                CalledPhoneNumber = "081-2249533",
                Starting_Time = dateTime,
                Duration = 54
            };
            DateTime.TryParse("12/13/2017 14:20:20", out DateTime dateTime1);
            CDR cdr2 = new CDR
            {
                CallingPhoneNo = "091-5232749",
                CalledPhoneNumber = "031-2536998",
                Starting_Time = dateTime1,
                Duration = 305
            };
            DateTime.TryParse("12/31/2017 21:09:00", out DateTime dateTime2);
            CDR cdr3 = new CDR
            {
                CallingPhoneNo = "091-5232749",
                CalledPhoneNumber = "091-2256328",
                Starting_Time = dateTime2,
                Duration = 102
            };

            List<CDR> expectedCDRS = new List<CDR>
            {
                cdr1,
                cdr2,
                cdr3
            };

            Assert.AreEqual(expectedCDRS.Count, cdrsForCallerPhoneNumber.Count, "1. Number of items in input and output are not equal.");
            Assert.AreEqual(expectedCDRS[0].CalledPhoneNumber, cdrsForCallerPhoneNumber[0].CalledPhoneNumber,"2. Called Phone numbers are not equal.");
            Assert.AreEqual(expectedCDRS[1].CalledPhoneNumber, cdrsForCallerPhoneNumber[1].CalledPhoneNumber, "3. Called Phone numbers are not equal.");
            Assert.AreEqual(expectedCDRS[2].CalledPhoneNumber, cdrsForCallerPhoneNumber[2].CalledPhoneNumber, "4. Called Phone numbers are not equal.");

        }

        [Test]
        public void OnGetCustomerDetailsForCallerPhoneNumber_WhenInputExistingCallerNumber_ShouldReturnCallerDetailsForTheGivenCallerPhoneNumber()
        {
            String callerPhoneNumber = "091-5232749";
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            DateTime.TryParse("5/29/2017", out DateTime dateTime);
            Caller expectedCallerDetails = new Caller
            {
                FullName = "Leshan Bashitha Wijegunawardana",
                BillingAddress = "Weliwatta Hapugala Wakwella Galle",
                PhoneNumber = "091-5232749",
                PackageName = "Package B",
                RegisteredDate = dateTime
            };
            Assert.AreEqual(expectedCallerDetails.FullName, caller.FullName, "1. Full names are not equal");
            Assert.AreEqual(expectedCallerDetails.BillingAddress, caller.BillingAddress, "2. Billing Addresses are not equal");
            Assert.AreEqual(expectedCallerDetails.PhoneNumber, caller.PhoneNumber, "3. Phone numbers are not equal");
            Assert.AreEqual(expectedCallerDetails.PackageName, caller.PackageName, "4. Package Codes are not equal");
            Assert.AreEqual(expectedCallerDetails.RegisteredDate, caller.RegisteredDate, "5. Registered dates are not equal");

        }

        [Test]
        public void OnCalculateTotalCallCharges_WhenInputCallerPhoneNumberAndCallerCDRList_ShouldReturnCalculatedTotalCharges()
        {
            String callerPhoneNumber = "091-5232749";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(37, (int)totalCallCharges,"1. Numbers are not equal.");
        }

        [Test]
        public void OnCalculateTotalCallCharges_WhenInputCallerPhoneNumberAndCallerCDRListWithCDRSIncludingBothPeakTimesandPeakOffTimesInOneCDR_ShouldReturnCalculatedTotalCharges()
        {
            String callerPhoneNumber = "041-2256588";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(94, (int)totalCallCharges, "1. Numbers are not equal.");
        }

        [Test]
        public void OnCalculateTotalCallCharges_WhenInputCallerPhoneNumberAndCallerCDRListWithCDRSLessThanOneMinuteIncludingBothPeakTimesAndPeakOffTimesInOneCDR_ShouldReturnTotalCallCharges()
        {
            String callerPhoneNumber = "011-2256983";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(10, (int)totalCallCharges, "1. Numbers are not equal.");
        }

        [Test]
        public void OnGenerateBill_WhenInputExistingPhoneNumberWithPackageBSubscription_ShouldReturnBillReport()
        {
            String callerPhoneNumber1 = "091-5232749";
            String callerPhoneNumber2 = "091-2243980";

            List<String> callerPhoneNumbersList = new List<string>
            {
                callerPhoneNumber1,
                callerPhoneNumber2
            };

            List<BillReport> billReports = _sut.GenerateBill(callerPhoneNumbersList,cdrListPath,callerListPath,packageListPath);

            Assert.AreEqual("091-5232749", billReports[0].PhoneNumber, "1. Phone Numbers are not equal.");
            Assert.AreEqual("Weliwatta Hapugala Wakwella Galle", billReports[0].BillingAddress, "2. Billing addresses are not Equal");
            Assert.AreEqual(37,(int)billReports[0].TotalCallCharges,"3. Total Call Charges are not equal.");
            Assert.AreEqual(0, billReports[0].TotalDiscount, "4. Total Discounts are not equal.");
            Assert.AreEqual(27, (int)billReports[0].Tax, "5. Taxes are not equal.");
            Assert.AreEqual(100, (int)billReports[0].Rental, "6. Rentals are not equal");
            Assert.AreEqual(164,(int)billReports[0].BillAmount, "7. Bill Amounts are not equal.");
            Assert.AreEqual(4,(int)billReports[0].ListOfCallDetails[0].Charge,"8. Call charge for first call detail is wrong.");
            Assert.AreEqual(30, (int)billReports[0].ListOfCallDetails[1].Charge, "9. Call charge for second call detail is wrong.");
            Assert.AreEqual(2, (int)billReports[0].ListOfCallDetails[2].Charge, "10. Call charge for third call detail is wrong.");

            Assert.AreEqual("091-2243980", billReports[1].PhoneNumber, "11. Phone Numbers are not equal.");
            Assert.AreEqual("Galle", billReports[1].BillingAddress, "12. Billing addresses are not Equal");
            Assert.AreEqual(27, (int)billReports[1].TotalCallCharges, "13. Total Call Charges are not equal.");
            Assert.AreEqual(0, billReports[1].TotalDiscount, "14. Total Discounts are not equal.");
            Assert.AreEqual(25, (int)billReports[1].Tax, "15. Taxes are not equal.");
            Assert.AreEqual(100, (int)billReports[1].Rental, "16. Rentals are not equal");
            Assert.AreEqual(152, (int)billReports[1].BillAmount, "17. Bill Amounts are not equal.");
            Assert.AreEqual(2, (int)billReports[1].ListOfCallDetails[0].Charge, "18. Call charge for first call detail is wrong.");
            Assert.AreEqual(25, (int)billReports[1].ListOfCallDetails[1].Charge, "19. Call charge for second call detail is wrong.");
            
        }
        [Test]
        public void OnGetPackageSubscribedByTheCustomer_WhenInputPhoneNumberAndPackagesList_ShouldReturnCorrespondingPackage()
        {
            String callerPhoneNumber = "091-5232749";
            
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber,callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName,packageListPath);
            Assert.AreEqual("Package B", package.PackageName, "Package Names are not equal.");
            Assert.AreEqual("Per Second", package.BillingType, "Billing types are not equal.");
            
        }
        [Test]
        public void OnCalculateTotalCallCharges_WhenInputPackageASubscriptionAndCDRListForThatParticualarCustomer_ShouldReturnTotalCallCharges()
        {
            String callerPhoneNumber = "091-2243980";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(27, (int)totalCallCharges, "1. Numbers are not equal.");
        }
        [Test]
        public void OnCalculateTotalCallCharges_WhenInputPackageCSubscriptionAndCDRListForThatParticualarCustomer_ShouldReturnTotalCallCharges()
        {
            String callerPhoneNumber = "091-2242020";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(13, (int)totalCallCharges, "1. Numbers are not equal.");
        }
        [Test]
        public void OnCalculateTotalCallCharges_WhenInputPackageDSubscriptionAndCDRListForThatParticualarCustomer_ShouldReturnTotalCallCharges()
        {
            String callerPhoneNumber = "091-2242850";
            List<CDR> cdrList = _sut.GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrListPath);
            Caller caller = _sut.GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, callerListPath);
            Package package = _sut.GetPackageSubscribedByTheCustomer(caller.PackageName, packageListPath);
            double totalCallCharges = _sut.CalculateTotalCallCharges(callerPhoneNumber, package, cdrList);
            Assert.AreEqual(23, (int)totalCallCharges, "1. Numbers are not equal.");
        }
        
        //get the peakStartTime and peakOffTime given the package
        [Test]
        public void OnGetPeakStartTimeAndPeakOffTime_WhenInputPackageName_ShouldReturnListOfDateTimeContainingPeakStartTimeAndPeakOffTime()
        {
            String packageName = "Package A";
            List<TimeSpan> peakStartTimeAndPeakOffTime = _sut.GetPeakStartTimeAndPeakOffTime(packageName);
            TimeSpan expectedPeakStartTime = new TimeSpan(10, 0, 0);
            TimeSpan expectedPeakOffTime = new TimeSpan(18, 0, 0);
            Assert.AreEqual(expectedPeakStartTime,peakStartTimeAndPeakOffTime[0],"Times are not equal.");
            Assert.AreEqual(expectedPeakStartTime, peakStartTimeAndPeakOffTime[0], "Times are not equal.");
        }
    }
}
