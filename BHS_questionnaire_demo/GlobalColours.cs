
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
using System.Drawing;

namespace BHS_questionnaire_demo
{
    class GlobalColours

    {



        //colours for the Questionnaire



        public static Color mainFormColour
        {

            
            //white fonts version
            get { return Color.FromArgb(0x0, 0x0, 0x0); }

            
            //black fonts version
            //get { return Color.FromArgb(0xFF, 0xFF, 0xFF); }

            


        }

        public static Color mainPanelColour
        {

            //black fonts version
            //get { return Color.FromArgb(0xF0, 0xF0, 0xF0); }
            
            
            //white fonts version
            get { return Color.FromArgb(0x40, 0x40, 0x40); }

          

        }

        public static Color mainButtonColour
        {
            //get { return Color.FromArgb(0x92, 0xA6, 0x8A); }

            get { return Color.FromArgb(0xC8, 0xC8, 0xC8); }

        }


        public static Color altButtonColour
        {

           // get { return Color.FromArgb(0xBC, 0xC4, 0x99); }

            get { return Color.FromArgb(0x88, 0x88, 0x88); }

        }

        public static Color controlBackColour
        {

            get { return Color.FromArgb(0xF5, 0xDD, 0x9D); }
        }


        public static Color mainTextColour
        {
            
            //black fonts version
            //get { return Color.FromArgb(0x0, 0x0, 0x0); }
            
            
            
            //white fonts version
            get { return Color.FromArgb(0xFF, 0xFF, 0xFF); }
            

        }




    }
}
