
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
    class QuestionTextDate : Question

    {

        //text box

        //fields
        private TextBox textbox;
        private Label label;

        //reference to a subroutine that will do the validation
        private string Validation;

        //reference to a subroutine that will do any needed processing
        private string Process;


        //the data the user entered, which may be different to the processed data
        private string userData= null;


        //properties
        

        private Label labelDays;
        private Label labelMonths;
        private Label labelYears;
        private Label labelResult;

        private ComboBox selectDays;
        private ComboBox selectMonths;
        private ComboBox selectYears;

        private string[] months = { "Don't Know", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        private Dictionary<string, int> monthMap;




        //constructor
        public QuestionTextDate(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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

            //save the user data

            if (userData != null)
            {

                //save the data stored in this object
                dhUserData.WriteLine(Code + "\t" + userData);

            }
            
            
        }

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            //can I find this code in the dictionary
            if (uDataDict.ContainsKey(Code))
            {
                userData = uDataDict[Code];

            }
            else
            {
                userData = null;

            }



        }




        public override void removeControls()
        {
           

            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(labelResult);
            getQM().getPanel().Controls.Remove(textbox);

           
            getQM().getPanel().Controls.Remove(labelDays);
            getQM().getPanel().Controls.Remove(labelMonths);
            getQM().getPanel().Controls.Remove(labelYears);
            getQM().getPanel().Controls.Remove(selectDays);
            getQM().getPanel().Controls.Remove(selectMonths);
            getQM().getPanel().Controls.Remove(selectYears);


            labelResult.Dispose();
            label.Dispose();
            labelDays.Dispose();
            labelMonths.Dispose();
            labelYears.Dispose();
            selectDays.Dispose();
            selectMonths.Dispose();
            selectYears.Dispose();

            
            textbox.Dispose();





        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            label = new Label();
            
            textbox = new TextBox();

            
            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);



            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos + 600;
            //int textBoxYpos = labelYpos + 50;

            //MessageBox.Show("gap is:" + LabelToBoxGap);

            int labelsYpos = labelYpos + 50;

            int controlsYpos = labelsYpos + 50;

            //position of the main Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //position of the textbox
            textbox.Location = new Point(textBoxXpos, controlsYpos);
            textbox.Size = new Size(300, 50);



            //labels for days, months, years
            labelDays = new Label();
            labelDays.Text = "Days";
            labelDays.Location = new Point(labelXpos + 10, labelsYpos);
            labelDays.AutoSize = true;
            labelDays.ForeColor = GlobalColours.mainTextColour;


            labelMonths = new Label();
            labelMonths.Text = "Months";
            labelMonths.Location = new Point(labelXpos + 180, labelsYpos);
            labelMonths.AutoSize = true;
            labelMonths.ForeColor = GlobalColours.mainTextColour;

            labelYears = new Label();
            labelYears.Text = "Years";
            labelYears.Location = new Point(labelXpos + 350, labelsYpos);
            labelYears.AutoSize = true;
            labelYears.ForeColor = GlobalColours.mainTextColour;

            labelResult = new Label();
            labelResult.Text = "Result";
            labelResult.Location = new Point(labelXpos + 700, labelsYpos);
            labelResult.AutoSize = true;
            labelResult.ForeColor = GlobalColours.mainTextColour;


           

            //select boxes for day, month, year
            selectDays = new ComboBox();
            selectMonths = new ComboBox();
            selectYears = new ComboBox();

            //set font size
            setFontSize(label, textbox, labelResult, labelDays, labelMonths, labelYears, selectDays, selectMonths, selectYears);

            //event handlers
            //trap any keypress to deselect the skip-controls
            selectDays.Click += new EventHandler(button_click);
            selectMonths.Click += new EventHandler(button_click);
            selectYears.Click += new EventHandler(button_click);


            //stop user being able to type in the combobox
            selectDays.DropDownStyle = ComboBoxStyle.DropDownList;
            selectMonths.DropDownStyle = ComboBoxStyle.DropDownList;
            selectYears.DropDownStyle = ComboBoxStyle.DropDownList;

            //show only 5 items at a time in drop-down list
            selectMonths.IntegralHeight = false; //won't work unless this is set to false
            selectMonths.MaxDropDownItems = 5;

            selectDays.IntegralHeight = false; //won't work unless this is set to false
            selectDays.MaxDropDownItems = 5;

            selectYears.IntegralHeight = false; //won't work unless this is set to false
            selectYears.MaxDropDownItems = 5;


            selectDays.BackColor = GlobalColours.controlBackColour;
            selectMonths.BackColor = GlobalColours.controlBackColour;
            selectYears.BackColor = GlobalColours.controlBackColour;

            selectDays.Location = new Point(labelXpos, controlsYpos);
            selectDays.Size = new Size(160, 20);

            selectMonths.Location = new Point(labelXpos + 170, controlsYpos);
            selectMonths.Size = new Size(160, 20);

            selectYears.Location = new Point(labelXpos + 340, controlsYpos);
            selectYears.Size = new Size(160, 20);

            //array of days
            string[] days = new string[32];

            days[0] = "Don't Know";


            for (int i = 1; i <= 31; i++)
            {
                days[i] = i.ToString();

            }
            selectDays.Items.AddRange(days);


            //array of months


            selectMonths.Items.AddRange(months);

            //get the current year.
            int currentYear = DateTime.Now.Year;

            //first year should also be don't know
            selectYears.Items.Add("Don't Know");

            for (int i = currentYear; i >= 1900; i--)
            {
                selectYears.Items.Add(i);


            }



            

            if (PageSeen)
            {
                


                //extract the day, months, years.

                if (processedData != null)
                {

                    //processedData is the value component
                    //the date is stored in SD
                    string date = getSD().Get(Code + "_DATE");

                    //Match match = Regex.Match(processedData, @"(.+)/(.+)/(.+):(.+)");
                    Match match = Regex.Match(date, @"(.+)/(.+)/(.+)");

                    string pDays, pMonths, pYears;

                    if (match.Success)
                    {
                        pDays = match.Groups[1].Value;
                        pMonths = match.Groups[2].Value;
                        pYears = match.Groups[3].Value;
                        //pText = match.Groups[4].Value;

                        textbox.Text = processedData;

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



                    }


                }



            }

            //add to the form
            //getForm().Controls.Add(label);
            //getForm().Controls.Add(textbox);


            //colours for controls
            label.ForeColor = GlobalColours.mainTextColour;


            textbox.BackColor = GlobalColours.controlBackColour;



            //add controls to the panel
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);
            //add to the form
            
            getQM().getPanel().Controls.Add(labelDays);
            getQM().getPanel().Controls.Add(labelMonths);
            getQM().getPanel().Controls.Add(labelYears);
            getQM().getPanel().Controls.Add(labelResult);

            getQM().getPanel().Controls.Add(selectDays);
            getQM().getPanel().Controls.Add(selectMonths);
            getQM().getPanel().Controls.Add(selectYears);

            textbox.Focus();


            //set the console radio buttons
            setSkipSetting();



        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;

           
            //get the raw data from the user
            userData = textbox.Text;

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            //if (getQM().SkipThisQuestion)

            string skipSetting = getSkipSetting();
            if(skipSetting != null)
            {
                //yes

                    processedData = skipSetting;
                    //userData = skipSetting;
                    userData = "";

                    if (Validation == "CheckSameAsPrevious")
                    {

                        //get the previous value.
                        bool thisDataOK = testCheckSameAsPrevious(skipSetting);

                        if (thisDataOK)
                        {

                            return ToCode;



                        }
                        else
                        {


                            ((Form2)getBigMessageBox()).setLabel("The value you entered is not the same as the previous question: please try again");
                            getBigMessageBox().ShowDialog();

                            return OnErrorQuestionCompare;


                        }



                    }
                    else
                    {

                        return ToCode;

                    }
                
               

            }




            string errorMessage = null;

            bool dataOK = false;

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


            //convert to std date format
            //format: date:text
            //e.g. 12/2/2012:some text

            


            //validate the data
            //run the test accoring to the Validation
            if (Validation == "TestNullEntry")
            {
                dataOK = testNullEntry(userData);
                errorMessage = "Error: please try again";

            }


            
            else if (Validation == "AllowNullEntry")
            {
                //anything is allowed even nothing
                dataOK = true;



            }
            else if (Validation == "TestNumeric")
            {
                //must be a number

                dataOK = TestNumeric(userData);
                errorMessage = "Please enter a Number";



            }
            else if (Validation == "TestNumericNotZero")
            {
                //must be a number > 0

                dataOK = TestNumericNotZero(userData);
                errorMessage = "Please enter a Number larger than zero";


            }
            

            


            else if (Validation == "TestNumericInt")
            {
                //must be an integer number

                try
                {
                    Convert.ToInt32(userData);
                    dataOK = true;



                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a Number";

                }




            }

            

            else if (Validation == "CheckSameAsPrevious")
            {


                dataOK = testCheckSameAsPrevious(userData);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            


            else
            {
                //unknown code label: error
                //for now ignore this
                dataOK = true;
                errorMessage = "Internal Error: invalid Validation";


            }

            //if data is OK, process the data if needed
            if (dataOK)
            {

                
                
                //process the data

                //processed data is the entry
                processedData = userData;


                //save the date as special data, as David wants it to appear as a separate variable
                string date = selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt;

                getSD().Add(Code + "_DATE", date);

                nextCode = ToCode;


                /*
                if (Process == "NoModify")
                {
                    //make no changes
                    processedData = selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt + ":" + userData;
                    //advance to the next question
                    nextCode = ToCode;

                }
                
                
                else
                {
                    //add calls to specific processing methods here
                    processedData = selectedDayInt + "/" + selectedMonthsInt + "/" + selectedYearInt + ":" + userData;

                    //advance to the next question
                    nextCode = ToCode;

                }
                 */
 

                //save in global store if set.
                string globalKey = SetKey;
                if (globalKey != null)
                {
                    getGS().Add(globalKey, processedData);

                }



            }
            else
            {
                //validation has failed.



                //no
                //the next code is the same as this one
                //MessageBox.Show("Error: please try again");
                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();

                if (Validation == "CheckSameAsPrevious")
                {
                    nextCode = OnErrorQuestionCompare;
                }
                else
                {
                    nextCode = Code;

                }



            }

            return nextCode;



        }




        private bool testLessThan(string userData, int testVal)
        {

            

            //is this a number ?

            int num;

            try
            {
                num = Convert.ToInt32(userData);


            }
            catch (FormatException e)
            {

                //was not a number
                return false;


            }

            if (num <= testVal)
            {
                //OK
                return true;

            }
            else
            {
                return false;



            }




        }

        private bool TestNoNumbers(string userData)
        {
            //does this contain any numbers ?
             Match match = Regex.Match(userData, @"\d");


             if (match.Success)
             {
                 //contains at least 1 number
                 return false;

             }
             else
             {
                 return true;


             }
            



        }

        private bool TestOnlyNumbers(string userData)
        {
            //does this contain ONLY numbers ?
            Match match = Regex.Match(userData, @"^\d+$");


            if (match.Success)
            {
                //contains only numbers
                return true;

            }
            else
            {
                return false;


            }




        }

       

        

        private bool testCheckAgeSameAsOrLessThanAGE(string userData)
        {


            //is this age the same as or less than the  value entered for AGE ?

            int smokingAge;
            int currentAge;

            try
            {
                smokingAge = Convert.ToInt32(userData);
                currentAge = Convert.ToInt32(getGS().Get("AGE"));



            }
            catch (FormatException e)
            {

                return false;

            }


            if (smokingAge <= currentAge)
            {

                //both are the same
                return true;

            }
            else
            {
                //these differ
                return false;


            }





        }








    }
}
