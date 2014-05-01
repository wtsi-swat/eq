using System;
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
