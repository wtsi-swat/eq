
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
    class QuestionSelect : Question, IoptionList
    {

        
        private Label label; 

        private ComboBox selectBox;

        private List<Option> optionList;

        
        //constructor
        public QuestionSelect(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            //init the optionslist
            optionList = new List<Option>();


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


        public bool testQuestionRefs(Dictionary<string, Question> questionHash)
        {

            return Utils.testQuestionRefs(questionHash, optionList);




        }




        public override void removeControls()
        {
            

            getQM().getPanel().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(selectBox);

            label.Dispose();
            selectBox.Dispose();



        }

        public void addOption(Option op)
        {
            optionList.Add(op);



        }

        public override void configureControls(UserDirection direction)
        {

            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();
            
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

             
            selectBox = new ComboBox();

            //set font size
            setFontSize(label, selectBox);
            

            //event handlers
            //trap any keypress to deselect the skip-controls
            selectBox.Click += new EventHandler(button_click);
            
            //stop user being able to type in the combobox
            selectBox.DropDownStyle = ComboBoxStyle.DropDownList;
            

            selectBox.BackColor = GlobalColours.controlBackColour;
            
            selectBox.Location = new Point(labelXpos, labelYpos + getWidgetHeight());
            selectBox.Size = new Size(SelectLength, 20);

            //stop the list spreading too far on tablet screen
            selectBox.IntegralHeight = false; //won't work unless this is set to false
            selectBox.MaxDropDownItems = 5;





            Option previouslySelectedOption = null;
            
            
            //add items to combobox
            foreach (Option op in optionList)
            {

                //selectBox.Items.Add(op.getText());

                selectBox.Items.Add(op);

                //is this option the one that was selected previously ?
                if (processedData != null)
                {
                    if (processedData == op.getValue())
                    {
                        previouslySelectedOption = op;

                    }



                }




                
               
            }



            if (PageSeen && (processedData != null))
            {

                if (previouslySelectedOption != null)
                {

                    selectBox.SelectedItem = previouslySelectedOption;

                }
                



            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            
            getQM().getPanel().Controls.Add(selectBox);
            
            setSkipSetting();

            //start audio recording if enabled
            audioRecording();



        }

        public override string processUserData()
        {

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
               

                processedData = skipSetting;

                return ToCode;




            }



            //get the selected date


            //object selectedData = selectBox.SelectedItem;

            Option selectedOption = (Option)(selectBox.SelectedItem);
            

            //are any of these null
            if (selectedOption == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose something");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            //convert to string

            //processedData = selectedData.ToString();

            //get the value not the text for the option
            processedData = selectedOption.getValue();

            string toCodeProcess = selectedOption.ToCodeProcess;

            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {

                if ((toCodeProcess != null) && (toCodeProcess == "HepC:setKeyPositive"))
                {
                    //treat this as if positive
                    getGS().Add(globalKey, "1");


                }
                else
                {

                    getGS().Add(globalKey, processedData);
                }
                
            }




            //does the selected option have a ToCode?

            string optionToCode = selectedOption.ToCode;

            if (optionToCode == null)
            {
                return ToCode;

            }
            else
            {
                return optionToCode;


            }


            

            






        }







    }
}
