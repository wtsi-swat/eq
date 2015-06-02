
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

        //a code label when we want to fetch the text of the option from a previously selected one
        public string PrevSelectedOpCode { get; set; }

        //a code to use to get the text when the above was 'Other'
        public string PrevSelectedOpCodeOther { get; set; }

        //is this option currently selected?
        public bool isSelected { get; set; }




        //constructor

        public Option(string opValue, string opText)
        {
            this.opValue = opValue;
            this.opText = opText;
            this.isSelected = false;


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

        public void setText(string text)
        {

            opText = text;


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
