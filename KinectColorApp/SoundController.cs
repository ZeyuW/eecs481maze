using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Media;

namespace KinectColorApp {
    class SoundController
    {

        // File paths for sounds
        const String kalimbaPath = @"../../Resources\Kalimba.mp3";
        const String redEffectPath = @"../../Resources\red_SFX.wav";
        const String greenEffectPath = @"../../Resources\green_SFX.wav";
        const String blueEffectPath = @"../../Resources\blue_SFX.wav";
        const String eraserEffectPath = @"../../Resources\eraser_SFX.wav";
        const String backgroundEffectPath = @"../../Resources\change_background_SFX.wav";

        const String loadedEffectPath = kalimbaPath;

        private MediaElement musicPlayer;
        private SoundPlayer effectPlayer;

        public SoundController()
        {
            musicPlayer = new MediaElement();
            musicPlayer.LoadedBehavior = MediaState.Manual;
            musicPlayer.UnloadedBehavior = MediaState.Manual;
            musicPlayer.MediaEnded += Media_Ended;
            effectPlayer = new SoundPlayer();
            musicPlayer.Source = new Uri(kalimbaPath, UriKind.RelativeOrAbsolute);
            musicPlayer.Volume = 0;
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            musicPlayer.Position = TimeSpan.Zero;
            musicPlayer.Play();
        }

        public void StartMusic()
        {
            musicPlayer.Play();

            DoubleAnimation newAnimation = new DoubleAnimation();
            newAnimation.From = musicPlayer.Volume;
            newAnimation.To = 0.5;
            newAnimation.Duration = new System.Windows.Duration(TimeSpan.FromSeconds(2));
            newAnimation.AutoReverse = false;

            musicPlayer.BeginAnimation(MediaElement.VolumeProperty, newAnimation, HandoffBehavior.SnapshotAndReplace);
        }

        public void StopMusic()
        {
            DoubleAnimation newAnimation = new DoubleAnimation();
            newAnimation.From = musicPlayer.Volume;
            newAnimation.To = 0.0;
            newAnimation.Duration = new System.Windows.Duration(TimeSpan.FromSeconds(5));
            newAnimation.AutoReverse = false;
            newAnimation.Completed += (s, e) =>
            {
                musicPlayer.Pause();
            };

            musicPlayer.BeginAnimation(MediaElement.VolumeProperty, newAnimation, HandoffBehavior.SnapshotAndReplace);
        }

        public void TriggerBackgroundEffect()
        {
            effectPlayer.Stop();
            effectPlayer.SoundLocation = backgroundEffectPath;
            effectPlayer.Play();
        }

        public void TriggerColorEffect(int c)
        {
            System.Console.WriteLine("Playing sound color effect " + c);
            effectPlayer.Stop();
            switch (c)
            {
                case 0:
                    effectPlayer.SoundLocation = redEffectPath;
                    effectPlayer.Play();
                    break;
                case 1:
                    effectPlayer.SoundLocation = greenEffectPath;
                    effectPlayer.Play();
                    break;
                case 2:
                    effectPlayer.SoundLocation = blueEffectPath;
                    effectPlayer.Play();
                    break;
                case 3:
                    effectPlayer.SoundLocation = eraserEffectPath;
                    effectPlayer.Play();
                    break;
                default:
                    effectPlayer.Stop();
                    break;
            }
        }
    }
}

