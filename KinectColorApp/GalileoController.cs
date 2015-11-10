using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace KinectColorApp
{
    class GalileoController
    {
        DrawController drawController;
        SoundController soundController;
        SerialPort port;
        Thread thread;
        string portName;
        int baudRate;
        bool _continue;
        char prevColor = '0';

        DateTime lastTime;

        public GalileoController(DrawController dc, SoundController sc, string pN, int bR)
        {
            lastTime = DateTime.Now;
            drawController = dc;
            soundController = sc;
            portName = pN;
            baudRate = bR;
            port = new SerialPort(portName, baudRate);
            port.Open();
            _continue = true;
            thread = new Thread(new ThreadStart(listenToSerial));
            thread.Start();
            
        }

        public void closePort()
        {
            _continue = false;
            thread.Join();
            port.Close();
        }

        public void listenToSerial()
        {
            Console.WriteLine("start thread");
            while (_continue)
            {
                try
                {
                    string message = port.ReadLine();
                    Console.WriteLine("MESSAGE IS: " + message[0]);
                    // Change color:
                    if (message[0] == '0' || message[0] == '1' || message[0] == '2' || message[0] == '3')
                    {
                        char currColor = message[0];
                        //Console.WriteLine("msg: " + currColor);
                        if (currColor != prevColor)
                        {
                            prevColor = currColor;
                            int colorNum = currColor - '0';
                            drawController.ColorChangeFlag(colorNum);
                            soundController.TriggerColorEffect(colorNum);
                        }
                        //Console.WriteLine(message);
                    } 
                    // Change background:
                    else if (message[0] == '5')
                    {
                        TimeSpan timeDiff = DateTime.Now - lastTime;

                        if (timeDiff.TotalMilliseconds > 1000)
                        {
                            drawController.CycleBackgrounds();
                            soundController.TriggerBackgroundEffect();
                            lastTime = DateTime.Now;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
