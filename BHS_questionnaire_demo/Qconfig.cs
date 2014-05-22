
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
using System.IO;
using System.Windows.Forms;
using System.Drawing;



namespace BHS_questionnaire_demo
{
    public class Qconfig
    {


        public string SkippedValue { get; set; }
        public string NoAnswerValue { get; set; }
        public string DontKnowValue { get; set; }
        public string NotApplicableValue { get; set; }
        public string SkippedBMIvalue { get; set; }
        
        
        
        private List<string> langList;
        public List<Section> sectionList;
        private List<string> logolist;
        public List<string> LogoList
        {
            get
            {

                return logolist;


            }



        }
        private Form2 errorBox;
        private Dictionary<string, string> sectionToCodeMap;
        private List<PictureBox> pbList;

        //countries
        

        //map from country to object that contains settings for that country
        public Dictionary<string, Country> countryMap { get; set; }

        //global optionsets
        private Dictionary<string, List<Option>> optionMap;


        public string selectedCountryName { get; set; }




        public string getGlobalName(string key)
        {
            //used for string interpolation of vars into a label
            //depends on the key what is returned

            if (key == "current-country")
            {
                return selectedCountryName;


            }

            else if (key == "currency")
            {

                //get the currency of the currently selected country
                return countryMap[selectedCountryName].currency;



            }


            else
            {

                throw new Exception("unknown global name:" + key);


            }


        }



        public Qconfig(Form2 errorBox)
        {

            langList = new List<string>();
            sectionList = new List<Section>();
            this.errorBox = errorBox;
            sectionToCodeMap = new Dictionary<string, string>();
            logolist = new List<string>();
            pbList = new List<PictureBox>();

            countryMap = new Dictionary<string, Country>();

            //option sets that are not specific to the selected country
            optionMap = new Dictionary<string, List<Option>>();



        }

        public List<Option> getGlobalOptionList(string opKey){

            return optionMap[opKey];




        }




        public string[] getCountryNames()
        {


            List<string> keyList = new List<string>(countryMap.Keys);

            

            return keyList.ToArray();





        }

        public bool hasMultipleCountries(){

            if (countryMap.Count == 0)
            {
                return false;

            }
            else
            {
                return true;

            }


        }

        public List<Section> getSectionList()
        {
            return sectionList;

        }

        public string getSectionStartCode(string sectionName){

            return sectionToCodeMap[sectionName];

        }

        public List<string> getLanguageList()
        {
            return langList;

        }

        public void setSectionsComplete(string firstSectionName, string lastSectionName)
        {
            //set all sections from the first to the last complete, but not including the last section
            bool insideCompleteSection = false;

            foreach (Section section in sectionList)
            {
                if (section.getSectionTitle() == firstSectionName){

                    //first section to mark as complete
                    section.setCompleteness(true);
                    insideCompleteSection = true;
                    continue;


                }

                if (section.getSectionTitle() == lastSectionName)
                {
                    //last section: exit
                    break;


                }



                if((section.getSectionTitle() != lastSectionName) && insideCompleteSection){

                    //middle section

                    section.setCompleteness(true);
                    continue;


                }


            }




        }


