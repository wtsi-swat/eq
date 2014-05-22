
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
    class QuestionTextDuplicate : Question
    {
        //this widget has 2 textboxes which must receive the same values otherwise error



        //fields
        private TextBox textbox1;
        private TextBox textbox2;

        private Label label;

        private Label box1Label;
        private Label box2Label;

        //reference to a subroutine that will do the validation
        //private string Validation;

        //reference to a subroutine that will do any needed processing
        //private string Process;


        //the data the user entered, which may be different to the processed data
        private string userData = null;


        //properties



        //constructor
        public QuestionTextDuplicate(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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
            getQM().getPanel().Controls.Remove(textbox1);
            getQM().getPanel().Controls.Remove(textbox2);
            getQM().getPanel().Controls.Remove(box1Label);
            getQM().getPanel().Controls.Remove(box2Label);

            label.Dispose();
            textbox1.Dispose();
            textbox2.Dispose();
            box1Label.Dispose();
            box2Label.Dispose();



        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            label = new Label();
            textbox1 = new TextBox();
            textbox2 = new TextBox();

            //trap any keypress to deselect the skip-controls
            textbox1.KeyPress += new KeyPressEventHandler(button_click);
            textbox2.KeyPress += new KeyPressEventHandler(button_click);





            box1Label = new Label();
            box2Label = new Label();

            //set font size
            setFontSize(label, textbox1, textbox2, box1Label, box2Label);


            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();



            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //box 1 label
            box1Label.Location = new Point(labelXpos, labelYpos + 50);
            box1Label.Size = new Size(200, 50);
            box1Label.Text = "First Entry";



            //position of the first textbox
            textbox1.Location = new Point(labelXpos + 200, labelYpos + 50);
            textbox1.Size = new Size(200, 50);

            //box2 label
            box2Label.Location = new Point(labelXpos, labelYpos + 100);
            box2Label.Size = new Size(200, 50);
            box2Label.Text = "Second Entry";


            //position of the second textbox
            textbox2.Location = new Point(labelXpos + 200, labelYpos + 100);
            textbox2.Size = new Size(200, 50);




            //if page seen before, populate the control with the previously entered text
            if (PageSeen)
            {
                textbox1.Text = userData;
                textbox2.Text = userData;


            }
            



            //colours for controls
            label.ForeColor = GlobalColours.mainTextColour;
            box1Label.ForeColor = GlobalColours.mainTextColour;
            box2Label.ForeColor = GlobalColours.mainTextColour;


            textbox1.BackColor = GlobalColours.controlBackColour;
            textbox2.BackColor = GlobalColours.controlBackColour;



            //add controls to the panel
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(box1Label);
            getQM().getPanel().Controls.Add(box2Label);
            getQM().getPanel().Controls.Add(textbox1);
            getQM().getPanel().Controls.Add(textbox2);

            textbox1.Focus();

            setSkipSetting();




        }



        public override string processUserData()
        {

            //code for the next question
            string nextCode;


            //get the raw data from the user
            //userData = textbox.Text;



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
                    userData = "";

               // }

                return ToCode;




            }




            string errorMessage = null;

            bool dataOK = false;

            //validate the data
            //run the test accoring to the Validation

            string userData1 = textbox1.Text;
            string userData2 = textbox2.Text;

            //in all cases, we need to check that these are the same

            if (userData1 != userData2)
            {
                dataOK = false;
                errorMessage = "Error: Both entries must be the same";


            }
            else
            {

                if (Validation == "TestNullEntry")
                {
                    dataOK = testNullEntry(userData1);
                    errorMessage = "Error: please try again";

                }

                else if (Validation == "TestNumeric")
                {
                    dataOK = TestNumeric(userData1);
                    errorMessage = "Error: please enter numbers";




                }

                else if (Validation == "TestSBPhigherThanDBP")
                {

                    //this is diast.
                    try
                    {

                        decimal diast = Convert.ToDecimal(userData1);

                        //get syst

                        string systStr;

                        if (Code == "DIAST1")
                        {

                            systStr = getGS().Get("SYST1");

                        }
                        else
                        {
                            //this is DIAST2
                            systStr = getGS().Get("SYST2");

                        }

                        if (systStr == null)
                        {

                            Form3 warningBox = getQM().getWarningBox();

                            warningBox.setLabel("Warning: Can't compare with previous SYST, which was not entered");
                            warningBox.ShowDialog();

                            dataOK = true;

                        }
                        else
                        {

                            decimal syst = Convert.ToDecimal(systStr);

                            //sys must be higher than diast

                            if (syst > diast)
                            {

                                dataOK = true;
                            }
                            else
                            {
                                dataOK = false;
                                errorMessage = "Error SBP must be > DBP";

                            }


                        }




                    }
                    catch
                    {

                        dataOK = false;
                        errorMessage = "Error: please enter numbers";
                    }

                    

                }



                else
                {

                    dataOK = true;

                }





            }




            //if data is OK, process the data if needed
            if (dataOK)
            {

                if (Process == "CalcAverageSecondAndThirdReadingsSYST")
                {
                    processedData = userData1;
                    userData = userData1;


                    //this has the 3rd reading: get the second reading from the global store
                    string reading3 = userData1;
                    string reading2 = getGS().Get("SYST2");

                    if (reading2 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate SYST average as reading 2 was not entered");
                        warningBox.ShowDialog();
                        nextCode = ToCode;


                    }
                    else
                    {
                        //calc average
                        //take the average of the second and third readings
                        decimal reading2AsDec = Convert.ToDecimal(reading2);
                        decimal reading3AsDec = Convert.ToDecimal(reading3);


                        decimal averageSYST = (reading2AsDec + reading3AsDec) / 2;

                        //save in GS 
                        getGS().Add("SYSTAVG", averageSYST.ToString());

                        //save in special store:
                        getSD().Add("SYSTAVG", averageSYST.ToString());

                        nextCode = ToCode;

                        //check if this result is outside the usual range
                        if ((averageSYST <= 55) || (averageSYST >= 299))
                        {

                            //ref to the Questionnaire form
                            Form1 mainForm = getQM().getMainForm();

                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The systolic value is unusal (average is " + averageSYST.ToString() + "). Is this correct ?";
                            confirmBox.setFormLabel(confLabel, mainForm);
                            confirmBox.ShowDialog();

                            //the confirmbox calls back to the mainForm which button was pressed
                            string buttonResult = mainForm.confirmResult;

                            if (buttonResult == "yes")
                            {
                                // a 'DialogResult.Yes' value was returned from the MessageBox

                                //alert data manager.

                                getSD().Add("DATA_MANAGER_REVIEW_SYST", processedData);



                            }
                            else
                            {
                                //not confirmed: show this question again.
                                nextCode = Code;



                            }


                        }



                    }
                        



                }


                else if (Process == "CalcAverageSyst")
                {

                    processedData = userData1;
                    userData = userData1;

                    string firstReading = getGS().Get("SYST1");

                    if (firstReading == null)
                    {
                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate SYST average as first reading was not entered");
                        warningBox.ShowDialog();
                        


                    }
                    else
                    {

                        string secondReading = userData1;

                        decimal average = CalcAverageOf2(firstReading, secondReading);

                        getSD().Add("SYSTAVG", average.ToString());
                        



                    }


                    nextCode = ToCode;
                    



                }

                else if (Process == "CalcAverageDiast")
                {

                    processedData = userData1;
                    userData = userData1;

                    string firstReading = getGS().Get("DIAST1");

                    if (firstReading == null)
                    {
                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate DIAST average as first reading was not entered");
                        warningBox.ShowDialog();



                    }
                    else
                    {

                        string secondReading = userData1;

                        decimal average = CalcAverageOf2(firstReading, secondReading);

                        getSD().Add("DIASTAVG", average.ToString());




                    }


                    nextCode = ToCode;




                }


                else if (Process == "CalcAverageSecondAndThirdReadingsDIAST")
                {

                    //take the average of the second and third readings
                    processedData = userData1;
                    userData = userData1;


                    //this has the 3rd reading: get the second reading from the global store
                    string reading3 = userData1;
                    string reading2 = getGS().Get("DIAST2");


                    if (reading2 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate DIAST average as reading 2 was not entered");
                        warningBox.ShowDialog();
                        nextCode = ToCode;


                    }
                    else
                    {
                        //calc average
                        //take the average of the second and third readings
                        decimal reading2AsDec = Convert.ToDecimal(reading2);
                        decimal reading3AsDec = Convert.ToDecimal(reading3);


                        decimal averageDIAST = (reading2AsDec + reading3AsDec) / 2;

                        //save in GS 
                        getGS().Add("DIASTAVG", averageDIAST.ToString());

                        //save in special store:
                        getSD().Add("DIASTAVG", averageDIAST.ToString());

                        nextCode = ToCode;

                        //check if this result is outside the usual range
                        if ((averageDIAST <= 40) || (averageDIAST >= 200))
                        {

                            //ref to the Questionnaire form
                            Form1 mainForm = getQM().getMainForm();

                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The diastolic value is unusual (average is " + averageDIAST.ToString() + "). Is this correct ?";
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
                            else
                            {
                                //not confirmed: show this question again.
                                nextCode = Code;



                            }


                        }
                        else
                        {

                            //check that Diast BP is smaller than SBP (Q80)

                            //get SYSTAVG
                            string systAVG = getGS().Get("SYSTAVG");

                            decimal averageSYST = Convert.ToDecimal(systAVG);


                            if (systAVG == null)
                            {

                                //warning:
                                Form3 warningBox = getQM().getWarningBox();

                                warningBox.setLabel("Warning: Can't compare SYST average with DIAST average as SYS was not entered");
                                warningBox.ShowDialog();
                                nextCode = ToCode;


                            }
                            else
                            {
                                if (averageDIAST > averageSYST)
                                {

                                    ((Form2)getBigMessageBox()).setLabel("Error: average DIASTOLIC BP is greater than average SYSTOLIC BP");
                                    getBigMessageBox().ShowDialog();

                                    nextCode = Code;


                                }




                            }



                        }


                    }





                }

                else if (Process == "CalcAverageSecondAndThirdReadingsPLS")
                {


                    processedData = userData1;
                    userData = userData1;


                    //this has the 3rd reading: get the second reading from the global store
                    string reading3 = userData1;
                    string reading2 = getGS().Get("PLS2");

                    if (reading2 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate PLS average as reading 2 was not entered");
                        warningBox.ShowDialog();
                        nextCode = ToCode;


                    }
                    else
                    {
                        //calc average
                        //take the average of the second and third readings
                        decimal reading2AsDec = Convert.ToDecimal(reading2);
                        decimal reading3AsDec = Convert.ToDecimal(reading3);


                        decimal averagePLS = (reading2AsDec + reading3AsDec) / 2;

                        //save in GS 
                        getGS().Add("PLSAVG", averagePLS.ToString());

                        //save in special store:
                        getSD().Add("PLSAVG", averagePLS.ToString());

                        nextCode = ToCode;

                        //check if this result is outside the usual range
                        if ((averagePLS <= 30) || (averagePLS >= 180))
                        {

                            //ref to the Questionnaire form
                            Form1 mainForm = getQM().getMainForm();

                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The pulse is unusal (average is " + averagePLS.ToString() + "). Is this correct ?";
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
                            else
                            {
                                //not confirmed: show this question again.
                                nextCode = Code;



                            }


                        }



                    }



                }

                else if (Process == "TakeWCaverageFirst2")
                {
                    //waist circumference

                    //is there a difference of > 3 cm between readings 1 and 2
                    processedData = userData1;
                    userData = userData1;



                    string reading2 = userData1;
                    string reading1 = getGS().Get("WC1");

                    if (reading1 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate waist circumference average as reading 1 was not entered");
                        warningBox.ShowDialog();
                        nextCode = ToCode;


                    }
                    else
                    {
                        //is the difference in the readings > 3 cm
                        decimal reading1AsDec = Convert.ToDecimal(reading1);
                        decimal reading2AsDec = Convert.ToDecimal(reading2);



                        if (Math.Abs(reading1AsDec - reading2AsDec) > 3)
                        {

                            //we need to get the 3rd reading

                            //save second reading in GS
                            getGS().Add("WC2", reading2);

                            //get the 3rd reading
                            nextCode = "WC3";



                        }
                        else
                        {
                            //we can use these 2 readings and don't need a third one
                            decimal averageWC = (reading2AsDec + reading1AsDec) / 2;

                            //save in global store and the special store
                            getGS().Add("WCAVG", averageWC.ToString());
                            getSD().Add("WCAVG", averageWC.ToString());

                            nextCode = ToCode;

                            //check if this result is outside the usual range
                            if ((averageWC <= 30) || (averageWC >= 180))
                            {

                                //ref to the Questionnaire form
                                Form1 mainForm = getQM().getMainForm();

                                ConfirmForm confirmBox = getQM().getConfirmBox();
                                string confLabel = "The waist circumference is unusal (average is " + averageWC.ToString() + "). Is this correct ?";
                                confirmBox.setFormLabel(confLabel, mainForm);
                                confirmBox.ShowDialog();

                                //the confirmbox calls back to the mainForm which button was pressed
                                string buttonResult = mainForm.confirmResult;

                                if (buttonResult == "yes")
                                {
                                    // a 'DialogResult.Yes' value was returned from the MessageBox

                                    //alert data manager.

                                    getSD().Add("DATA_MANAGER_REVIEW_WC", processedData);



                                }
                                else
                                {
                                    //not confirmed: show this question again.
                                    nextCode = Code;



                                }


                            }


                        }



                    }

                }

                else if (Process == "TakeWCaverageAll3")
                {

                    //we are at WC3: take the closest 2 of WC1, WC2, WC3 (or all 3 if evenly spaced) and calc average

                    processedData = userData1;
                    userData = userData1;

                    nextCode = ToCode;

                    string reading3 = userData1;
                    string reading1 = getGS().Get("WC1");
                    string reading2 = getGS().Get("WC2");

                    if (reading1 == null || reading2 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate waist circumference average as reading 1 or reading 2 was not entered");
                        warningBox.ShowDialog();



                    }
                    else
                    {

                        decimal reading1asDec = Convert.ToDecimal(reading1);
                        decimal reading2asDec = Convert.ToDecimal(reading2);
                        decimal reading3asDec = Convert.ToDecimal(reading3);

                        decimal average;





                        //are these evenly spaced ?
                        if (areEvenlySpaced(reading1asDec, reading2asDec, reading3asDec))
                        {
                            //yes

                            average = (reading1asDec + reading2asDec + reading3asDec) / 3;


                        }
                        else
                        {
                            //no
                            //which are the 2 closest measurements, i.e. with smallest difference ?
                            //find each possible difference

                            //reading1 vs reading2
                            decimal r1_r2 = Math.Abs(reading1asDec - reading2asDec);

                            //reading1 vs reading 3
                            decimal r1_r3 = Math.Abs(reading1asDec - reading3asDec);

                            //reading2 vs reading3
                            decimal r2_r3 = Math.Abs(reading2asDec - reading3asDec);

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




                        }

                        //save average in special store
                        getSD().Add("WCAVG", average.ToString());


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
                            else
                            {

                                //not confirmed: show this question again.
                                nextCode = Code;

                            }


                        }

                    }

                }

                else if(Process == "calcBMI:HEPC"){


                    processedData = userData1;
                    userData = userData1;
                    
                    calcBMIhepC();
                    nextCode = ToCode;

                }

                else if (Process == "TakeHIPSaverageFirst2")
                {
                    //hips circumference

                    //is there a difference of > 3 cm between readings 1 and 2
                    processedData = userData1;
                    userData = userData1;



                    string reading2 = userData1;
                    string reading1 = getGS().Get("HIPC1");

                    if (reading1 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate hips circumference average as reading 1 was not entered");
                        warningBox.ShowDialog();
                        nextCode = ToCode;


                    }
                    else
                    {
                        //is the difference in the readings > 3 cm
                        decimal reading1AsDec = Convert.ToDecimal(reading1);
                        decimal reading2AsDec = Convert.ToDecimal(reading2);



                        if (Math.Abs(reading1AsDec - reading2AsDec) > 3)
                        {

                            //we need to get the 3rd reading

                            //save second reading in GS
                            getGS().Add("HIPC2", reading2);

                            //get the 3rd reading
                            nextCode = "HIPC3";



                        }
                        else
                        {
                            //we can use these 2 readings and don't need a third one
                            decimal averageHIPSC = (reading2AsDec + reading1AsDec) / 2;

                            //save in global store and the special store
                            getGS().Add("HIPCAVG", averageHIPSC.ToString());
                            getSD().Add("HIPCAVG", averageHIPSC.ToString());

                            nextCode = ToCode;

                            //check if this result is outside the usual range
                            if ((averageHIPSC <= 45) || (averageHIPSC >= 200))
                            {

                                //ref to the Questionnaire form
                                Form1 mainForm = getQM().getMainForm();

                                ConfirmForm confirmBox = getQM().getConfirmBox();
                                string confLabel = "The hips circumference is unusal (average is " + averageHIPSC.ToString() + "). Is this correct ?";
                                confirmBox.setFormLabel(confLabel, mainForm);
                                confirmBox.ShowDialog();

                                //the confirmbox calls back to the mainForm which button was pressed
                                string buttonResult = mainForm.confirmResult;

                                if (buttonResult == "yes")
                                {
                                    // a 'DialogResult.Yes' value was returned from the MessageBox

                                    //alert data manager.

                                    getSD().Add("DATA_MANAGER_REVIEW_HIPC", processedData);



                                }
                                else
                                {
                                    //not confirmed: show this question again.
                                    nextCode = Code;



                                }


                            }


                        }



                    }

                }

                else if (Process == "TakeHIPSaverageAll3")
                {

                    //we are at WC3: take the closest 2 of WC1, WC2, WC3 (or all 3 if evenly spaced) and calc average

                    processedData = userData1;
                    userData = userData1;

                    nextCode = ToCode;

                    string reading3 = userData1;
                    string reading1 = getGS().Get("HIPC1");
                    string reading2 = getGS().Get("HIPC2");

                    if (reading1 == null || reading2 == null)
                    {

                        //warning:
                        Form3 warningBox = getQM().getWarningBox();

                        warningBox.setLabel("Warning: Can't Calculate hips circumference average as reading 1 or reading 2 was not entered");
                        warningBox.ShowDialog();



                    }
                    else
                    {

                        decimal reading1asDec = Convert.ToDecimal(reading1);
                        decimal reading2asDec = Convert.ToDecimal(reading2);
                        decimal reading3asDec = Convert.ToDecimal(reading3);

                        decimal average;





                        //are these evenly spaced ?
                        if (areEvenlySpaced(reading1asDec, reading2asDec, reading3asDec))
                        {
                            //yes

                            average = (reading1asDec + reading2asDec + reading3asDec) / 3;


                        }
                        else
                        {
                            //no
                            //which are the 2 closest measurements, i.e. with smallest difference ?
                            //find each possible difference

                            //reading1 vs reading2
                            decimal r1_r2 = Math.Abs(reading1asDec - reading2asDec);

                            //reading1 vs reading 3
                            decimal r1_r3 = Math.Abs(reading1asDec - reading3asDec);

                            //reading2 vs reading3
                            decimal r2_r3 = Math.Abs(reading2asDec - reading3asDec);

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




                        }

                        //save average in special store
                        getSD().Add("HIPCAVG", average.ToString());


                        if ((average <= 45) || (average >= 200))
                        {
                            Form1 mainForm = getQM().getMainForm();

                            ConfirmForm confirmBox = getQM().getConfirmBox();
                            string confLabel = "The hip circumference is unusal (average is " + average.ToString() + "). Is this correct ?";
                            confirmBox.setFormLabel(confLabel, mainForm);
                            confirmBox.ShowDialog();

                            //the confirmbox calls back to the mainForm which button was pressed
                            string buttonResult = mainForm.confirmResult;



                            //if (MessageBox.Show("The waist circumference is unusal (average is " + average.ToString() + "). Is this correct ?", "Please Check:", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            if (buttonResult == "yes")
                            {
                                // a 'DialogResult.Yes' value was returned from the MessageBox

                                //alert data manager.

                                getSD().Add("DATA_MANAGER_REVIEW_HIPC", processedData);



                            }
                            else
                            {

                                //not confirmed: show this question again.
                                nextCode = Code;

                            }


                        }

                    }

                }

                else
                {
                    //make no changes
                    processedData = userData1;
                    userData = userData1;
                    nextCode = ToCode;

                }


                //save globals
                string globalKey = SetKey;
                if (globalKey != null)
                {
                    getGS().Add(globalKey, processedData);

                }
                




            }
            else
            {
                //validation has failed.
                ((Form2)getBigMessageBox()).setLabel(errorMessage);
                getBigMessageBox().ShowDialog();

                nextCode = Code;




            }

            return nextCode;



        }

        private void calcBMIhepC()
        {

            //calculate the BMI
            //version for HepC

            string weight = getGS().Get("Weight");
            string height = getGS().Get("Height");

            //check that these are not null: which might happen if previous questions were skipped

            if ((weight != null) && (height != null) && (Convert.ToDecimal(height) != 0M))
            {
                decimal weightAsDec = Convert.ToDecimal(weight);
                decimal heightAsDec = Convert.ToDecimal(height) / 100;      //convert from cm to m

                decimal bmi = Math.Round(weightAsDec / (heightAsDec * heightAsDec), 2);

                //save the BMI in the special store
                getSD().Add("BMI", bmi.ToString());

                //add a warning if BMI > 30

                if (bmi > 30)
                {

                    getSD().Add("BMI_LEVEL", "HIGH");
                }
                else
                {

                    getSD().Add("BMI_LEVEL", "NOT_HIGH");
                }



            }
            else
            {
                //some data not present



                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't calculate BMI due to missing data (height/weight)");
                warningBox.ShowDialog();



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


        private bool areEvenlySpaced(decimal v1, decimal v2, decimal v3)
        {

            List<decimal> list = new List<decimal>();

            list.Add(v1);
            list.Add(v2);
            list.Add(v3);

            list.Sort();

            //difference between the first and second values
            decimal diff1;

            //difference between the second and third values
            decimal diff2;


            diff1 = list[1] - list[0];
            diff2 = list[2] - list[1];

            if (diff1 == diff2)
            {
                //evenly spaced
                return true;

            }
            else
            {
                //not evenly spaced
                return false;


            }





        }


    }


}
