
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
    class QuestionTextTriple : Question
    {
        //text box

        //fields
        private TextBox textbox1;
        private TextBox textbox2;
        private TextBox textbox3;
        private Label mainLabel;
        private Label subLabel1;
        private Label subLabel2;
        private Label subLabel3;

        private string reading1= null;
        private string reading2= null;
        private string reading3= null;

        private decimal reading1asDec;
        private decimal reading2asDec;
        private decimal reading3asDec;

        private bool has3Readings = false;



        //reference to a subroutine that will do the validation
        private string Validation;

        //reference to a subroutine that will do any needed processing
        private string Process;

        

        //the data the user entered, which may be different to the processed data
        private string userData;

        private string userDataFormatted;


        //properties
        //public string OnError { get; set; }

        //constructor
        public QuestionTextTriple(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
           


        }



        //methods

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            Char[] delim = new Char[] {'\t' };

            //can I find this code in the dictionary
            if (uDataDict.ContainsKey(Code))
            {
                string line = uDataDict[Code];

                string[] parts = line.Split(delim);

                reading1 = parts[0];
                reading2 = parts[1];
                reading3 = parts[2];



            }
            



        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //save the user data
            //convert null strings to empty

            string r1, r2, r3;

            bool hasData = false;

            if (reading1 == null)
            {
                r1 = "";

            }
            else
            {
                r1 = reading1;
                hasData = true;

            }

            if (reading2 == null)
            {
                r2 = "";

            }
            else
            {
                r2 = reading2;
                hasData = true;

            }
            if (reading3 == null)
            {
                r3 = "";

            }
            else
            {
                r3 = reading3;
                hasData = true;

            }

            if (hasData)
            {
                dhUserData.WriteLine(Code + "\t" + r1 + "\t" + r2 + "\t" + r3);

            }
            

           

        }


        public string getReading1()
        {
            return reading1;

        }

        public string getReading2()
        {
            return reading2;


        }

        public string getReading3()
        {
            return reading3;


        }


        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(mainLabel);
            getQM().getPanel().Controls.Remove(textbox1);
            getQM().getPanel().Controls.Remove(textbox2);
            getQM().getPanel().Controls.Remove(textbox3);
            getQM().getPanel().Controls.Remove(subLabel1);
            getQM().getPanel().Controls.Remove(subLabel2);
            getQM().getPanel().Controls.Remove(subLabel3);



            textbox1.Dispose();
            textbox2.Dispose();
            textbox3.Dispose();
            mainLabel.Dispose();
            subLabel1.Dispose();
            subLabel2.Dispose();
            subLabel3.Dispose();

            




        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            mainLabel = new Label();
            mainLabel.ForeColor = GlobalColours.mainTextColour;

            textbox1 = new TextBox();
            textbox1.BackColor = GlobalColours.controlBackColour;

            textbox2 = new TextBox();
            textbox2.BackColor = GlobalColours.controlBackColour;

            textbox3 = new TextBox();
            textbox3.BackColor = GlobalColours.controlBackColour;

            subLabel1 = new Label();
            subLabel1.ForeColor = GlobalColours.mainTextColour;

            subLabel2 = new Label();
            subLabel2.ForeColor = GlobalColours.mainTextColour;

            subLabel3 = new Label();
            subLabel3.ForeColor = GlobalColours.mainTextColour;



            //the question Text shown to the user
            mainLabel.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();


            //position of the Label
            mainLabel.Location = new Point(labelXpos, labelYpos);
            mainLabel.Size = new Size(getWidgetWidth(), getWidgetHeight());

            //label1
            subLabel1.Location = new Point(labelXpos, labelYpos + 50);
            subLabel1.Size = new Size(200, 50);
            subLabel1.Text = "Reading 1";


            //label2
            subLabel2.Location = new Point(labelXpos, labelYpos + 100);
            subLabel2.Size = new Size(200, 50);
            subLabel2.Text = "Reading 2";

            //label3
            subLabel3.Location = new Point(labelXpos, labelYpos + 150);
            subLabel3.Size = new Size(200, 50);
            subLabel3.Text = "Reading 3";

            //textbox1
            textbox1.Location = new Point(labelXpos + 250, labelYpos + 50);
            textbox1.Size = new Size(200, 50);


            //textbox2
            textbox2.Location = new Point(labelXpos + 250, labelYpos + 100);
            textbox2.Size = new Size(200, 50);

            //textbox3
            textbox3.Location = new Point(labelXpos + 250, labelYpos + 150);
            textbox3.Size = new Size(200, 50);



            //if page seen before, populate the control with the previously entered text
            if (PageSeen)
            {

                if (reading1 == null)
                {
                    textbox1.Text = "";
                }
                else
                {
                    textbox1.Text = reading1;

                }

                if (reading2 == null)
                {
                    textbox2.Text = "";
                }
                else
                {
                    textbox2.Text = reading2;

                }

                if (reading3 == null)
                {
                    textbox3.Text = "";
                }
                else
                {
                    textbox3.Text = reading3;

                }



            }

            //add to the form
            //getForm().Controls.Add(mainLabel);
            //getForm().Controls.Add(textbox1);
            //getForm().Controls.Add(textbox2);
            //getForm().Controls.Add(textbox3);
            //getForm().Controls.Add(subLabel1);
            //getForm().Controls.Add(subLabel2);
            //getForm().Controls.Add(subLabel3);

            getQM().getPanel().Controls.Add(mainLabel);
            getQM().getPanel().Controls.Add(textbox1);
            getQM().getPanel().Controls.Add(textbox2);
            getQM().getPanel().Controls.Add(textbox3);
            getQM().getPanel().Controls.Add(subLabel1);
            getQM().getPanel().Controls.Add(subLabel2);
            getQM().getPanel().Controls.Add(subLabel3);

            textbox1.Focus();


            



        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;


            //get the raw data from the user
            reading1 = textbox1.Text;
            reading2 = textbox2.Text;
            reading3 = textbox3.Text;




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

            bool dataOK = true;

            //validate the data

            //1 or both fields must contain something
            if (Validation == "Check3NumericReadings")
            {

                //there must be 3 values

                

                if (string.IsNullOrWhiteSpace(reading1) || string.IsNullOrWhiteSpace(reading2) || string.IsNullOrWhiteSpace(reading3))
                {
                    dataOK = false;

                }
                else{

                    //check the data are numeric
                    try{

                        reading1asDec= Convert.ToDecimal(reading1);
                        reading2asDec= Convert.ToDecimal(reading2);
                        reading3asDec= Convert.ToDecimal(reading3);
                    }
                    catch(FormatException e){

                        dataOK= false;


                    }


                }
                
                

            }

            else if (Validation == "Check2NumericReadings")
            {

                //the first 2 values must be present and be numbers
                //the third value is optional



                if (string.IsNullOrWhiteSpace(reading1) || string.IsNullOrWhiteSpace(reading2))
                {
                    dataOK = false;

                }
                else
                {

                    //check the data are numeric
                    try
                    {

                        reading1asDec = Convert.ToDecimal(reading1);
                        reading2asDec = Convert.ToDecimal(reading2);
                        
                    }
                    catch (FormatException e)
                    {

                        dataOK = false;


                    }


                }

                //check for the 3rd reading
                if (string.IsNullOrWhiteSpace(reading3)){

                    //not present
                    has3Readings = false;



                }
                else{

                    has3Readings = true;
                    
                    //is present
                    try
                    {

                        reading3asDec = Convert.ToDecimal(reading3);
                       
                        
                    }
                    catch (FormatException e)
                    {

                        dataOK = false;


                    }


                }



            }





            else if (Validation == "CheckSameAsPrevious")
            {
                

                dataOK = testCheckSameAsPrevious(userDataFormatted);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            else
            {

                //should not get to here:
                throw new Exception();


            }

            //if data is OK, process the data if needed
            if (dataOK)
            {

                if (Process == "CalcAverageSecondAndThirdReadingsSYST")
                {

                    //take the average of the second and third readings

                    decimal averageSYST = (reading2asDec + reading3asDec) / 2;
                    processedData = averageSYST.ToString();

                    //save in GS also
                    getGS().Add("SYSTAVG", processedData);

                    nextCode = ToCode;

                    //the average is stored as the main result, but we also want to store each reading as well
                    //use the special store

                    getSD().Add("SYST1", reading1);
                    getSD().Add("SYST2", reading2);
                    getSD().Add("SYST3", reading3);

                    //check if this result is outside the usual range
                    if((averageSYST  <= 55) || (averageSYST >= 299)){

                        //ref to the Questionnaire form
                        Form1 mainForm = getQM().getMainForm();
                        
                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The systolic value is unusal (average is " + averageSYST.ToString() + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;


                        //if (MessageBox.Show("The systolic value is unusal (average is " + averageSYST.ToString() + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                       
                        
                        
                        if(buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox
                            
                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_SYST", processedData);



                        }


                    }


                    



                }

                else if (Process == "CalcAverageSecondAndThirdReadingsDIAST")
                {

                    //take the average of the second and third readings

                    decimal averageDIAST = (reading2asDec + reading3asDec) / 2;
                    processedData = averageDIAST.ToString();

                    //save in GS also
                    getGS().Add("DIASTAVG", processedData);

                    nextCode = ToCode;

                    //the average is stored as the main result, but we also want to store each reading as well
                    //use the special store

                    getSD().Add("DIAST1", reading1);
                    getSD().Add("DIAST2", reading2);
                    getSD().Add("DIAST3", reading3);

                    //check if this result is outside the usual range
                    if ((averageDIAST <= 40) || (averageDIAST >= 200))
                    {

                        //ref to the Questionnaire form
                        Form1 mainForm = getQM().getMainForm();

                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The diastolic value is unusal (average is " + averageDIAST.ToString() + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;
                        
                        
                        
                        
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_DIAST", processedData);



                        }


                    }






                }

                else if (Process == "CalcAverageSecondAndThirdReadingsPLS")
                {

                    //take the average of the second and third readings

                    decimal averagePLS = (reading2asDec + reading3asDec) / 2;
                    processedData = averagePLS.ToString();

                    nextCode = ToCode;

                    //the average is stored as the main result, but we also want to store each reading as well
                    //use the special store

                    getSD().Add("PLS1", reading1);
                    getSD().Add("PLS2", reading2);
                    getSD().Add("PLS3", reading3);

                    //check if this result is outside the usual range
                    if ((averagePLS <= 30) || (averagePLS >= 180))
                    {

                        //ref to the Questionnaire form
                        Form1 mainForm = getQM().getMainForm();

                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The pulse value is unusal (average is " + averagePLS.ToString() + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;



                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_PLS", processedData);



                        }


                    }


                }

                else if (Process == "WCAverage")
                {
                    ///waist circumference
                    nextCode = ToCode;

                    decimal average;
                    
                    //do we have 2 measurements or 3 measurements ?
                    if (has3Readings)
                    {
                        
                        
                        //3 readings
                        getSD().Add("WC1", reading1);
                        getSD().Add("WC2", reading2);
                        getSD().Add("WC3", reading3);

                        //average the 2 closest measurements out of the 3

                        //reading1 vs reading2
                        decimal r1_r2 = Math.Abs(reading1asDec - reading2asDec);

                        //reading1 vs reading 3
                        decimal r1_r3 = Math.Abs(reading1asDec - reading3asDec);

                        //reading2 vs reading3
                        decimal r2_r3 = Math.Abs(reading2asDec - reading3asDec);

                        //which are the 2 closest measurements, i.e. with smallest difference ?

                        decimal smallestDiff = findSmallestDiff(r1_r2, r1_r3, r2_r3);

                        

                        if (smallestDiff == r1_r2)
                        {
                            average = (reading1asDec + reading2asDec) / 2;

                        }
                        else if (smallestDiff == r1_r3)
                        {

                            average = (reading1asDec + reading3asDec) / 2;

                        }
                        else
                        {
                            //r2_r3
                            average = (reading2asDec + reading3asDec) / 2;


                        }

                        processedData = average.ToString();

                        //save in GS also
                        getGS().Add("WCAVG", processedData);



                    }
                    else
                    {
                        //2 readings
                        //is there a difference of > 3 cm between readings 1 and 2
                        if (Math.Abs(reading1asDec - reading2asDec) > 3)
                        {

                            //prompt for third measurement
                            ((Form2)getBigMessageBox()).setLabel("There is more than a 3 cm difference between readings. Please enter a 3rd reading");
                            getBigMessageBox().ShowDialog();


                            nextCode = Code;

                            //dummy value to stop triggering messagebox below
                            average = 50;




                        }
                        else
                        {
                            //2 readings OK, take average
                            getSD().Add("WC1", reading1);
                            getSD().Add("WC2", reading2);

                            average = (reading1asDec + reading2asDec) / 2;
                            processedData = average.ToString();

                            //save in GS also
                            getGS().Add("WCAVG", processedData);




                        }



                    }

                    if ((average <= 30) || (average >= 180))
                    {
                        Form1 mainForm = getQM().getMainForm();

                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The waist circumference is unusal (average is " + average.ToString() + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;

                        //if (MessageBox.Show("The waist circumference is unusal (average is " + average.ToString() + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_WC", processedData);



                        }


                    }


                }

                else if (Process == "HCAverage")
                {
                    ///hip circumference
                    nextCode = ToCode;

                    decimal average;

                    //do we have 2 measurements or 3 measurements ?
                    if (has3Readings)
                    {


                        //3 readings
                        getSD().Add("HIPC1", reading1);
                        getSD().Add("HIPC2", reading2);
                        getSD().Add("HIPC3", reading3);

                        //average the 2 closest measurements out of the 3

                        //reading1 vs reading2
                        decimal r1_r2 = Math.Abs(reading1asDec - reading2asDec);

                        //reading1 vs reading 3
                        decimal r1_r3 = Math.Abs(reading1asDec - reading3asDec);

                        //reading2 vs reading3
                        decimal r2_r3 = Math.Abs(reading2asDec - reading3asDec);

                        //which are the 2 closest measurements, i.e. with smallest difference ?

                        decimal smallestDiff = findSmallestDiff(r1_r2, r1_r3, r2_r3);



                        if (smallestDiff == r1_r2)
                        {
                            average = (reading1asDec + reading2asDec) / 2;

                        }
                        else if (smallestDiff == r1_r3)
                        {

                            average = (reading1asDec + reading3asDec) / 2;

                        }
                        else
                        {
                            //r2_r3
                            average = (reading2asDec + reading3asDec) / 2;


                        }

                        processedData = average.ToString();

                        //save in GS also
                        getGS().Add("HIPCAVG", processedData);



                    }
                    else
                    {
                        //2 readings
                        //is there a difference of > 3 cm between readings 1 and 2
                        if (Math.Abs(reading1asDec - reading2asDec) > 3)
                        {

                            //prompt for third measurement
                            ((Form2)getBigMessageBox()).setLabel("There is more than a 3 cm difference between readings. Please enter a 3rd reading");
                            getBigMessageBox().ShowDialog();


                            nextCode = Code;

                            //dummy value to stop triggering messagebox below
                            average = 50;




                        }
                        else
                        {
                            //2 readings OK, take average
                            getSD().Add("HIPC1", reading1);
                            getSD().Add("HIPC2", reading2);

                            average = (reading1asDec + reading2asDec) / 2;
                            processedData = average.ToString();

                            //save in GS also
                            getGS().Add("HIPCAVG", processedData);




                        }



                    }

                    if ((average <= 45) || (average >= 200))
                    {
                        //ref to the Questionnaire form
                        Form1 mainForm = getQM().getMainForm();

                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The hip circumference is unusal (average is " + average.ToString() + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;

                        //if (MessageBox.Show("The hip circumference is unusal (average is " + average.ToString() + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)

                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_HIPC", processedData);



                        }


                    }


                }



                else
                {

                    nextCode = ToCode;

                }
                
                



            }
            else
            {
                //validation has failed.
                
                
                
                //no
                //the next code is the same as this one
                //MessageBox.Show("Error: please try again");
                ((Form2)getBigMessageBox()).setLabel("Error: please try again");
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


        protected override bool testCheckSameAsPrevious(string userData)
        {
            //Note: this overrides the method in Question, as we have 3 values to check.
            
            //is this the same as the previous userdata ?

            //get the previous question code
            string previousQuestionCode = FromCode;

            //get the previous question object

            QuestionTextTriple previousQuestion = (QuestionTextTriple)getQM().getQuestion(previousQuestionCode);


            string previousReading1 = previousQuestion.getReading1();
            string previousReading2 = previousQuestion.getReading2();
            string previousReading3 = previousQuestion.getReading3();

            //compare readings 1,2,3 from this question with the previous one.

            //the average of the readings
            string previousProcessedData = previousQuestion.processedData;

            //check the special case where that was skipped
            if (previousProcessedData == "No User Entry")
            {
                //skip this test
                return true;



            }
            else
            {

                if ((reading1 == previousReading1) && (reading2 == previousReading2) && (reading3 == previousReading3))
                {
                    //data OK
                    return true;

                }
                else
                {
                    //error
                    return false;

                }




            }


        }

        private decimal findSmallestDiff(decimal v1, decimal v2, decimal v3)
        {

            List<decimal> list = new List<decimal>();

            list.Add(v1);
            list.Add(v2);
            list.Add(v3);

            list.Sort();

            return list[0];

           


        }




        

        





    }
}


