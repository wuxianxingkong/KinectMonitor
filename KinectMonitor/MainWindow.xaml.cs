using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using Microsoft.Kinect;
namespace KinectMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Kinect3D : Window
    {
        KinectSensor kinect;
        public Kinect3D()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            sks.Close();
            //log.Dispose();

        }
       // private Log log;
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            DiscoverKinectSensor();
            VideoDirectory();
           // log=new Log(".\\", Log.LogType.Monthly);
            //log.Write("初始化队列...", Log.MsgType.Information);
           // log.Write("应用程序正常启动了...", Log.MsgType.Information);
            //log.Write("应用程序正常启动了...嘿嘿嘿", Log.MsgType.Information);
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            //throw new NotImplementedException();
            DiscoverKinectSensor();
            string info = "状态: " + e.Status + "\n传感器ID: " + e.Sensor.UniqueKinectId + "\n连线ID: " + e.Sensor.DeviceConnectionId;
            TextBlock tb = new TextBlock()
            {
                Text = info
            };

            //log.Write(info, Log.MsgType.Information);
            WindowOpenAndClose(e.Status, e.Sensor,tb);

        }
        Dictionary<string, Window> knowtable = new Dictionary<string, Window>();
        ShowKinectStatus sks;
        private void WindowOpenAndClose(KinectStatus kinectStatus, KinectSensor kinectSensor,TextBlock textBlock)
        {
            //throw new NotImplementedException();
            //string kenectDecId=null;
            switch (kinectStatus)
            {
                case KinectStatus.Connected:
                    if (!knowtable.ContainsKey(kinectSensor.DeviceConnectionId))
                    {
                        sks = new ShowKinectStatus();
                        kinect = kinectSensor;
                        knowtable[kinectSensor.DeviceConnectionId] = sks; //把Connection ID和窗体关联起来
                        //kenectDecId = kinectSensor.DeviceConnectionId;
                        sks.Status.Items.Add(textBlock);
                        sks.Show();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (knowtable.ContainsKey(kinectSensor.DeviceConnectionId))
                    {
                        ShowKinectStatus w = (ShowKinectStatus)knowtable[kinectSensor.DeviceConnectionId];
                        w.Close();
                        knowtable.Remove(kinectSensor.DeviceConnectionId); //移除关联
                        count = 0;
                    }
                    break;
            }

        }
        static int count = 0;
        private void DiscoverKinectSensor()
        {
           // throw new NotImplementedException();
            string info = "侦测到" + KinectSensor.KinectSensors.Count + "台传感器\n";
            //TextBlock tb = new TextBlock() { Text = info, Foreground = Brushes.Red };
            //Status.Items.Add(tb);
            
            foreach (var s in KinectSensor.KinectSensors)
            {
                string i=null;
                if (count < 1) i += info;
                i += "侦测到的传感器ID: " + s.UniqueKinectId + "\n连线ID: " + s.DeviceConnectionId + "\n状态" + s.Status;
                TextBlock t = new TextBlock() { Text = i };
                //Status.Items.Add(t);
                count++;
                WindowOpenAndClose(s.Status, s,t);

            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (count > 0)
            {
                SafeFunction fs = new SafeFunction(kinect);
                fs.Show();
                this.Close();
            }
            else 
            {
                MessageBox.Show("对不起，没有任何Kinect设备连接，无法使用该功能","注意");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (count > 0)
            {
                SecurityPersonnel sp = new SecurityPersonnel(kinect);
            sp.Show();
            this.Close();
            }
            else
            {
                MessageBox.Show("对不起，没有任何Kinect设备连接，无法使用该功能", "注意");
            }
        }
        string _baseDirectory = "..\\video\\";
        private void VideoDirectory()
        {
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(_baseDirectory))
                {
                    // Create the directory it does not exist.
                    Directory.CreateDirectory(_baseDirectory);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "KinectFusionExplorer-WPF.exe";
            process.Start();
            this.Close();
        }
        
    }
}
