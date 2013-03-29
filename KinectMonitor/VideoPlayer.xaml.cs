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
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
namespace KinectMonitor
{
    /// <summary>
    /// VideoPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayer : Window
    {
        public VideoPlayer()
        {
            InitializeComponent();
            this.timer = new DispatcherTimer();
           
        }
        private DispatcherTimer timer;
        private OpenFileDialog videoDialog = new OpenFileDialog();
        private OpenFileDialog skinDialog = new OpenFileDialog();
        private bool isFull = false;
        private bool isMaxWindow = false;
        private string videoName = "";
        private string skinName = "";
        private bool isPlaying = false;
        private double windowWidth = 0;
        private double windowHeight = 0;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SCREENSAVE = 0xF140;
        // private  double baseValue=0;
        // private double total = 0;


        //如果全屏禁止屏保
        //protected override void WndProc(ref Message System)
        //视频被打开时调用
        private void myVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            
            double seconds = myVideo.NaturalDuration.TimeSpan.TotalSeconds;
            videoSlider.Maximum = seconds / 10;
            volumeSlider.Value = myVideo.Volume * 10.0;
            double baseSecond = seconds / videoSlider.Maximum;
            this.timer.Interval = new TimeSpan(0, 0, 1);
            this.timer.Tick += new EventHandler(timer_Tick);
            this.timer.Start();

        }

        void timer_Tick(object sender, EventArgs e)
        {
            
            this.videoSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(videoSlider_ValueChanged);      
            this.videoSlider.Value = this.myVideo.Position.TotalSeconds / 10.0;
            this.videoSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(videoSlider_ValueChanged);
            try
            {
                this.showTimeBlock.Text = this.myVideo.Position.Hours.ToString() + ":" + this.myVideo.Position.Minutes.ToString() + ":" + this.myVideo.Position.Seconds.ToString() + "/" + this.myVideo.NaturalDuration.TimeSpan.Hours.ToString() + ":" + this.myVideo.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + this.myVideo.NaturalDuration.TimeSpan.Seconds.ToString();
            }
            catch { }
        }

