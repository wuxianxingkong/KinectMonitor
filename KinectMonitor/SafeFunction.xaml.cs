using System;
using System.IO;
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
using System.Speech.Synthesis;
using System.Threading;
//
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
namespace KinectMonitor
{
    /// <summary>
    /// SafeFunction.xaml 的交互逻辑
    /// </summary>
    public partial class SafeFunction : Window
    {
         KinectSensor kinect;
        public SafeFunction()
        {
            InitializeComponent();
            Loaded += SafeFunction_Loaded;
            Unloaded += SafeFunction_Unloaded;
        }

        void SafeFunction_Unloaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.DepthStream.Disable();
                kinect.SkeletonStream.Disable();
                kinect.AllFramesReady -= kinect_AllFramesReady;
                kinect.Stop();
            }
        }
        private WriteableBitmap ColorImageBitmap;
        private Int32Rect ColorImageBitmapRect;
        private int ColorImageStride;

        void SafeFunction_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (kinect != null)
            {
                ColorImageStream colorStream = kinect.ColorStream;
                kinect.ColorStream.Enable();
                ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = ColorImageBitmap;

                DepthImageStream depthStream = kinect.DepthStream;
                kinect.DepthStream.Enable();
              
                kinect.SkeletonStream.Enable();

                kinect.AllFramesReady += kinect_AllFramesReady;

                kinect.Start();
            }

        }
        DepthImageFrame depthframe;
        //short[] depthpixelData;
        ColorImageFrame colorframe;
        byte[] colorpixelData;
        short[] depthpixelData;
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();
            depthframe = e.OpenDepthImageFrame();
            colorframe = e.OpenColorImageFrame();

            if (depthframe != null && colorframe != null)
            {
                depthpixelData = new short[depthframe.PixelDataLength];
                depthframe.CopyPixelDataTo(depthpixelData);
                colorpixelData = new byte[colorframe.PixelDataLength];
                colorframe.CopyPixelDataTo(colorpixelData);

                if (startOrStop)
                {
                    PlayerFilter();
                    if (isClick)
                    {
                        Record(colorframe);
                    }
                    else
                    {
                        StopRecording();
                        if (isAuto)
                            Alarm(colorframe);//发出警告音或者拍摄视频
                    }
                    
                }

                ColorImageBitmap.WritePixels(ColorImageBitmapRect, colorpixelData, ColorImageStride, 0);
                depthframe.Dispose();
                colorframe.Dispose();
            }  
        }
        ColorImagePoint[] colorpoints;
        void PlayerFilter()
        {
            colorpoints = new ColorImagePoint[depthpixelData.Length];
            DepthImagePixel [] depthImagePixel=new DepthImagePixel[depthframe.PixelDataLength];
            depthframe.CopyDepthImagePixelDataTo(depthImagePixel);
            kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthframe.Format, depthImagePixel, colorframe.Format, colorpoints);
            for (int i = 0; i < depthpixelData.Length; i++)
            {
                PlayerColor(i);
            }
        }
        const int MAX_RANGE = 8000;
        const int ALARM_RANGE = 1000;
        int nearst = ALARM_RANGE;
        private void PlayerColor(int i)
        {
            //throw new NotImplementedException();

            int playerIndex = depthpixelData[i] & DepthImageFrame.PlayerIndexBitmask;
            ColorImagePoint p = colorpoints[i];
            int colorindex = (p.X + p.Y * colorframe.Width) * colorframe.BytesPerPixel;
            if (playerIndex > 0)
            {
                colorpixelData[colorindex + 1] = 0x00;
                if (nearst > ALARM_RANGE)
                    colorpixelData[colorindex + 2] = 0x00;
                else
                    colorpixelData[colorindex] = 0x00;

                colorpixelData[colorindex + 3] = 128;

                int depth = depthpixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                Nearst(depth);
            }
            else
                colorpixelData[colorindex + 3] = 0xFF;

        }

        void Nearst(int depth)
        {
            if (depth < nearst)
                nearst = depth;
        }
        void Alarm(ColorImageFrame colorframe)
        {
            if (nearst < ALARM_RANGE)
            {
                Title = "有人接近";
                Speak();
                Record(colorframe);

                
            }
            else
            {
                Title = "监视中";
                StopRecording();
            }
            nearst = MAX_RANGE;
        }
        SpeechSynthesizer synthesizer = new SpeechSynthesizer() { Rate = 0, Volume = 100 };
        private void Speak()
        {
            //throw new NotImplementedException();
            if (synthesizer.State != SynthesizerState.Speaking)
                synthesizer.SpeakAsync("Warning");

        }

        public SafeFunction(KinectSensor sensor) : this()
        {
            kinect = sensor;
        }

        private void SafeFunctionLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void SafeFunctionUnloaded(object sender, RoutedEventArgs e)
        {

        }
        bool startOrStop = false;
        private void StartOrStop_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;
            if (!startOrStop)
            {
                startOrStop = true;
                bt.Content = "关闭功能";
            }
            else
            {
                startOrStop = false;
                bt.Content = "开启功能";
            }
        }
        bool isAuto = false;
        bool isClick = false;    
        //private void  Balance()
        //{
        //    if (isAuto) isClick = false;
        //    else isClick = true;
        //}
        private void VideoCapture_Click(object sender, RoutedEventArgs e)
        {

            if (startOrStop)
            {
                isClick = true;
                AutoCapture.IsEnabled = false;
                VideoCaptureButton.IsEnabled = false;
                VideoCaptureStopButton.IsEnabled = true;
            }
            else
                MessageBox.Show("您还没有开启录像，不能录制");
        }
        private void VideoCaptureStop_Click(object sender, RoutedEventArgs e)
        {
                isClick = false;
                AutoCapture.IsEnabled = true;
                VideoCaptureButton.IsEnabled = true;
                VideoCaptureStopButton.IsEnabled = false;

        }
        //private bool bol=true;
        //private void ChangeAutoInTime() 
        //{
        //    while (bol)
        //    {
        //        Thread.Sleep(5000);
        //        if (isAuto)
        //            isAuto = false;
        //        else
        //            isAuto = true;
        //        //thread.Abort();
        //    }
        //}
        
        private void AutoCapture_Click(object sender, RoutedEventArgs e)
        {
            if (startOrStop)
            {
                if (!isClick)
                {
                    if (!isAuto)
                    {
                        isAuto = true;
                        
                        VideoCaptureButton.IsEnabled = false;
                        VideoCaptureStopButton.IsEnabled = false;
                        MessageBox.Show("自动录像开始！");
                        AutoCapture.ToolTip = "关闭自动录像";
                        //thread = new Thread(new ThreadStart(ChangeAutoInTime));
                        //thread.Start();
                        //while (!thread.IsAlive) { }
                        //Thread.Sleep(10);
                        //thread.Join();
                    }
                    else
                    {
                        isAuto = false;
                        //bol = false;
                        VideoCaptureButton.IsEnabled = true;
                        VideoCaptureStopButton.IsEnabled = false;
                        AutoCapture.ToolTip = "开启自动录像";
                        MessageBox.Show("自动录像结束！");
                    }
                }
                //else
                //    MessageBox.Show("请先关闭正在录制的视频再开启自动录制");
            }
            else
                MessageBox.Show("您还没有开启录像，不能录制");
        }
        //多线程执行视频抓取
        //private void VideoCaptureThread(ColorImageFrame image)
        //{
        //    Thread thread1 = new Thread(new ParameterizedThreadStart(DispatchVideoRecording));
        //    thread1.Start(image);
        //}

        //private void DispatchVideoRecording(object obj)
        //{
        //    //throw new NotImplementedException();
        //    ColorImageFrame image = (ColorImageFrame)obj;
        //    Record(image);
        //}
        
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

        

        
        
    }
}
