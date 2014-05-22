
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
using NAudio.Wave;
using System.Diagnostics;
using NAudio.CoreAudioApi;

namespace BHS_questionnaire_demo
{
    class AudioRecordingTest
    {
        //this version for use in the settings to make a quick test to see if microphone working.

        private IWaveIn waveIn = null;
        private WaveFileWriter writer;

        private string audioOutFilename;

        public AudioRecordingTest(string outFile)
        {

            audioOutFilename = outFile;


        }

        public void startRecording()
        {

           
            if (waveIn == null)
            {

                waveIn = new WaveIn();
                waveIn.WaveFormat = new WaveFormat(8000, 1);


                writer = new WaveFileWriter(audioOutFilename, waveIn.WaveFormat);

                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;
                waveIn.StartRecording();

               
            }


            


        }



        void OnDataAvailable(object sender, WaveInEventArgs e)
        {

           
            writer.Write(e.Buffer, 0, e.BytesRecorded);

            
            /*
            int secondsRecorded = (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);

            
            if (secondsRecorded >= 5)
            {
                stopRecording();
            }
             */ 
            
            



        }
        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {

            Cleanup();

           

        }

        private void Cleanup()
        {



            if (waveIn != null) // working around problem with double raising of RecordingStopped
            {
                waveIn.Dispose();
                waveIn = null;
            }
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }


            


        }

        public void stopRecording()
        {
            //Debug.WriteLine("StopRecording");


            if (waveIn != null)
            {
                waveIn.StopRecording();


            }

           
            Cleanup();



        }


    }
}
