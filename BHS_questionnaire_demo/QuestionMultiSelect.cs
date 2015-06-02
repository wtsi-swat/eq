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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;



namespace BHS_questionnaire_demo
{
    class QuestionMultiSelect : Question, IoptionList
    {

        private Label label;

        private CheckedListBox selectBox;

        private List<Option> optionList;


        //constructor
        public QuestionMultiSelect(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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


            selectBox = new CheckedListBox();

            //set font size
            setFontSize(label, selectBox);


            //event handlers
            //trap any keypress to deselect the skip-controls
            selectBox.Click += new EventHandler(button_click);

            //stop user being able to type in the combobox
            //selectBox.DropDownStyle = ComboBoxStyle.DropDownList;


            selectBox.BackColor = GlobalColours.controlBackColour;

            selectBox.Location = new Point(labelXpos, labelYpos + getWidgetHeight());
            selectBox.Size = new Size(SelectLength, 200);


            //make sure that the options are the same as processed Data if any
            if (processedData != null)
            {
                //parse
                
                string[] selectedValues = Regex.Split(processedData, "~~");
                //add to a set
                HashSet<string> valSet= new HashSet<string>();

                foreach(string value in selectedValues){

                    valSet.Add(value);

                }

                foreach (Option op in optionList)
                {

                    //is the value of this option in our list?
                    if (valSet.Contains(op.getValue()))
                    {

                        op.isSelected = true;

                    }
                    else
                    {

                        op.isSelected = false;

                    }


                }


            }



            //add items to combobox
            int i = 0;
            foreach (Option op in optionList)
            {

                selectBox.Items.Add(op);

                if (op.isSelected)
                {

                    //show as checked
                    
                    selectBox.SetItemChecked(i, true);



                }

                i++;

                


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


            //we can have any number of selected options
            if (selectBox.CheckedItems.Count == 0)
            {
                //nothing selected
                ((Form2)getBigMessageBox()).setLabel("You must choose something");
                getBigMessageBox().ShowDialog();

                return Code;




            }

            
            //get selected options
            // If so, loop through all checked items and print results.
            Option selectedOp;
            string opStr = "";

            List<Option> selectedOptions = new List<Option>();


            //set all options as not selected
            foreach (Option op in optionList)
            {
                op.isSelected = false;

            }



            
            for (int x = 0; x < selectBox.CheckedItems.Count; x++)
            {

                selectedOp = (Option)selectBox.CheckedItems[x];

                //set the option as selected so we can remember it if we see this Question again
                selectedOp.isSelected = true;

                selectedOptions.Add(selectedOp);
                
                opStr += (selectedOp.getValue());


                //is this None?
                if (selectedOp.getText() == "None")
                {

                    //this must be the only selected item, otherwise error
                    if (selectBox.CheckedItems.Count > 1)
                    {
                        //error:
                        ((Form2)getBigMessageBox()).setLabel("You Can't select 'None' AND other option(s)");
                        getBigMessageBox().ShowDialog();

                        return Code;


                    }

                }




                //add delimiter if not the last item
                if (x < (selectBox.CheckedItems.Count - 1))
                {

                    opStr += ("~~");
                }
                
               
            }


            
                







            //convert to string

            //processedData = selectedData.ToString();

            //get the value not the text for the option
            processedData = opStr;




            //SelectedOptionTitle = selectedOption.getText();

            //string toCodeProcess = selectedOption.ToCodeProcess;

            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {

                    getGS().Add(globalKey, processedData);
                

            }




            //does the selected option have a ToCode?
            //check each in the list
            foreach(Option op in selectedOptions){

                if (op.ToCode != null)
                {

                    return op.ToCode;

                }


            }

            //if no selected option has a tocode

            return ToCode;


           



        }



    }
}
