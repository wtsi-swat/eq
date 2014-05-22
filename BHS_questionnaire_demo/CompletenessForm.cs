
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

namespace BHS_questionnaire_demo
{
    public partial class CompletenessForm : Form
    {

        //reference to parent form
        private BaseForm baseForm = null;
        private QuestionManager qm;
        private Form2 bigMessageBox;

        //panel location

        private int currentPanelXpos = 10;
        private int currentPanelYpos = 10;
        private int panelWidth = 950;
        private int panelHeight = 50;

        //is the form complete ?
        private bool isComplete = true;



        
        
        public CompletenessForm(bool lockingEnabled)
        {
            InitializeComponent();


            bigMessageBox = new Form2();

            

        }

        private void CompletenessForm_Load(object sender, EventArgs e)
        {

        }

        public Panel getNewPanel()
        {
            //create a new Panel for a section and return it
            Panel panel = new Panel();
            panel.Location = new Point(currentPanelXpos, currentPanelYpos);
            panel.Size = new Size(panelWidth, panelHeight);
            panel.BackColor = Color.FromArgb(0xF5, 0xDD, 0x9D);

            //add to the main panel
            panel1.Controls.Add(panel);

            //change the position for the next panel

            currentPanelYpos += (panelHeight + 2);

            return panel;






        }

        


        private void CompletenessForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            //fires when the form is closing, but not yet closed

            //tell the baseForm that we are closing so it can let other forms open
            //baseForm.questionFormClosing();





        }

        


        private void button2_Click(object sender, EventArgs e)
        {
            //user pressed OK: exit
            Close();




        }






    }
}
