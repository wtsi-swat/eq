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
    class QuestionYear : Question
    {

        //fields

        private Label label;    //main label

        private ComboBox selectYears;
       

        //the data the user entered, which may be different to the processed data
        //private string userData;

        //properties
        public string OnError { get; set; }

        //constructor
        public QuestionYear(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {


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



        public override void removeControls()
        {
            getQM().getPanel().Controls.Remove(label);
           
            getQM().getPanel().Controls.Remove(selectYears);
            
            label.Dispose();
            
            selectYears.Dispose();

            

        }

        public override void configureControls(UserDirection direction)
        {

            //direction is either 'forward' or 'reverse'
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
            label.AutoSize = true;
            //label.Size = new Size(getWidgetWidth(), getWidgetHeight());

            //int yPosDateLabel= labelYpos + 50;

            int yPosSelect = labelYpos + 50;

            selectYears = new ComboBox();

            //set font size
            setFontSize(label, selectYears);

            //event handlers
            //trap any keypress to deselect the skip-controls
           
            selectYears.Click += new EventHandler(button_click);


            //stop user being able to type in the combobox
            selectYears.DropDownStyle = ComboBoxStyle.DropDownList;


            selectYears.BackColor = GlobalColours.controlBackColour;

            selectYears.Location = new Point(labelXpos, yPosSelect);
            selectYears.Size = new Size(160, 20);

            //restrict how many shown at once
            selectYears.IntegralHeight = false;
            selectYears.MaxDropDownItems = 5;


            

            //get the current year.
            int currentYear= DateTime.Now.Year;

            //first year should also be don't know
            selectYears.Items.Add("Don't Know");

            for (int i = currentYear; i >= 1900; i--)
            {
                selectYears.Items.Add(i);


            }




            //if userdirection is reverse, populate the control with the previously entered text
            if (PageSeen)
            {

                //assume the date is stored as dd/mm/yyyy

                //extract the day, months, years.

                if (processedData != null)
                {


                    string pYears = processedData;
                    if (pYears == "0")
                    {

                        selectYears.SelectedItem = "Don't Know";
                    }
                    else
                    {
                        //years must be numeric rather than string, otherwise won't be recognised by selectbox
                        selectYears.SelectedItem = Convert.ToInt32(pYears);

                    }
                    
                    

                }
               

                

            }

            //add to the form
            getQM().getPanel().Controls.Add(label);
            
            getQM().getPanel().Controls.Add(selectYears);

            setSkipSetting();



        }



        public override string processUserData()
        {

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

               // }

                return ToCode;


            }



            //get the selected date


           
            object sYears = selectYears.SelectedItem;

            //are any of these null
            if ( sYears == null)
            {
                ((Form2)getBigMessageBox()).setLabel("You must choose a year");
                getBigMessageBox().ShowDialog();

                return Code;
            }

            //convert to string

            
            string selectedYears = sYears.ToString();

            

            if (selectedYears == "Don't Know")
            {

                selectedYears = "0";
            }
            

            //convert to std date format
            processedData = selectedYears;



            //if we have a global setting: save to the global object
            string globalKey = SetKey;
            if (globalKey != null)
            {
                getGS().Add(globalKey, processedData);

            }

            //the control does all the validation

            return ToCode;






        }












    }
}
