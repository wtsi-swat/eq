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
    public partial class AdviceForm : Form
    {
        public AdviceForm()
        {
            InitializeComponent();
        }


        public void setAdviceText(string heading, string advice)
        {
            label1.Text = heading;
            label2.Text = advice;

          

        }



        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();





        }
    }
}
