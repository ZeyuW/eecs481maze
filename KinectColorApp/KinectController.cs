using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Shapes;
using System.Windows.Media.Effects;

namespace KinectColorApp
{
    class KinectController
    {
        private Image debugImage;
        private DrawController drawController;
        private SoundController soundController;
        Ellipse[] buttons;

        DateTime last_background_change = DateTime.Now;
        private bool hasSetDepthThreshold = false;
        private int DepthThreshold = 9000000;
        const int TextileSpacing = 5; // How deep do we have to push in to start drawing?

        // Variables used for calibration
        public double[] calibration_coefficients;
        private Point topLeft;
        private Point bottomRight;

        public KinectController(DrawController dController, Image image, SoundController sController, Ellipse[] buttons)
        {
            debugImage = image;
            drawController = dController;
            soundController = sController;
            this.buttons = buttons;
        }

        public void Calibrate(int top_left_x, int top_left_y, int bottom_right_x, int bottom_right_y)
        {
            if (top_left_y > 480 || top_left_y < 0)
            {
                top_left_y = 0;
            }

            if (bottom_right_y > 480 || bottom_right_y < 0)
            {
                bottom_right_y = 480;
            }

            topLeft = new Point(top_left_x, top_left_y);
            bottomRight = new Point(bottom_right_x, bottom_right_y);
        }

        public void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Check if we need to change background
            if (drawController.backgroundAlreadySet == false)
            {
                drawController.ChangeBackground(drawController.background);
            }
            
