using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows;

namespace KinectColorApp
{

    class DrawController
    {
        private Colors color = Colors.Red;

		public bool backgroundAlreadySet = true;

        public int shouldChangeColor = -1;
        int prevBackground = 0;

        public bool isSeaweedShow = false;

        string fishPath = @"..\..\Resources\nimo.png";

        public Canvas drawingCanvas;
        public Image backgroundImage;
        public Rectangle colorRect;
        public Image canvasImage;

        double last_x = 9999;
        double last_y = 9999;


        Image[] buttons;
        Image[] Seaweed_list;
		public List<Background> backgrounds;
		public Background background;

        public DrawController(Canvas canvas, Image image, Rectangle rect, Image canvasImage, Image[] buttons)
        {
            drawingCanvas = canvas;
            backgroundImage = image;
            colorRect = rect;
            this.canvasImage = canvasImage;
            this.buttons = buttons;

			//Get Backgrounds in Dropbox
			backgrounds = new List<Background>();
			findAndInitializeBackgrounds();
			background = backgrounds[0];

        }

        public void CycleBackgrounds()
        {
			int currBackground = prevBackground + 1;
			if (currBackground >= backgrounds.Count)
			{
				currBackground = 0;
			}

			prevBackground = currBackground;
			background = backgrounds[currBackground];
			backgroundAlreadySet = false;
        }

        public void ColorChangeFlag(int new_color)
        {
            shouldChangeColor = new_color;
        }

		public void ChangeBackground()
        {
            ChangeBackground(background);
		}
        public void ChangeBackground(Background new_background)
        {
			Console.WriteLine("Changing background to " + new_background.uri);
            

			backgroundAlreadySet = true;
			backgroundImage.Source = new BitmapImage(new_background.uri);
            backgroundImage.Height = 900;
            backgroundImage.Width = 1367;
			// And, in any case, clear screen:
			//ClearScreen();

        }


        public void changeFishImage(string fishName)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            fishPath = @"..\..\Resources\" + fishName + ".png"; 
        }


        public void ChangeColor(Colors new_color)
        {
            color = new_color;
            
            // Reset shouldChangeColor:
            shouldChangeColor = -1;

            // Change colorRects color:
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0.5, 0);
            gradientBrush.EndPoint = new Point(0.5, 1);

