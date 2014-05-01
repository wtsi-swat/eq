using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Drawing;






namespace BHS_questionnaire_demo
{
    class Utils
    {

        //static utility functions

        public static bool GPSenabled { get; set; }
        


        public static List<string> getSectionList(){

            List<string> sections= new List<string>();

            //Malawi
            /*
            sections.Add("Consents");
            sections.Add("Demographic Information");
            sections.Add("Education, Occupation and Livelihood");
            sections.Add("Health: Tobacco Use");
            sections.Add("Health: Alcohol Consumption");
            sections.Add("Health: Diet");
            sections.Add("Physical Activity: Work");
            sections.Add("Physical Activity: Travel to and from places");
            sections.Add("Physical Activity: Recreational Activities");
            sections.Add("Physical Activity: Sedentary Behaviour");
            sections.Add("History of Raised Blood Pressure");
            sections.Add("History of Diabetes");
            sections.Add("History of High Cholesterol");
            sections.Add("History of Immunisation");
            sections.Add("Physical Measurements: Blood Pressure");
            sections.Add("Physical Measurements: Anthropometry");
            sections.Add("Blood Sample");
            sections.Add("Advice");
            sections.Add("Final Comments");
            */


            //SA
            sections.Add("Consents");
            sections.Add("Personal History");
            sections.Add("Zulu");
            sections.Add("Family History");
            sections.Add("Diabetes");
            sections.Add("Clinical Information");
            sections.Add("Clinical Measurements");



            return sections;

            



        }



        public static string getPosition(string nmeaNumber, string hemisphere, bool pureDecimal)
        {

            //convert the NMEA string data into a valid position
            //nmea number is Latitude or longitude
            //nmea hemisphere is N,E,S or W
            //pureDecimal: return with hemisphere as + for North, - for South, + for East, - for West


            /*
             * 
             * NMEA Format:
             * 
            $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

            Where:
                 GGA          Global Positioning System Fix Data
                 123519       Fix taken at 12:35:19 UTC
                 4807.038,N   Latitude 48 deg 07.038' N
                 01131.000,E  Longitude 11 deg 31.000' E
                 1            Fix quality: 0 = invalid
                                           1 = GPS fix (SPS)
                                           2 = DGPS fix
                                           3 = PPS fix
			                   4 = Real Time Kinematic
			                   5 = Float RTK
                                           6 = estimated (dead reckoning) (2.3 feature)
			                   7 = Manual input mode
			                   8 = Simulation mode
                 08           Number of satellites being tracked
                 0.9          Horizontal dilution of position
                 545.4,M      Altitude, Meters, above mean sea level
                 46.9,M       Height of geoid (mean sea level) above WGS84
                                  ellipsoid
                 (empty field) time in seconds since last DGPS update
                 (empty field) DGPS station ID number
                 *47          the checksum data, always begins with *


            */


            //e.g. GPGGA,124053.000,5204.6890,N,00011.1347,E,1,05,3.2,38.0,M,47.0,M,,0000*64
            // latitude component:5204.6890
            //longitude component:00011.1347



            string degs;
            string minutes;


            //extract degs,minutes from nmean string
            //minutes are always the 2 digits left of the decimal point and all of the digits right of it
            //degrees are all the digits left of the minutes, if any.


            Match match = Regex.Match(nmeaNumber, @"^(\d+)(\d\d\.\d+)$");

            if(match.Success){

                degs = match.Groups[1].Value;
                minutes= match.Groups[2].Value;


            }
            else{

                //serious error
                throw new Exception();


            }

            //convert minutes to decimal form by / 60

            decimal converted = Convert.ToDecimal(degs) + (Convert.ToDecimal(minutes) / 60M);

            //round to 12 decimal places
            converted = Math.Round(converted, 12);

            

            //final formatting by adding the hemisphere component as either a sign or symbol
            string finalValue;

            if (pureDecimal)
            {
                //sign
                if (hemisphere == "S" || hemisphere == "W")
                {
                    //- (Note: don't need to do anything for N, E which are +, which is implied)

                    finalValue = "-" + converted.ToString();


                }
                else
                {

                    finalValue = converted.ToString();

                }
                



            }
            else
            {
                //symbol: Add N,S,E,W letter at start
                finalValue = hemisphere + converted.ToString();





            }

            return finalValue;






        }

        public static void setPartLocked(string userConfigFileName)
        {
            //lock this participant in the config.
            Dictionary<string,string> userDict= new Dictionary<string,string>();

            userDict["lock_status"] = "locked";

            updateUserConfigFile(userDict, userConfigFileName);





        }

        public static bool isCompleted(Dictionary<string, string> rawData)
        {

            //extract all the keys related to completeness
            Match match;
            bool sectionComplete;
            bool hasSections = false;


            foreach (KeyValuePair<string, string> kv in rawData)
            {

                //showInPanel(kv.Value, kv.Key, null, cf);
                match = Regex.Match(kv.Key, @"^section_complete\^.+$");
                //section_complete^A. Patient Details~True


                if (match.Success)
                {

                    hasSections = true;

                    sectionComplete = Convert.ToBoolean(kv.Value);

                    if (!sectionComplete)
                    {
                        //whole form not complete
                        return false;


                    }

                    


                }



            }

            if (hasSections)
            {

                //all sections complete
                return true;

            }
            else
            {
                //nothing found in config for completeness
                return false;


            }








        }