            // Check if we need to change color
            if (drawController.shouldChangeColor != -1)
            {
                drawController.ChangeColor((Colors)drawController.shouldChangeColor);
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    this.ParseDepthFrame(depthFrame);
                } 
            }
        }

        #region Getting textile touches

        bool gotTouch = false;

        private void ParseDepthFrame(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            int minDepth = DepthThreshold;
            //int bestDepthIndex = -1;
            int minDepthIndex = (int)this.topLeft.Y * depthFrame.Width;
            int maxDepthIndex = (int)this.bottomRight.Y * depthFrame.Width;

            minDepthIndex = 0;
            maxDepthIndex = 479 * depthFrame.Width;

            // a vector of touchIndexes to record locations of multi-touch
            List<int> touchIndexes = new List<int>();
            List<int> touchDepths = new List<int>();

            Console.WriteLine(minDepthIndex + " " + depthFrame.Width);

            if (!this.hasSetDepthThreshold)
            {
                int temp_minDepth = maxDepthIndex;
                for (int cali_temp = minDepthIndex; cali_temp < maxDepthIndex; cali_temp++)
                {
                    int cali_depth = rawDepthData[cali_temp] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    // Ignore invalid depth values
                    if (cali_depth == -1 || cali_depth == 0) continue;

                    if (cali_depth < temp_minDepth)
                    {
                        temp_minDepth = cali_depth;
                    }
                }

                this.DepthThreshold = temp_minDepth - TextileSpacing;
                this.hasSetDepthThreshold = true;
            }
           


            for (int depthIndex = minDepthIndex; depthIndex < maxDepthIndex; depthIndex++)
            {
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                
                // Ignore invalid depth values
                if (depth == -1 || depth == 0) continue;
                
               
                if (depth < DepthThreshold && this.hasSetDepthThreshold )
                {
                    int touchIndexesSize = touchIndexes.Count;
                    int cur_x = depthIndex % depthFrame.Width;
                    int cur_y = depthIndex / depthFrame.Width;
                    if (touchIndexesSize == 0)
                    {
                        touchIndexes.Add(depthIndex);
                        touchDepths.Add(depth);
                    }

                    bool is_add = true;
                    for (int touchNum = 0; touchNum < touchIndexesSize; touchNum++)
                    {
                        int touchIdx_x = touchIndexes[touchNum] % depthFrame.Width;
                        int touchIdx_y = touchIndexes[touchNum] / depthFrame.Width;
                        if (Math.Abs(cur_x - touchIdx_x) < 200 && Math.Abs(cur_y - touchIdx_y) < 200) {
                            if (depth < touchDepths[touchNum])
                            {
                                touchIndexes[touchNum] = depthIndex;
                                touchDepths[touchNum] = depth;
                            }
                            is_add = false;
                            break;
                        }
 
                    }
                    if (is_add)
                    {
                        touchIndexes.Add(depthIndex);
                        touchDepths.Add(depth);
                    }

                }
            }

            // Draw if a touch was found
            if (touchIndexes.Count > 0)
            {
                soundController.StartMusic();

                prepareDrawFish(depthFrame, touchIndexes, touchDepths);
                gotTouch = true;
                
            }
            else
            {
                if (gotTouch == true)
                {
                    soundController.StopMusic();
                }
                gotTouch = false;
            }
        }

        private void prepareDrawFish(DepthImageFrame depthFrame, List<int> touchIndexes, List<int> touchDepths)
        {
            List<int> depthList = new List<int>();
            List<Point> pointList = new List<Point>();
            for (int i = 0; i < touchIndexes.Count; i++)
            {
                double x_kinect = (touchIndexes[i] % depthFrame.Width);
                double y_kinect = (touchIndexes[i] / depthFrame.Width);

                double x = x_kinect * calibration_coefficients[0] + y_kinect * calibration_coefficients[1] + calibration_coefficients[2] + 3;
                double y = x_kinect * calibration_coefficients[3] + y_kinect * calibration_coefficients[4] + calibration_coefficients[5] + 10;

                Point point = new Point(x, y);
                depthList.Add(DepthThreshold - touchDepths[i]);
                pointList.Add(point);
            }
            drawController.DrawFishes(pointList, depthList);
        }


        /*
        private void DrawPoint(DepthImageFrame depthFrame, int depthIndex, int minDepth)
        {
            double x_kinect = (depthIndex % depthFrame.Width);
            double y_kinect = (depthIndex / depthFrame.Width);

            double x = x_kinect * calibration_coefficients[0] + y_kinect * calibration_coefficients[1] + calibration_coefficients[2] + 3;
            double y = x_kinect * calibration_coefficients[3] + y_kinect * calibration_coefficients[4] + calibration_coefficients[5] + 10;
            
            foreach (Ellipse ellipse in buttons)
            {
                double top = Canvas.GetTop(ellipse);
                double left = Canvas.GetLeft(ellipse);
                
                if (y >= top && x >= left && y <= top + ellipse.Height && x <= left + ellipse.Width)
                {
                    DropShadowEffect glowEffect = new DropShadowEffect();
                    glowEffect.ShadowDepth = 0;
                    glowEffect.Opacity = 1;
                    glowEffect.BlurRadius = 30;

                    if (ellipse.Name != "refresh_selector" && ellipse.Name != "background_selector")
                    {
                        foreach (Ellipse el in buttons)
                        {
                            if (el.Name != "refresh_selector" && el.Name != "background_selector")
                            {
                                el.Fill.Opacity = 0.3;
                                el.Effect = null;
                            }
                        }
                    }

                    // Use this button
                    
                    if (ellipse.Name == "red_selector")
                    {
                        ellipse.Fill.Opacity = 1;
                        glowEffect.Color = Color.FromArgb(255, 255, 44, 44);
                        ellipse.Effect = glowEffect;
                        drawController.ChangeColor(Colors.Red);
                    }
                    else if (ellipse.Name == "green_selector")
                    {
                        ellipse.Fill.Opacity = 1;
                        glowEffect.Color = Color.FromArgb(255, 53, 255, 53);
                        ellipse.Effect = glowEffect;
                        drawController.ChangeColor(Colors.Green);
                    }
                    else if (ellipse.Name == "blue_selector")
                    {
                        ellipse.Fill.Opacity = 1;
                        glowEffect.Color = Color.FromArgb(255, 115, 78, 255);
                        ellipse.Effect = glowEffect;
                        drawController.ChangeColor(Colors.Blue);
                    } 
                    else if (ellipse.Name == "eraser_selector")
                    {
                        ellipse.Fill.Opacity = 1;
                        glowEffect.Color = Color.FromArgb(255, 255, 255, 255);
                        ellipse.Effect = glowEffect;
                        drawController.ChangeColor(Colors.White);
                    }
                    else if (ellipse.Name == "background_selector")
                    {
                        TimeSpan interval = DateTime.Now - last_background_change;
                        if (interval.Seconds >= 0.5)
                        {
                            drawController.CycleBackgrounds();
                            last_background_change = DateTime.Now;
                        }
                    }
                    else if (ellipse.Name == "refresh_selector")
                    {
                        drawController.ClearScreen();
                    }

                    return;
                }
            }
            drawController.DrawEllipseAtPoint(x, y, (DepthThreshold - minDepth));
        }
        */

        #endregion

        #region Image creation

        // Call this function inside AllFramesReady to display a depth debugging feed
        void display_depth_feed(DepthImageFrame depthFrame)
        {
            byte[] pixels = this.GenerateColoredBytes(depthFrame);
            int stride = depthFrame.Width * 4;
            debugImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        }

        // Call this function inside AllFramesReady to display a color debugging feed
        void display_color_feed(ColorImageFrame colorFrame)
        {
            byte[] pixels = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(pixels);
            int stride = colorFrame.Width * 4;
            debugImage.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        }

        // Generates a color image from the depth frame
        public byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            // Create the RGB pixel array
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            // Loop through data and set colors for each pixel
            for (int depthIndex = 0, colorIndex = 0;
                 depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                 depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth == -1 || depth == 0) continue;

                byte intensity = CalculateIntensityFromDepth(depth);
                pixels[colorIndex + BlueIndex] = intensity;
                pixels[colorIndex + GreenIndex] = intensity;
                pixels[colorIndex + RedIndex] = intensity;
            }

            return pixels;
        }

        private static byte CalculateIntensityFromDepth(int distance)
        {
            return (byte)(255 - (255 * Math.Max(distance - 800, 0) / 2000));
        }

        #endregion
    }
}
