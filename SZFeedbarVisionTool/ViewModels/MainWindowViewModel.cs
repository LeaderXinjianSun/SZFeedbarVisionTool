using BingLibrary.hjb.file;
using ControlzEx.Theming;
using HalconDotNet;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using System.Windows;

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
        private string loginMenuItemHeader;

        public string LoginMenuItemHeader
        {
            get { return loginMenuItemHeader; }
            set
            {
                loginMenuItemHeader = value;
                this.RaisePropertyChanged("LoginMenuItemHeader");
            }
        }
        private string halconWindowVisibility;

        public string HalconWindowVisibility
        {
            get { return halconWindowVisibility; }
            set
            {
                halconWindowVisibility = value;
                this.RaisePropertyChanged("HalconWindowVisibility");
            }
        }
        private bool isLogin;

        public bool IsLogin
        {
            get { return isLogin; }
            set
            {
                isLogin = value;
                this.RaisePropertyChanged("IsLogin");
            }
        }
        #endregion
        #region 方法绑定
        public DelegateCommand AppLoadedEventCommand { get; set; }
        public DelegateCommand AppClosedEventCommand { get; set; }
        public DelegateCommand GrabCommand { get; set; }
        public DelegateCommand ContinueGrabCommand { get; set; }
        public DelegateCommand ReadImageCommand { set; get; }
        public DelegateCommand LoginCommand { get; set; }
        public DelegateCommand<object> SelectIndexCommand { get; set; }
        #endregion
        #region 变量
        bool SystemRunFlag = true;
        private HFramegrabber Framegrabber;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Fx5u Fx5u;
        HObject newImage;
        bool isContinueGrab = false;
        int SelectIndexValue = 0;
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
            GrabCommand = new DelegateCommand(new Action(this.GrabCommandExecute));
            ContinueGrabCommand = new DelegateCommand(new Action(this.ContinueGrabCommandExecute));
            ReadImageCommand = new DelegateCommand(new Action(this.ReadImageCommandExecute));
            LoginCommand = new DelegateCommand(new Action(this.LoginCommandExecute));
            SelectIndexCommand = new DelegateCommand<object>(new Action<object>(SelectIndexCommandExecute));
            Init();
        }
        #endregion
        #region 方法绑定函数
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
        private void GrabCommandExecute()
        {
            isContinueGrab = false;
            if (newImage != null)
            {
                CameraIamge = new HImage(newImage);
            }
        }
        private void ContinueGrabCommandExecute()
        {
            isContinueGrab = true;
        }
        private void ReadImageCommandExecute()
        {
            isContinueGrab = false;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Image文件(*.bmp;*.jpg)|*.bmp;*.jpg|所有文件|*.*";
            ofd.ValidateNames = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strFileName = ofd.FileName;
                HObject image;
                HOperatorSet.ReadImage(out image, strFileName);
                CameraIamge = new HImage(image);
            }
        }
        private async void LoginCommandExecute()
        {
            if (IsLogin)
            {
                IsLogin = false;
                LoginMenuItemHeader = "登录";
                AddMessage("已登出");
            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
                HalconWindowVisibility = "Collapsed";
                //var r = await metro.ShowLoginOnlyPassword("请登录");
                string r = (await ((MetroWindow)Application.Current.MainWindow).ShowLoginAsync("请登录", "输入你的凭证:", new LoginDialogSettings { ColorScheme = MetroDialogColorScheme.Accented , PasswordWatermark = "请输入你的密码", ShouldHideUsername = true }))?.Password;
                if (r == GetPassWord())
                {
                    IsLogin = true;
                    LoginMenuItemHeader = "登出";
                }
                else
                {
                    AddMessage("密码错误");
                }
                HalconWindowVisibility = "Visible";
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
            }
        }
        private void SelectIndexCommandExecute(object p)
        {
            switch (p.ToString())
            {
                case "0":
                    SelectIndexValue = 0;
                    AddMessage("选择1号产品参数");
                    break;
                case "1":
                    SelectIndexValue = 1;
                    AddMessage("选择2号产品参数");
                    break;
                case "2":
                    SelectIndexValue = 2;
                    AddMessage("选择3号产品参数");
                    break;
                case "3":
                    SelectIndexValue = 3;
                    AddMessage("选择4号产品参数");
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 自定义函数
        private void Init()
        {
            WindowTitle = "SZFeedbarVisionTool20200606";
            HalconWindowVisibility = "Visible";
            LoginMenuItemHeader = "登录";
            MessageStr = "";
            IsLogin = false;
            StatusPLC = true;
            SelectIndexValue = 0;
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

                    if (StatusCamera)
                    {
                        HOperatorSet.GrabImageAsync(out newImage, Framegrabber, -1);
                        if (isContinueGrab)
                        {
                            CameraIamge = new HImage(newImage);
                        }
                        GC.Collect();//垃圾回收
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
        private string GetPassWord()
        {
            int day = System.DateTime.Now.Day;
            int month = System.DateTime.Now.Month;
            string ss = (day + month).ToString();
            string passwordstr = "";
            for (int i = 0; i < 4 - ss.Length; i++)
            {
                passwordstr += "0";
            }
            passwordstr += ss;
            return passwordstr;
        }
        #endregion
    }
}
