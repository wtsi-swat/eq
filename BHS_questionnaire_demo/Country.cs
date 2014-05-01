using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    public class Country
    {

        private string name;

        public string currency { get; set; }

        public List<Option> tribes { get; set; }

        public List<Option> langs { get; set; }


        public Country(string name)
        {
            this.name = name;
            tribes = new List<Option>();
            langs = new List<Option>();



        }

        public void addTribe(Option tribe)
        {

            tribes.Add(tribe);

        }

        public void addLang(Option lang)
        {

            langs.Add(lang);

        }







    }
}