        ///----------------------获取视频的宽度-------------------------
        public double getVideoWidth(MediaElement video)
        {
            if (video != null)
                return video.Width;
            else
                return 0;
        }
        ///----------------------获取视频的高度-------------------------
        public double getVideoHeight(MediaElement video)
        {
            if (video != null)
                return video.Height;
            else
                return 0;
        }
        ///----------------------设置视频的高度-------------------------
        public bool setVideoHeight(MediaElement video, double outHeight)
        {
            if (video != null)
            {
                video.Height = outHeight;
                return true;
            }
            else
                return false;
        }
        ///----------------------设置视频的宽度-------------------------
        public bool setVideoWidth(MediaElement video, double outWidth)
        {
            if (video != null)
            {
                video.Width = outWidth;
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////
        /// </summary>

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("程序被加载了");
            if (Directory.Exists("..\\Configure") == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory("..\\Configure");
            }
            //if (Directory.Exists("..\\WallPaper") == false)//如果不存在就创建file文件夹
            //{
            //    Directory.CreateDirectory("..\\WallPaper");
            //}
            if (File.Exists("..\\Configure\\configure.txt"))
            //{
            //    FileStream fs = new FileStream("..\\Configure\\configure.txt", FileMode.Create);
            //    fs.Close();
            //}
            //else 
            {
                StreamReader objReader = new StreamReader("..\\Configure\\configure.txt");
                string sLine = "";
                sLine = objReader.ReadLine();             
                objReader.Close();
                if (sLine != null)
                {
                    if (File.Exists(sLine))
                        BackgroundImage.ImageSource = new BitmapImage(new Uri(sLine, UriKind.Absolute));
                    else
                    {
                        //SolidColorBrush backsolid = new SolidColorBrush();
                        //Color back = Color.FromArgb(255, 0, 0, 0);
                        //backsolid.Color = back;
                        ////BackgroundImage.ImageSource =(ImageSource)backsolid;
                        //BorderSkin.Background = backsolid;
                        //BackgroundImage.ImageSource = new BitmapImage(new Uri("..\\Image\\skin_0.jpg", UriKind.Absolute));
                    }
                }
                else
                {
                    //SolidColorBrush backsolid = new SolidColorBrush();
                    //Color back = Color.FromArgb(255, 0, 0, 11);
                    //backsolid.Color = back;
                    //BorderSkin.Background = backsolid;
                   // BackgroundImage.ImageSource = new BitmapImage(new Uri("..\\Image\\skin_0.jpg", UriKind.Absolute));
                }
                    

                //System.Windows.MessageBox.Show(sLine);
            }
            //FileStream fs = new FileStream("..\\Configure\\configure", FileMode.Create);
        }


        //移动窗口
        protected void DragWindow(Object sender, MouseButtonEventArgs e)
        {
            // MessageBox.Show("aa");
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        //左键暂停 播放
        protected void PauseByClick(Object sender, MouseButtonEventArgs e)
        {
            if (isPlaying)
            {
                try
                {
                    if (myVideo.Source != null)
                    {
                        myVideo.Pause();
                        isPlaying = false;
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    if (myVideo.Source != null)
                    {
                        myVideo.Play();
                        isPlaying = true;
                    }
                }
                catch { }
            }

        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {

        }
        //全屏播放
        private void FullScreen()
        {
            if (isFull == false)
            {
                MediaGrid.Margin = new Thickness(0, 0, 0, 0);
                this.WindowState = WindowState.Maximized;
                BaseGrid.Visibility = Visibility.Hidden;
                this.Visibility = Visibility.Hidden;

                this.Visibility = Visibility.Visible;
                isFull = true;
            }
        }
        //退出全屏
        private void ExitFullScreen()
        {
            if (isFull == true)
            {
                MediaGrid.Margin = new Thickness(43, 22, 90, 25);
                this.WindowState = WindowState.Normal;
                BaseGrid.Visibility = Visibility.Visible;
                isFull = false;
            }
        }
        //鼠标双击 全屏与否
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (isFull)
            {
                ExitFullScreen();
            }
            else if (myVideo.Source != null && !isFull)
                FullScreen();

        }
        //打开视频
        private void OpenAndPlay()
        {
            videoDialog.Filter = "All Files(*.*)|*.*";
            videoDialog.InitialDirectory = "..\\";
            //videoDialog.ShowDialog();
            if (videoDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                

                videoName = videoDialog.FileName;
                if ((videoName != null) && (videoName != ""))
                {
                    Uri file = new Uri(videoName);
                 
                    myVideo.Source = file;
                    myVideo.LoadedBehavior = MediaState.Manual;


                    myVideo.Play();

                    isPlaying = true;

                  
                }
            }
        }

        //点击停止按钮
        private void btstop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myVideo.Source != null)
                {
                    myVideo.Stop();
                    isPlaying = false;
                }
            }
            catch
            {

            }
        }
        //点击暂停按钮
        private void btpause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myVideo.Source != null)
                {
                    myVideo.Pause();
                    isPlaying = false;
                }
            }
            catch { }
        }
        //点击播放按钮
        private void btplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myVideo.Source != null)
                {
                    myVideo.Play();
                    isPlaying = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                OpenAndPlay();

            }
        }
        //点击打开
        private void btopen_Click(object sender, RoutedEventArgs e)
        {
            OpenAndPlay();
        }
        //点击缩放按钮
        private void btsize_Click(object sender, RoutedEventArgs e)
        {
            if (sizeMenu.Visibility == Visibility.Hidden)
                sizeMenu.Visibility = Visibility.Visible;

        }


        //点击皮肤按钮
        private void skin_Click(object sender, RoutedEventArgs e)
        {
            skinDialog.Filter = "jpg格式(*.jpg)|*.jpg|gif格式(*.gif)|*.gif|png格式(*.png)|*.png|bmp格式(*.bmp)|*.bmp";
            skinDialog.InitialDirectory = "..\\";
            if (skinDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                skinName = skinDialog.FileName;
                if ((skinName != null) && (skinName != ""))
                {
                    //Uri file = new Uri(videoName);

                    //myVideo.Source = file;
                    //myVideo.LoadedBehavior = MediaState.Manual;


                    //myVideo.Play();

                    //isPlaying = true;                  
                        FileStream fs = new FileStream("..\\Configure\\configure.txt", FileMode.Create);
                        byte[] data = new UTF8Encoding().GetBytes(skinName);
                        //开始写入
                        fs.Write(data, 0, data.Length);
                        //清空缓冲区、关闭流
                        fs.Flush();
                        fs.Close();
                        BackgroundImage.ImageSource = new BitmapImage(new Uri(skinName, UriKind.Absolute));

                }
            }
        }
        //菜单失去焦点
        private void size_LoseFocus(object sender, RoutedEventArgs e)
        {
            sizeMenu.Visibility = Visibility.Hidden;
        }
        //最小化窗口
        private void min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        //最大化窗口
        private void max_Click(object sender, RoutedEventArgs e)
        {
            if (isMaxWindow == false)
            {
                windowHeight = this.ActualHeight;
                windowWidth = this.ActualWidth;
                this.WindowState = WindowState.Maximized;
                max.Content = "二";
                isMaxWindow = true;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                max.Content = "口";
                isMaxWindow = false;

            }
        }
        //点击关闭窗口
        private void shutDown_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //全屏播放
        private void fullScreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }


        public void videoSlider_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            TimeSpan span = new TimeSpan(0, 0, (int)(videoSlider.Value) * 10);
            myVideo.Position = span;
            
        }


        
        public void volumnSlider_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            myVideo.Volume = volumeSlider.Value / 10.0;
        }
    }
}