        public static void updateUserConfigFile(Dictionary<string,string> userDict, string userConfigFileName)
        {

            //load current contents (if any) into a dict, modify the dict then write back to the file.
            //get each key/value pair from userData and insert into the dict, then output the dict
            //in th config file, key/value pairs are split via ~


            //read values from file into Dict.

            StreamReader dhConf = null;
            Dictionary<string, string> confDict = new Dictionary<string, string>();


            try
            {
                dhConf = new StreamReader(userConfigFileName);

                Char[] delim = new Char[] { '~' };

                while (dhConf.EndOfStream == false)
                {
                    string line = dhConf.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    confDict[parts[0]] = parts[1];



                }

            }
            finally
            {

                if (dhConf != null)
                {

                    dhConf.Close();
                }

            }


            //insert the new values into the Dict.
            foreach (KeyValuePair<string, string> kv in userDict)
            {
                confDict[kv.Key] = kv.Value;

            }



            //write the Dict back to the file
            StreamWriter dhConfOut = null;

            try
            {
                dhConfOut = new StreamWriter(userConfigFileName);

                foreach (KeyValuePair<string, string> kv in confDict)
                {

                    dhConfOut.WriteLine(kv.Key + "~" + kv.Value);



                }




            }
            finally
            {

                if (dhConfOut != null)
                {

                    dhConfOut.Close();
                }

            }




        }


        public static Dictionary<string, bool> readCompletionData(string userConfigFileName)
        {

            //get the raw data from the file
            Dictionary<string, string> rawData = readUserConfigFile(userConfigFileName);

            //get only the completion data from this set
            Dictionary<string, bool> compDict = new Dictionary<string, bool>();

            Match match= null;
            string sectionName;
            bool completed;


            foreach (KeyValuePair<string, string> kv in rawData)
            {

                //showInPanel(kv.Value, kv.Key, null, cf);
                match = Regex.Match(kv.Key, @"^section_complete\^(.+)$");
                //section_complete^A. Patient Details~True


                if(match.Success){

                    sectionName = match.Groups[1].Value;
                    completed= Convert.ToBoolean(kv.Value);

                    compDict[sectionName]= completed;



                }
           



                

            }


            return compDict;




        }


        public static string readConfigSetting(Dictionary<string, string> conf, string key)
        {

            //get the value for this key

            if(conf.ContainsKey(key)){

                return conf[key];

            }
            else{

                return null;

            }



        }





        public static Dictionary<string, string> readUserConfigFile(string userConfigFileName)
        {
            //reads contents and returns as a Dict.

            //read values from file into Dict.

            StreamReader dhConf = null;
            Dictionary<string, string> confDict = new Dictionary<string, string>();


            try
            {
                dhConf = new StreamReader(userConfigFileName);

                Char[] delim = new Char[] { '~' };

                while (dhConf.EndOfStream == false)
                {
                    string line = dhConf.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    confDict[parts[0]] = parts[1];



                }

                return confDict;


            }
            finally
            {

                if (dhConf != null)
                {

                    dhConf.Close();
                }

            }





        }


        public static void displayCompletionWindow(Dictionary<string,bool> compDict){

            //compData is section, complete (true,false)
            
            CompletenessForm cf = new CompletenessForm(false);

            

            foreach (KeyValuePair<string, bool>  kv in compDict)
            {

                showInPanel(kv.Value, kv.Key, null, cf);


            }


            cf.Show();

            


        }

        private static void showInPanel(bool sectionComplete, string sectionName, string message, CompletenessForm cf)
        {

            //show message in a panel on the completenessform
            //get a new panel to use

            Panel panel = cf.getNewPanel();

            Label label = new Label();

            if (message != null)
            {
                label.Text = sectionName + " (" + message + ")";

            }
            else
            {
                label.Text = sectionName;

            }


            

            label.Location = new Point(0, 10);
            label.Size = new Size(700, 50);
            setFontSize(label);


            //draw an icon to show a tick or cross, i.e. if sectionComplete is true draw a tick otherwise draw a cross
            //capture the paint event of the panel.
            PictureBox pb = new PictureBox();
            pb.Location = new Point(900, 0);
            pb.Size = new Size(48, 48);

            if (sectionComplete)
            {
                pb.Image = BHS_questionnaire_demo.Properties.Resources.onebit_34;

            }
            else
            {
                pb.Image = BHS_questionnaire_demo.Properties.Resources.onebit_33;

            }
            
            


            panel.Controls.Add(label);
            panel.Controls.Add(pb);





        }

        public static void setFontSize(params Control[] controls)
        {

            //varargs function, takes any number of Controls

            float fontSize = 18;

            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.Name, fontSize, control.Font.Style, control.Font.Unit);

            }



        }



        public static bool testQuestionRefs(Dictionary<string, Question> questionHash, List<Option> opList)
        {

            string toCode;
            foreach (Option op in opList)
            {

                //does this option have a tocode
                toCode = op.ToCode;

                if (toCode != null)
                {

                    if (!questionHash.ContainsKey(toCode))
                    {

                        return false;


                    }

                }

                toCode = op.ToCodeErr;

                if (toCode != null)
                {

                    if (!questionHash.ContainsKey(toCode))
                    {

                        return false;


                    }

                }

                toCode = op.ToCodeSecond;

                if (toCode != null)
                {

                    if (!questionHash.ContainsKey(toCode))
                    {

                        return false;


                    }

                }



            }

            return true;




        }










    }
}
