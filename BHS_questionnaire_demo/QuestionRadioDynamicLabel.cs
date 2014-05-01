using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;


namespace BHS_questionnaire_demo
{
    
    //same as a radio button, but with an extra function that will be used to generate the 
    //label, i.e. based on some other thing in the system

    class QuestionRadioDynamicLabel : QuestionRadio
    {

        public string LabelGeneratorFunc { get; set; }

         //constructor
        public QuestionRadioDynamicLabel(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
           



        }


        public override void configureControls(UserDirection direction)
        {


            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();

            radioGroup = new GroupBox();
            radioGroup.ForeColor = GlobalColours.mainTextColour;

            setFontSize(radioGroup);


            //position the group box under the label, i.e. at the same xpos but an increased ypos
            int groupBoxXpos = getWidgetXpos();
            int groupBoxYpos = getWidgetYpos();



            //position of the groupbox
            radioGroup.Location = new Point(groupBoxXpos, groupBoxYpos);
            radioGroup.Size = new Size(getWidgetWidth(), getWidgetHeight());


            //label: this needs to be calcuated

            if (LabelGeneratorFunc == "DisplayBMI")
            {

                string label;

                //get the precalculated BMI
                string BMI = getSD().Get("BMI");

                if (BMI == null)
                {
                    label = "BMI is unavailable. Please select 'No', below";

                }
                else
                {
                    label = "BMI calculated as: " + BMI + ". Is this correct?";


                }

                radioGroup.Text = Val + " " + label;



            }
            else
            {
                throw new Exception();


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

                if (!PageSeen)
                {
                    if (op.Default)
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



        }





    }
}
