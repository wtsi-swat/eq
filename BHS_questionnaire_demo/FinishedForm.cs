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
    public partial class FinishedForm : Form
    {

        Timer timer;
        
        public FinishedForm()
        {
            InitializeComponent();
        }


        public void setLabel(string text)
        {

            label1.Text = text;



        }

        private void button1_Click(object sender, EventArgs e)
        {

            //close this form
            this.Close();
        }

        private void FinishedForm_Load(object sender, EventArgs e)
        {


            //center the form
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //Divide the screen in half, and find the center of the form to center it
            this.Top = (rect.Height / 3) - (this.Height / 2);
            this.Left = (rect.Width / 2) - (this.Width / 2);

            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = 1000;
            timer.Start();

            
                




        }

        private void timer_Tick(object sender, EventArgs e)
        {

            //stop the timer
            timer.Stop();
            timer.Dispose();


            //close this form
            this.Close();

        }






    }
}
