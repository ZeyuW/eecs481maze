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
        Image[] buttons;

        DateTime last_background_change = DateTime.Now;
        private bool hasSetDepthThreshold = false;
        private int DepthThreshold = int.MaxValue;
        const int TextileSpacing = 5; // How deep do we have to push in to start drawing?

        List<Point> lastFrameDrawnPoints = new List<Point>();

        // Variables used for calibration
        public double[] calibration_coefficients;
        private Point topLeft;
        private Point bottomRight;

        public KinectController(DrawController dController, Image image, SoundController sController, Image[] buttons)
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


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    this.ParseDepthFrame(depthFrame);
                } 
            }
        }

        #region Getting textile touches

        

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

            
            for (int last_point_idx = 0; last_point_idx < lastFrameDrawnPoints.Count; last_point_idx++)
            {
                double last_x_kinect = lastFrameDrawnPoints[last_point_idx].X;
                double last_y_kinect = lastFrameDrawnPoints[last_point_idx].Y;
                
                int tmp_max_depth = 0;
                bool if_found = false;
                int last_index = 0;
                for (double start_x_kinect = last_x_kinect - 40; start_x_kinect <= last_x_kinect + 40; start_x_kinect++)
                {
                    for (double start_y_kinect = last_y_kinect - 40; start_y_kinect <= last_y_kinect + 40; start_y_kinect++)
                    {
                        last_index = (int)(last_y_kinect * depthFrame.Width + last_x_kinect);
                        int tmp_depth = rawDepthData[last_index] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                        if (tmp_depth > tmp_max_depth && DepthThreshold - tmp_depth > 50)
                        {
                            if_found = true;
                            tmp_max_depth = tmp_depth;
                        }
                    }
                }
                if (if_found)
                {
                    touchIndexes.Add(last_index);
                    touchDepths.Add(tmp_max_depth);
                }

            }
           


            for (int depthIndex = minDepthIndex; depthIndex < maxDepthIndex; depthIndex++)
            {
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                
                // Ignore invalid depth values
                if (depth == -1 || depth == 0) continue;
                
               
                if (DepthThreshold - depth > 20 && this.hasSetDepthThreshold )
                {
                    int touchIndexesSize = touchIndexes.Count;
                    int cur_x = depthIndex % depthFrame.Width;
                    int cur_y = depthIndex / depthFrame.Width;
            

                    bool is_add = true;
                    for (int touchNum = 0; touchNum < touchIndexesSize; touchNum++)
                    {
                        is_add = true;
                        int touchIdx_x = touchIndexes[touchNum] % depthFrame.Width;
                        int touchIdx_y = touchIndexes[touchNum] / depthFrame.Width;
                        if (Math.Abs(cur_x - touchIdx_x) < 150 && Math.Abs(cur_y - touchIdx_y) < 150) {
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
                prepareDrawFish(depthFrame, touchIndexes, touchDepths);
                
            }
           
        }


        public void resetFishSize()
        {
            foreach (Image tmp in buttons)
            {
                if (tmp.Name == "fish_nimo")
                {
                    tmp.Height = 35;
                    tmp.Width = 60;
                }
                else if (tmp.Name == "fish_a")
                {
                    tmp.Height = 50;
                    tmp.Width = 50;
                }
                else if (tmp.Name == "fish_b")
                {
                    tmp.Height = 55;
                    tmp.Width = 44;
                }
                else if (tmp.Name == "fish_c")
                {
                    tmp.Height = 47;
                    tmp.Width = 40;
                }
            }
        }


        public void increaseFishSize(Image in_fish)
        {
            if (in_fish.Name == "fish_nimo")
            {
                in_fish.Height = 53;
                in_fish.Width = 90;
            }
            else if (in_fish.Name == "fish_a")
            {
                in_fish.Height = 75;
                in_fish.Width = 75;
            }
            else if (in_fish.Name == "fish_b")
            {
                in_fish.Height = 83;
                in_fish.Width = 66;
            }
            else if (in_fish.Name == "fish_c")
            {
                in_fish.Height = 71;
                in_fish.Width = 60;
            }
        }


        private void prepareDrawFish(DepthImageFrame depthFrame, List<int> touchIndexes, List<int> touchDepths)
        {
            List<int> depthList = new List<int>();
            List<Point> pointList = new List<Point>();
            List<Point> kinectPointList = new List<Point>();
            
            for (int i = 0; i < touchIndexes.Count; i++)
            {
                double x_kinect = (touchIndexes[i] % depthFrame.Width);
                double y_kinect = (touchIndexes[i] / depthFrame.Width);

                double x = x_kinect * calibration_coefficients[0] + y_kinect * calibration_coefficients[1] + calibration_coefficients[2] + 3;
                double y = x_kinect * calibration_coefficients[3] + y_kinect * calibration_coefficients[4] + calibration_coefficients[5] + 10;


                bool is_button = false;
                foreach (Image image in buttons)
                {

                    double top = Canvas.GetTop(image);
                    double left = Canvas.GetLeft(image);
                    if (y >= top && x >= left && y <= top + image.Height && x <= left + image.Width)
                    {
                        is_button = true;

                        resetFishSize();
                        increaseFishSize(image);

                        if (image.Name == "fish_nimo")
                        {
                            drawController.changeFishImage("nimo");
                        }
                        else if (image.Name == "fish_a")
                        {
                            drawController.changeFishImage("fish_a");
                        }
                        else if (image.Name == "fish_b")
                        {
                            drawController.changeFishImage("fish_b");
                        }
                        else if (image.Name == "fish_c")
                        {
                            drawController.changeFishImage("fish_c");
                        }
                    }
                }



                Point point = new Point(x, y);
                depthList.Add(DepthThreshold - touchDepths[i]);
                pointList.Add(point);

                if (!is_button)
                {
                    kinectPointList.Add(new Point(x_kinect, y_kinect));
                }
            }
            lastFrameDrawnPoints = kinectPointList;
            drawController.DrawFishes(pointList, depthList);
        }


        

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
