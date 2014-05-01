using System;
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
