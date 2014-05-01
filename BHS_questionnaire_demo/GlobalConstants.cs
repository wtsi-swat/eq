using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    public class GlobalConstants
        //global vars

    {

        //properties

        public static string NoAnswer { get; set; }
        public static string DontKnow { get; set; }
        public static string NotApplicable { get; set; }
        public static string Skipped { get; set; }
        public static string SkippedBMI { get; set; }

        
        public static void setUpConstants(Qconfig conf)
        {
            
            
            
            //load from config
            if (conf.DontKnowValue == null)
            {
                DontKnow = "888";

            }
            else
            {
                DontKnow = conf.DontKnowValue;

            }


            if (conf.NoAnswerValue == null)
            {

                NoAnswer = "777";

            }
            else
            {

                NoAnswer = conf.NoAnswerValue;

            }

            if (conf.NotApplicableValue == null)
            {

                NotApplicable = "999";

            }
            else
            {
                NotApplicable = conf.NotApplicableValue;


            }

            if (conf.SkippedValue == null)
            {

                Skipped = "555";
            }
            else
            {
                Skipped = conf.SkippedValue;


            }


            if (conf.SkippedBMIvalue == null)
            {

                SkippedBMI = "444";

            }
            else
            {

                SkippedBMI = conf.SkippedBMIvalue;

            }



        }
        
        
        
        
        







    }
}
