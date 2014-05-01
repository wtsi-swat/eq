using System;
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
