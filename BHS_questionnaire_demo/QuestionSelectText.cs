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
    class QuestionSelectText : Question, IoptionList
    {

        //fields
        protected TextBox textbox;
        private Label mainLabel;
        private Label textLabel;
        private ComboBox selectBox;


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



        public string TextLabel { get; set; }   //label for the textbox

        //constructor
        public QuestionSelectText(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
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
            getQM().getPanel().Controls.Remove(mainLabel);
            getQM().getPanel().Controls.Remove(textbox);
            getQM().getPanel().Controls.Remove(selectBox);
            getQM().getPanel().Controls.Remove(textLabel);


            mainLabel.Dispose();
            textbox.Dispose();
            selectBox.Dispose();
            textLabel.Dispose();






        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control
            mainLabel = new Label();
            textLabel = new Label();
            textbox = new TextBox();

            //trap any keypress to deselect the skip-controls
            textbox.KeyPress += new KeyPressEventHandler(button_click);



            //the question Text shown to the user
            mainLabel.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            int selectBoxYpos = labelYpos + getWidgetHeight() + 20;
            int textLabelYpos = selectBoxYpos + 50;
            int textBoxYpos = textLabelYpos + 50;


            //position of the Main (top)Label
            mainLabel.Location = new Point(labelXpos, labelYpos);
            mainLabel.Size = new Size(getWidgetWidth(), getWidgetHeight());
            mainLabel.ForeColor = GlobalColours.mainTextColour;

            selectBox = new ComboBox();


            //event handlers
            //trap any keypress to deselect the skip-controls
            selectBox.Click += new EventHandler(button_click);

            //stop user being able to type in the combobox
            selectBox.DropDownStyle = ComboBoxStyle.DropDownList;


            selectBox.BackColor = GlobalColours.controlBackColour;

            selectBox.Location = new Point(labelXpos, selectBoxYpos);


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
                    if (selectedOptionValue == op.getValue())
                    {
                        previouslySelectedOption = op;

                    }

                }


            }


            //position the text box under the label, i.e. at the same xpos but an increased ypos
            int textBoxXpos = labelXpos;



            //label for the textBox
            textLabel.Location = new Point(textBoxXpos, textLabelYpos);
            textLabel.Size = new Size(getWidgetWidth(), 30);
            textLabel.ForeColor = GlobalColours.mainTextColour;
            textLabel.Text = TextLabel;



            //position of the textbox
            textbox.Location = new Point(textBoxXpos, textBoxYpos);
            textbox.Size = new Size(500, 50);
            textbox.BackColor = GlobalColours.controlBackColour;

            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {
                if (userData != null)
                {

                    textbox.Text = userData;

                }



                if (previouslySelectedOption != null)
                {

                    selectBox.SelectedItem = previouslySelectedOption;

                }


            }

            //set font size
            setFontSize(mainLabel, textLabel, selectBox, textbox);

            //float selectFontSize = 10;
            //selectBox.Font = new Font(selectBox.Font.Name, selectFontSize, selectBox.Font.Style, selectBox.Font.Unit);

            //selectBox.ItemHeight = 300;


            //add to the form
            getQM().getPanel().Controls.Add(mainLabel);
            getQM().getPanel().Controls.Add(textLabel);
            getQM().getPanel().Controls.Add(textbox);
            getQM().getPanel().Controls.Add(selectBox);


            textbox.Focus();



            setSkipSetting();

            //start audio recording if enabled
            audioRecording();



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

                return ToCode;


            }

            string errorMessage = "foo";

            bool dataOK;

            Option selectedOption = (Option)(selectBox.SelectedItem);

            //are any of these null
            if (selectedOption == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose something");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            selectedOptionValue = selectedOption.getValue();

            //process the selected option



            //process the text data



            //validate the data



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
                        //processedData = userData;

                        processedData = selectedOptionValue + ":" + userData;

                    }


                    //if we have a global setting: save to the global object
                    string globalKey = SetKey;
                    if (globalKey != null)
                    {
                        getGS().Add(globalKey, processedData);

                    }




                }

                //advance to the next question
                //if the selected option contains a tocode, use that, otherwise use the main tocode

                if (selectedOption.ToCode == null)
                {
                    nextCode = ToCode;

                }
                else
                {
                    nextCode = selectedOption.ToCode;


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


                nextCode = Code;


            }



            return nextCode;



        }














    }
}
