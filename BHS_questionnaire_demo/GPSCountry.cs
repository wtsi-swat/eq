using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    class GPSCountry
    {

        //properties
        public decimal limitNorth { get; set; }
        public decimal limitSouth { get; set; }
        public decimal limitEast { get; set; }
        public decimal limitWest { get; set; }

        public string countryName { get; set; }


        public GPSCountry(string countryName, decimal limitNorth, decimal limitSouth, decimal limitEast, decimal limitWest)
        {
            this.countryName = countryName;
            this.limitNorth = limitNorth;
            this.limitSouth = limitSouth;
            this.limitEast = limitEast;
            this.limitWest = limitWest;


        }


        public bool checkLimits(decimal latitude, decimal longitude){

            //is this latitude within the allowed limits for this country ?

            if ((latitude > limitNorth) || (latitude < limitSouth))
            {
                //error
                return false;

            }



            //is this longitude within the allowed limits for this country ?
            if ((longitude > limitEast) || (longitude < limitWest))
            {
                //error
                return false;

            }


            return true;



        }














    }
}
