using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Kinect;
using ZXing.Kinect;

namespace KinectColorApp
{
    public delegate void MethodCaller();

    class CalibrationController
    {
        private KinectSensor sensor;
        private KinectController kinectController;
        private Canvas canvas;
        private Image debugImage;

        int threshold = -1;

        Image[] codes;
        Point[] code_points = new Point[5];
        Point code_size;

        public delegate void calibrationDidCompleteHandler();
        public event calibrationDidCompleteHandler CalibrationDidComplete;

        public CalibrationController(KinectSensor sensor, KinectController kinectController, Canvas canvas, Image[] codes, Image debugImage)
        {
            this.canvas = canvas;
            this.codes = codes;
            this.debugImage = debugImage;
            this.kinectController = kinectController;
            this.sensor = sensor;
        }

        public void DisplayColorImageAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                //Console.WriteLine("here");
                if (colorFrame == null) return;

                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
                int stride = colorFrame.Width * 4;
                debugImage.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    short[] rawDepthData = new short[depthFrame.PixelDataLength];
                    depthFrame.CopyPixelDataTo(rawDepthData);
                    int depth = rawDepthData[200*depthFrame.Width + 300] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    threshold = depth;
                    
                }

            }
        }

        int next_code_num = 0;
        bool working = false; // Skip frames if we're still processing stuff.

        public void CalibrationAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (working) return;
            working = true;

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    working = false;
                    return;
                }

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame == null)
                    {
                        working = false;
                        return;
                    }

                    //byte[] pixels = new byte[colorFrame.PixelDataLength];
                    //colorFrame.CopyPixelDataTo(pixels);
                    //int stride = colorFrame.Width * 4;
                    //debugImage.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
                    //debugImage.Visibility = Visibility.Visible;

                    //int code_num = find_code(colorFrame, depthFrame);
                    int code_num = find_touch(colorFrame, depthFrame);
                    if (code_num >= 0)
                    {
                        // Make the next code visible.
                        if (code_num < 4)
                        {
                            codes[code_num].Visibility = Visibility.Hidden;
                            codes[code_num + 1].Visibility = Visibility.Visible;
                            next_code_num++;
                            Thread.Sleep(3000);
                        }
                        else
                        {
                            Thread.Sleep(3000);
                            // We are done. Calculate the coefficients.
                            sensor.AllFramesReady -= this.CalibrationAllFramesReady;
                            codes[4].Visibility = Visibility.Hidden;
                            kinectController.calibration_coefficients = get_calibration_coeffs();
                            
                            Point center_top_left = code_points[0];
                            Point center_bot_right = code_points[4];
                            kinectController.Calibrate((int)(center_top_left.X + 1.25*code_size.X), (int)(center_top_left.Y + 0.7*code_size.Y), (int)(center_bot_right.X - 1.25*code_size.X), (int)(center_bot_right.Y - 0.8*code_size.Y));
                            sensor.AllFramesReady += kinectController.SensorAllFramesReady;
                            CalibrationDidComplete();
                        }
                    }
                }
            }

            working = false;
        }

        

        int find_touch(ColorImageFrame colorFrame, DepthImageFrame depthFrame)
        {
            int minDepthIndex = 0;
            int maxDepthIndex = 479 * depthFrame.Width;
            //int maxDepthIndex = 350 * depthFrame.Width;
            //int maxDepthIndex = 250 * 500;

            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            for (int depthIndex = minDepthIndex; depthIndex < maxDepthIndex; depthIndex++)
            {
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // Ignore invalid depth values
                if (depth == -1 || depth == 0) continue;
                /*
                for (int depthIndex1 = minDepthIndex; depthIndex1 < maxDepthIndex; depthIndex1++)
                {
                    int depth1 = depth;
                    if (threshold - depth1 > 150)
                    {
                        Console.WriteLine("Column = " + depthIndex1 % depthFrame.Width + ", Row = " + depthIndex1 / depthFrame.Width);

                    }
                }
                */
                    if ((threshold - depth) > 150 && (threshold - depth) < 350)
                {
                    //Console.WriteLine(threshold - depth);
                    // Get the point in the depth frame at the center of the barcode
                    double x_kinect = (depthIndex % depthFrame.Width);
                    double y_kinect = (depthIndex / depthFrame.Width);
                    //double x_kinect = (depthIndex % 500);
                    //double y_kinect = (depthIndex / 500);
                    Point p = new Point(x_kinect, y_kinect);
                    code_points[next_code_num] = p;

                    Console.WriteLine("Found code " + next_code_num + " at (" + x_kinect + ", " + y_kinect + ").");
                    return next_code_num;
                }
            }

            return -1;
        }

        int find_code(ColorImageFrame colorFrame, DepthImageFrame depthFrame)
        {
            ZXing.Kinect.BarcodeReader reader = new ZXing.Kinect.BarcodeReader();
            if (colorFrame != null)
            {
                //Decode the colorFrame
                var result = reader.Decode(colorFrame);
                if (result != null)
                {
                    string val = result.Text;
                    int code_num = Convert.ToInt32(val);
                    double center_x = result.ResultPoints[0].X + 0.5 * (result.ResultPoints[2].X - result.ResultPoints[0].X);
                    double center_y = result.ResultPoints[0].Y + 0.5 * (result.ResultPoints[2].Y - result.ResultPoints[0].Y);

                    code_size = new Point((result.ResultPoints[2].X - result.ResultPoints[0].X), (result.ResultPoints[2].Y - result.ResultPoints[0].Y));

                    // Must mirror the coordinate here -- the depth frame comes in mirrored.
                    center_x = 640 - center_x;

                    // Map the color frame onto the depth frame
                    DepthImagePixel[] depthPixel = new DepthImagePixel[depthFrame.PixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(depthPixel);
                    DepthImagePoint[] depthImagePoints = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                    sensor.CoordinateMapper.MapColorFrameToDepthFrame(sensor.ColorStream.Format, sensor.DepthStream.Format, depthPixel, depthImagePoints);

                    // Get the point in the depth frame at the center of the barcode
                    int center_point_color_index = (int)center_y * 640 + (int)center_x;
                    DepthImagePoint converted_depth_point = depthImagePoints[center_point_color_index];
                    Point p = new Point(converted_depth_point.X, converted_depth_point.Y);
                    code_points[code_num] = p;

                    Console.WriteLine("Found code " + code_num + " at (" + center_x + ", " + center_y + ") in color coordinates.");
                    Console.WriteLine("Translated to (" + p.X + ", " + p.Y + ") in depth coordinates.");
                    return code_num;
                }
            }

            return -1;
        }

        double[] get_calibration_coeffs()
        {
            double[] coeffs = new double[6];

            // Make the Z matrix
            Matrix Z = new Matrix(5, 3);
            for (int i = 0; i < 5; i++)
            {
                Z[i, 0] = code_points[i].X;
                Z[i, 1] = code_points[i].Y;
                Z[i, 2] = 1;
            }

            // Make the display_x and display_y matrices
            Matrix D_x = new Matrix(5, 1);
            Matrix D_y = new Matrix(5, 1);
            for (int i = 0; i < 5; i++)
            {
                // Get the position of the code
                Point center = codes[i].TransformToAncestor(canvas).Transform(new Point(codes[i].ActualWidth / 2, codes[i].ActualHeight / 2));
                //dc.DrawEllipseAtPoint(center.X, center.Y, 50);
                D_x[i, 0] = center.X;
                D_y[i, 0] = center.Y;
            }

            // Calculate ABC, DEF
            Matrix ABC = (Matrix.Transpose(Z) * Z).Invert() * Matrix.Transpose(Z) * D_x;
            Matrix DEF = (Matrix.Transpose(Z) * Z).Invert() * Matrix.Transpose(Z) * D_y;

            coeffs[0] = ABC[0, 0];
            coeffs[1] = ABC[1, 0];
            coeffs[2] = ABC[2, 0];
            coeffs[3] = DEF[0, 0];
            coeffs[4] = DEF[1, 0];
            coeffs[5] = DEF[2, 0];

            return coeffs;
        }
    }
}

