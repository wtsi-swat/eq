
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
using System.IO;



namespace BHS_questionnaire_demo
{
    class QuestionRadio : Question, IoptionList
    {
        //a Radio Button set

        //fields
        protected List<Option> optionList;

        protected GroupBox radioGroup;

        public string LabelToGroupBoxGap { get; set; }


        //constructor
        public QuestionRadio(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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


        }

        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);

            //Note: no userdata for this widget



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
            
            //getForm().Controls.Remove(radioGroup);
            getQM().getPanel().Controls.Remove(radioGroup);

            
            radioGroup.Dispose();





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
            
            
            
            
            radioGroup = new GroupBox();
            radioGroup.ForeColor = GlobalColours.mainTextColour;

            setFontSize(radioGroup);
           

            //position the group box under the label, i.e. at the same xpos but an increased ypos
            int groupBoxXpos = getWidgetXpos();
            int groupBoxYpos = getWidgetYpos();



            //position of the groupbox
            radioGroup.Location = new Point(groupBoxXpos, groupBoxYpos);
            radioGroup.Size = new Size(getWidgetWidth(), getWidgetHeight());
            radioGroup.Text = Val;
            

            //should we prepopulate this?
            string popFromValue = null;

            if (PopulateFrom != null)
            {

                //get the question
                Question popFromQ = getQM().getQuestion(PopulateFrom);

                

                if (popFromQ != null)
                {
                    popFromValue = popFromQ.processedData;




                }



            }

           
            
            //create the RadioButton objects that we need, i.e. one for each option.

            RadioButton rb;
           
            //create a radiobutton for each option
            foreach (Option op in optionList)
            {
                //create a radiobutton
                rb = new RadioButton();

                //trap the click
                rb.Click += new EventHandler(button_click);


                rb.ForeColor = GlobalColours.mainTextColour;

                rb.Text = op.getText();
                //rb.Tag = op.getValue();
                rb.Tag = op;
                rb.Location = new Point(op.getWidgetXpos(), op.getWidgetYpos());
                rb.Size = new Size(op.getWidgetWidth(), op.getWidgetHeight());

                if (! PageSeen)
                {
                    if (op.Default)
                    {
                        rb.Checked = true;
                    }

                    else if ((popFromValue != null) && (popFromValue == op.getValue()))
                    {
                        rb.Checked = true;

                    }

                }
                else
                {
                    //page has been seen, set the previous data
                    if (op.getValue() == processedData)
                    {
                        rb.Checked = true;

                    }

                }

                
                

                radioGroup.Controls.Add(rb);

                



            }

            //add controls to the form
            

            //getForm().Controls.Add(radioGroup);
            getQM().getPanel().Controls.Add(radioGroup);

            setSkipSetting();

