using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Tracking
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Main();
        }

        public void Main()
        {
            //new SampleReadFromFile(this);
            new SampleFromNet();
        }
    }

    class SampleReadFromFile
    {
        TrackingBase tracking = new TrackingNaive();
        int framePerSecond = 2;

        public SampleReadFromFile(MainWindow mainWindow)
        {
            TrackingBase.mainWindow = mainWindow;
            Thread thread = new Thread(Main);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Main()
        {
            Frame.rel = new List<KeyValuePair<int, int>>();
            Frame.rel.Add(new KeyValuePair<int, int>(0, 1));
            Frame.rel.Add(new KeyValuePair<int, int>(1, 2));
            Frame.rel.Add(new KeyValuePair<int, int>(2, 3));
            Frame.rel.Add(new KeyValuePair<int, int>(3, 4));

            TrackingBase.GetFrameStd(new StreamReader(new FileStream("std.txt", FileMode.Open)));

            StreamReader srTrack = new StreamReader(new FileStream("track.txt", FileMode.Open));
            StreamWriter swRes = new StreamWriter(new FileStream("res.txt", FileMode.OpenOrCreate));
            while (true)
            {
                Frame frame = TrackingBase.GetNextFrame(srTrack);
                if (frame == null)
                {
                    break;
                }
                Frame frameTarget = tracking.Track(frame);
                swRes.WriteLine("framestart");
                for (int i = 0; i < frameTarget.pl.Count; ++i)
                {
                    swRes.WriteLine("p " + frameTarget.pl[i]);
                }
                swRes.WriteLine("frameend");
                swRes.WriteLine("");
                Thread.Sleep(1000 / framePerSecond);
            }
            srTrack.Close();
            swRes.Close();
        }
    }

    class SampleFromNet
    {
        TrackingBase Tracking = new TrackingNaive();
        
        public SampleFromNet()
        {
            TcpClient socket = new TcpClient();
            socket.Connect("192.168.1.195", 1510);
            Console.WriteLine("Xx");
            StreamReader sr = new StreamReader(socket.GetStream());
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null) break;
                Console.WriteLine(line);
            }
            socket.Close();
        }
    }
}