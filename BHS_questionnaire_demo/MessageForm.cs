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
    public partial class MessageForm : Form
    {
        public MessageForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();





        }

        

        public void setMainLabel(string labelText)
        {

            //set the dialog text to this
            label1.Text = labelText;


        }

        private void MessageForm_Load(object sender, EventArgs e)
        {

            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 3) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);



        }

        public void setLabel(string labelText)
        {

            //set the dialog text to this
            label1.Text = labelText;


        }
    }
}
