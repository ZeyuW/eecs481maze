using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;
//using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using FishGame.Speech;

namespace KinectColorApp
{
    enum Colors {Red, Green, Blue, White};
    //enum Backgrounds {Farm, Pokemon, Turtle, Planets, Pony, Car, AlreadySet};
    

    public partial class MainWindow : Window
    {
        private SpeechRecognizer mySpeechRecognizer; 

        public MainWindow()
        {
            InitializeComponent();
            drawingCanvas.Width = drawingGrid.ActualWidth;
            drawingCanvas.Height = drawingCanvas.Width * (3.0 / 4.0);
            backgroundImage.Width = drawingGrid.ActualWidth;
            backgroundImage.Height = drawingGrid.ActualHeight;
            backgroundImage.Visibility = Visibility.Hidden;
            drawBorder.Visibility = Visibility.Hidden;
            colorRect.Visibility = Visibility.Hidden;

            //buttons = new Ellipse[] { red_selector, blue_selector, green_selector, eraser_selector, background_selector, refresh_selector };

            buttons = new Image[] { fish_nimo, fish_a, fish_b, fish_c };

            drawController = new DrawController(drawingCanvas, backgroundImage, colorRect, image1, buttons);
            soundController = new SoundController();
            kinectController = new KinectController(drawController, image1, soundController, buttons);
        }

        private CalibrationController calController;
        private DrawController drawController;
        private SoundController soundController;
        private KinectController kinectController;
        private KinectSensor sensor;
        bool has_started_calibrating = false;
        //Ellipse[] buttons;
        Image[] buttons;
        Image[] Seaweed_list;
        bool isMazeOn = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors[0];

                if (this.sensor.Status == KinectStatus.Connected)
                {


                    Image[] codes = new Image[] { _0_code, _1_code, _2_code, _3_code, _4_code, };
                    foreach (Image i in codes)
                    {
                        i.Visibility = Visibility.Hidden;
                    }

                    
                    Seaweed_list = new Image[] { Seaweed };
                    foreach (Image ii in Seaweed_list)
                    {
                        ii.Visibility = Visibility.Hidden;
                    }
                    

                    _0_code.Visibility = Visibility.Visible;
                    calController = new CalibrationController(sensor, kinectController, drawingCanvas, codes, image1);
                    calController.CalibrationDidComplete += new CalibrationController.calibrationDidCompleteHandler(calibrationCompleted);
   

                    this.sensor.AllFramesReady += calController.DisplayColorImageAllFramesReady;
                    this.sensor.ColorStream.Enable();
                    this.sensor.DepthStream.Enable();
                    //this.sensor.ColorStream.CameraSettings.Contrast = 2.0;

                    Console.WriteLine("abc");

                    this.sensor.Start();

                    this.mySpeechRecognizer = SpeechRecognizer.Create();

                    if (null != this.mySpeechRecognizer)
                    {
                        this.mySpeechRecognizer.SaidSomething += this.RecognizerSaidSomething;
                        this.mySpeechRecognizer.Start(sensor.AudioSource);
                    }
                }
            }

            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.MouseDown += new MouseButtonEventHandler(OnClick);
            this.MouseDoubleClick += new MouseButtonEventHandler(OnDoubleClick);
            soundController.StartMusic();
            drawController.ChangeBackground();
            //drawController.ChangeColor(Colors.Red);

