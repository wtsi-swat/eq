
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
    class QuestionTextOneCheck : Question
    {
        //text box

        //fields
        private TextBox textbox;
        private Label label;
        private CheckBox checkbox;
        private Label checkBoxLabel;

        //reference to a subroutine that will do the validation
        private string Validation;

        //reference to a subroutine that will do any needed processing
        private string Process;

        

        //the data the user entered, which may be different to the processed data
        private string userData;


        //properties
        

        public string CheckBoxLabel { get; set; }

        public string CheckBoxCheckCode { get; set; }



        //constructor
        public QuestionTextOneCheck(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            


        }



        //methods

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

        

        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(textbox);
            getQM().getPanel().Controls.Remove(checkbox);
            getQM().getPanel().Controls.Remove(checkBoxLabel);

            checkbox.Dispose();
            checkBoxLabel.Dispose();

            label.Dispose();
            textbox.Dispose();



        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'


            //create a label and textbox control
            label = new Label();
            label.ForeColor = GlobalColours.mainTextColour;

            textbox = new TextBox();
            textbox.BackColor = GlobalColours.controlBackColour;

            checkBoxLabel = new Label();
            checkBoxLabel.ForeColor = GlobalColours.mainTextColour;

            checkbox = new CheckBox();


            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos;
            int textBoxYpos = labelYpos + 50;



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //position of the textbox
            textbox.Location = new Point(textBoxXpos, textBoxYpos);
            textbox.Size = new Size(500, 50);

            //label for the checkbox

            checkBoxLabel.Location = new Point(labelXpos + 20, labelYpos + 100);
            checkBoxLabel.Size = new Size(300, 50);
            checkBoxLabel.Text = CheckBoxLabel;

            //checkBox
            checkbox.Location = new Point(labelXpos, labelYpos + 100);
            checkbox.Size = new Size(20, 20);






            //if page seen before, populate the control with the previously entered text
            if (PageSeen)
            {
                
                //check for the special value which means user did not know
                if (userData == CheckBoxCheckCode)
                {
                    //check the checkbox
                    checkbox.Checked = true;


                }
                else
                {
                    textbox.Text = userData;

                }
                
                
                
                
                


            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);
            getQM().getPanel().Controls.Add(checkbox);
            getQM().getPanel().Controls.Add(checkBoxLabel);

            textbox.Focus();


           

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
            if (getQM().SkipThisQuestion)
            {
                //yes

                //do not overwrite data that may be there already
                if (processedData == null)
                {

                    processedData = "ENTRY_REFUSED";

                }

                return ToCode;




            }




            string errorMessage = null;

            bool dataOK = false;

            //validate the data
            //run the test accoring to the Validation

            //is the checkbox checked ?
            if (checkbox.Checked)
            {
                //this means that the user is not entering any text, so we should create a special value

                processedData = CheckBoxCheckCode;
                userData = CheckBoxCheckCode;

                nextCode = ToCode;
                dataOK = true;

               //return ToCode;



            }

            if (Validation == "TestNullEntry")
            {

                if (string.IsNullOrWhiteSpace(userData))
                {
                    dataOK = false;
                    errorMessage = "Please try again.";

                }
                else
                {
                    dataOK = true;

                }
                
                
            }
           

            else if (Validation == "TestDOB")
            {

                dataOK = testDOB(userData);
                errorMessage = "The date you entered is not consistent with the age: please try again.";
            }

            else if (Validation == "TestNumeric")
            {
                //must be a number

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

            else if (Validation == "TestSameAsRES")
            {
                //the answer must be the same as RES
                string previousData = getGS().Get("RES");

                if (userData != previousData)
                {
                    errorMessage = "This answer is not the same as the previous one: please try again";

                    ((Form2)getBigMessageBox()).setLabel(errorMessage);
                    getBigMessageBox().ShowDialog();
                    return "RES";



                }
                else
                {
                    dataOK = true;
                }



            }

            else if (Validation == "TestSameAsRMS")
            {
                //the answer must be the same as RMS
                string previousData = getGS().Get("RMS");

                if (userData != previousData)
                {
                    errorMessage = "This answer is not the same as the previous one: please try again";

                    ((Form2)getBigMessageBox()).setLabel(errorMessage);
                    getBigMessageBox().ShowDialog();
                    return "RMS";



                }
                else
                {
                    dataOK = true;

                }



            }

            else if (Validation == "CheckSameAsPrevious")
            {


                dataOK = testCheckSameAsPrevious(userData);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            else if ((Validation == "CheckAgeSameAsOrLessThanAGE") || (Validation == "CheckAgeSmokingStartLessThanAgeSmokingStop"))
            {

                
                //skip the test if checkbox was not checked.
                if ( ! checkbox.Checked)
                {
                    
                    //did the user enter anything ?
                    if (string.IsNullOrWhiteSpace(userData))
                    {

                        dataOK = false;
                        errorMessage = "Error: please try again";

                    }
                    else
                    {

                        //did they enter an integer ?
                        try
                        {
                            Convert.ToInt32(userData);

                            //skip the test if the AGE was not entered previously
                            string currentAge = getGS().Get("AGE");

                            if (currentAge == null)
                            {

                                Form3 warningBox = getQM().getWarningBox();

                                warningBox.setLabel("Warning: Can't compare with AGE as AGE was not entered");
                                warningBox.ShowDialog();

                                dataOK = true;


                            }
                            else
                            {
                                dataOK = testCheckAgeSameAsOrLessThanAGE(userData);
                                errorMessage = "The age entered here is not consistent with the previously entered age";

                                if (dataOK && (Validation == "CheckAgeSmokingStartLessThanAgeSmokingStop"))
                                {
                                    //do an extra check to make sure that age started smoking < age stopped smoking

                                    string ageStopped = getGS().Get("AGE_STOPPED_SMOKING");

                                    if (ageStopped == null)
                                    {

                                        Form3 warningBox = getQM().getWarningBox();

                                        warningBox.setLabel("Warning: Can't compare with AGE when smoking was stopped as that was not entered");
                                        warningBox.ShowDialog();

                                        dataOK = true;


                                    }
                                    else
                                    {


                                        decimal ageSmokingStarted = Convert.ToDecimal(userData);
                                        decimal ageSmokingStopped = Convert.ToDecimal(ageStopped);

                                        if (ageSmokingStarted > ageSmokingStopped)
                                        {

                                            dataOK = false;
                                            errorMessage = "The stop-smoking age must be greater than the start-smoking age";

                                        }
                                        else
                                        {
                                            dataOK = true;

                                        }
                                        
                                        

                                    }




                                }





                            }


                        }
                        catch
                        {

                            dataOK = false;
                            errorMessage = "Error: please enter a number";



                        }
                        
                       

                    }
                    
                   

                }



            }

            else if (Validation == "TestLessThan7")
            {
                dataOK = testLessThan(userData, 7);
                errorMessage = "You must enter a number that is 7 or less";

            }

            else if (Validation == "TestLessThan20")
            {
                dataOK = testLessThan(userData, 20);
                errorMessage = "You must enter a number that is 20 or less";

            }

            else if (Validation == "TestLessThan10")
            {
                dataOK = testLessThan(userData, 10);
                errorMessage = "You must enter a number that is 10 or less";

            }


            else
            {
                //unknown code label: error
                //for now ignore this
                dataOK = true;



            }

            //if data is OK, process the data if needed
            if (dataOK)
            {

                //process the data
                if (Process == "NoModify")
                {
                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;

                }

                else if (Process == "CalcCRWD")
                {
                    //calculate the crowding: CRWD
                    processedData = userData;

                    //int res = Convert.ToInt32(getGS().Get("RES"));
                    //int rms = Convert.ToInt32(userData);
                    decimal res = Convert.ToDecimal(getGS().Get("RES"));
                    decimal rms = Convert.ToDecimal(userData);


                    decimal crwd = res / rms;

                    getSD().Add("CRWD", crwd.ToString());

                    //advance to the next question
                    nextCode = ToCode;




                }
                else if (Process == "CheckForTOBCOM")
                {
                    processedData = userData;

                    //if this field contains 0 or is blank then goto ToCode
                    //if not then goto TOBCOM

                    if (string.IsNullOrWhiteSpace(userData) || userData == "0")
                    {
                        nextCode = ToCode;

                    }
                    else
                    {

                        nextCode = "TOBCOM";
                    }



                }
                else if (Process == "If0GotoDIET3")
                {
                    processedData = userData;

                    if (userData == "0")
                    {
                        nextCode = "DIET3";

                    }
                    else
                    {
                        nextCode = ToCode;

                    }


                }

                else if (Process == "If0GotoDIET5")
                {
                    processedData = userData;

                    if (userData == "0")
                    {
                        nextCode = "DIET5";

                    }
                    else
                    {
                        nextCode = ToCode;

                    }


                }







                else
                {
                    //add calls to specific processing methods here
                    processedData = userData;

                    //advance to the next question
                    nextCode = ToCode;

                }

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
                //increment the number of tries
                
                
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

                //if days==0 or months==0 => values are unknown
                //use a default day/month as 1 for this usage

                if (days == 0)
                {
                    days = 1;
                }

                if (months == 0)
                {
                    months = 1;

                }

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


        private bool testLessThan(string userData, int testVal)
        {

            //ignore this test if user checked don't know
            if (checkbox.Checked)
            {

                return true;
            }

            //is this a number ?

            int num;

            try
            {
                num = Convert.ToInt32(userData);


            }
            catch(FormatException e){

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
            catch(FormatException e){

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
