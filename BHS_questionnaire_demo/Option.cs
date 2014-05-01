using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    public class Option
    {
        //An option in a set of radio buttons or select list

        //fields

        private string opValue;
        private string opText;

        private int widgetWidth;
        private int widgetHeight;
        private int widgetXpos;
        private int widgetYpos;


        //properties
        public bool Default { get; set; }

        //this is optional, i.e. only used if this option causes a branch in the processing
        public string ToCode { get; set; }

        //optional: same as ToCode, but also shows an error message
        public string ToCodeErr { get; set; }

        //used when the question has been shown too many times
        public string ToCodeSecond { get; set; }

        //a code label when we need special logic to work out where to go next
        public string ToCodeProcess { get; set; }

        

        //constructor

        public Option(string opValue, string opText)
        {
            this.opValue = opValue;
            this.opText = opText;

        }


        //methods
        public override string ToString()
        {
            return opText;
        }



        public string getValue()
        {
            return opValue;
        }

        public string getText()
        {

            return opText;
        }

        public int getWidgetWidth()
        {

            return widgetWidth;

        }

        public void setWidgetWidth(string width)
        {

            widgetWidth = Convert.ToInt32(width);

        }

        public int getWidgetHeight()
        {

            return widgetHeight;


        }

        public void setWidgetHeight(string height)
        {

            widgetHeight = Convert.ToInt32(height);
        }

        public int getWidgetXpos()
        {

            return widgetXpos;
        }

        public void setWidgetXpos(string xpos)
        {

            widgetXpos = Convert.ToInt32(xpos);


        }

        public void setWidgetYpos(string ypos)
        {

            widgetYpos = Convert.ToInt32(ypos);

        }

        public int getWidgetYpos()
        {

            return widgetYpos;
        }




    }
}
