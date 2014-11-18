
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
    class QuestionText : Question
    {
        //text box

        //fields
        private TextBox textbox;
        private Label label;

        //reference to a subroutine that will do the validation
        //private string Validation;

        //reference to a subroutine that will do any needed processing
        //private string Process;


        //the data the user entered, which may be different to the processed data
        private string userData= null;


        //properties
       


        //constructor
        public QuestionText(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



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
            //getForm().Controls.Remove(label);
            //getForm().Controls.Remove(textbox);

            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(textbox);

            label.Dispose();
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

            //set font size
            setFontSize(label, textbox);

 

            
            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);



            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos;
            //int textBoxYpos = labelYpos + 50;

            //MessageBox.Show("gap is:" + LabelToBoxGap);

            //int textBoxYpos = labelYpos + LabelToBoxGap + 50;
            int textBoxYpos = labelYpos + LabelToBoxGap + getWidgetHeight();



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //position of the textbox
            textbox.Location = new Point(textBoxXpos, textBoxYpos);
            


            //is this a textarea ?
            if (MultiLine)
            {
                textbox.Multiline = true;
                textbox.Size = new Size(700, 300);



            }
            else
            {
                //1 line textbox
                textbox.Size = new Size(500, 50);


            }





            

            if (PageSeen)
            {
                textbox.Text = userData;


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


            //make sure we see the label for multiline
            if (MultiLine)
            {
                label.Focus();

            }
            else
            {

                textbox.Focus();

            }

           


            //set the console radio buttons
            setSkipSetting();

            //start audio recording if enabled
            audioRecording();


            //set scrollbar to zero
            //getQM().getMainForm().setVerticalScrollBarPos();



        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;

            //these are used to avoid a standard error box when height or weight are shown as unusal in warning box.
            bool heightBad = false;
            bool weightBad = false;
            bool armCircBad = false;


            //get the raw data from the user
            userData = textbox.Text;

            if (MultiLine)
            {
                //userData might contain line-breaks, which will mess up the data formatting
                userData = userData.Replace("\r\n", " ");
                




            }

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

                    else if (Process == "SPECIAL:H3A_TOB4B")
                    {
                        //If  no answer or don’t know or 00 go to question 30a [TOB30a]
                        if (processedData == "No Answer" || processedData == "Don't Know")
                        {
                            return "TOB30A";


                        }
                        else
                        {

                            return ToCode;

                        }




                    }

                    else
                    {

                        return ToCode;

                    }
                
               

            }




            string errorMessage = null;

            bool dataOK = false;

            //validate the data
            //run the test accoring to the Validation
            if (Validation == "TestNullEntry")
            {

                //MessageBox.Show("testing null entry");
                
                dataOK = testNullEntry(userData);
                errorMessage = "Error: please try again";

            }


            else if (Validation == "TestDiagnosisAge")
            {

                dataOK = testDiagnosisAge(userData);
                errorMessage = "Error: age is not consistent with years of birth and diagnosis";


            }

            else if (Validation == "TestInKnownIDNOset")
            {
                //OK if the IDNO is in the set of IDNOs that is included in the .conf file
                //get config object.

                errorMessage = "Error: this IDNO is not in the allowed set";

                Qconfig q = getQM().getMainForm().config;

                HashSet<string> idnoSet = q.IDNOset;

                if (idnoSet.Contains(userData))
                {
                    dataOK = true;


                }
                else
                {
                    dataOK = false;


                }





            }

            else if (Validation == "TestSameAsUserID")
            {
                //the value entered here must match the original user ID entered when the form was created

                string userID = getQM().getUserID();
                errorMessage = "Error: Not the same as Questionnaire ID";

                if (userID == userData)
                {
                    dataOK = true;

                }
                else
                {

                    dataOK = false;


                }



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

            else if (Validation == "TestNumberBetween0and100")
            {

                try
                {
                    decimal amount = Convert.ToDecimal(userData);

                    if (amount >= 0 && amount <= 100)
                    {
                        dataOK = true;

                    }
                    else
                    {
                        dataOK = false;
                        errorMessage = "Error: Please enter a number between 0 and 100";

                    }


                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Error: Please enter a number between 0 and 100";

                }







            }
            else if (Validation == "TestNumericNotZero")
            {
                //must be a number > 0

                dataOK = TestNumericNotZero(userData);
                errorMessage = "Please enter a Number larger than zero";


            }

            else if (Validation == "TestBetween1and20")
            {
                dataOK = TestBetween(userData, 1, 20);
                errorMessage = "Please enter a number between 1 and 20";



            }

            else if (Validation == "TestBetween0and20")
            {
                dataOK = TestBetweenDecimal(userData, 0, 20);
                errorMessage = "Please enter a number > 0 and <= 20";



            }



            else if (Validation == "TestNoNumbers")
            {

                //first, check we have something
                dataOK = testNullEntry(userData);
                if (dataOK)
                {


                    //string cannot contain any numbers
                    dataOK = TestNoNumbers(userData);
                    errorMessage = "Entry must not contain number(s)";



                }
                else
                {

                    errorMessage = "Error: please try again";
                }





            }

            else if (Validation == "TestOnlyNumbers")
            {

                //first, check we have something
                dataOK = testNullEntry(userData);
                if (dataOK)
                {


                    //string must contain only numbers
                    dataOK = TestOnlyNumbers(userData);
                    errorMessage = "Entry must contain ONLY numbers";



                }
                else
                {

                    errorMessage = "Error: please try again";
                }





            }
            else if (Validation == "TestArmCirc")
            {

                //arm circumference (no smaller than 10 cm and no larger than 45cm)

                try
                {
                    decimal circ = Convert.ToDecimal(userData);

                    if (circ < 10)
                    {
                        dataOK = false;
                        errorMessage = "arm circumference must be between 10 cm and 60 cm";



                    }
                    else
                    {
                        dataOK = true;

                        if (circ > 60)
                        {
                            //warning
                            Form1 mainForm = getQM().getMainForm();
                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The arm circumference is unusal (" + userData + "). Is this correct ?";
                            confirmBox.setFormLabel(confLabel, mainForm);
                            confirmBox.ShowDialog();

                            //the confirmbox calls back to the mainForm which button was pressed
                            string buttonResult = mainForm.confirmResult;



                            if (buttonResult == "no")
                            {
                                armCircBad = true;

                            }




                        }

                    }



                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a Number";

                }



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
            else if (Validation == "TestPhoneNumber")
            {

                dataOK = TestPhoneNumber(userData);
                errorMessage = "Please enter a phone number (10 digits, no other symbols)";



            }

            else if (Validation == "TestDOB")
            {

                //make sure that a number was entered
                try
                {
                    Convert.ToDouble(userData);

                    //OK it is an integer
                    dataOK = testDOB(userData, 18);
                    errorMessage = "The age you entered is not consistent with the date";


                }
                catch
                {

                    //not a number

                    dataOK = false;
                    errorMessage = "Please enter a number";

                }



            }

            else if (Validation == "HEPC:TestDOB")
            {

                //make sure that a number was entered
                try
                {
                    Convert.ToDouble(userData);

                    //OK it is an integer
                    dataOK = testDOB(userData, 13);
                    errorMessage = "The age you entered is not consistent with the date";


                }
                catch
                {

                    //not a number

                    dataOK = false;
                    errorMessage = "Please enter a number";

                }







                /*
                //make sure that a number was entered
                try
                {
                    Convert.ToDouble(userData);

                    //OK it is an integer
                    dataOK = testDOB(userData);

                    if (!dataOK)
                    {
                        errorMessage = "The age you entered is not consistent with the date: please try again.";
                        ((Form2)getBigMessageBox()).setLabel(errorMessage);
                        getBigMessageBox().ShowDialog();

                        return "DOB";

                    }



                }
                catch
                {

                    //not a number

                    dataOK = false;
                    errorMessage = "Please enter a number";

                }
                 */ 



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

            else if (Validation == "TestLessThanTotalSibs")
            {
                //this value must be an integer and be <= total siblings
                try
                {
                    int numSibsThis = Convert.ToInt32(userData);

                    string numSibsStr = getGS().Get("NUMSIBS");

                    if (numSibsStr == null)
                    {
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with total number of siblings, as the total was not entered previously");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {
                        int numSibsTotal = Convert.ToInt32(numSibsStr);

                        if (numSibsThis <= numSibsTotal)
                        {
                            //OK
                            dataOK = true;


                        }
                        else
                        {
                            //error
                            dataOK = false;
                            errorMessage = "Error: this number must be <= the total number of siblings";

                        }



                    }





                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a whole Number";

                }



            }

            else if (Validation == "TestBrotherPlusSistersLessThanTotalSibs")
            {
                //this value is the total sisters (when both brothers and sisters effected)
                //check that the total of this and the num brothers is <= total siblings

                try
                {
                    int numSisters = Convert.ToInt32(userData);

                    //total siblings
                    string numSibsStr = getGS().Get("NUMSIBS");

                    //total brothers
                    string numBrothersStr = getGS().Get("NUM_AFFECTED_BROTHERS");

                    if (numSibsStr == null || numBrothersStr == null)
                    {
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't compare with total number of siblings, as neccessary data was not entered previously");
                        warningBox.ShowDialog();

                        dataOK = true;


                    }
                    else
                    {
                        int numSibsTotal = Convert.ToInt32(numSibsStr);
                        int numBrothers = Convert.ToInt32(numBrothersStr);

                        if ((numSisters + numBrothers) <= numSibsTotal)
                        {
                            //OK
                            dataOK = true;


                        }
                        else
                        {
                            //error
                            dataOK = false;
                            errorMessage = "Error: affected sisters + affected bothers must be <= total siblings";

                        }



                    }





                }
                catch
                {
                    dataOK = false;
                    errorMessage = "Please enter a whole Number";

                }



            }

            else if (Validation == "CheckSameAsPrevious")
            {


                dataOK = testCheckSameAsPrevious(userData);
                errorMessage = "The value you entered is not the same as the previous question: please try again";


            }

            else if ((Validation == "CheckAgeSameAsOrLessThanAGE") || (Validation == "CheckAgeSmokingStartLessThanAgeSmokingStop"))
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


            else if (Validation == "TestHeight")
            {
                //check height in range

                //check number
                decimal height;

                try
                {
                    height = Convert.ToDecimal(userData);
                    dataOK = true;

                    if ((height <= 80) || (height >= 210))
                    {
                        Form1 mainForm = getQM().getMainForm();
                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The Height is unusal (" + userData + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;


                        //if (MessageBox.Show("The height is unusal (" + userData + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_HEIGHT", userData);



                        }
                        else
                        {
                            heightBad = true;

                        }

                    }

                }
                catch (FormatException e)
                {

                    //not a decimal
                    dataOK = false;
                    errorMessage = "This is not a number";

                }


            }

            else if (Validation == "TestWeight")
            {
                //check height in range

                //check number
                decimal weight;

                try
                {
                    weight = Convert.ToDecimal(userData);
                    dataOK = true;

                    if ((weight <= 20) || (weight >= 180))
                    {

                        Form1 mainForm = getQM().getMainForm();
                        ConfirmForm confirmBox = getQM().getConfirmBox();
                        string confLabel = "The Weight is unusal (" + userData + "). Is this correct ?";
                        confirmBox.setFormLabel(confLabel, mainForm);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = mainForm.confirmResult;



                        //if (MessageBox.Show("The weight is unusal (" + userData + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        if (buttonResult == "yes")
                        {
                            // a 'DialogResult.Yes' value was returned from the MessageBox

                            //alert data manager.

                            getSD().Add("DATA_MANAGER_REVIEW_WEIGHT", userData);



                        }
                        else
                        {
                            weightBad = true;

                        }

                    }

                }
                catch (FormatException e)
                {

                    //not a decimal
                    dataOK = false;
                    errorMessage = "This is not a number";

                }


            }

            else if (Validation == "TestBloodSerum" || Validation == "TestBloodEDTA" || Validation == "TestBloodNAF")
            {
                //this is the barcode from the serum tube: we need to check that it matches the same group as the master lab barcode
                string typeSuffix;

                if (Validation == "TestBloodSerum")
                {
                    typeSuffix = "S1";

                }
                else if (Validation == "TestBloodEDTA")
                {

                    typeSuffix = "E1";

                }
                else
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

            else if (Validation == "TestSameAsParticipantID")
            {

                //the barcode should be the same as previously entered in IDNO
                errorMessage = "this barcode does not match the participant ID";

                dataOK = TestSameAsParticipantID(userData);



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

                if (heightBad == true || weightBad == true || armCircBad == true)
                {

                    nextCode = Code;

                }
                
                
                //process the data
                else if (Process == "NoModify")
                {
                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;

                }
                else if (Process == "CalcBMI")
                {

                    calcBMI(userData);
                    
                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;


                }
                else if (Process == "SaveBMI")
                {
                    //user has entered their own BMI reading, so we need to override our 
                    //calculated one
                    //save the BMI in the special store
                    getSD().Add("BMI", userData);


                    //make no changes
                    processedData = userData;
                    //advance to the next question
                    nextCode = ToCode;


                }

                else if (Process == "CalcCRWD")
                {
                    //calculate the crowding: CRWD
                    processedData = userData;


                    string resStr = getGS().Get("RES");
                    if (resStr == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate CRWD as Question 14 was not entered");
                        warningBox.ShowDialog();




                    }
                    else
                    {

                        decimal res = Convert.ToDecimal(resStr);
                        decimal rms = Convert.ToDecimal(userData);


                        decimal crwd = Math.Round(res / rms, 2);

                        getSD().Add("CRWD", crwd.ToString());


                    }


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
                else if (Process == "CorrectCuffSize")
                {
                    processedData = userData;

                    //direct to the correct label
                    decimal armCirc = Convert.ToDecimal(userData);

                    if (armCirc < 24)
                    {
                        nextCode = "AC-CUFF-1";


                    }
                    else if (armCirc >= 24 && armCirc <= 32)
                    {
                        nextCode = "AC-CUFF-2";

                    }
                    else
                    {
                        nextCode = "AC-CUFF-3";

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

                else if (Process == "KeepFirst20Chars")
                {


                    if (userData.Length > 20)
                    {
                        processedData = userData.Substring(0, 20);

                    }
                    else
                    {

                        processedData = userData;

                    }
                    
                    
                    

                    nextCode = ToCode;


                }

                else if (Process == "SPECIAL:H3A_TOB4B")
                {

                    //If  no answer or don’t know or 00 go to question 30a [TOB30a]
                    processedData = userData;

                    if (processedData == "0")
                    {

                        nextCode = "TOB30A";

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



                //no
                //the next code is the same as this one
                //MessageBox.Show("Error: please try again");
                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();

                if (Validation == "CheckSameAsPrevious")
                {
                    nextCode = OnErrorQuestionCompare;
                }

                else if ((Validation == "TestDOB" || Validation == "HEPC:TestDOB") && (getNumTimesShown() > 1))
                {

                   
                        nextCode = "THANKYOU";


                }


                else
                {
                    nextCode = Code;

                }



            }

            return nextCode;



        }


        private bool testDiagnosisAge(string userData)
        {

            //get DOB:
            string dobAsStr = getGS().Get("DOB");

            //get diagnosis year
            string diagYearAsStr = getGS().Get("DIAGYEAR");

            if (dobAsStr == null || diagYearAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't preform check as DOB or Diagnosis year was not entered");
                warningBox.ShowDialog();

                return true;


            }


            //get the year from the DOB
            //extract the day, months, years.
            Match match = Regex.Match(dobAsStr, @"(\d+)/(\d+)/(\d+)");

            int years;

            if (match.Success)
            {
                
                years = Convert.ToInt32(match.Groups[3].Value);

                //if years or months or days ==0, these are unknown so we can't do this test
                if (years == 0 )
                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't perform check as DOB years is not known");
                    warningBox.ShowDialog();


                    return true;
                }


            }
            else
            {
                throw new Exception("date parsing failed");

            }


            int diagYear = Convert.ToInt32(diagYearAsStr);

            if (diagYear == 0)
            {

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't perform check as diagnosis years is not known");
                warningBox.ShowDialog();


                return true;

            }


            //calculate the elapsed years from birth to diagnosis
            int elapsedYears = diagYear - years;

            //if the diagnosis was before the person's birthday the elapsed years will be less by 1
            //we don't know the precise date of diagnosis, only the year

            if (elapsedYears == Convert.ToInt32(userData))
            {
                return true;

            }
            else if ((elapsedYears - 1) == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;


            }





        }








        private bool testDOB(string userData, int minAge)
        {
            //is this age consistent with the previously entered Date of Birth
            string dobAsStr = getGS().Get("DOB");

            if (dobAsStr == null)
            {
                //user skipped previous Q: can't do this test

                //show a warning

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check that DOB matches age as DOB was not entered");
                warningBox.ShowDialog();


                //check that the age is >=18
                int ageWarn = Convert.ToInt32(userData);

                if (ageWarn >= minAge)
                {
                    return true;

                }
                else
                {

                    warningBox.setLabel("Warning: Age is < " + minAge);
                    warningBox.ShowDialog();
                    
                    return false;

                }




            }

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

                
               
                //if years or months or days ==0, these are unknown so we can't do this test
                if (years == 0 || months ==0 || days == 0 )

                {

                    //show a warning

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't check that DOB matches age as some parts of age (day, month or year) are not known");
                    warningBox.ShowDialog();
                    
                    
                    return true;
                }



                dob = new DateTime(years, months, days);



            }
            else
            {
                throw new Exception("date parsing failed");

            }


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

            if (age == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;

            }





            /*



            //calc the elapsed time
            TimeSpan elapsedTime = today - dob;

            //double elapsedYears = elapsedTime.TotalDays / 365.25;

           


            //get number of whole years (ignore any parts therafter)
            int elapsedYears = (int)(elapsedTime.TotalDays / 365.25);

            if (elapsedYears == Convert.ToInt32(userData))
            {
                return true;

            }
            else
            {
                return false;

            }
             * 
             * */



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


        private bool TestBetween(string userdata, int start, int stop)
        {
            //true if the data is >= start and <= stop
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

            if ((num >= start) && (num <= stop))
            {
                //OK
                return true;

            }
            else
            {
                return false;



            }





        }



        private bool TestBetweenDecimal(string userdata, decimal start, decimal stop)
        {
            //true if the data is >= start and <= stop
            decimal num;

            try
            {
                num = Convert.ToDecimal(userData);


            }
            catch (FormatException e)
            {

                //was not a number
                return false;


            }

            if ((num > start) && (num <= stop))
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

        private bool TestPhoneNumber(string userData)
        {
            //must be 10 digit number
            Match match = Regex.Match(userData, @"^\d{10}$");


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

        private void calcBMI(String userData)
        {

            //calculate the BMI

            string weight = userData;
            string height = getGS().Get("Height");

            //check that these are not null: which might happen if previous questions were skipped

            if ((weight != null) && (height != null) && (Convert.ToDecimal(height) != 0M))
            {
                decimal weightAsDec = Convert.ToDecimal(weight);
                decimal heightAsDec = Convert.ToDecimal(height) / 100;      //convert from cm to m

                decimal bmi = Math.Round(weightAsDec / (heightAsDec * heightAsDec), 2);

                //save the BMI in the special store
                getSD().Add("BMI", bmi.ToString());

                

            }
            else
            {
                //some data not present

                

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't calculate BMI due to missing data (height/weight)");
                warningBox.ShowDialog();



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
