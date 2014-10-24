﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectColorApp
{
    class KinectController
    {
        private Image debugImage;
        private DrawController drawController;

        public KinectController(DrawController controller, Image image)
        {
            debugImage = image;
            drawController = controller;
        }

        public void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    Console.WriteLine("No depth frame");
                    return;
                }

                byte[] pixels = this.GenerateColoredBytes(depthFrame);

                int stride = depthFrame.Width * 4;
                debugImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            // Create the RGB pixel array
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            bool didDrawPoint = false;

            // Loop through data and set colors for each pixel
            for (int depthIndex = 0, colorIndex = 0;
                 depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                 depthIndex++, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                //Console.WriteLine(depth);

                if (depth == -1 || depth == 0) continue;

                byte intensity = CalculateIntensityFromDepth(depth);
                pixels[colorIndex + BlueIndex] = intensity;
                pixels[colorIndex + GreenIndex] = intensity;
                pixels[colorIndex + RedIndex] = intensity;

                if (depth <= 900)
                {
                    pixels[colorIndex + BlueIndex] = Convert.ToByte(255 * (1 - (depth / 900.0)));
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 255;

                    int x_kinect = (int)((depthIndex) % depthFrame.Width);
                    int y_kinect = (int)((depthIndex) / depthFrame.Width);

                    double x_ratio = x_kinect / (double)depthFrame.Width;
                    double y_ratio = y_kinect / (double)depthFrame.Height;

                    int x = (int)(x_ratio * drawController.drawingCanvas.Width);
                    int y = (int)(y_ratio * drawController.drawingCanvas.Height);

                    if (!didDrawPoint)
                    {
                        drawController.drawEllipseAtPoint(x, y);
                        didDrawPoint = true;
                    }
                }
            }

            return pixels;
        }

        private static byte CalculateIntensityFromDepth(int distance)
        {
            return (byte)(255 - (255 * Math.Max(distance - 800, 0) / 2000));
        }
    }
}
