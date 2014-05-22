
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
