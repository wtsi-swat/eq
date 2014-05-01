using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace BHS_questionnaire_demo
{
    class GlobalStore
    {

        //a key-value store that all questions can access

        private Dictionary<string, string> store;

        //constructor
        public GlobalStore()
        {
            store = new Dictionary<string, string>();




        }

        //add a key/value to the store
        public void Add(string key, string val)
        {

            //store.Add(key, val);
            store[key] = val;


        }

        //get a value from the store, given a key
        public string Get(string key)
        {
            try
            {
                return store[key];


            }
            catch (KeyNotFoundException e)
            {

                return null;

            }



        }



        public void save(System.IO.TextWriter dataOut)
        {

            //save the data stored in this object to a file

            foreach (KeyValuePair<string, string> kv in store)
            {

                dataOut.WriteLine(kv.Key + "\t" + kv.Value);

            }
            
            
           
        }

        public void load(StreamReader reader)
        {
            //read data out of this file

            Char[] delim = new Char[] { '\t' };

            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();

                //using tab as delim
                string[] parts = line.Split(delim);

                //add to the store

                store[parts[0]] = parts[1];



            }



        }





    }
}
