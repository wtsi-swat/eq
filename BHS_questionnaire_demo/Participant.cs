
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
