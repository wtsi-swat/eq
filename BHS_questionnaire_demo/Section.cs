using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    public class Section
    {

        private string sectionTitle;
        private string firstQuestion;
        private bool complete = false;

        public Section(string sectionTitle, string firstQuestion){

            this.sectionTitle = sectionTitle;
            this.firstQuestion = firstQuestion;


        }

        public string getSectionTitle()
        {
            return sectionTitle;

        }

        public string getFirstQuestion()
        {
            return firstQuestion;


        }

        public void setCompleteness(bool complete)
        {
            this.complete = complete;


        }

        public bool getCompleteness()
        {
            return complete;

        }









    }
}
