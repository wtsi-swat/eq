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
