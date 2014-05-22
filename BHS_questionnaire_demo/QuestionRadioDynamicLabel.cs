
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
