using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;




namespace BHS_questionnaire_demo
{
    class AudioPlayer
    {

        private IWavePlayer wavePlayer;
        private AudioFileReader file;
        private string fileName;        //wav file to play
        private BaseForm bf;

        public AudioPlayer(string fileName, BaseForm bf){

            this.fileName = fileName;
            this.bf= bf;



        }

        public void playFile()
        {

            this.wavePlayer = new WaveOut();
            this.file = new AudioFileReader(fileName);
            this.file.Volume = 1.0f;
            this.wavePlayer.Init(file);
            this.wavePlayer.PlaybackStopped += wavePlayer_PlaybackStopped;
            this.wavePlayer.Play();

        }


        private void wavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (this.file != null)
            {
                this.file.Dispose();
                this.file = null;
            }
            if (this.wavePlayer != null)
            {
                this.wavePlayer.Dispose();
                this.wavePlayer = null;
            }

            bf.playbackFinished();


           
        }

        















    }
}
