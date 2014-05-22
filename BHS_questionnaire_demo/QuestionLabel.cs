
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
