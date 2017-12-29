using BillEngineApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BillEngineApp.Controllers
{
    public class BillGenerator : IBillGenerator
    {
        private TimeSpan peakStartTime = new TimeSpan(8,0,0);
        private TimeSpan peakOffTime = new TimeSpan(20,0,0);
        private List<CallDetails> callDetailsForCallingPhoneNumber = new List<CallDetails>();
        private double disount = 0;
        private double monthlyRental = 100.0;
        public BillGenerator()
        {
        }

        public bool IsPhoneNumber(string phoneNumber)
        {
            return Regex.Match(phoneNumber, @"^(\d{3}-)?\d{7}$").Success;
        }

        public bool AreLocalPhoneNumbers(string callerParty, string calledParty)
        {
            String callerLocationId = callerParty.Split("-")[0];
            String calledLocationId = calledParty.Split("-")[0];
            return callerLocationId.Equals(calledLocationId);
        }

        public List<CDR> GetCDRSForCallerPhoneNumber(string callerPhoneNumber, string filePath)
        {
            List<CDR> cdrList = new List<CDR>();

            //check whether the file is empty
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentOutOfRangeException(nameof(filePath), "Invalid input file.");
            else
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        reader.ReadLine();
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            var values = line.Split(',');

                            if (values[0] == callerPhoneNumber)
                            {
                                DateTime.TryParse(values[2], out DateTime dateTime);
                                CDR cdr = new CDR
                                {
                                    CallingPhoneNo = values[0],
                                    CalledPhoneNumber = values[1],
                                    Starting_Time= dateTime,
                                    Duration = Int32.Parse(values[3])
                                };

                                cdrList.Add(cdr);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }
            if (cdrList.Count!=0)
            {
                return cdrList;
            }
            else
            {
                return null;
            }

        }

        public Caller GetCustomerDetailsForCallerPhoneNumber(string callerPhoneNumber, string filePath)
        {
            Caller caller = new Caller();
            //check whether the file is empty
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentOutOfRangeException(nameof(filePath), "Invalid input file.");
            else
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        reader.ReadLine();
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            var values = line.Split(',');
                            
                            if (values[2] == callerPhoneNumber)
                            {
                                
                                DateTime.TryParse(values[4], out DateTime dateTime);

                                return new Caller
                                {
                                    FullName = values[0],
                                    BillingAddress = values[1],
                                    PhoneNumber = values[2],
                                    PackageCode = int.Parse(values[3]),
                                    RegisteredDate = dateTime
                                };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            return null;
        }
        public double CalculateTotalCallCharges(string callerPhoneNumber, List<CDR> cdrList)
        {
            int perMinuteCharge = 0;            
            double totalCallCharges = 0;
            foreach (CDR cdr in cdrList)
            {                
                double partNotCatchedInDurationInMinutes = (cdr.Duration % 60) / 60.0 ;
                double chargeForTheCall = 0.0;
                bool areCallingAndCalledPartiesLocal = AreLocalPhoneNumbers(cdr.CallingPhoneNo, cdr.CalledPhoneNumber);

                //add minute by minute until the required duration in minutes
                for (int i=0; i < cdr.Duration/60; i++)
                {
                    
                    if (cdr.Starting_Time.TimeOfDay < peakStartTime || cdr.Starting_Time.TimeOfDay >= peakOffTime)
                    {
                        
                        if(areCallingAndCalledPartiesLocal)
                        {
                            perMinuteCharge = 2;
                        }
                        else
                        {
                            perMinuteCharge = 4;
                        }

                    }
                    else
                    {
                       
                        if (areCallingAndCalledPartiesLocal)
                        {                            
                            perMinuteCharge = 3;
                        }
                        else
                        {                            
                            perMinuteCharge = 5;
                        }
                    }

                    totalCallCharges = totalCallCharges + perMinuteCharge;
                    chargeForTheCall = chargeForTheCall + perMinuteCharge;
                    cdr.Starting_Time = cdr.Starting_Time.AddMinutes(1);

                    // check whether after adding minute the Starting_Time has passed the peakStartTime and charged accordingly
                    if(cdr.Starting_Time.TimeOfDay > peakStartTime && cdr.Starting_Time.AddMinutes(-1).TimeOfDay < peakStartTime)
                    {
                        int noOfSecondsPassedPeakStartTime = cdr.Starting_Time.Second - peakStartTime.Seconds;
                        //reducing the charge added wrong with the wrong perMinuteCharge and add the correct perMinuteCharge
                        totalCallCharges = totalCallCharges - (noOfSecondsPassedPeakStartTime / 60.0) * perMinuteCharge;
                        chargeForTheCall = chargeForTheCall - (noOfSecondsPassedPeakStartTime / 60.0) * perMinuteCharge;

                        if (areCallingAndCalledPartiesLocal)
                        {
                            totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakStartTime / 60.0) * 3;
                            chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakStartTime / 60.0) * 3;
                        }
                        else
                        {
                            totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakStartTime / 60.0) * 5;
                            chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakStartTime / 60.0) * 5;
                        }
                    }
                    // check whether after adding minute the Starting_Time has passed the peakOffTime and charged accordingly
                    else if (cdr.Starting_Time.TimeOfDay > peakOffTime && cdr.Starting_Time.AddMinutes(-1).TimeOfDay < peakOffTime)
                    {
                        int noOfSecondsPassedPeakOffTime = cdr.Starting_Time.Second - peakOffTime.Seconds;

                        //reducing the charge added wrong with the wrong perMinuteCharge and add the correct perMinuteCharge
                        totalCallCharges = totalCallCharges - (noOfSecondsPassedPeakOffTime / 60.0) * perMinuteCharge;
                        chargeForTheCall = chargeForTheCall - (noOfSecondsPassedPeakOffTime / 60.0) * perMinuteCharge;

                        if (areCallingAndCalledPartiesLocal)
                        {
                            totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakOffTime / 60.0) * 2;
                            chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakOffTime / 60.0) * 2;
                        }
                        else
                        {
                            totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakOffTime / 60.0) * 4;
                            chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakOffTime / 60.0) * 4;
                        }
                    }
                }
                //Adding the charges that not catched in the loop 
                if (cdr.Starting_Time.TimeOfDay < peakStartTime || cdr.Starting_Time.TimeOfDay >= peakOffTime)
                {
                    if (areCallingAndCalledPartiesLocal)
                    {
                        perMinuteCharge = 2;
                    }
                    else
                    {
                        perMinuteCharge = 4;
                    }
                }
                else
                {
                    if (areCallingAndCalledPartiesLocal)
                    {
                        perMinuteCharge = 3;
                    }
                    else
                    {
                        perMinuteCharge = 5;
                    }
                }

                totalCallCharges = totalCallCharges + perMinuteCharge * partNotCatchedInDurationInMinutes;
                chargeForTheCall = chargeForTheCall + perMinuteCharge * partNotCatchedInDurationInMinutes;

                //check whether the duration that is not charged passes the peakStartTime
                DateTime callEndTime = cdr.Starting_Time.AddMinutes(partNotCatchedInDurationInMinutes);

                if(cdr.Starting_Time.TimeOfDay < peakStartTime && callEndTime.TimeOfDay > peakStartTime)
                {
                    int noOfSecondsPassedPeakStartTime = callEndTime.Second - peakStartTime.Seconds;
                    totalCallCharges = totalCallCharges - (noOfSecondsPassedPeakStartTime / 60.0) * perMinuteCharge;
                    chargeForTheCall = chargeForTheCall - (noOfSecondsPassedPeakStartTime / 60.0) * perMinuteCharge;

                    if (areCallingAndCalledPartiesLocal)
                    {
                        totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakStartTime / 60.0) * 3;
                        chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakStartTime / 60.0) * 3;
                    }
                    else
                    {
                        totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakStartTime / 60.0) * 5;
                        chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakStartTime / 60.0) * 5;
                    }
                }
                else if (cdr.Starting_Time.TimeOfDay < peakOffTime && callEndTime.TimeOfDay > peakOffTime)
                {
                    int noOfSecondsPassedPeakOffTime = callEndTime.Second - peakOffTime.Seconds;
                    totalCallCharges = totalCallCharges - (noOfSecondsPassedPeakOffTime / 60.0) * perMinuteCharge;
                    chargeForTheCall = chargeForTheCall - (noOfSecondsPassedPeakOffTime / 60.0) * perMinuteCharge;

                    if (areCallingAndCalledPartiesLocal)
                    {
                        totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakOffTime / 60.0) * 2;
                        chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakOffTime / 60.0) * 2;
                    }
                    else
                    {
                        totalCallCharges = totalCallCharges + (noOfSecondsPassedPeakOffTime / 60.0) * 4;
                        chargeForTheCall = chargeForTheCall + (noOfSecondsPassedPeakOffTime / 60.0) * 4;
                    }
                }
                
                //put call details into the list
                CallDetails callDetails = new CallDetails
                {
                    StartTime = cdr.Starting_Time,
                    Duration = cdr.Duration,
                    DestinationNumber = cdr.CalledPhoneNumber,
                    Charge = chargeForTheCall
                };

                callDetailsForCallingPhoneNumber.Add(callDetails);
                perMinuteCharge = 0;
                
            }

            return totalCallCharges;
        }

        public BillReport GenerateBill(string callerPhoneNumber, string cdrFilePath, string customerFilePath)
        {
            if (IsPhoneNumber(callerPhoneNumber))
            {
                Caller caller = GetCustomerDetailsForCallerPhoneNumber(callerPhoneNumber, customerFilePath);
                List<CDR> cdrListForTheCaller = GetCDRSForCallerPhoneNumber(callerPhoneNumber, cdrFilePath);
                double totalCallCharges = CalculateTotalCallCharges(callerPhoneNumber, cdrListForTheCaller);
                double tax = (monthlyRental + totalCallCharges) * (20.0 / 100);
                Console.WriteLine(totalCallCharges);
                BillReport billReport = new BillReport()
                {
                    PhoneNumber = callerPhoneNumber,
                    BillingAddress = caller.BillingAddress,
                    TotalCallCharges = totalCallCharges,
                    TotalDiscount = disount,
                    Tax = tax,
                    Rental = monthlyRental,
                    BillAmount = totalCallCharges + monthlyRental + tax - disount,
                    ListOfCallDetails = callDetailsForCallingPhoneNumber
                };
                return billReport;
            }
            else
            {
                return null;
            }
            
        }
    }
}
