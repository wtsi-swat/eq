
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
    class GPSCountryManager
    {

        
        //maps country name to object that contains the limits for gps coordinates
        private GPSCountry thisCountry;

        private string countryName;

        public GPSCountryManager(string countryName)
        {

            this.countryName = countryName;

            if (countryName == "Any")
            {

                thisCountry = new GPSCountry("Any", 90M, -90M, 180M, -180M);

            }
            else if (countryName == "United Kingdom")
            {

                thisCountry= new GPSCountry("United Kingdom", 60.865M, 49.865M, 1.7676M, -8.1828M);



            }
            else if (countryName == "Malawi")
            {
                thisCountry= new GPSCountry("Malawi", -9.3636M, -17.1396M, 35.9244M, 32.6772M);


            }
            else
            {
                throw new Exception();

            }




        }


        public bool checkLimits(decimal latitude, decimal longitude)
        {

            return thisCountry.checkLimits(latitude, longitude);


        }

        public string getSelectedCountryName()
        {
            return countryName;



        }
















    }
}
