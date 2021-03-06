
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
    public partial class ConfirmForm : Form
    {

        private Form parentForm = null;

        
        
        
        public ConfirmForm()
        {
            InitializeComponent();
        }


        public void setFormLabel(string text, Form parentForm)
        {

            label1.Text = text;
            this.parentForm = parentForm;



        }

        private void button1_Click(object sender, EventArgs e)
        {
            //user has clicked the Yes button
            //send a message to the parent form.

            if (parentForm is BaseForm)
            {
                ((BaseForm)parentForm).confirmResult= "yes";



            }
            else if (parentForm is Form1)
            {

                ((Form1)parentForm).confirmResult = "yes";

            }
            else
            {

                throw new Exception();

            }


            this.Close();





        }

        private void button2_Click(object sender, EventArgs e)
        {
            //user has clicked the No button

            if (parentForm is BaseForm)
            {
                ((BaseForm)parentForm).confirmResult = "no";



            }
            else if (parentForm is Form1)
            {

                ((Form1)parentForm).confirmResult = "no";

            }
            else
            {

                throw new Exception();

            }


            this.Close();





        }

        private void ConfirmForm_Load(object sender, EventArgs e)
        {
            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 3) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);




        }





    }
}