            buttons = new Image[] { fish_nimo, fish_a, fish_b, fish_c };
            foreach (Image i in buttons)
            {
                i.Visibility = Visibility.Hidden;
            }


        }

        private void Window_Size_Did_Change(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Width = drawingGrid.ActualWidth;
            drawingCanvas.Height = drawingCanvas.Width * (3.0 / 4.0);

            image1.Width = drawingGrid.ActualWidth;
            image1.Height = drawingCanvas.Width * (3.0 / 4.0);

            backgroundImage.Width = drawingGrid.ActualWidth - 40;
            backgroundImage.Height = drawingGrid.ActualHeight - 40;
            Canvas.SetLeft(backgroundImage, 0);
        }

        private void calibrationCompleted()
        {
            calibrationLabel.Content = "Done!";
            DoubleAnimation newAnimation = new DoubleAnimation();
            newAnimation.From = calibrationLabel.Opacity;
            newAnimation.To = 0.0;
            newAnimation.Duration = new System.Windows.Duration(TimeSpan.FromSeconds(2));
            newAnimation.AutoReverse = false;

            calibrationLabel.BeginAnimation(MediaElement.OpacityProperty, newAnimation, HandoffBehavior.SnapshotAndReplace);

            backgroundImage.Visibility = Visibility.Visible;
            drawBorder.Visibility = Visibility.Visible;
            Canvas.SetZIndex(calibrationLabel, 2);
            Canvas.SetZIndex(backgroundImage, 1);

            foreach (Image image in buttons)
            {
                Canvas.SetLeft(image, drawingCanvas.Width - image.Width - 10);
                image.Visibility = Visibility.Visible;
   
            }
            
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (!has_started_calibrating)
            {
                Canvas.SetZIndex(image1, 0);
                this.sensor.AllFramesReady -= calController.DisplayColorImageAllFramesReady;
                this.sensor.AllFramesReady += calController.CalibrationAllFramesReady;
                _0_code.Visibility = Visibility.Visible;
                calibrationLabel.Content = "Calibrating...";
                has_started_calibrating = true;
                image1.Source = null;
            }
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.Key.ToString());

            if (e.Key.ToString() == "R" || e.Key.ToString() == "F") {
                drawController.ClearScreen();
            }
            else if (e.Key.ToString() == "B" || e.Key.ToString() == "G")
            {
                soundController.TriggerBackgroundEffect();
                drawController.CycleBackgrounds();
            }
            else if (e.Key.ToString() == "Q" || e.Key.ToString() == "Space")
            {
                Application.Current.Shutdown();
            }
            else if (e.Key.ToString() == "U")
            {
                if (this.sensor.ColorStream.CameraSettings.Contrast < 2.0)
                {
                    this.sensor.ColorStream.CameraSettings.Contrast += 0.1;
                } 
            }
            else if (e.Key.ToString() == "D")
            {
                if (this.sensor.ColorStream.CameraSettings.Contrast > 0.6)
                {
                    this.sensor.ColorStream.CameraSettings.Contrast -= 0.1;
                } 
            }

            else if ((e.Key >= Key.D0 && e.Key <= Key.D3) || e.Key == Key.W || e.Key == Key.A || e.Key == Key.S)
            {
                if (e.Key == Key.W)
                {
                    HandleColorChange(0);
                }
                else if (e.Key == Key.A)
                {
                    HandleColorChange(1);
                }
                else if (e.Key == Key.S)
                {
                    HandleColorChange(2);
                }
                else
                {
                    HandleColorChange(e.Key - Key.D0);
                }

            }
        }

        void HandleColorChange(int inColor)
        {
            Colors c = (Colors)(inColor);
            soundController.TriggerColorEffect((int)c);
            drawController.ChangeColor(c);
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(this.sensor);
        }

        private void RecognizerSaidSomething(object sender, SpeechRecognizer.SaidSomethingEventArgs e)
        {
            switch (e.Verb)
            {
                case SpeechRecognizer.Verbs.ChangeBackground:
                    drawController.CycleBackgrounds();
                    break;
                case SpeechRecognizer.Verbs.ShowMaze:
                    isMazeOn = true;
                    drawController.showSeaweed(Seaweed_list);
                    Console.WriteLine("Swap background picture with picture that has the maze");
                    break;

                case SpeechRecognizer.Verbs.RemoveMaze:
                    if (isMazeOn)
                    {
                        drawController.hideSeaweed(Seaweed_list);
                        isMazeOn = false;
                    }
                    Console.WriteLine("Swap background picture with picture that does not have the maze");
                    break;

                case SpeechRecognizer.Verbs.Reset:
                    drawController.ClearScreen();
                    break;
            }
        }
    }
}
