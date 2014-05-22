
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
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;




namespace BHS_questionnaire_demo
{
    class AudioRecorder
    {

        private IWaveIn waveIn = null;
        private WaveFileWriter writer;

        private string audioOutFilename;

        private string recorderState = "off";
        //off: not recording
        //on currently recording
        //paused: recorder on but paused


        private QuestionManager qm;


        public bool DiskSpaceOK    //if NOT true then we are running low on disk space, so recording should be stopped.
        {
            get;
            set;

        }



        //constructor
        public AudioRecorder(QuestionManager qm)
        {

            this.qm = qm;
            DiskSpaceOK = true;




        }



        public string getRecorderState()
        {
            return recorderState;


        }




        public void pauseRecording()
        {
            //stop data being written to disk
            if (DiskSpaceOK)
            {
                recorderState = "paused";
                qm.getMainForm().stopRecording();

            }
            
        }

        public void resumeRecording()
        {
            //allow data write again
            if (DiskSpaceOK)
            {
                recorderState = "on";
                qm.getMainForm().startRecording();

            }
            

        }

        public void startRecording()
        {

            if (DiskSpaceOK)
            {

                //get base dir
                //use a single file to keep the master recording
                audioOutFilename = qm.getBaseDir() + @"\raw_audio_data.wav";

               


                if (waveIn == null)
                {

                    waveIn = new WaveIn();
                    waveIn.WaveFormat = new WaveFormat(8000, 1);


                    writer = new WaveFileWriter(audioOutFilename, waveIn.WaveFormat);

                    waveIn.DataAvailable += OnDataAvailable;
                    waveIn.RecordingStopped += OnRecordingStopped;
                    waveIn.StartRecording();

                    recorderState = "on";
                    qm.getMainForm().startRecording();


                }


            }
            

        }
        
        
        /*
        public void startAudioRecording()
        {
            

                //enable visual that recording has started
                qm.getMainForm().setRecording();

                //we need to record audio
                string userDir = qm.getUserDir();
                audioOutFilename = userDir + @"\rawAudioRecorded.wav";
                mp3FileName = userDir + @"\audioRecording_" + Code + ".mp3";

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
         * 
         */
 


        void OnDataAvailable(object sender, WaveInEventArgs e)
        {

            //if we are paused, don't write incoming data to file
            if (recorderState == "on" && DiskSpaceOK)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);


            }
            


            //int secondsRecorded = (int)(writer.Length / writer.WaveFormat.AverageBytesPerSecond);
            
            
            /*
            if (secondsRecorded >= 30)
            {
                StopRecording();
            }
             */ 



        }
        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {


            Cleanup();

            if (e.Exception != null)
            {
                MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                                              e.Exception.Message));
            }


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


            //save data as MP3
            //MP3 file should be saved in the questionnaire dir

            //get current file version (0= no file)
            string userDir = qm.getUserDir();

            int fileVersion = getMp3FileVersion(userDir);
            fileVersion++;


            string mp3FileName = userDir + @"\audioRecording" + fileVersion + ".mp3";


            Process converter = new Process();
            converter.StartInfo.CreateNoWindow = true;
            converter.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            converter.StartInfo.UseShellExecute = false;

            //get location of the lame.exe program that does the conversion:



            converter.StartInfo.FileName = qm.getBaseDir() + @"\lame.exe";
            converter.StartInfo.Arguments = "-V2 \"" + audioOutFilename + "\" \"" + mp3FileName + "\"";
            // converter.Start(@"c:\users\sr7\documents\lame3.99.5\lame.exe", "-V2 \"" + Path.Combine(outputFolder, outputFilename) + "\" \"" + Path.Combine(outputFolder, mp3FileName) + "\"");

            converter.Start();
            //converter.WaitForExit();

            //MessageBox.Show("MP3 built");





        }

        public void stopRecording()
        {
            //Debug.WriteLine("StopRecording");


            if (waveIn != null)
            {
                waveIn.StopRecording();


            }

            qm.getMainForm().stopRecording();
            Cleanup();





        }

        private int getMp3FileVersion(string dirPath)
        {

            //will return 0 if no files exist or the most recent version number if any do.
            
            DirectoryInfo di = new DirectoryInfo(dirPath);
            string fileName;
            Match match;
            int maxFileVersion = 0;
            int thisFileVersion;


            foreach (FileInfo file in di.GetFiles()){

                fileName = file.Name;

                match = Regex.Match(fileName, @"audioRecording(\d+).mp3");


                if (match.Success)
                {
                    thisFileVersion = Convert.ToInt32(match.Groups[1].Value);

                    if (thisFileVersion > maxFileVersion)
                    {
                        maxFileVersion = thisFileVersion;


                    }

                }
                

            }

            return maxFileVersion;





        }
        


       


    }
}
