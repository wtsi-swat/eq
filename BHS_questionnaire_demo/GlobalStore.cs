
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
