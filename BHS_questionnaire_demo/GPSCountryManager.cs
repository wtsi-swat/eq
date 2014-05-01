using System;
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
