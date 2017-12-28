﻿using BillEngineApp.Models;
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
                double partNotCatchedInDurationInMinutes = cdr.Duration / 60.0 - cdr.Duration / 60;
                double chargeForTheCall = 0.0;
                //add minute by minute until the required duration in minutes
                for (int i=0; i < cdr.Duration/60; i++)
                {
                    cdr.Starting_Time = cdr.Starting_Time.AddMinutes(1);

                    if (cdr.Starting_Time.TimeOfDay < peakStartTime || cdr.Starting_Time.TimeOfDay >= peakOffTime)
                    {
                        if(AreLocalPhoneNumbers(cdr.CallingPhoneNo,cdr.CalledPhoneNumber))
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
                        if(AreLocalPhoneNumbers(cdr.CallingPhoneNo,cdr.CalledPhoneNumber))
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
                }
                //Adding the charges that not catched in the loop 
                if (cdr.Starting_Time.TimeOfDay < peakStartTime || cdr.Starting_Time.TimeOfDay >= peakOffTime)
                {
                    if (AreLocalPhoneNumbers(cdr.CallingPhoneNo, cdr.CalledPhoneNumber))
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
                    if (AreLocalPhoneNumbers(cdr.CallingPhoneNo, cdr.CalledPhoneNumber))
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
