using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace BHS_questionnaire_demo
{
    class RankBox
    {
        private Label label;
        private ComboBox select;
        private Question question;
        private int spacer = 5;
        private QuestionManager qm;
        private Option previouslySelectedOption;
        private string processedData;



        public RankBox(string labelText, Question question, int xPos, int yPos, int width, int selectLength, QuestionManager qm)
        {

            this.question = question;
            label = new Label();
            label.Text = labelText;
            label.ForeColor = GlobalColours.mainTextColour;
            label.Location = new Point(xPos, yPos);
            label.Size = new Size(width, 30);


            select = new ComboBox();
            
            select.Click += new EventHandler(question.button_click);

            //stop user being able to type in the combobox
            select.DropDownStyle = ComboBoxStyle.DropDownList;
            select.BackColor = GlobalColours.controlBackColour;

            select.Location = new Point(xPos, yPos + 30 + spacer);
            select.Size = new Size(selectLength, 20);
            select.IntegralHeight = false; //won't work unless this is set to false
            select.MaxDropDownItems = 5;

            question.setFontSize(label, select);

            this.qm = qm;

            qm.getPanel().Controls.Add(label);
            qm.getPanel().Controls.Add(select);

            //MessageBox.Show("Adding label at Y:" + yPos + ". Adding select at Y:" + (yPos + 30 + spacer));
            




        }

        public void AddOption(Option op)
        {
            select.Items.Add(op);

        }

       

        public void removeControls()
        {


            qm.getPanel().Controls.Remove(label);
            qm.getPanel().Controls.Remove(select);
            label.Dispose();
            select.Dispose();



        }

        public void setProcessedData(string pd)
        {
            processedData = pd;

        }

        public string getProcessedData()
        {
            return processedData;

        }
        
        
        
        /*
        public void setProcessedData(Option op)
        {

            if (processedData == op.getValue())
            {
                previouslySelectedOption = op;

            }


        }
         */
 

        public void setSelectedOption(Option op)
        {

           
            select.SelectedItem = op;

           

        }

        public Option getSelectedOption()
        {

            return (Option)(select.SelectedItem);

        }

        


    }
}
