using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    class GPSposition
    {

        private string latitude;
        private string longitude;

        public GPSposition(string latitude, string longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;




        }


        public string getXMLposition(string icon)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<Placemark>");
            sb.Append("<styleUrl>");
            sb.Append(icon);
            sb.Append("</styleUrl>");
            sb.Append("<Point>");
            sb.Append("<coordinates>");
            sb.Append(longitude);
            sb.Append(",");
            sb.Append(latitude);
            sb.Append("</coordinates>");
            sb.Append("</Point>");
            sb.Append("</Placemark>");

            return sb.ToString();



        }












    }
}
