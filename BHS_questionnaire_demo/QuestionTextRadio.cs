
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
    class QuestionTextRadio : Question, IoptionList
    {
        //text box

        //fields
        protected TextBox textbox;
        private Label label;

        //reference to a subroutine that will do the validation
        //private string Validation;

        //reference to a subroutine that will do any needed processing
        //private string Process;

        

        //the data the user entered, which may be different to the processed data
        private string userData;

        //the value of the selected option
        private string selectedOptionValue;

        //a Radio Button set

        //fields
        List<Option> optionList;

        private GroupBox radioGroup;


        //properties
        //public string OnError { get; set; }

        public string RadioLabel { get; set; }

        //constructor
        public QuestionTextRadio(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            

            //init the optionslist
            optionList = new List<Option>();


        }



        //methods

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            Char[] delim = new Char[] { '\t' };

            //can I find this code in the dictionary
            if (uDataDict.ContainsKey(Code))
            {
                string line = uDataDict[Code];

                string[] parts = line.Split(delim);

                userData = parts[0];
                selectedOptionValue = parts[1];


            }
            else
            {
                userData = null;

            }



        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //save the user data

            if (userData != null)
            {

                //save the data stored in this object
                dhUserData.WriteLine(Code + "\t" + userData + "\t" + selectedOptionValue);

            }



        }

        public bool testQuestionRefs(Dictionary<string, Question> questionHash)
        {

            return Utils.testQuestionRefs(questionHash, optionList);




        }





        public void addOption(Option op)
        {
            optionList.Add(op);



        }

       
       

        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(textbox);

            label.Dispose();
            textbox.Dispose();

            getQM().getPanel().Controls.Remove(radioGroup);


            radioGroup.Dispose();



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
            label.Text = RadioLabel;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            //int textBoxXpos = labelXpos;
            //int textBoxYpos = labelYpos + 80;



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos + getWidgetHeight() + 20);
            //label.Size = new Size(getWidgetWidth(), 30);
            label.AutoSize = true;
            label.ForeColor = GlobalColours.mainTextColour;


            //position of the textbox
            textbox.Location = new Point(labelXpos, labelYpos + getWidgetHeight() + 60);
            textbox.Size = new Size(500, 50);
            textbox.BackColor = GlobalColours.controlBackColour;

            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {
                textbox.Text = userData;


            }

           

            
            //radio buttons
            
            radioGroup = new GroupBox();

            //set font size
            setFontSize(label, textbox, radioGroup);



            //position the group box under the label, i.e. at the same xpos but an increased ypos
            int groupBoxXpos = getWidgetXpos();
            int groupBoxYpos = getWidgetYpos();



            //position of the groupbox
            radioGroup.Location = new Point(groupBoxXpos, groupBoxYpos);

            


            radioGroup.Size = new Size(getWidgetWidth(), getWidgetHeight());
            radioGroup.Text = Val;
            radioGroup.ForeColor = GlobalColours.mainTextColour;



            //create the RadioButton objects that we need, i.e. one for each option.

            RadioButton rb;

            //create a radiobutton for each option
            foreach (Option op in optionList)
            {
                //create a radiobutton
                rb = new RadioButton();

                //trap the click
                rb.Click += new EventHandler(button_click);


                rb.Text = op.getText();
                //rb.Tag = op.getValue();
                rb.Tag = op;
                rb.Location = new Point(op.getWidgetXpos(), op.getWidgetYpos());
                rb.Size = new Size(op.getWidgetWidth(), op.getWidgetHeight());
                rb.ForeColor = GlobalColours.mainTextColour;

                if (!PageSeen)
                {
                    if (op.Default)
                    {
                        rb.Checked = true;
                    }

                }
                else
                {
                    //page has been seen, set the previous data
                    if (op.getValue() == selectedOptionValue)
                    {
                        rb.Checked = true;

                    }

                }




                radioGroup.Controls.Add(rb);
                

            }

            //add controls to the form


            getQM().getPanel().Controls.Add(radioGroup);
            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);

            textbox.Focus();

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
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
                //yes

               
                    processedData = skipSetting;



                    if (Process == "CalcYearlyIncomeAndComparePrevious")
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

            string errorMessage="foo";

            bool dataOK;

            string optionData;

            //process the selected option

            //find the selected button
            RadioButton selectedButton = null;
            foreach (RadioButton rb in radioGroup.Controls)
            {

                if (rb.Checked)
                {
                    selectedButton = rb;
                    break;


                }



            }

            //check if the user has not made a selection
            if (selectedButton == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must select an option");
                getBigMessageBox().ShowDialog();

                return Code;


            }

            //get the tag from the selected button
            Option selectedOption = (Option)(selectedButton.Tag);
            optionData = selectedOption.getValue();
            selectedOptionValue = optionData;

            
            
                //do validation
                if (Validation == "TestNullEntry")
                {
                    dataOK = testNullEntry(userData);
                    errorMessage = "Please Try Again";

                }

                else if (Validation == "TestNumeric")
                {
                    //must be a real number

                    try
                    {
                        Convert.ToDecimal(userData);
                        dataOK = true;



                    }
                    catch
                    {
                        dataOK = false;
                        errorMessage = "Please enter a Number";

                    }




                }

                else if (Validation == "TestDOB")
                {

                    dataOK = testDOB(userData);
                    errorMessage = "The date you entered is not consistent with the age: please try again.";
                }

                else if (Validation == "CheckAgeSameAsOrLessThanAGE")
                {

                    dataOK = testCheckAgeSameAsOrLessThanAGE(userData, selectedOptionValue);
                    errorMessage = "The age you entered is not consistent with the previous age (Question 8): please try again.";


                }

                else if (Validation == "CheckSameAsPrevious")
                {


                    dataOK = testCheckSameAsPrevious(userData);
                    errorMessage = "The value you entered is not the same as the previous question: please try again";


                }


                else if (Validation == "TestSameAsParticipantID")
                {

                    //the barcode should be the same as previously entered in IDNO
                    errorMessage = "this barcode does not match the participant ID";

                    dataOK = TestSameAsParticipantID(userData);



                }



                else if (Validation == "TestBloodSerum" || Validation == "TestBloodEDTA" || Validation == "TestBloodNAF" || Validation == "TestBlood")
                {
                    //this is the barcode from the serum tube: we need to check that it matches the same group as the master lab barcode
                    string typeSuffix="";


                    if (Validation == "TestBloodSerum")
                    {
                        typeSuffix = "S1";

                    }
                    else if (Validation == "TestBloodEDTA")
                    {

                        typeSuffix = "E1";

                    }
                    else if (Validation == "TestBloodNAF")
                    
                    {
                        //TestBloodNAF
                        typeSuffix = "G1";


                    }

                    string masterBarCode = getGS().Get("BLOODMASTER");

                    if (masterBarCode == null)
                    {

                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with the master barcode, which was not entered");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {

                        //e.g. master BGZ100
                        //the serum tube should be <master>S1

                        if (string.IsNullOrWhiteSpace(userData))
                        {
                            //no entry
                            dataOK = false;
                            errorMessage = "Please scan a barcode";


                        }
                        else
                        {
                            //compare with master
                            string expectedBarcode = masterBarCode + typeSuffix;

                            if (expectedBarcode == userData)
                            {
                                dataOK = true;

                            }
                            else
                            {
                                dataOK = false;
                                errorMessage = "You have entered the wrong barcode";


                            }



                        }

                    }



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

                if (selectedOptionValue == "Don't know")
                {
                    processedData = "Don't know";


                }
                else
                {

                    //process the data
                    if (Process == "NoModify")
                    {
                        //make no changes
                        //processedData = userData;
                        processedData = selectedOptionValue + ":" + userData;

                    }

                    else if (Process == "CalcYearlyIncome")
                    {
                        //calc yearly income and save this as special data

                        //for this qcode save the original values

                        processedData = selectedOptionValue + ":" + userData;

                        string yearlyIncome = calcYearlyIncome(userData, optionData);

                        getSD().Add("YEARLY_INCOME", yearlyIncome);





                    }

                    else if (Process == "CalcYearlyIncomeAndComparePrevious")
                    {
                        //this is the second try at entering the yearly income and it should give the same result as the first try

                        //string previousYearlyIncome = getGS().Get("AverageYearlyEarnings1");

                        string previousYearlyIncome = getSD().Get("YEARLY_INCOME");
                        string thisYearlyIncome = calcYearlyIncome(userData, optionData);

                        if (previousYearlyIncome == null)
                        {
                            //user has skipped first attempt
                            //issue warning:

                            Form3 warningBox = getQM().getWarningBox();

                            warningBox.setLabel("Warning: Can't compare Yearly Income with first entry (not entered)");
                            warningBox.ShowDialog();

                            //processedData = thisYearlyIncome;
                            processedData = selectedOptionValue + ":" + userData;


                        }
                        else
                        {



                            //are these the same ?
                            if (previousYearlyIncome != thisYearlyIncome)
                            {
                                //failed.
                                ((Form2)getBigMessageBox()).setLabel("Error: Yearly income not the same for both attempts");
                                getBigMessageBox().ShowDialog();
                                nextCode = OnErrorQuestionCompare;
                                return nextCode;


                            }
                            else
                            {

                                //processedData = thisYearlyIncome;
                                processedData = selectedOptionValue + ":" + userData;

                            }

                        }



                    }

                    else if (Process == "SetNullEntry")
                    {
                        //if userdata is null save as "No Entry"
                        if (string.IsNullOrEmpty(userData))
                        {
                            processedData = "No User Entry";

                        }
                        else
                        {
                            processedData = userData;

                        }


                    }




                    else
                    {
                        //add calls to specific processing methods here
                        processedData = userData;

                    }


                    //if we have a global setting: save to the global object
                    string globalKey = SetKey;
                    if (globalKey != null)
                    {
                        getGS().Add(globalKey, processedData);

                    }


                    

                }


                if (selectedOption.ToCode == null)
                {
                    nextCode = ToCode;

                }
                else
                {

                    nextCode = selectedOption.ToCode;

                }
                
                
                
                
                
                //advance to the next question
                
                
               

            }
            else
            {
                //validation has failed.
                //increment the number of tries
               
                
                
                //no
                //the next code is the same as this one
                //MessageBox.Show("Error: please try again");
                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();


                nextCode = Code;


            }



            return nextCode;



        }



        private string calcYearlyIncome(string userData, string timeOption)
        {

            //option will be W (week), M (month) or Y (year)

            decimal income = Convert.ToDecimal(userData);
            decimal incomePerYear;

            if (timeOption == "W")
            {
                //user entered earnings per week -> convert to per year
                incomePerYear= income * 52.177457M;



            }

            else if(timeOption == "M"){

                //user entered earnings per month -> convert to per year
                incomePerYear= income * 12.0M;


            }
            
            else {

                //user entered earnings per year -> return unchanged
                incomePerYear= income;


            }

            //convert to a string (round to 2 decimal places)
            return Math.Round(incomePerYear, 2).ToString();






        }






        private bool testDOB(string userData)
        {
            //is this age consistent with the previously entered Date of Birth
            string dobAsStr = getGS().Get("DOB");

            DateTime today = DateTime.Now;
            DateTime dob;

            //extract the day, months, years.
            Match match = Regex.Match(dobAsStr, @"(\d+)/(\d+)/(\d+)");

            int days, months, years;

            if (match.Success)
            {
                days = Convert.ToInt32(match.Groups[1].Value);
                months = Convert.ToInt32(match.Groups[2].Value);
                years = Convert.ToInt32(match.Groups[3].Value);

                //MessageBox.Show("days:" + days + " months:" + months + " years:" + years);

                dob = new DateTime(years, months, days);



            }
            else
            {
                throw new Exception("date parsing failed");

            }

            //calc the elapsed time
            TimeSpan elapsedTime = today - dob;
            double elapsedYears = elapsedTime.TotalDays / 365.25;

            //MessageBox.Show("years:" + elapsedYears);

            //is this approx the same as the userData ?
            //lets leave a window of 1 year

            //the number of years entered by the user
            double userYears = Convert.ToDouble(userData);


            double yearsLowerBound = userYears - 1;
            double yearsUpperBound = userYears + 2;

            if ((elapsedYears >= yearsLowerBound) && (elapsedYears <= yearsUpperBound))
            {

                //OK
                return true;

            }
            else
            {
                //failed: outside allowed range
                return false;


            }




        }





    

        private bool testCheckAgeSameAsOrLessThanAGE(string userData, string selectedOptionValue)
        {
            //has the user entered an age or did they choose don't know

            if (selectedOptionValue == "age")
            {
                //is this age the same as or less than the  value entered for AGE ?

                

                int smokingAge = Convert.ToInt32(userData);
                int currentAge= Convert.ToInt32(getGS().Get("AGE"));




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
            else
            {
                //don't know
                //we can't do anything here
                return true;

            }





        }





    }
}
