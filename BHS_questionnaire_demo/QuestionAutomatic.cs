
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
    class QuestionAutomatic : Question
    {
        //text box

        //fields
       

        public string UnderWeightAdvice
        {
            get;
            set;
        }

        public string OverWeightAdvice
        {
            get;
            set;
        }

        public string HypertensiveAdvice
        {
            get;
            set;

        }

        

        //constructor
        public QuestionAutomatic(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {

            


        }



        //methods

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //load the processed data via the base class
            load(pDataDict);

            
        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //Note: no userdata for this widget



        }
        

        public void setProcess(string process)
        {
            Process = process;

        }

       

        public override string processUserData()
        {

            //code for the next question
            string nextCode;
            

            //we have seen this page
            PageSeen = true;

            if (Process == "SetParticipantID")
            {
                //get this from the form
                processedData = getQM().getUserID();



            }

            else if (Process == "SetCurrentTimeBlood")
            {

                //get the current time.
                DateTime now = DateTime.Now;


                string day = now.Day.ToString();
                string month = now.Month.ToString();
                string year = now.Year.ToString();

                getSD().Add("DEXAM2", day);
                getSD().Add("MEXAM2", month);
                getSD().Add("YEXAM2", year);


            }

            else if (Process == "SetCurrentTimeInterview")
            {

                //get the current time.
                DateTime now = DateTime.Now;


                string day = now.Day.ToString();
                string month = now.Month.ToString();
                string year = now.Year.ToString();

                getSD().Add("DEXAM", day);
                getSD().Add("MEXAM", month);
                getSD().Add("YEXAM", year);


            }
            else if (Process == "SetCurrentDateTime")
            {
                //get the current time.
                DateTime now = DateTime.Now;

                processedData = now.ToString();





            }

            else if (Process == "MachineName")
            {

                //name of the computer running this survey
                processedData = System.Environment.MachineName;

                

            }

            else if (Process == "CalcBMI")
            {
                //calculate the BMI

                //get weight and height that were previously entered

                string weight = getGS().Get("Weight");
                string height = getGS().Get("Height");



                //check that these are not null: which might happen if previous questions were skipped

                if ((weight != null) && (height != null) && (Convert.ToDecimal(height) != 0M))
                {
                    decimal weightAsDec = Convert.ToDecimal(weight);
                    decimal heightAsDec = Convert.ToDecimal(height) / 100;      //convert from cm to m

                    decimal bmi = Math.Round(weightAsDec / (heightAsDec * heightAsDec), 2);

                    //check if this is outside the normal range:

                    processedData = bmi.ToString();



                    if (bmi < 18.5M || bmi >= 25M)
                    {
                        //outside normal range

                        string message;

                        if (bmi < 18.5M)
                        {
                            //underweight
                            message = "You have a BMI of " + bmi.ToString() + ", indicating underweight\n";
                            message += UnderWeightAdvice;

                        }
                        else if (bmi >= 30)
                        {
                            //obese
                            message = "You have a BMI of " + bmi.ToString() + ", indicating obese";
                            message += OverWeightAdvice;



                        }
                        else
                        {
                            //overweight
                            message = "You have a BMI of " + bmi.ToString() + ", indicating overweight";
                            message += OverWeightAdvice;



                        }

                        //show advice screen
                        AdviceForm af = getQM().getAdviceBox();
                        af.setAdviceText("BMI Advice", message);
                        af.ShowDialog();







                    }



                }
                else
                {
                    //some data not present

                    //set a value for processedData to prevent problems with locking the Questionnaire
                    processedData = "BMI_NOT_AVAILABLE";

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show BMI advice due to missing data");
                    warningBox.ShowDialog();



                }


            }

            else if (Process == "CalcWHR")
            {

                string waist = getGS().Get("WCAVG");
                string hip = getGS().Get("HIPCAVG");
                string sex = getGS().Get("SEX");        //1= male, 2= female

                string message;

                //check these are not null

                if ((waist != null) && (hip != null) && (sex != null) && (Convert.ToDecimal(hip) != 0M))
                {
                    decimal waistAsDec = Convert.ToDecimal(waist);
                    decimal hipAsDec = Convert.ToDecimal(hip);

                    decimal ratio = Math.Round(waistAsDec / hipAsDec, 2);

                    processedData = ratio.ToString();


                    if (sex == "1")
                    {
                        //male
                        if (ratio > 0.95M)
                        {
                            //abnormal
                            message = "You have a waist/hip ratio of " + ratio + " indicating Overweight:";
                            message += OverWeightAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Waist-hip ratio Advice", message);
                            af.ShowDialog();


                        }



                    }
                    else
                    {
                        //female
                        if (ratio > 0.8M)
                        {
                            //abnormal
                            message = "You have a waist/hip ratio of " + ratio + " indicating Overweight:";
                            message += OverWeightAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Waist-hip ratio Advice", message);
                            af.ShowDialog();



                        }


                    }



                }
                else
                {
                    //some data not present

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show HWR advice due to missing data");
                    warningBox.ShowDialog();



                }



            }
            else if (Process == "CheckSYST")
            {
                string message;

                //check systolic blood pressure
                string systAVG = getGS().Get("SYSTAVG");

                if (systAVG == null)
                {
                    //missing data

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show SYST advice due to missing data");
                    warningBox.ShowDialog();



                }
                else
                {
                    decimal syst = Convert.ToDecimal(systAVG);

                    if (syst > 120)
                    {
                        if (syst >= 140)
                        {

                            //abnormal

                            message = "You have a systolic blood pressure of " + systAVG + " indicating Abnormal";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Systolic Blood Pressure", message);
                            af.ShowDialog();


                        }
                        else
                        {
                            //pre-hypertensive
                            message = "You have a systolic blood pressure of " + systAVG + " indicating Pre-Hypertensive";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Systolic Blood Pressure", message);
                            af.ShowDialog();


                        }



                    }




                }



            }

            else if (Process == "CheckDIAST")
            {
                string message;

                //check diastolic blood pressure
                string diastAVG = getGS().Get("DIASTAVG");

                if (diastAVG == null)
                {
                    //missing data

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't show DIAST advice due to missing data");
                    warningBox.ShowDialog();



                }
                else
                {
                    decimal diast = Convert.ToDecimal(diastAVG);

                    if (diast > 80)
                    {
                        if (diast >= 90)
                        {

                            //abnormal

                            message = "You have a diastolic blood pressure of " + diastAVG + " indicating Abnormal";
                            message += HypertensiveAdvice;


                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Diastolic Blood Pressure", message);
                            af.ShowDialog();


                        }
                        else
                        {
                            //pre-hypertensive
                            message = "You have a diastolic blood pressure of " + diastAVG + " indicating Pre-Hypertensive";
                            message += HypertensiveAdvice;

                            //show advice screen
                            AdviceForm af = getQM().getAdviceBox();
                            af.setAdviceText("Diastolic Blood Pressure", message);
                            af.ShowDialog();



                        }



                    }




                }
            }

            else if (Process == "SetCountryCode")
            {

                //get the country from the config.
                string country= getQM().getMainForm().config.getGlobalName("current-country");

                if (country == null)
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Warning: Can't set Country as none was selected");
                    warningBox.ShowDialog();

                }
                else
                {

                    processedData = country;


                }





            }



            else
            {
                throw new Exception();

            }




            //advance to the next question
            nextCode = ToCode;

            
            return nextCode;



        }

        public override void removeControls()
        {
            


        }

        public override void configureControls(UserDirection direction)
        {

            

        }





    }
}

