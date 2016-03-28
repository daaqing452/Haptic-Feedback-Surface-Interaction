//#define CANVAS_SHOW_ALL_FRAME
#define TRACKING_INDEX_PRINT
#define TRACKING_DRAW

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections;

namespace Tracking
{
    class SampleReadFromFile
    {
        TrackingBase tracking = new TrackingNaive();
        int framePerSecond = 5;

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
                Thread.Sleep(1000 / framePerSecond);
            }
            srTrack.Close();
            swRes.Close();
        }
    }

    class TrackingBase : DispatcherObject
    {
        public const int MAX_MARKER_COUNT = 19;
        public static MainWindow mainWindow;
        public static Frame frameStd;
        static SolidColorBrush[] colorList = new SolidColorBrush[] { Brushes.Red, Brushes.DarkOrange, Brushes.Gold, Brushes.Green, Brushes.Aqua, Brushes.Blue, Brushes.Purple, Brushes.Pink, };

        public virtual Frame Track(Frame frame)
        {
            return frame;
        }

        public delegate void DrawDelegate();

        public void Draw(Frame frame, int canvasType)
        {
            Canvas canvas;
            if (canvasType == 0)
            {
                canvas = mainWindow.xRawCanvas;
            }
            else
            {
                canvas = mainWindow.xTargetCanvas;
            }
            Dispatcher.BeginInvoke(new DrawDelegate(() =>
            {
#if CANVAS_SHOW_ALL_FRAME
#else
                Type typeEllipse = (new Ellipse()).GetType();
                List<UIElement> removeList = new List<UIElement>();
                foreach (UIElement u in canvas.Children)
                {
                    if (u.GetType() == typeEllipse)
                    {
                        removeList.Add(u);
                    }
                }
                foreach (UIElement u in removeList)
                {
                    canvas.Children.Remove(u);
                }
#endif
                double[] b = new double[] { 450, 300, 450, 350 };
                for (int i = 0; i < frame.pl.Count; ++i)
                {
                    if (frame.pl[i] == null)
                    {
                        continue;
                    }
                    DrawPoint(canvas, frame.pl[i].x * b[0] + b[1], frame.pl[i].y * b[2] + b[3], colorList[i % colorList.Length]);
                }
            }
            ));
        }

        public void DrawPoint(Canvas canvas, double x, double y, SolidColorBrush color)
        {
            Ellipse e = new Ellipse();
            e.Height = e.Width = 5;
            e.Fill = color;
            e.Stroke = color;
            Canvas.SetTop(e, y);
            Canvas.SetLeft(e, x);
            canvas.Children.Add(e);
        }

        public static double Np(double x, double mu, double sigma)
        {
            return 1.0 / (Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-Math.Pow(x - mu, 2) / (2 * sigma * sigma));
        }

        public static Frame IndexToFrame(Frame frame, int[] index)
        {
#if TRACKING_INDEX_PRINT
            Console.Write("index: [");
#endif
            Frame frameTarget = new Frame(frame.rb, new List<Point3D>());
            for (int i = 0; i < frameStd.pl.Count; ++i)
            {
#if TRACKING_INDEX_PRINT
                Console.Write(index[i] + ", ");
#endif
                if (index[i] == -1)
                {
                    frameTarget.pl.Add(null);
                }
                else
                {
                    frameTarget.pl.Add(frame.pl[index[i]]);
                }
            }
#if TRACKING_INDEX_PRINT
            Console.WriteLine("]");
#endif 
            return frameTarget;
        }

        public static Frame GetFrameStd(StreamReader sr)
        {
            frameStd = GetNextFrame(sr);
            sr.Close();
            return frameStd;
        }

        public static Frame GetNextFrame(StreamReader sr)
        {
            int status = 0;
            Frame frame = new Frame();
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                string[] arr = line.Split(' ');
                switch (arr.Length)
                {
                    case 1:
                        if (status >= 1)
                        {
                            status = 3;
                        }
                        break;
                    case 2:
                        status = 1;
                        break;
                    case 4:
                        double x = double.Parse(arr[1]);
                        double y = double.Parse(arr[2]);
                        double z = double.Parse(arr[3]);
                        if (arr[0] == "rb")
                        {
                            frame.rb = new Point3D(x, y, z);
                            frame.pl.Add(frame.rb);
                            status = 2;
                        }
                        else
                        {
                            frame.pl.Add(new Point3D(x, y, z));
                            status = 2;
                        }
                        break;
                    default:
                        Console.WriteLine("read error " + arr.Length);
                        break;
                }
                if (status == 3)
                {
                    break;
                }
            }
            if (status == 0)
            {
                return null;
            }
            else
            {
                return frame;
            }
        }
    }

    class TrackingSingleFrame : TrackingBase
    {
        int[] dfsIndex = new int[MAX_MARKER_COUNT];
        bool[] dfsVisit = new bool[MAX_MARKER_COUNT];
        List<int>[] dfsRelList = new List<int>[MAX_MARKER_COUNT];
        int[] dfsIndexBest = new int[MAX_MARKER_COUNT];
        double dfsProbBest;

        public override Frame Track(Frame frame)
        {
            int[] index = TrackSingleFrame(frame);
            Frame frameTarget = IndexToFrame(frame, index);
#if TRACKING_DRAW
            Draw(frame, 0);
            Draw(frameTarget, 1);
#endif
            return frameTarget;
        }

        public int[] TrackSingleFrame(Frame frame)
        {
            for (int i = 1; i < frameStd.pl.Count; ++i)
            {
                dfsIndex[i] = -1;
                dfsVisit[i] = false;
                dfsRelList[i] = new List<int>();
            }
            dfsIndex[0] = 0;
            dfsVisit[0] = true;
            dfsProbBest = -1;
            foreach (KeyValuePair<int, int> kvp in Frame.rel)
            {
                dfsRelList[kvp.Value].Add(kvp.Key);
            }
            Dfs(frame, 1, 1, 1.0);
            return dfsIndexBest;
        }

        public void Dfs(Frame frame, int x, int n, double prob)
        {
            if (prob < dfsProbBest)
            {
                return;
            }
            if (x >= frameStd.pl.Count)
            {
                if (n >= Math.Min(frame.pl.Count, frameStd.pl.Count))
                {
                    dfsProbBest = prob;
                    dfsIndex.CopyTo(dfsIndexBest, 0);
                }
                return;
            }
            for (int i = 0; i < frame.pl.Count(); ++i)
            {
                if (dfsVisit[i])
                {
                    continue;
                }
                dfsVisit[i] = true;
                dfsIndex[x] = i;
                double p = 1.0;
                foreach (int y in dfsRelList[x])
                {
                    if (dfsIndex[y] != -1)
                    {
                        double lenStd = (frameStd.pl[y] - frameStd.pl[x]).Length();
                        double lenNow = (frame.pl[dfsIndex[y]] - frame.pl[dfsIndex[x]]).Length();
                        p *= Np(lenNow, lenStd, 1);
                    }
                }
                Dfs(frame, x + 1, n + 1, prob * p);
                dfsIndex[x] = -1;
                dfsVisit[i] = false;
            }
            Dfs(frame, x + 1, n, prob);
        }
    }

    class TrackingNaive : TrackingBase
    {
        Frame frameLast;
        Frame frameTargetLast;
        int[] indexLast = new int[MAX_MARKER_COUNT];

        public override Frame Track(Frame frame)
        {
            Frame frameTarget;
            if (frameLast == null || Math.Abs(frame.pl.Count - frameLast.pl.Count) > 0)
            {
                TrackingSingleFrame tracking2 = new TrackingSingleFrame();
                int[] index = tracking2.TrackSingleFrame(frame);
                frameTarget = IndexToFrame(frame, index);
                indexLast = index;
            }
            else
            {
                frameTarget = IndexToFrame(frame, indexLast);
            }

            for (int i = 0; i < frameTarget.pl.Count; ++i)
            {
                if (frameTarget.pl[i] == null)
                {
                    if (frameTargetLast != null)
                    {
                        frameTarget.pl[i] = frameTargetLast.pl[i];
                    }
                    else
                    {
                        frameTarget.pl[i] = new Point3D(0, 0, 0);
                    }
                }
            }
#if TRACKING_DRAW
            Draw(frame, 0);
            Draw(frameTarget, 1);
#endif
            frameLast = frame;
            frameTargetLast = frameTarget;
            return frameTarget;
        }
    }

    class TrackingLastFix : TrackingBase
    {
        Frame frameLast;
        Frame frameTargetLast;
        int[] indexLast = new int[MAX_MARKER_COUNT];


    }

    class Frame
    {
        public static List<KeyValuePair<int, int>> rel;
        public Point3D rb;
        public List<Point3D> pl;

        public Frame()
        {
            rb = new Point3D(0, 0, 0);
            pl = new List<Point3D>();
        }

        public Frame(Point3D rb, List<Point3D> pl)
        {
            this.rb = rb;
            this.pl = pl;
        }
    }

    class Point3D
    {
        public double x;
        public double y;
        public double z;

        public Point3D()
        {

        }

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        static public Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public bool Equals(Point3D b)
        {
            return x == b.x && y == b.y && z == b.z;
        }

        override public string ToString()
        {
            return "(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")";
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
    }
}
