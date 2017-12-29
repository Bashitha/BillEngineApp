# BillEngineApp
User Cases

1. As a User I want to check whether a phone number is valid.

	Test Cases:-

	1.
	Input - "091-2243533"
	Output - true

	2. 
	Input - "091-22435330"
	Output - false

	3. 
	Input - "wu1-3333333"
	Output - false

	4.
	Input - "1234-567899"
	Output - false
	
2. As a User I want to get a list of CDRS given a caller Id.

	Test Cases:
	1.
	Input- "091-5232749"
	Output - 
	081-2249533
	031-2536998
	091-2256328
	
3. As a user I want to check the call type of a CDR is local or long when calling and called phone numbers
   are given

	Test Cases:
	1.
	Input - 091-5232749, 081-2249533
	Output - false
	
4. As a user I want to get the customer details for a given caller
	Test Cases:
	1.
	Input- "091-5232749"
	Output - "Leshan Bashitha Wijegunawardana....."
	
5. As a user I want to calculate the total call charges

	TestCases:
	1. when input a phone number and a CDR List
	Input - "091-5232749"
	Output - 32.42
	
	2. When input a phone number with a cdr list that contains the cdrs which have the durations
	in between the peakStartTime and peakOffTime
	Input - "041-2256588"
	Output - 74
	
	3. When inpur a phone number with a cdr which contains duration of less than a minute and within that duration peakStartTime contains

	Input - "011-2256983"
	Output - 8
	
6. As a user I want to generate the bill when input a phone number and the cdr list

	TestCases :
	1. 
	Input - "091-5232749"
	Output - Bill Report