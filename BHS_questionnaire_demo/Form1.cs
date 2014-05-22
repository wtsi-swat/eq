
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BHS_questionnaire_demo
{

    enum UserDirection
    {
        forward,
        reverse


    };
    
    
    
    
    
    
    public partial class Form1 : Form
    {

        //dir to store data files
        private String dataDir = "C:\\Program Files\\BHS_survey_data";
        //private TextBox myTextBox;
        //private Label myLabel;
        //private GroupBox myRadioGroup;  //container for a radio button set

        //a big version of a message box for errors
        private Form2 bigMessageBox;

        //a message box for warnings
        private Form3 warningMessageBox;

        //an info box
        private MessageForm stdMessageBox;

        //advice box
        private AdviceForm adviceBox;

        //confirm box (has yes/No buttons)
        private ConfirmForm confirmBox;


        private QuestionManager qm;

        private string xmlFileName;

        //map section name to first QuestionCode in that section
        //private Dictionary<string, string> sectionToCodeMap;

        private string latitude = null;
        private string longitude = null;

        //reference to parent form
        private BaseForm baseForm = null;

        //is this a new or existing user?
        private bool newUser;

        //used by confirm window to send back result
        public string confirmResult
        {
            get;
            set;

        }


        private string language = null;

        public Qconfig config { get; set; }     //config info about the questionnaire type
        
        
        public Form1()
        {
            InitializeComponent();

            

            
            //set colours of main widgets
            BackColor = GlobalColours.mainFormColour;

            panel1.BackColor = GlobalColours.mainPanelColour;

            button1.BackColor = GlobalColours.mainButtonColour;

            button2.BackColor = GlobalColours.mainButtonColour;

           

            button4.BackColor = GlobalColours.altButtonColour;

            //populate the map for sections
            //sectionToCodeMap = new Dictionary<string, string>();

            //South Africa
            /*
            sectionToCodeMap.Add("Consents", "START");
            sectionToCodeMap.Add("Personal History", "PERSONAL");
            sectionToCodeMap.Add("Zulu", "ZULU");
            sectionToCodeMap.Add("Family History", "FAMILY");
            sectionToCodeMap.Add("Diabetes", "DIABETES");
            sectionToCodeMap.Add("Clinical Information", "CLINICAL");
            sectionToCodeMap.Add("Clinical Measurements", "MEASURE");
            */



            //Malawi
            /*
            sectionToCodeMap.Add("Consents", "START");
            sectionToCodeMap.Add("Demographic Information", "DEM0");
            sectionToCodeMap.Add("Education, Occupation and Livelihood", "EDLEV0");
            sectionToCodeMap.Add("Health: Tobacco Use", "TOB0");
            sectionToCodeMap.Add("Health: Alcohol Consumption", "ALC0");
            sectionToCodeMap.Add("Health: Diet", "DIET0");
            sectionToCodeMap.Add("Physical Activity: Work", "PHYS00");
            sectionToCodeMap.Add("Physical Activity: Travel to and from places", "PHYS02");
            sectionToCodeMap.Add("Physical Activity: Recreational Activities", "PHYS03");
            sectionToCodeMap.Add("Physical Activity: Sedentary Behaviour", "PHYS04");
            sectionToCodeMap.Add("History of Raised Blood Pressure", "HBP00");
            sectionToCodeMap.Add("History of Diabetes", "HD0");
            sectionToCodeMap.Add("History of High Cholesterol", "HC0");
            sectionToCodeMap.Add("History of Immunisation", "HI0");
            sectionToCodeMap.Add("Physical Measurements: Blood Pressure", "BP0");
            sectionToCodeMap.Add("Physical Measurements: Anthropometry", "HTID");
            sectionToCodeMap.Add("Blood Sample", "NURCODE");
            sectionToCodeMap.Add("Advice", "ADVICE_BMI");
            sectionToCodeMap.Add("Final Comments", "THANKYOU");
             */ 


            bigMessageBox = new Form2();

            warningMessageBox = new Form3();

            stdMessageBox = new MessageForm();

            adviceBox = new AdviceForm();

            confirmBox = new ConfirmForm();
            
            



        }

        public void startSurvey(string xmlFileName, string userDir, string userID, bool newUser, string gpsPort, string gpsBaud, BaseForm baseForm, string gpsCountry, string language, string baseDir, Qconfig config, string userConfigFileName)
        {
            //called after loading the form: passes in needed vars from baseform
            //open a file dialog box

            button1.Enabled = false;

            //reference the the parent form
            this.baseForm = baseForm;

            this.newUser = newUser;

            this.language = language;

            this.config = config;

            this.xmlFileName = xmlFileName;


            //setup global config, e.g. values for skipped questions, etc.
            GlobalConstants.setUpConstants(config);



            //application title
            Text = "Questionnaire for: " + userID;



            qm = new QuestionManager(bigMessageBox, userDir, panel1, label2, userID, label3, label1, adviceBox, confirmBox, this, gpsCountry, language, baseDir, userConfigFileName);
            qm.setWarningBox(warningMessageBox);

            try
            {

                qm.ParseConfigXML(xmlFileName, this);



            }
            catch(Exception e)
            {
                //parsing error
                
                bigMessageBox.setLabel("Error: There is an error in the XML configuration file");
                bigMessageBox.ShowDialog();

                MessageBox.Show(e.Message);

                MessageBox.Show(e.StackTrace);

                //exit form

                Close();


            }
            
            //test the questions for any invalid references, i.e. look for any ToCodes that don't point to real question objects
            qm.testQuestionRefs();

            //test for invalid sections
            qm.testSectionRefs();


            string startQuestionCode= null;


            //load in previous data if this is not a new user
            if (!newUser)
            {
                //old user

                try
                {

                    qm.load();


                    //check for a special start question
                    string specialStartQuestion = qm.getSpecialStartQuestion();

                    if (specialStartQuestion == null)
                    {
                        //pop the stack to get the start question
                        startQuestionCode = qm.getCodeAtTopOfStack();

                    }
                    else
                    {

                        startQuestionCode = specialStartQuestion;

                    }


                    


                }
                catch
                {
                    //probably a Questionnaire that was terminated on the start page, i.e. counts as existing in the parent form
                    //but has no data files
                    //startQuestionCode = "START";

                    bigMessageBox.setLabel("Error: Cannot load data: please delete this Questionnaire and start a new one");
                    bigMessageBox.ShowDialog();

                    

                    //exit form

                    Close();

                    return;


                }
                

            }
            else
            {
                //new user
                startQuestionCode = "START";


            }



            //set the question manager to point to the first question: Code= Name
            //qm.setCurrentQuestion("START");

            qm.setCurrentQuestion(startQuestionCode);

            //configure the controls appropriately for the first question:

            qm.configureControls(UserDirection.forward);

            setMainButtonStatus();

            
            //populate the sections combobox
            List<Section> sections= config.getSectionList();

            foreach(Section section in sections){

                comboBox1.Items.Add(section.getSectionTitle());


            }
            
            


        }


        public void setVerticalScrollBarPos(){

            panel1.VerticalScroll.Value = 0;
            



        }

        private void Form1_Load(object sender, EventArgs e)
        {

           
            





        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {

            //user has clicked the 'next' button.
            

            //enable the previous-page button
            button1.Enabled = true;

            //we need to process the data entered by the user
            qm.processUserData();

            //save the current data to disk
            try
            {
                qm.save(false, config);


            }
            catch
            {

                //save failed: exit
                bigMessageBox.setLabel("Save Failed: Data for this Questionnaire may be corrupt !");
                bigMessageBox.ShowDialog();

                //exit the window.
                this.Close();




            }
            


            //delete the current widgets on the form
            qm.removeControls();

            //display the next question
            qm.setNextQuestion();

            qm.configureControls(UserDirection.forward);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();

            



        }

        private void button1_Click(object sender, EventArgs e)
        {

            //user has clicked the 'previous' button, i.e. we need to reverse to the previous question

            //delete the current widgets on the form
            qm.removeControls();

            //display the previous question
            qm.setPreviousQuestion();

            qm.configureControls(UserDirection.reverse);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();

           




        }

        private void setMainButtonStatus()
        {
            //is this the first page?
            if (qm.isFirstPage())
            {

                button1.Enabled = false;
                button2.Enabled = true;


            }
                //is this the last page ?

            else if (qm.isLastPage())
            {
                button1.Enabled = true;
                button2.Enabled = false;

            }
            else
            {
                //both enabled for middle pages
                button1.Enabled = true;
                button2.Enabled = true;


            }


        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {

            //skip this question

            //set a flag in the QuestionManager, the process the forward button

            qm.SkipThisQuestion = true;
            
            button2_MouseClick(sender, e);

            //reset flag
            qm.SkipThisQuestion = false;


        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {

            string selectedSection = (string) comboBox1.SelectedItem;
            
           //map the section name to the question code at the start of that section

            //string qCode = sectionToCodeMap[selectedSection];
            string qCode = config.getSectionStartCode(selectedSection);

            //delete the current widgets on the form
            qm.removeControls();

            //display the next question
            qm.setNextQuestion(qCode);

            qm.configureControls(UserDirection.forward);

            //make sure the forward and reverse buttons have the correct enabled/disabled
            setMainButtonStatus();




        }

        

       

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //fires when the form is closing, but not yet closed

            //stop any audio recording if it was enabled

            string recordingState = qm.getRecorderState();

            if (recordingState == "on" || recordingState == "paused")
            {

                qm.stopRecording();



            }


            //update the user config with current completeness state: do not display window
            qm.checkCompleteness(config, false);

            //save current data: useful for the final question
            try
            {
                qm.save(true, config);

                //add the result 111 for all skipped questions
                qm.postProcessFinalData();


            }
            catch
            {

                //save failed: exit
                bigMessageBox.setLabel("Save Failed: Data for this Questionnaire may be corrupt !");
                bigMessageBox.ShowDialog();

                

            }


            //close any controls in the QM
            qm.close();

            //tell the baseForm that we are closing so it can let other forms open
            baseForm.questionFormClosing();





        }


        public string getSkipSetting()
        {

            //return the currently selected skip radio button or null if none have been selected

            string selectedText = null;
            
            foreach (RadioButton rb in groupBox1.Controls)
            {

                if (rb.Checked)
                {
                    selectedText= rb.Text;

                }



            }

            return selectedText;



        }


        public void clearControlButtons()
        {
            //set all control buttons (no answer, don't know, etc) to unselected

            foreach (RadioButton rb in groupBox1.Controls)
            {

                rb.Checked = false;

            }


        }




        public void setSkipSetting(string buttonText)
        {
            //set the correct radiobutton
            foreach (RadioButton rb in groupBox1.Controls)
            {

                if (rb.Text == buttonText)
                {
                    rb.Checked = true;

                }
                else
                {
                    rb.Checked = false;

                }
                
                
              


            }

            //clear controls if this is not the OK button
            if (buttonText != "OK")
            {

                clearMainControls();

            }



        }

        private void clearMainControls()
        {

            //remove text and disable main controls
            bool hasAsked = false;  //true if we have already warned the user about deleting text
            
            foreach (Control control in panel1.Controls)
            {


                if (control is TextBox)
                {

                    //did the user type something in the box ?
                    if ((control.Text.Length > 0) && (! hasAsked))
                    {

                        hasAsked = true;
                        
                        //warn the user in case this is not what was intended.

                        string confLabel = "You will delete what you typed, are you sure?";
                        confirmBox.setFormLabel(confLabel, this);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = confirmResult;

                        if (buttonResult == "yes")
                        {
                            //go ahead and delete
                            control.Text = "";

                        }
                        else
                        {
                            //user has changed mind: cancel
                            clearControlButtons();
                            return;


                        }



                    }
                    else if (control.Text.Length > 0)
                    {
                        control.Text = "";

                    }
                    
                   
                }
                else if (control is GroupBox)
                {

                    foreach (RadioButton rb in control.Controls)
                    {
                        rb.Checked = false;
                        //rb.Enabled = false;



                    }



                }
                else if (control is ComboBox)
                {

                    object selectedItem= ((ComboBox)control).SelectedItem;
                    
                    if ((selectedItem != null) && (!hasAsked))
                    {

                        hasAsked = true;

                        //warn the user in case this is not what was intended.

                        string confLabel = "You will delete what you selected, are you sure?";
                        confirmBox.setFormLabel(confLabel, this);
                        confirmBox.ShowDialog();

                        //the confirmbox calls back to the mainForm which button was pressed
                        string buttonResult = confirmResult;

                        if (buttonResult == "yes")
                        {
                            //go ahead and delete
                            ((ComboBox)control).SelectedItem = null;
                            

                        }
                        else
                        {
                            //user has changed mind: cancel
                            clearControlButtons();
                            return;


                        }



                    }
                    else 
                    {
                        ((ComboBox)control).SelectedItem = null;

                    }




                }
                else if (control is Button)
                {
                    //control.Enabled = false;


                }



            }


        }





        private void radioButton2_Click(object sender, EventArgs e)
        {
            //user has clicked the "don't know" radio button on console.
            //make sure anything that has been selected or typed in is cleared, then disable the control

            clearMainControls();





        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            //user has clicked "No answer" radio button
            clearMainControls();



        }

        private void radioButton4_Click(object sender, EventArgs e)
        {
            //user has clicked "not applicable" radiobutton
            clearMainControls();



        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            //user has clicked "OK" radiobutton on console

            //enable main controls.

            foreach (Control control in panel1.Controls)
            {

                control.Enabled= true;
               
                if (control is GroupBox)
                {

                    foreach (RadioButton rb in control.Controls)
                    {

                        rb.Enabled = true;



                    }



                }



            }



        }


        public void setSkipControlsInvisible()
        {
            //turn off for labels which have no use of them
            groupBox1.Visible = false;




        }

        public void setSkipControlsVisible()
        {
            groupBox1.Visible = true;



        }

        public void startRecording()
        {
            label5.Visible = true;
            progressBar1.Visible = true;

        }

        public void stopRecording()
        {
            label5.Visible = false;
            progressBar1.Visible = false;



        }


        

        private void button4_Click(object sender, EventArgs e)
        {

            //debug: 
            qm.showDebug();






        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            //show the form-status window, i.e. the completeness of each section
            //disable locking

            

            qm.checkCompleteness(config, true);







        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

       

        
    }
}
