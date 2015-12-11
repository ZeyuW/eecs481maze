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

		public bool backgroundAlreadySet = true;

        int prevBackground = 0;

        public bool isSeaweedShow = false;

        double last_x = Double.MaxValue;
        double last_y = Double.MaxValue;

        private SoundController soundController;
        string fishName = "nimo";
        bool need_change_image = false;

        public Canvas drawingCanvas;
        public Image backgroundImage;
        public Image canvasImage;


        List<Point> actual_draw_points = new List<Point>();
        List<int> actual_draw_depths = new List<int>();
        List<string> actual_draw_fishname = new List<string>();
        Image[] buttons;
        Image[] Seaweed_list;
		public List<Background> backgrounds;
		public Background background;

       

        public DrawController(Canvas canvas, Image image, Image canvasImage, Image[] buttons, SoundController in_soundController)
        {
            drawingCanvas = canvas;
            backgroundImage = image;
            this.canvasImage = canvasImage;
            this.buttons = buttons;
            soundController = in_soundController;

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
        }


        public void changeFishImage(string in_fishName)
        {
            fishName = in_fishName;
            need_change_image = true;
        }

       
        public bool isInSeaweedArea(double x, double y)
        {
            if (!isSeaweedShow)
                return false;

            foreach (Image i in Seaweed_list)
            {
                double top = Canvas.GetTop(i);
                double left = Canvas.GetLeft(i);
                if (y >= top + 30 && x >= left && y <= top + i.Height && x <= left + i.Width)
                {
                    return true;
                }
            }

            return false;
        }






        public void DrawFishes(List<Point> pointList, List<int> depthList)
        {
            //ClearScreen();
            bool need_clean_screen = true;
            last_x = Double.MaxValue;
            last_y = Double.MaxValue;
            List<Point> temp_draw_points = new List<Point>();
            List<int> temp_draw_depths = new List<int>();
            List<string> temp_draw_fishname = new List<string>();

            for (int touchNum = 0; touchNum < depthList.Count; touchNum++)
            {
                double x = pointList[touchNum].X;
                double y = pointList[touchNum].Y;

                if (Math.Abs(x - last_x) < 100 && Math.Abs(y - last_y) < 100)
                    continue;

                int depth = depthList[touchNum];

                if (isInSeaweedArea(x, y))
                {
                    //soundController.TriggerColorEffect(0);
                    bool draw_last_point = false;
                    for (int i = 0; i < actual_draw_points.Count; i++)
                    {
                        if (Math.Abs(x - actual_draw_points[i].X) < 30 && Math.Abs(y - actual_draw_points[i].Y) < 30)
                        {
                            x = actual_draw_points[i].X;
                            y = actual_draw_points[i].Y;
                            depth = actual_draw_depths[i];
                            draw_last_point = true;
                        }
                    }
                    if (!draw_last_point)
                        continue;
                }

                // The loop will decide fish direction
                string tmp_fishName = fishName;
                
                double minDist = Double.MaxValue;
                int point_index = 0;
                for (int k = 0; k < actual_draw_points.Count; k++)
                {
                    // if within the range: the same fish

                    if ((x - actual_draw_points[k].X) * (x - actual_draw_points[k].X) + (y - actual_draw_points[k].Y) * (y - actual_draw_points[k].Y) < minDist)
                    {
                        minDist = (x - actual_draw_points[k].X) * (x - actual_draw_points[k].X) + (y - actual_draw_points[k].Y) * (y - actual_draw_points[k].Y);
                        point_index = k;

                    }

                }
                if (actual_draw_points.Count != 0 && !need_change_image)
                {
                    tmp_fishName = actual_draw_fishname[point_index];
                    if (x - actual_draw_points[point_index].X > 10 && tmp_fishName.Length <= 6)
                    {
                        tmp_fishName = tmp_fishName + "_flipped";
                    }
                    else if (x - actual_draw_points[point_index].X < -10 && tmp_fishName.Length > 6)
                    {
                        tmp_fishName = fishName;
                    }
                }

                if (need_clean_screen)
                {
                    ClearScreen();
                    need_clean_screen = false;
                }

                if (need_change_image)
                {
                    need_change_image = false;
                }

                temp_draw_points.Add(new Point(x, y));
                temp_draw_depths.Add(depth);
                temp_draw_fishname.Add(tmp_fishName);

                last_x = x;
                last_y = y;
                
            
                Image fish = new Image();
                string fishPath = @"..\..\Resources\" + tmp_fishName + ".png";
                Uri fishUri = new Uri(fishPath, UriKind.Relative);
                BitmapImage bi = new BitmapImage(fishUri);
                fish.Source = bi;
                fish.Name = "fish";

                fish.Width = 50 + 60 * (depth / 60.0);
                fish.Height = 50 + 60 * (depth / 60.0);

                Canvas.SetTop(fish, y - fish.Height / 2);
                Canvas.SetLeft(fish, x - fish.Width / 2);
                Canvas.SetZIndex(fish, 10);

                drawingCanvas.Children.Add(fish);
            }
            actual_draw_points.Clear();
            actual_draw_points = temp_draw_points;
            actual_draw_depths.Clear();
            actual_draw_depths = temp_draw_depths;
            actual_draw_fishname.Clear();
            actual_draw_fishname = temp_draw_fishname;
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


        
       
        
        
		public void findAndInitializeBackgrounds()
		{
            string dropBox = @"..\..\Resources\bg";

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
				uri = new Uri(inUriString, UriKind.Relative);
		}
	}
}
