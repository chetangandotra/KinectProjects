using Microsoft.Kinect;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        bool _displayBody = true;//false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                    printSpeedSoFar();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void printSpeedSoFar()
        {
            int i = 0;
            float totalDistance = 0.0f;
            float lastX = 0.0f;
            float lastY = 0.0f;
            float lastZ = 0.0f;
            int distinctSecs = 0;
            DateTime dateTime2 = new DateTime();

            string path = "C:\\Users\\Chetan\\Documents\\Kinect\\KinectStreams\\test.csv";
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                i = 0;
                while (!parser.EndOfData)
                {
                    //Process row
                    if (i == 0) // header
                    {
                        i += 1;
                        continue;
                    }
                    string[] fields = parser.ReadFields();
                    if (fields == null || fields.Length == 0 || fields[0].StartsWith("X"))
                        continue;

                    string time = fields[fields.Length - 1];
                    DateTime dateTime1 = DateTime.ParseExact(time, "HH:mm:ss tt",
                                        System.Globalization.CultureInfo.CurrentCulture);
                    
                    float currX = float.Parse(fields[0]);
                    float currY = float.Parse(fields[1]);
                    float currZ = float.Parse(fields[2]);

                    if (i != 1)
                    {
                        var diffInSeconds = (dateTime1 - dateTime2).TotalSeconds;
                        if (diffInSeconds != 0)
                        {
                            distinctSecs++;
                        }
                        float x = currX-lastX;
                        float y = currY-lastY;
                        float z = currZ-lastZ;
                        totalDistance += (float)(Math.Sqrt((double)(x * x) + (y * y) + (z * z)));
                    }

                    lastX = currX;// float.Parse(fields[0]);
                    lastY = currY;// float.Parse(fields[1]);
                    lastZ = currZ;// float.Parse(fields[2]);
                    dateTime2 = dateTime1;

                    i++;
                }
            }

            float totalTime = distinctSecs;
            if (totalTime != 0 && totalTime % 3 == 0)
            {
                float speedTillNow = totalDistance / totalTime;
                Console.WriteLine(speedTillNow + "m/sec");
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        #endregion
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
