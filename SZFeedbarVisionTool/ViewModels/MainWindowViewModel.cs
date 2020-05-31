using BingLibrary.hjb.file;
using HalconDotNet;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using SXJLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZFeedbarVisionTool.ViewModels
{
    class MainWindowViewModel : NotificationObject
    {
        #region 属性绑定
        private string windowTitle;

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                this.RaisePropertyChanged("WindowTitle");
            }
        }
        private bool statusCamera;

        public bool StatusCamera
        {
            get { return statusCamera; }
            set
            {
                statusCamera = value;
                this.RaisePropertyChanged("StatusCamera");
            }
        }
        private bool statusPLC;

        public bool StatusPLC
        {
            get { return statusPLC; }
            set
            {
                statusPLC = value;
                this.RaisePropertyChanged("StatusPLC");
            }
        }
        private long cycle;

        public long Cycle
        {
            get { return cycle; }
            set
            {
                cycle = value;
                this.RaisePropertyChanged("Cycle");
            }
        }
        private HImage cameraIamge;

        public HImage CameraIamge
        {
            get { return cameraIamge; }
            set
            {
                cameraIamge = value;
                this.RaisePropertyChanged("CameraIamge");
            }
        }
        private string messageStr;

        public string MessageStr
        {
            get { return messageStr; }
            set
            {
                messageStr = value;
                this.RaisePropertyChanged("MessageStr");
            }
        }

        #endregion
        #region 方法绑定
        public DelegateCommand AppLoadedEventCommand { get; set; }
        public DelegateCommand AppClosedEventCommand { get; set; }
        #endregion
        #region 变量
        bool SystemRunFlag = true;
        private HFramegrabber Framegrabber;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Fx5u Fx5u;
        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("SZFeedbarVisionTool");//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
                System.Windows.MessageBox.Show("不允许重复打开软件");
                System.Windows.Application.Current.Shutdown();
            }
            AppLoadedEventCommand = new DelegateCommand(new Action(this.AppLoadedEventCommandExecute));
            AppClosedEventCommand = new DelegateCommand(new Action(this.AppClosedEventCommandExecute));
            Init();
        }
        #endregion
        #region 自定义函数
        private void Init()
        {
            WindowTitle = "SZFeedbarVisionTool20200531";
            MessageStr = "";
            StatusPLC = true;
        }
        private void AppLoadedEventCommandExecute()
        {
            string plc_ip = Inifile.INIGetStringValue(iniParameterPath, "System", "PLCIP", "192.168.1.13");
            int plc_port = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "PLCPORT", "3900"));
            Fx5u = new Fx5u(plc_ip, plc_port);
            Fx5u.ConnectStateChanged += Fx5uConnectStateChanged;
            Task.Run(() => { CameraRun(); });
            Task.Run(() => { SystemRun(); });
        }
        private void AppClosedEventCommandExecute()
        {
            SystemRunFlag = false;
            Framegrabber?.Dispose();
        }
        private void SystemRun()
        {
            while (SystemRunFlag)
            {
                System.Threading.Thread.Sleep(100);
                if (Fx5u.ReadM("M3100"))
                {
                    Fx5u.SetM("M3100", false);
                }
            }
        }
        private void CameraRun()
        {
            Stopwatch sw = new Stopwatch();
            StatusCamera = false;
            while (SystemRunFlag)
            {
                sw.Restart();
                try
                {
                    if (!StatusCamera)
                    {
                        HTuple information, valueList;
                        HOperatorSet.InfoFramegrabber("DirectShow", "device", out information, out valueList);
                        string device = Inifile.INIGetStringValue(iniParameterPath, "Camera", "Device", "cam0");
                        if (valueList.SArr.Contains(device))
                        {
                            Framegrabber = new HFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", -1, "default", -1, "false", "default", device, 0, -1);
                            HOperatorSet.GrabImageStart(Framegrabber, -1);
                            StatusCamera = true;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(500);
                        }

                    }

                    HObject image;
                    if (StatusCamera)
                    {
                        HOperatorSet.GrabImageAsync(out image, Framegrabber, -1);
                        CameraIamge = new HImage(image);
                    }

                }
                catch (Exception ex)
                {
                    StatusCamera = false;
                    AddMessage(ex.Message);
                    Framegrabber?.Dispose();
                    System.Threading.Thread.Sleep(1000);
                }
                Cycle = sw.ElapsedMilliseconds;
            }
        }
        private void AddMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            RunLog(str);
            MessageStr += System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        void Fx5uConnectStateChanged(object sender, bool e)
        {
            StatusPLC = e;
        }
        void RunLog(string str)
        {
            try
            {
                string tempSaveFilee5 = System.AppDomain.CurrentDomain.BaseDirectory + @"RunLog";
                DateTime dtim = DateTime.Now;
                string DateNow = dtim.ToString("yyyy/MM/dd");
                string TimeNow = dtim.ToString("HH:mm:ss");

                if (!Directory.Exists(tempSaveFilee5))
                {
                    Directory.CreateDirectory(tempSaveFilee5);  //创建目录 
                }

                if (File.Exists(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt"))
                {
                    //第一种方法：
                    FileStream fs = new FileStream(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("TTIME：" + TimeNow + " 执行事件：" + str);
                    sw.Dispose();
                    fs.Dispose();
                    sw.Close();
                    fs.Close();
                }
                else
                {
                    //不存在就新建一个文本文件,并写入一些内容 
                    StreamWriter sw;
                    sw = File.CreateText(tempSaveFilee5 + "\\" + DateNow.Replace("/", "") + ".txt");
                    sw.WriteLine("TTIME：" + TimeNow + " 执行事件：" + str);
                    sw.Dispose();
                    sw.Close();
                }
            }
            catch { }
        }
        #endregion
    }
}
