using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    class Participant
    {

        private string dirPath;
        private string partID;
        private string language;

        private Dictionary<string, string> config= null;
        private string configFilePath;

        


        public void setConfig(Dictionary<string, string> config, string configFilePath)
        {

            this.config = config;
            this.configFilePath = configFilePath;


        }

        

        
        //integer version, when ids are always ints
        private int partIDAsInt;

        public string getDisplayLabel()
        {
            return qType + ":" + partID;



        }


        public string qType
        {
            set;
            get;


        }

        public void unlock()
        {

            //update the config file
            Dictionary<string,string> userDict= new Dictionary<string,string>();
            userDict.Add("lock_status", "unlocked");


            Utils.updateUserConfigFile(userDict, configFilePath);




        }


        public bool Completed
        {

            get
            {

                return Utils.isCompleted(config);




            }




        }
        
        
        
        public bool Locked
        {
            //set;
            //get;


            //read the setting from the config file
            //lock_status~locked

            get{

                string lockedStatus = Utils.readConfigSetting(config, "lock_status");

                if (lockedStatus == null)
                {
                    //if not set then not locked
                    return false;

                }
                else
                {
                    if (lockedStatus == "locked")
                    {

                        return true;
                    }
                    else
                    {
                        return false;


                    }


                }

            }
            


        }

        public int getPartIDint()
        {
            return partIDAsInt;


        }

        public void setPartIDint(int id)
        {

            partIDAsInt = id;

        }


        public Participant(string dirPath, string partID, string language){

            this.dirPath = dirPath;
            this.partID = partID;
            this.language = language;
            //Locked = false;



        }

        public string getGlobalID()
        {
            //ID which includes the study type

            return qType + ":" + partID;



        }

        public override string ToString()
        {

            string baseName= qType + ":" + partID;

            bool locked = Locked;
            bool completed = Completed;

            
            
            if (locked && completed)
            {
                return baseName + " (locked, complete)";

            }

            else if (locked)
            {

                return baseName + " (locked)";

            }
            else if (completed)
            {
                return baseName + " (complete)";

            }

            else
            {
                return baseName;

            }
            


        }

        public string getPath()
        {
            return dirPath;

        }

        public string getID()
        {

            return partID;

        }

        public string getLanguage()
        {

            return language;

        }

       











    }
}
