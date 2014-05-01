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
    class QuestionGetTime : Question
    {

        //fields
        private TextBox textbox;
        private Label label;
        private Button button;

        bool timeRecorded = false;


        //the data the user entered, which may be different to the processed data
        private string userData;

        private long timestamp=0;

        //private string Process;


        public QuestionGetTime(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm) : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            


        }

        public void setProcess(string process)
        {
            Process = process;

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
            getQM().getPanel().Controls.Remove(button);

            label.Dispose();
            textbox.Dispose();
            button.Dispose();




        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();


            //create a label and textbox control and a button
            label = new Label();
            label.ForeColor = GlobalColours.mainTextColour;

            textbox = new TextBox();
            textbox.BackColor = GlobalColours.controlBackColour;

            textbox.KeyPress += new KeyPressEventHandler(button_click);

            button = new Button();
            button.BackColor = GlobalColours.mainButtonColour;

            //set font size
            setFontSize(label, textbox, button);



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


            //position of the button
            button.Text = "click here when you want to record the time";
            button.Location = new Point(textBoxXpos, textBoxYpos + 50);
            button.Size = new Size(500, 50);
            button.Click += new EventHandler(button_click2);






            //if page seen before, populate the control with the previously entered text
            if (PageSeen)
            {
                textbox.Text = userData;


            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            getQM().getPanel().Controls.Add(textbox);
            getQM().getPanel().Controls.Add(button);


            setSkipSetting();





        }


        public void button_click2(object sender, EventArgs e)
        {
            //called when the user clicks the button to record the time

            //get the current time.
            DateTime now= DateTime.Now;

            //save as a timestamp
            timestamp = now.Ticks;


            //string timeHours = now.Hour.ToString();
            //string timeMins = now.Minute.ToString();
            


            //place this in the textbox
            //textbox.Text = timeHours + ":" + timeMins;

            //userData = timeHours + ":" + timeMins;

            userData = now.ToLongTimeString();
            textbox.Text = userData;


            //set the time as recorded
            //timeRecorded = true;

            //clear the skip control buttons
            clearControlButtons();

            //save the timestamp

            if (Process == "SetTimestamp")
            {
                getGS().Add("timestamp", "" + timestamp);

            }


            else if (Process == "CalcElapsedTime")
                    {

                        //calc the elapsed time since the questionnaire was started

                       
                            string ts = getGS().Get("timestamp");
                            if (ts == null)
                            {

                                Form3 warningBox = getQM().getWarningBox();

                                warningBox.setLabel("Warning: Can't calculate elapsed time as start time was not entered");
                                warningBox.ShowDialog();



                            }
                            
                            else {

                                long start = Convert.ToInt64(ts);

                                DateTime startDT = new DateTime(start);

                                //what is the difference between the start and now?

                                TimeSpan elapsed = now - startDT;

                                //save as the total elapsed seconds
                                getSD().Add("questionnaire elapsed seconds", "" + elapsed.TotalSeconds);




                            }
                            
                            
                           
                      

                    }




        }



        public override string processUserData()
        {

            


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

                //}

                return ToCode;




            }
            else
            {

                //get the hours and mins


                if (testNullEntry(userData))
                {
                    processedData = userData;

                    
                    
                    

                    return ToCode;

                }
                else
                {
                    ((Form2)getBigMessageBox()).setLabel("You must set the time");
                    getBigMessageBox().ShowDialog();
                    return Code;

                }
                
                
                


            }



            



        }





    }
}