        public void parseConfig(string configFilePath)
        {
            StreamReader reader = null;
            Country thisCountry = null;
            string countryName= null;
            string lang = null;
            string tribe = null;
            string opValue = null;  //the part of the option that is saved as data
            string opKey;
            string opText;
            string currency;


            try
            {

                reader = new StreamReader(configFilePath);
                Char[] delim = new Char[] { '~' };

                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(delim);


                    if (parts[0] == "")
                    {

                        //skip
                        continue;


                    }
                    
                    
                    else if (parts[0] == "language")
                    {

                        //add this as an option in the language box.
                        langList.Add(parts[1]);


                    }

                    else if (parts[0] == "section")
                    {

                        sectionList.Add(new Section(parts[1], parts[2]));
                        sectionToCodeMap[parts[1]] = parts[2];


                    }
                    else if (parts[0] == "logo")
                    {

                        logolist.Add(parts[1]);


                    }
                    else if (parts[0] == "country")
                    {

                        countryName= parts[1];
                        thisCountry = new Country(countryName);
                        countryMap[countryName] = thisCountry;
                       

                    }

                    else if (parts[0] == "country-language")
                    {
                        countryName = parts[1];
                        lang = parts[2];
                        opValue = parts[3];

                        //add this lang to this country
                        //assumes that the Countries are always parsed beforehand
                        countryMap[countryName].addLang(new Option(opValue, lang));


                    }

                    else if(parts[0] == "country-tribe"){

                        countryName = parts[1];
                        tribe = parts[2];
                        opValue = parts[3];

                        //add this lang to this country
                        //assumes that the Countries are always parsed beforehand
                        countryMap[countryName].addTribe(new Option(opValue, tribe));

                    }

                    else if (parts[0] == "country-currency")
                    {

                        countryName = parts[1];
                        currency = parts[2];
                        countryMap[countryName].currency = currency;




                    }

                    else if(parts[0] == "skipped_value"){

                        SkippedValue = parts[1];
                    }
                    else if(parts[0] == "no_answer_value"){

                        NoAnswerValue = parts[1];

                    }
                    else if(parts[0] == "dont_know_value"){

                        DontKnowValue = parts[1];

                    }
                    else if(parts[0] == "not_applicable_value"){


                        NotApplicableValue = parts[1];


                    }
                    else if(parts[0] == "skipped_bmi_value"){

                        SkippedBMIvalue = parts[1];


                    }

                    else
                    {

                        //assume part of optionMap
                        //e.g. country-list~Zambia~6

                        opKey = parts[0];
                        opText = parts[1];
                        opValue = parts[2];

                        List<Option> opList = null;

                        if (optionMap.ContainsKey(opKey))
                        {

                            opList = optionMap[opKey];

                        }
                        else
                        {
                            //blank list
                            opList = new List<Option>();
                            optionMap[opKey] = opList;


                        }

                        opList.Add(new Option(opValue, opText));





                    }




                }



            }
            catch
            {
                errorBox.setLabel("Error: Could not load config file");
                errorBox.ShowDialog();
                


            }
            finally
            {

                if (reader != null)
                {
                    reader.Close();

                }



            }





        }


        public void hideLogos(BaseForm baseform)
        {

            foreach (PictureBox pbox in pbList)
            {

                baseform.removeLogo(pbox);


            }






        }




        public void displayLogos(BaseForm baseform)
        {

            
            
            PictureBox pb;

            int xPos = 10;
            //int yPos = 400;
            int yPos = 480;
            int width;
            int height;
            int xSpacer = 10;    //gap between images

            //do we already have a list of picture boxes?
            if (pbList.Count == 0)
            {
                //no
                foreach (string logo in logolist)
                {

                    pb = new PictureBox();

                    pbList.Add(pb);



                    switch (logo)
                    {

                        case "cambridge_uni":
                            pb.Image = BHS_questionnaire_demo.Properties.Resources.cambridge_uni;

                            break;

                        case "logo_mrc":
                            pb.Image = BHS_questionnaire_demo.Properties.Resources.logo_mrc;

                            break;

                        case "sanger_logo":
                            pb.Image = BHS_questionnaire_demo.Properties.Resources.sanger_logo;

                            break;

                        case "univ_kwazulu_natal_logo":

                            pb.Image = BHS_questionnaire_demo.Properties.Resources.univ_kwazulu_natal_logo;

                            break;


                        default:
                            throw new Exception();

                            break;



                    }

                    width = pb.Image.Width;
                    height = pb.Image.Height;



                    //pb.Location = new Point(xPos, yPos);
                    pb.Location = new Point(xPos, yPos - height);
                    pb.Size = new Size(width, height);

                    baseform.addLogo(pb);

                    xPos = xPos + width + xSpacer;





                }



            }
            else
            {
                //yes
                //get from list
                foreach (PictureBox pbox in pbList)
                {

                    baseform.addLogo(pbox);


                }


            }




        }






















    }
}
