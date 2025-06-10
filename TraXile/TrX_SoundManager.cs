using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using log4net;

namespace TraXile
{
    internal class TrX_SoundManager
    {
        private ILog log;

        public TrX_SoundManager()
        {
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("TrX_SoundManager initialized.");
        }

        public void PlaySound(string sound_id)
        {
            try
            {
                WaveOutEvent waveOut = new WaveOutEvent();
                AudioFileReader audioFileReader = new AudioFileReader(Application.StartupPath + $@"\audio\{sound_id}");
                waveOut.Init(audioFileReader);
                waveOut.Play();
            }
            catch(Exception ex)
            {
               log.Error($"Error playing sound {sound_id}: {ex.Message}", ex);
            }

        }
    }

}