            if (new_color == Colors.Red)
            {
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 200, 0), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0, 0), 1.0));
            }
            else if (new_color == Colors.Green)
            {
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 255, 200), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 255, 0), 1.0));
            }
            else if (new_color == Colors.Blue)
            {
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 200, 0, 255), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 0, 255), 1.0));
            }
            else if (new_color == Colors.White)
            {
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 250, 250, 250), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 100, 100, 100), 1.0));
            }
            
            // Change color indicator:
            //colorRect.Fill = gradientBrush;
        }

        public bool isInSeaweedArea(double x, double y)
        {
            if (!isSeaweedShow)
                return false;

            foreach (Image i in Seaweed_list)
            {
                double top = Canvas.GetTop(i);
                double left = Canvas.GetLeft(i);
                if (y >=top + 60 && x >= left && y <= top + i.Height- 10 && x <= left + i.Width)
                    return true;
            }

            return false;
        }



        public void DrawFishes(List<Point> pointList, List<int> depthList)
        {
            ClearScreen();
            

            for (int touchNum = 0; touchNum < depthList.Count; touchNum++)
            {
                double x = pointList[touchNum].X;
                double y = pointList[touchNum].Y;

                if (Math.Abs(x - last_x) < 150 && Math.Abs(y - last_y) < 150)
                    continue;

                if (isInSeaweedArea(x, y))
                    continue;

                last_x = x;
                last_y = y;
                int depth = depthList[touchNum];

                Image fish = new Image();
                
                Uri fishUri = new Uri(fishPath, UriKind.Relative);
                BitmapImage bi = new BitmapImage(fishUri);
                fish.Source = bi;
                fish.Name = "fish";

                fish.Width = 50 + 60 * (depth / 60.0);
                fish.Height = 50 + 60 * (depth / 60.0);

                Canvas.SetTop(fish, y - fish.Height / 2);
                Canvas.SetLeft(fish, x - fish.Width / 2);
                Canvas.SetZIndex(fish, 2);

                drawingCanvas.Children.Add(fish);
            }
        }


        public void showSeaweed(Image[] in_seaweed_list)
        {
            isSeaweedShow = true;
            Seaweed_list = in_seaweed_list;
            foreach(Image i in in_seaweed_list)
            {
                i.Visibility = Visibility.Visible;
            }
        }

        public void hideSeaweed(Image[] seaweed_list)
        {
            isSeaweedShow = false;
            Seaweed_list = new Image[]{};
            foreach (Image i in seaweed_list)
            {
                i.Visibility = Visibility.Hidden;
            }
        }

        /*
        public void DrawEllipseAtPoint(double x, double y, int depth)
        {
            // i add
            ClearScreen();


            // Create an ellipse with a gradient brush
            Ellipse myEllipse = new Ellipse();
            RadialGradientBrush brush = new RadialGradientBrush();


            fish = new Image();
            string fishPath = @"C:\Users\Shuoyang\Desktop\481\KinectColorApp\KinectColorApp\Resources\nimo.png";
            Uri fishUri = new Uri(fishPath);
            BitmapImage bi = new BitmapImage(fishUri);
            //bi.BeginInit();
            //bi.UriSource = fishUri;
            fish.Source = bi;

            fish.Name = "fish";



            int colorValue = (int)(255 * (depth / ColorChangeSpeed));
            if (colorValue < 0) colorValue = 0;
            if (colorValue > 255) colorValue = 255;

            // Set the color based on depth data
            if (color == Colors.Red)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, 255, (byte)colorValue, 0), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, 255, (byte)colorValue, 0), 0.4));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, (byte)colorValue, 0), 1.0));
            }
            else if (color == Colors.Green)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, 0, 255, (byte)colorValue), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, 0, 255, (byte)colorValue), 0.4));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 255, (byte)colorValue), 1.0));
            }
            else if (color == Colors.Blue)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, (byte)colorValue, 0, 255), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(200, (byte)colorValue, 0, 255), 0.4));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, (byte)colorValue, 0, 255), 1.0));
            }
            else if (color == Colors.White) 
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 1.0));
            }

            myEllipse.Fill = brush;
            myEllipse.StrokeThickness = 0;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 20 + 15 * (depth / 60.0);
            myEllipse.Height = 20 + 15 * (depth / 60.0);

            fish.Width = 50 + 60 * (depth / 60.0);
            fish.Height = 50 + 60 * (depth / 60.0);

            //Canvas.SetTop(myEllipse, y - myEllipse.Height/2);
            //Canvas.SetLeft(myEllipse, x - myEllipse.Width/2);
            //Canvas.SetZIndex(myEllipse, 0);

            Canvas.SetTop(fish, y - fish.Height / 2);
            Canvas.SetLeft(fish, x - fish.Width / 2);
            Canvas.SetZIndex(fish, 2);

            // Add the Ellipse to the drawingCanvas


            drawingCanvas.Children.Add(fish);
        }
        */

        public void ClearScreen()
        {
            // Remove ellipses only
            var shapes = drawingCanvas.Children.OfType<Image>().ToList();
            foreach (var shape in shapes)
            {
                if (shape.Name == "fish")
                {
                    drawingCanvas.Children.Remove(shape);
                }
            }

            canvasImage.Source = null;
        }


        
        public void SaveCanvas()
        {
            Size size = new Size(System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);
            drawingCanvas.Measure(size);
            backgroundImage.Visibility = Visibility.Hidden;

            foreach (Image ellipse in buttons)
            {
                ellipse.Visibility = Visibility.Hidden;
            }

            var rtb = new RenderTargetBitmap(
                (int)System.Windows.SystemParameters.PrimaryScreenWidth, //width 
                (int)System.Windows.SystemParameters.PrimaryScreenHeight, //height 
                96, //dpi x 
                96, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(drawingCanvas);
            backgroundImage.Visibility = Visibility.Visible;
            foreach (Image ellipse in buttons)
            {
                ellipse.Visibility = Visibility.Visible;
            }

            canvasImage.Source = rtb;

            // Remove ellipses only
            var shapes = drawingCanvas.Children.OfType<Ellipse>().ToList();
            foreach (var shape in shapes)
            {
                if (shape.Name != "red_selector" && shape.Name != "blue_selector" && shape.Name != "green_selector" && shape.Name != "eraser_selector" && shape.Name != "background_selector" && shape.Name != "refresh_selector")
                {
                    drawingCanvas.Children.Remove(shape);
                }
            }
        }
        

		public void findAndInitializeBackgrounds()
		{
            string dropBox = @"C:\Users\Shuoyang\Desktop\481Git\eecs481maze\KinectColorApp\Resources\bg";
            //string dropBox = @"..\..\Resources\bg\";
            //Console.WriteLine(Directory.GetCurrentDirectory());

            string[] fileEntries = Directory.GetFiles(dropBox);
			foreach(string file in fileEntries)
			{ 
				if(file.Substring(file.Length - 4, 4).Equals(".png"))
				{ 
					backgrounds.Add(new Background(file));
					Console.WriteLine(file + ": Accepted");
				}
				else
				{
					Console.WriteLine(file + ": Not Accepted");
				}
			}
		}
    }

	class Background
	{
		public Uri uri;

		public Background(string inUriString)
		{
				uri = new Uri(inUriString);
		}
	}
}
