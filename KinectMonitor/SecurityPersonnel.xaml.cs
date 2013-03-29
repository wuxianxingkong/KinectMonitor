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
using System.Windows.Shapes;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using Microsoft.Samples.Kinect.KinectFusionExplorer;
using System.Threading;
namespace KinectMonitor
{
    /// <summary>
    /// SecurityPersonnel.xaml 的交互逻辑
    /// </summary>
    public partial class SecurityPersonnel : Window
    {
        KinectSensor kinect;
        public SecurityPersonnel()
        {
            InitializeComponent();
            Loaded += SecurityPersonnel_Loaded;
            Unloaded += SecurityPersonnel_Unloaded;
        }
        public SecurityPersonnel(KinectSensor sensor): this()
        {
            kinect = sensor;
        }

        void SecurityPersonnel_Unloaded(object sender, RoutedEventArgs e)
        {
           
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.DepthStream.Disable();
                kinect.SkeletonStream.Disable();
                kinect.AllFramesReady -= kinect_AllFramesReady;
              
                kinect.Stop();
            }
        }
        ColorImageFrame colorframe;
        byte[] colorpixelData;
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();
            colorframe = e.OpenColorImageFrame();
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null && colorframe != null)
                {
                   canvas.Children.Clear();
                    colorpixelData = new byte[colorframe.PixelDataLength];
                    colorframe.CopyPixelDataTo(colorpixelData);

                    ColorImageBitmap.WritePixels(ColorImageBitmapRect, colorpixelData, ColorImageStride, 0);
                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);

                    var skeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

                    if (skeleton != null)
                    {
                        // Calculate height.
                        double height = Math.Round(skeleton.Height(), 2);
                        double armExtendsWidth = Math.Round(skeleton.ArmExtendWith(), 2);
                        // Draw skeleton joints.
                        foreach (JointType joint in Enum.GetValues(typeof(JointType)))
                        {
                            DrawJoint(skeleton.Joints[joint].ScaleTo(640, 480));
                        }

                        // Display height.
                        tblHeight.Text = String.Format("身高: {0} m", height);
                       tblArmExtendWidth.Text = String.Format("臂展: {0} m", armExtendsWidth);
                    }
                    if (isClick)
                    {
                        Record(colorframe);
                    }
                    else
                        StopRecording();
                    colorframe.Dispose();
                }
            }

        }

        private void DrawJoint(Joint joint)
        {
            System.Windows.Shapes.Ellipse ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.LightCoral)
            };

            Canvas.SetLeft(ellipse, joint.Position.X);
            Canvas.SetTop(ellipse, joint.Position.Y);

            canvas.Children.Add(ellipse);
        }
        private WriteableBitmap ColorImageBitmap;
        private Int32Rect ColorImageBitmapRect;
        private int ColorImageStride;
        void SecurityPersonnel_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (kinect != null)
            {
                ColorImageStream colorStream = kinect.ColorStream;
               
                ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = ColorImageBitmap;
                kinect.ColorStream.Enable();
               //DepthImageStream depthStream = kinect.DepthStream;
               //kinect.DepthStream.Enable();

                kinect.SkeletonStream.Enable();

                kinect.AllFramesReady += kinect_AllFramesReady;

                kinect.Start();
            }
        }
        bool isClick = false;    
        private void VideoCapture_Click(object sender, RoutedEventArgs e)
        {
            isClick = true;
            VideoCaptureButton.IsEnabled = false;
            VideoCaptureStopButton.IsEnabled = true;
        }

        private void VideoCaptureStop_Click(object sender, RoutedEventArgs e)
        {
            isClick = false;
            VideoCaptureButton.IsEnabled = true;
            VideoCaptureStopButton.IsEnabled = false;
        }
        bool _isRecording = false;
        string _baseDirectory = "..\\video\\";
        string _fileName;
        List<Image<Rgb, Byte>> _videoArray = new List<Image<Rgb, Byte>>();

        private void Record(ColorImageFrame image)
        {
            if (!_isRecording)
            {
                _fileName = string.Format("{0}{1}{2}", _baseDirectory, DateTime.Now.ToString("MMddyyyyHmmss"), ".avi");
                _isRecording = true;
            }
            _videoArray.Add(image.ToOpenCVImage<Rgb, Byte>());
        }

        private void StopRecording()
        {
            if (!_isRecording)
                return;

            CvInvoke.CV_FOURCC('P', 'I', 'M', '1');   //= MPEG-1 codec
            CvInvoke.CV_FOURCC('M', 'J', 'P', 'G');  //= motion-jpeg codec (does not work well)
            CvInvoke.CV_FOURCC('M', 'P', '4', '2');//= MPEG-4.2 codec
            CvInvoke.CV_FOURCC('D', 'I', 'V', '3'); //= MPEG-4.3 codec
            CvInvoke.CV_FOURCC('D', 'I', 'V', 'X'); //= MPEG-4 codec
            CvInvoke.CV_FOURCC('U', '2', '6', '3'); //= H263 codec
            CvInvoke.CV_FOURCC('I', '2', '6', '3'); //= H263I codec
            CvInvoke.CV_FOURCC('F', 'L', 'V', '1'); //= FLV1 codec

            using (VideoWriter vw = new VideoWriter(_fileName, 0, 30, 640, 480, true))
            {
                for (int i = 0; i < _videoArray.Count(); i++)
                    vw.WriteFrame<Rgb, Byte>(_videoArray[i]);
            }
            _fileName = string.Empty;
            _videoArray.Clear();
            _isRecording = false;

        }

        private void VideoChecking_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer vp = new VideoPlayer();
            vp.Show();
        }
        private void KinectFusion_Click(object sender, RoutedEventArgs e)
        {

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "KinectFusionExplorer-WPF.exe";
            process.Start();
            this.Close();
        }
    }
}
