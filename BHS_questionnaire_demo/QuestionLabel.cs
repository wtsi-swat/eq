using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;



namespace BHS_questionnaire_demo
{
    class QuestionLabel : Question
    {

        //fields
        private Label label;
        
        
        
        //constructor: pass the form to the base constructor
        public QuestionLabel(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {

        }

        public override void removeControls()
        {
            //getForm().Controls.Remove(label);
            getQM().getPanel().Controls.Remove(label);
            label.Dispose();



        }



        public override void configureControls(UserDirection direction)
        {
            
            //turn off the skip controls
            getQM().getMainForm().setSkipControlsInvisible();
            
            
            
            
            label = new Label();

            //set font size
            setFontSize(label);
            
            //the question Text shown to the user
            label.Text = Val;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

           

            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());
            label.ForeColor = GlobalColours.mainTextColour;

            //getForm().Controls.Add(label);

            getQM().getPanel().Controls.Add(label);

            //start audio recording if enabled
            audioRecording();



          


        }

        public override string processUserData()
        {

            
            
            
            //there is no user data for this control
            return ToCode;



        }

        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {
            //do nothing

        }



        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

           //do nothing

        }






    }
}