            //start audio recording if enabled
            audioRecording();



        }

        

        public override string processUserData()
        {

            PageSeen = true;

            //did the user skip this question ?
            //if (getQM().SkipThisQuestion)
            string skipSetting = getSkipSetting();  //returns null if none of those skip-controls were selected

            if(skipSetting != null)
            {
                //yes

                //do not overwrite data that may be there already
               // if (processedData == null)
               // {

                    //processedData = "ENTRY_REFUSED";
                    processedData = skipSetting;

              //  }

                return ToCode;




            }
            
            //find the selected button
            RadioButton selectedButton= null;
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
            processedData = selectedOption.getValue();
            string optionToCode = selectedOption.ToCode;

            //in some cases we need to show an error box
            string optionToCodeErr = selectedOption.ToCodeErr;

            string toCodeProcess = selectedOption.ToCodeProcess;


            //processedData = (string)(selectedButton.Tag);


            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {
                getGS().Add(globalKey, processedData);

            }

            //get the next question code

            //do we have a special process label

            if (toCodeProcess != null)
            {
                //special processing
                if (toCodeProcess == "HepC1")
                {
                    //HepC questionnaire: STCST7

                    //if HCVT==1 or HDXT==1, goto STCST9
                    string answerHCVT = getGS().Get("HCVT");
                    string answerHDXT = getGS().Get("HDXT");

                    if ((answerHCVT != null) && (answerHCVT == "1"))
                    {
                        return "STCST9";


                    }

                    if ((answerHDXT != null) && (answerHDXT == "1"))
                    {
                        return "STCST9";


                    }

                    return optionToCode;    //this MUST be set in these cases


                }
                else if(toCodeProcess == "Hyp2:CON6"){

                    //Hypertension Q.
                    string answerCON2 = getGS().Get("CON2");
                    string answerCON3 = getGS().Get("CON3");

                    if ((answerCON2 != null) && (answerCON2 == "2") && (answerCON3 != null) && (answerCON3 == "1"))
                    {
                        return "PHYMESS_INTRO";


                    }


                    return optionToCode;    //this MUST be set in these cases


                }
                else if (toCodeProcess == "Hyp2:CON3")
                {
                    //user answered No to CON3

                    //is this the first answer
                    if (getNumTimesShown() > 1)
                    {
                        //second showing (second NO)
                        //if CON2 was also no -> exit
                        string answerCON2 = getGS().Get("CON2");

                        if ((answerCON2 != null) && (answerCON2 == "2"))
                        {

                            return "THANKYOU";
                        }
                        else
                        {
                            return "CON4";

                        }

                    }
                    else
                    {
                        //first showing:
                        return "CON3";

                    }
                    
                    
                   


                }

                else if (toCodeProcess == "Durbin Diabetes:FAST_yes")
                {

                    //user said Yes to FAST:Flag questionnaire as missing OGTT blood samples.
                    getSD().Add("Missing OGTT blood samples", "true");
                    

                }

                else if (toCodeProcess == "Durbin Diabetes:FAST_no")
                {

                    //user said Yes to FAST:Flag questionnaire as missing OGTT blood samples.
                    getSD().Add("Missing OGTT blood samples", "false");


                }

                else if (toCodeProcess == "RESTART_Q_AT_FAST2")
                {
                    //when this form is next edited, restart at question FAST2
                    setStartQuestion("FAST2");



                }

                else
                {

                    throw new Exception();

                }




            }






            //do we want to forward to another question if this one has been displayed already?
            string toCodeSecond = selectedOption.ToCodeSecond;

            if ((getNumTimesShown() > 1) && (toCodeSecond != null))
            {

                //show a message if one is defined
                if (MessageOnSecondFail != null)
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel(MessageOnSecondFail);
                    warningBox.ShowDialog();


                }
                
                return toCodeSecond;

            }




            if (optionToCodeErr != null)
            {

                //show error message
                ((Form2)getBigMessageBox()).setLabel("Please try again");
                getBigMessageBox().ShowDialog();

                return optionToCodeErr;

            }


            


            else if (optionToCode == null)
            {
                //no tocode for this option, i.e. use the std tocode
                return ToCode;
            }
            else
            {
                //this option has a toCode
                return optionToCode;

            }
            





        }

        public override bool isFormExit()
        {
            //will this exit the form
            if (processedData == null)
            {

                //question was not answered:
                return false;



            }
            else
            {

                //get the appropriate ToCode.
                //find the option for this data
                string useToCode = null;

                string fromCode = null;


                foreach (Option option in optionList)
                {


                    if (option.ToCodeErr != null)
                    {

                        fromCode = option.ToCodeErr;
                    }
                    
                    if (processedData == option.getValue())
                    {
                        //does this option have a tocode ?


                        if (option.ToCode == null)
                        {
                            useToCode = ToCode;

                        }
                        else
                        {

                            useToCode = option.ToCode;

                        }

                        break;



                    }





                }

                // if useToCode is still null, there was no matching option, so must have been a skip button
                if (useToCode == null)
                {

                    useToCode = ToCode;
                }


                //does this tocode point to the final section ?
                if (useToCode == "THANKYOU")
                {
                    
                    //certain questions in the consents section are second-answer types, where sometimes the 
                    //user has changed their mind for the first answer from no to yes. In these cases, we should not count them 
                    //as true

                    if (Section == "Consents")
                    {
                        //get the linking question which brought us here

                        if (fromCode != null)
                        {

                            //get the question object and get the processedData for the fromCode
                            string fromQdata= getQM().getQuestion(fromCode).processedData;

                            if (fromQdata == "1")
                            {
                                //ignore this as the user has changed their mind from unconsented to consented
                                return false;

                            }
                            else
                            {
                                return true;

                            }


                        }
                        else
                        {

                            return true;

                        }




                    }
                    else
                    {
                        return true;

                    }
                    
                    
                    
                    
                    
                    
                    
                    
                    
                }
                else
                {
                    return false;

                }



            }
           




        }



        public override bool isSectionExit(Dictionary<string, string> questionToSectionMap)
        {

            //is this question a valid section exit, i.e. 
            //we must have a non-null processedData AND must point to a question outside of this section

            if (processedData == null)
            {

                //question was not answered:
                return false;



            }
            else
            {
                
                //get the appropriate ToCode.
                //find the option for this data
                string useToCode = null;
                string fromCode = null;

                foreach (Option option in optionList)
                {

                    if (option.ToCodeErr != null)
                    {

                        fromCode = option.ToCodeErr;
                    }
                    
                    
                    
                    if (processedData == option.getValue())
                    {
                        //does this option have a tocode ?
                        

                        if (option.ToCode == null)
                        {
                            useToCode = ToCode;

                        }
                        else
                        {

                            useToCode = option.ToCode;

                        }

                        break;



                    }

                    



                }

                // if useToCode is still null, there was no matching option, so must have been a skip button
                if (useToCode == null)
                {

                    useToCode = ToCode;
                }
                

                //is this code outside the section?
                //this question has been answered, but does the ToCode point to a different section
                string toCodeSection = questionToSectionMap[useToCode];

                if (toCodeSection == Section)
                {
                    //next question is in this section
                    return false;

                }
                else
                {
                    //next question is in another section -> exit question
                    //Note: there are some exceptions to this in the Consents section, if this is a second entry where the user has changed their mind on the first entry

                    if (Section == "Consents")
                    {
                        //get the linking question which brought us here

                        if (fromCode != null)
                        {

                            //get the question object and get the processedData for the fromCode
                            string fromQdata = getQM().getQuestion(fromCode).processedData;

                            if (fromQdata == "1")
                            {
                                //ignore this as the user has changed their mind from unconsented to consented
                                return false;

                            }
                            else
                            {
                                return true;

                            }


                        }
                        else
                        {

                            return true;

                        }




                    }
                    else
                    {
                        return true;

                    }





                    


                }
                
                

            }




        }





    }
}
