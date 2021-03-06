
/*
Copyright (c) 2014 Genome Research Ltd.
Author: Stephen Rice <sr7@sanger.ac.uk>
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
3. Neither the names Genome Research Ltd and Wellcome Trust Sanger
Institute nor the names of its contributors may be used to endorse or promote
products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY GENOME RESEARCH LTD AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL GENOME RESEARCH LTD OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;



namespace BHS_questionnaire_demo
{
    class QuestionDatePickerSelect : Question
    {


        //fields

        private Label label;    //main label


        private Label labelDays;
        private Label labelMonths;
        private Label labelYears;

        //private ComboBox selectDays;
        private ComboBox selectDays;
        private ComboBox selectMonths;
        private ComboBox selectYears;

        //reference to a subroutine that will do the validation
        //private string Validation;



        //the data the user entered, which may be different to the processed data
        //private string userData;

        private string[] months2 = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        private string[] months = { "Don't Know", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };



        private Dictionary<string, int> monthMap;


        //properties
        public string OnError { get; set; }

        //constructor
        public QuestionDatePickerSelect(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



            //mapping from month as int to month as string
            monthMap = new Dictionary<string, int>();

            for (int i = 0; i <= 12; i++)
            {

                monthMap.Add(months[i], i);

            }

            

        }



        //methods



        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);





        }
        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);




        }



        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(labelDays);
            getQM().getPanel().Controls.Remove(labelMonths);
            getQM().getPanel().Controls.Remove(labelYears);
            getQM().getPanel().Controls.Remove(selectDays);
            getQM().getPanel().Controls.Remove(selectMonths);
            getQM().getPanel().Controls.Remove(selectYears);



            label.Dispose();
            labelDays.Dispose();
            labelMonths.Dispose();
            labelYears.Dispose();
            selectDays.Dispose();
            selectMonths.Dispose();
            selectYears.Dispose();






        }

        public override void configureControls(UserDirection direction)
        {


            //do we want to show the skip controls?
            if (NoAnswerDontKnowNotApplicable)
            {
                //yes
                //turn the skip controls on again
                getQM().getMainForm().setSkipControlsVisible();


            }
            else
            {
                //turn off the skip controls
                getQM().getMainForm().setSkipControlsInvisible();


            }

            //do we want to hide a specific skip control?
            if (HideSkipControl != null)
            {

                getQM().getMainForm().setSpecificSkipControlInvisible(HideSkipControl);

            }



            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            //getQM().getMainForm().setSkipControlsVisible();


            //create a label 
            label = new Label();

            //the question Text shown to the user
            label.Text = Val;
            label.ForeColor = GlobalColours.mainTextColour;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            //label.AutoSize = true;
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());

            int yPosDateLabel = labelYpos + getWidgetHeight();


            //labels for days, months, years
            labelDays = new Label();
            labelDays.Text = "Days";
            labelDays.Location = new Point(labelXpos + 10, yPosDateLabel);
            labelDays.AutoSize = true;
            labelDays.ForeColor = GlobalColours.mainTextColour;


            labelMonths = new Label();
            labelMonths.Text = "Months";
            labelMonths.Location = new Point(labelXpos + 180, yPosDateLabel);
            labelMonths.AutoSize = true;
            labelMonths.ForeColor = GlobalColours.mainTextColour;

            labelYears = new Label();
            labelYears.Text = "Years";
            labelYears.Location = new Point(labelXpos + 350, yPosDateLabel);
            labelYears.AutoSize = true;
            labelYears.ForeColor = GlobalColours.mainTextColour;

            int yPosSelect = yPosDateLabel + 50;
            //int yPosSelect = yPosDateLabel;

            //select boxes for day, month, year
            //selectDays = new ComboBox();
            selectDays = new ComboBox();
            //selectDays.ItemHeight = 5;

            selectMonths = new ComboBox();


            selectYears = new ComboBox();

            //set font size
            setFontSize(label, labelDays, labelMonths, labelYears, selectDays, selectMonths, selectYears);

            //event handlers
            //trap any keypress to deselect the skip-controls
            selectDays.Click += new EventHandler(button_click);
            selectMonths.Click += new EventHandler(button_click);
            selectYears.Click += new EventHandler(button_click);


            //stop user being able to type in the combobox
            selectDays.DropDownStyle = ComboBoxStyle.DropDownList;
            selectMonths.DropDownStyle = ComboBoxStyle.DropDownList;
            selectYears.DropDownStyle = ComboBoxStyle.DropDownList;



            selectDays.BackColor = GlobalColours.controlBackColour;
            selectMonths.BackColor = GlobalColours.controlBackColour;
            selectYears.BackColor = GlobalColours.controlBackColour;

            selectDays.Location = new Point(labelXpos, yPosSelect);
            //selectDays.Location = new Point(labelXpos + 90, yPosSelect);
            selectDays.Size = new Size(160, 20);



            selectMonths.Location = new Point(labelXpos + 170, yPosSelect);
            selectMonths.Size = new Size(160, 20);

            selectYears.Location = new Point(labelXpos + 340, yPosSelect);
            selectYears.Size = new Size(160, 20);
            //selectYears.Size = new Size(160, 150);



            //show only 5 items at a time in drop-down list
            selectMonths.IntegralHeight = false; //won't work unless this is set to false
            selectMonths.MaxDropDownItems = 5;

            selectDays.IntegralHeight = false; //won't work unless this is set to false
            selectDays.MaxDropDownItems = 5;

            selectYears.IntegralHeight = false; //won't work unless this is set to false
            selectYears.MaxDropDownItems = 5;





            //array of days
            string[] days = null;

            

            if (ShowDaysDontKnow)
            {

                days = new string[32];
                days[0] = "Don't Know";
                
                for (int i = 1; i <= 31; i++)
                {
                    days[i] = i.ToString();

                }


            }
            else
            {
                //leave out Don't know

                days = new string[31];
                for (int i = 0; i <= 30; i++)
                {
                    days[i] = (i + 1).ToString();

                }


            }

            

            
            selectDays.Items.AddRange(days);


            //array of months
            if (ShowMonthsDontKnow)
            {
                //include "don't know"
                selectMonths.Items.AddRange(months);


            }
            else
            {
                //leave out don't know
                selectMonths.Items.AddRange(months2);

            }


            //selectMonths.Items.AddRange(months);
            selectMonths.IntegralHeight = false;
            selectMonths.MaxDropDownItems = 5;

            //get the current year.
            int currentYear = DateTime.Now.Year;

            //first year should also be don't know
            if (ShowYearsDontKnow)
            {
                selectYears.Items.Add("Don't Know");

            }
            


            //special case TestEDD: only allow the current and next year (expected delivery date for preg)

            if (Validation == "TestEDD")
            {
                selectYears.Items.Add(currentYear + 1);
                selectYears.Items.Add(currentYear);



            }
            else if(StartYear == 0)
            {
                //normal
                for (int i = currentYear; i >= 1900; i--)
                {
                    selectYears.Items.Add(i);


                }


            }
            else
            {
                //show only from current year to start year
                for (int i = currentYear; i >= StartYear; i--)
                {
                    selectYears.Items.Add(i);


                }


            }







            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {

                //assume the date is stored as dd/mm/yyyy

                //extract the day, months, years.

                if (processedData != null)
                {

                    Match match = Regex.Match(processedData, @"(.+)/(.+)/(.+)");

                    string pDays, pMonths, pYears;

                    if (match.Success)
                    {
                        pDays = match.Groups[1].Value;
                        pMonths = match.Groups[2].Value;
                        pYears = match.Groups[3].Value;

                        if (pDays == "0")
                        {
                            pDays = "Don't Know";
                        }

                        selectDays.SelectedItem = pDays;

                        //convert month from integer to string form.
                        pMonths = months[Convert.ToInt32(pMonths)];


                        selectMonths.SelectedItem = pMonths;

                        if (pYears == "0")
                        {

                            selectYears.SelectedItem = "Don't Know";
                        }
                        else
                        {
                            //years must be numeric rather than string, otherwise won't be recognised by selectbox
                            selectYears.SelectedItem = Convert.ToInt32(pYears);

                        }








                        //set the selected items in the combo boxes



                    }






                }




            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(labelDays);
            getQM().getPanel().Controls.Add(labelMonths);
            getQM().getPanel().Controls.Add(labelYears);
            getQM().getPanel().Controls.Add(selectDays);
            getQM().getPanel().Controls.Add(selectMonths);
            getQM().getPanel().Controls.Add(selectYears);

            setSkipSetting();



        }



        public override string processUserData()
        {

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

                //do not overwrite data that may be there already
                //if (processedData == null)
                //{

                processedData = skipSetting;

                // }

                return ToCode;




            }



            //get the selected date


            object sDays = selectDays.SelectedItem;
            object sMonths = selectMonths.SelectedItem;
            object sYears = selectYears.SelectedItem;

            //are any of these null
            if (sDays == null || sMonths == null || sYears == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose a day, month and year");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            //convert to string

            string selectedDays = sDays.ToString();
            string selectedMonths = sMonths.ToString();
            string selectedYears = sYears.ToString();

            //convert the month from a string to a number
            int selectedMonthsInt = monthMap[selectedMonths];

            //convert the day to a number, where 'don't know' = 0.
            int selectedDayInt;

            if (selectedDays == "Don't Know")
            {

                selectedDayInt = 0;
            }
            else
            {
                selectedDayInt = Convert.ToInt32(selectedDays);
            }


            int selectedYearInt;

            if (selectedYears == "Don't Know")
            {

                selectedYearInt = 0;
            }
            else
            {
                selectedYearInt = Convert.ToInt32(selectedYears);
            }



            //check if this date is valid
            if ((selectedDays != "Don't Know") && (selectedMonths != "Don't Know") && (selectedYears != "Don't Know"))
            {

                try
                {

                    DateTime dt = new DateTime(selectedYearInt, selectedMonthsInt, selectedDayInt);

                }
                catch (ArgumentOutOfRangeException e)
                {

                    //an invalid date was entered
                    ((Form2)getBigMessageBox()).setLabel("You must choose a valid date");
                    getBigMessageBox().ShowDialog();

                    return Code;


                }



            }

            if (Validation == "CheckOver18")
            {

                if (!isOverAge(selectedDayInt, selectedMonthsInt, selectedYearInt, 18))
                {

                    //not vover 18
                    ((Form2)getBigMessageBox()).setLabel("You are under 18");
                    getBigMessageBox().ShowDialog();

                    if (getNumTimesShown() > 1)
                    {
                        //too many attempts
                        return "THANKYOU";

                    }
                    else
                    {

                        return Code;
                    }





                }


            }

            else if (Validation.StartsWith("CheckThisDateAfterDate::"))
            {

                string otherDateQ = Validation.Substring(24);

                //fetch the date to compare with
                string otherDate = getGS().Get(otherDateQ);

                if (otherDate == null)
                {
                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't compare with previous date: " + otherDateQ + " as that question has not been answered");
                    warningBox.ShowDialog();


                }
                else
                {

                    Match match = Regex.Match(otherDate, @"(\d+)/(\d+)/(\d+)");

                    int daysOther;
                    int monthsOther;
                    int yearsOther;

                    if (match.Success)
                    {

                        daysOther = Convert.ToInt32(match.Groups[1].Value);
                        monthsOther = Convert.ToInt32(match.Groups[2].Value);
                        yearsOther = Convert.ToInt32(match.Groups[3].Value);

                        if (!isFirstDateBeforeSecondDate(daysOther, monthsOther, yearsOther, selectedDayInt, selectedMonthsInt, selectedYearInt))
                        {
                            //error
                            ((Form2)getBigMessageBox()).setLabel("This date is before the date from: " + otherDateQ);
                            getBigMessageBox().ShowDialog();

                            return Code;



                        }




                    }
                    else
                    {
                        throw new Exception("date parsing failed");

                    }


                }



            }

            else if(Validation == "CheckAtLeast3MonthsBeforeDENT"){

                if (!isAtLeast3MonthsBeforeDENT(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {
                    //failed

                    //was this asked before?
                    if (getNumTimesShown() > 1)
                    {

                        //alert data-manager
                        getSD().Add("DATA_MANAGER_ALERT_POH10", selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt);
                        

                    }
                    else
                    {
                        //show warning:

                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Please confirm this date");
                        warningBox.ShowDialog();

                        return Code;
                        
                    }

                }

            }


            else if (Validation.StartsWith("CheckNoMoreThan1MonthBeforeDENT::"))
            {

                if (! isNotMoreThan1MonthBeforeDENT(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {
                    //failed

                    //alert data-manager
                    getSD().Add("DATA_MANAGER_ALERT_" + Validation.Substring(33), selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt);

                }

            }




            else if (Validation == "CalcTimeSinceLastPeriod")
            {
                int elapsedWeeks;

                if (! CalcTimeSinceLastPeriod(selectedDayInt, selectedMonthsInt, selectedYearInt, out elapsedWeeks))
                {

                    //failed
                    if (getNumTimesShown() > 1)
                    {

                        //alert data-manager
                        getSD().Add("DATA_MANAGER_ALERT_LNMP", elapsedWeeks.ToString());


                    }
                    else
                    {
                        //show warning:

                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Please confirm this date");
                        warningBox.ShowDialog();

                        return Code;

                    }



                }



            }

            else if (Validation == "CheckOver13")
            {

                if (!isOverAge(selectedDayInt, selectedMonthsInt, selectedYearInt, 13))
                {

                    //not vover 13
                    ((Form2)getBigMessageBox()).setLabel("You are under 13");
                    getBigMessageBox().ShowDialog();

                    if (getNumTimesShown() > 1)
                    {
                        //too many attempts
                        return "THANKYOU";

                    }
                    else
                    {

                        return Code;
                    }





                }


            }




            else if (Validation == "CheckDateBeforeCurrent")
            {
                //the entered date must be before or on the current date

                if (!isBeforeOrOnCurrentDay(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {


                    ((Form2)getBigMessageBox()).setLabel("This date is after today");
                    getBigMessageBox().ShowDialog();

                    return Code;


                }


            }

            else if (Validation == "TestEDD")
            {
                //the entered date must be after or on the current date

                if (!isAfterOrOnCurrentDay(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {


                    ((Form2)getBigMessageBox()).setLabel("This date is before today");
                    getBigMessageBox().ShowDialog();

                    return Code;


                }


            }

            else if (Validation == "TestConsistentDate")
            {

                if (!isConsistentDate(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {


                    ((Form2)getBigMessageBox()).setLabel("This date is inconsistent (before DOB or after today)");
                    getBigMessageBox().ShowDialog();

                    return Code;


                }



            }

            else if (Validation == "HepcDOBcheck")
            {

                if (!hepCageCheck(selectedDayInt, selectedMonthsInt, selectedYearInt))
                {


                    ((Form2)getBigMessageBox()).setLabel("Sorry, you are too young to participate");
                    getBigMessageBox().ShowDialog();

                    return "THANKYOU";


                }




            }

            /*
            if (Process == "CalcTimeSinceLastPeriod")
            {
                CalcTimeSinceLastPeriod(selectedDayInt, selectedMonthsInt, selectedYearInt);


            }
             * */





            //convert to std date format
            processedData = selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt;


            if (Process.StartsWith("IfDontKnowFlagDataManager::"))
            {
                //if any part of the date is unknown: flag data manager
                if (selectedDayInt == 0 || selectedMonthsInt == 0 || selectedYearInt == 0)
                {

                    getSD().Add("DATA_MANAGER_REVIEW_" + Process.Substring(27), processedData);
                }



            }



            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {
                getGS().Add(globalKey, processedData);

            }

            //the control does all the validation

            return ToCode;






        }


        private bool isConsistentDate(int days, int months, int years)
        {
            //true if the date is after/on the DOB and before/on the current date

            //check that we don't have any uncertainty in this date

            if (days == 0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't validate this date as some part(s) uncertain");
                warningBox.ShowDialog();

                return true;


            }

            DateTime today = DateTime.Now;

            //is this date before NOW?

            DateTime thisDate = new DateTime(years, months, days);

            if (thisDate > today)
            {

                return false;


            }




            //get DOB
            string dobAsStr = getGS().Get("DOB");

            if (dobAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check that DOB matches age as DOB was not entered");
                warningBox.ShowDialog();

                return true;



            }


            DateTime dob;

            //extract the day, months, years.
            Match match = Regex.Match(dobAsStr, @"(\d+)/(\d+)/(\d+)");



            if (match.Success)
            {
                days = Convert.ToInt32(match.Groups[1].Value);
                months = Convert.ToInt32(match.Groups[2].Value);
                years = Convert.ToInt32(match.Groups[3].Value);



                //if years or months or days ==0, these are unknown so we can't do this test
                if (years == 0 || months == 0 || days == 0)
                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Some parts of DOB (day, month or year) are not known");
                    warningBox.ShowDialog();


                    return true;
                }



                dob = new DateTime(years, months, days);



                if (thisDate < dob)
                {
                    //before DOB
                    return false;


                }
                else
                {
                    return true;


                }



            }
            else
            {
                throw new Exception("date parsing failed");

            }







        }


        private bool isAtLeast3MonthsBeforeDENT(int days, int months, int years)
        {
            //true if the date is after/on the DOB and before/on the current date

            //check that we don't have any uncertainty in this date

            if (months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't validate this date as some part(s) uncertain");
                warningBox.ShowDialog();

                return true;


            }

            int daysToUse;
            if (days == 0)
            {
                //days not known: use the last day in the month
                daysToUse = DateTime.DaysInMonth(years, months);

            }
            else
            {

                daysToUse = days;

            }

            DateTime thisDate = new DateTime(years, months, daysToUse);

           
            //get DOB
            string dentAsStr = getGS().Get("DENT");

            if (dentAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check this date against DENT as DENT was not entered");
                warningBox.ShowDialog();

                return true;



            }


            DateTime dent;

            //extract the day, months, years.
            Match match = Regex.Match(dentAsStr, @"(\d+)/(\d+)/(\d+)");

            int dentDays;
            int dentMonths;
            int dentYears;

            if (match.Success)
            {
                dentDays = Convert.ToInt32(match.Groups[1].Value);
                dentMonths = Convert.ToInt32(match.Groups[2].Value);
                dentYears = Convert.ToInt32(match.Groups[3].Value);



                //if years or months or days ==0, these are unknown so we can't do this test
                if (dentYears == 0 || dentMonths == 0 || dentDays == 0)
                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Some parts of DENT (day, month or year) are not known");
                    warningBox.ShowDialog();


                    return true;
                }



                dent = new DateTime(dentYears, dentMonths, dentDays);

                
                //to pass test: date must be at least 3 months (90 days) before DENT
                DateTime dentMinus3m = dent - new TimeSpan(90, 0, 0, 0);



                if (thisDate < dentMinus3m)
                {

                    return true;


                }
                else
                {
                    return false;



                }



            }
            else
            {
                throw new Exception("date parsing failed");

            }







        }




        private bool isNotMoreThan1MonthBeforeDENT(int days, int months, int years)
        {
            //true if the date is 1 month before DENT or less

            //check that we don't have any uncertainty in this date

            if (days ==0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't validate this date as some part(s) uncertain");
                warningBox.ShowDialog();

                return true;


            }

            

            DateTime thisDate = new DateTime(years, months, days);


            
            string dentAsStr = getGS().Get("DENT");

            if (dentAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check this date against DENT as DENT was not entered");
                warningBox.ShowDialog();

                return true;



            }


            DateTime dent;

            //extract the day, months, years.
            Match match = Regex.Match(dentAsStr, @"(\d+)/(\d+)/(\d+)");

            int dentDays;
            int dentMonths;
            int dentYears;

            if (match.Success)
            {
                dentDays = Convert.ToInt32(match.Groups[1].Value);
                dentMonths = Convert.ToInt32(match.Groups[2].Value);
                dentYears = Convert.ToInt32(match.Groups[3].Value);



                //if years or months or days ==0, these are unknown so we can't do this test
                if (dentYears == 0 || dentMonths == 0 || dentDays == 0)
                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Some parts of DENT (day, month or year) are not known");
                    warningBox.ShowDialog();


                    return true;
                }



                dent = new DateTime(dentYears, dentMonths, dentDays);


                //to pass test: date must no more than 1 month before DENT
                DateTime dentMinus1m = dent - new TimeSpan(30, 0, 0, 0);



                if (thisDate >= dentMinus1m)
                {

                    return true;


                }
                else
                {
                    return false;



                }



            }
            else
            {
                throw new Exception("date parsing failed");

            }







        }


        private bool hepCageCheck(int days, int months, int years)
        {

            // If CASE=1 check that participants are 25 years and above. If CASE=2 check that participants are 18 years and above.

            string hCase = getGS().Get("CASE");

            //warning if case is null
            if (hCase == null)
            {

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check DOB as CASE has not been entered");
                warningBox.ShowDialog();

                return true;



            }


            else if (hCase == "1")
            {

                return isOverAge(days, months, years, 25);

            }
            else if (hCase == "2")
            {

                return isOverAge(days, months, years, 18);

            }
            else
            {

                throw new Exception("unknown CASE value:" + hCase);


            }



        }

        /*
        private bool isFirstDateBeforeSecondDate(int days1, int months1, int years1, int days2, int months2, int years2)
        {

            // true if the first date occurs before the second date or both are the same date
            
            
            //check if any parts of either date are unknown, i.e. set as 0

            if (days1 == 0 || months1 == 0 || years1 == 0 || days2 == 0 || months2 == 0 || years2 == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't compare dates as some information uncertain");
                warningBox.ShowDialog();

                return true;


            }


            //convert to date objects
            DateTime date1 = new DateTime(years1, months1, days1);
            DateTime date2 = new DateTime(years2, months2, days2);

            if (date1 <= date2)
            {

                return true;
            }
            else
            {

                return false;


            }






        }

        */





        private bool isOverAge(int days, int months, int years, int minAge)
        {

            //if any of these are 0 we can't do this test, i.e. data uncertain

            if (days == 0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check over " + minAge + " as age uncertain");
                warningBox.ShowDialog();

                return true;


            }

            DateTime today = DateTime.Now;

            //subtract the year part of the current date from the year part of the dob

            int age = today.Year - years;

            //this will give the maximum possible age in years:
            //if today is before the birthday then we subtract 1

            //what date is the birthday this year ?

            DateTime dobThisYear = new DateTime(today.Year, months, days);

            if (today < dobThisYear)
            {

                //today is before birthday
                age -= 1;


            }

            //compare age based on todays date and the previously entered dob with the age the user entered

            if (age >= minAge)
            {
                return true;

            }
            else
            {
                return false;

            }





        }


        private bool isBeforeOrOnCurrentDay(int days, int months, int years)
        {

            //true if the date is before or on the current date


            //if any of these are 0 we can't do this test, i.e. data uncertain

            if (days == 0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check validity as date uncertain");
                warningBox.ShowDialog();

                return true;


            }

            DateTime today = DateTime.Now;

            DateTime thisDate = new DateTime(years, months, days);

            if (thisDate <= today)
            {

                return true;


            }
            else
            {
                return false;


            }



        }


        private bool CalcTimeSinceLastPeriod(int days, int months, int years, out int elapsedWeeks)
        {

            //find elapsed time since this date and the current date

            //check for unknown parts
            if (days == 0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't calculate time since last period as date uncertain");
                warningBox.ShowDialog();

                elapsedWeeks = 0;

                return true;


            }


            DateTime today = DateTime.Now.Date;


            DateTime periodDate = new DateTime(years, months, days);

            //calc diff

            TimeSpan timeSincePeriod = today - periodDate;

            int elapsedDays = timeSincePeriod.Days;

            elapsedWeeks = elapsedDays / 7;

            int remainingDays= elapsedDays - (7 * elapsedWeeks);

            //save
            getSD().Add("LNMWKS", "" + elapsedWeeks + "," + remainingDays);

            if (elapsedWeeks < 12)
            {
                return false;


            }
            else
            {
                return true;


            }



        }






        private bool isAfterOrOnCurrentDay(int days, int months, int years)
        {

            //true if the date is After or on the current date


            //if any of these are 0 we can't do this test, i.e. data uncertain

            if (days == 0 || months == 0 || years == 0)
            {


                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check validity as date uncertain");
                warningBox.ShowDialog();

                return true;


            }

            DateTime today = DateTime.Now.Date;


            DateTime thisDate = new DateTime(years, months, days);

            if (thisDate >= today)
            {

                return true;


            }
            else
            {
                return false;


            }



        }


    }


}

