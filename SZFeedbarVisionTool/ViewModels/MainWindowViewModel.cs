using BingLibrary.hjb.file;
using ControlzEx.Theming;
using HalconDotNet;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using SXJLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewROI;

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
        private double distanceDiffValue;

        public double DistanceDiffValue
        {
            get { return distanceDiffValue; }
            set
            {
                distanceDiffValue = value;
                this.RaisePropertyChanged("DistanceDiffValue");
            }
        }
        private double angleDiffValue;

        public double AngleDiffValue
        {
            get { return angleDiffValue; }
            set
            {
                angleDiffValue = value;
                this.RaisePropertyChanged("AngleDiffValue");
            }
        }
        private double distanceABSDiffValue;

        public double DistanceABSDiffValue
        {
            get { return distanceABSDiffValue; }
            set
            {
                distanceABSDiffValue = value;
                this.RaisePropertyChanged("DistanceABSDiffValue");
            }
        }
        private double angleABSDiffValue;

        public double AngleABSDiffValue
        {
            get { return angleABSDiffValue; }
            set
            {
                angleABSDiffValue = value;
                this.RaisePropertyChanged("AngleABSDiffValue");
            }
        }

        private HObject cameraAppendHObject;

        public HObject CameraAppendHObject
        {
            get { return cameraAppendHObject; }
            set
            {
                cameraAppendHObject = value;
                this.RaisePropertyChanged("CameraAppendHObject");
            }
        }
        private Tuple<string, object> cameraGCStyle;

        public Tuple<string, object> CameraGCStyle
        {
            get { return cameraGCStyle; }
            set
            {
                cameraGCStyle = value;
                this.RaisePropertyChanged("CameraGCStyle");
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
        public DelegateCommand ShapeModelCommand { get; set; }
        public DelegateCommand LineLeftCommand { get; set; }
        public DelegateCommand LineTopCommand { get; set; }
        public DelegateCommand LineRightCommand { get; set; }
        public DelegateCommand LineBottomCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand RecognizeCommand { get; set; }
        #endregion
        #region 变量
        bool SystemRunFlag = true;
        private HFramegrabber Framegrabber;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Fx5u Fx5u;
        HObject newImage;
        bool isContinueGrab = false;
        int SelectIndexValue = 0;
        ParamValue ParamValue1, ParamValue2, ParamValue3, ParamValue4;
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
            SelectIndexCommand = new DelegateCommand<object>(new Action<object>(this.SelectIndexCommandExecute));
            ShapeModelCommand = new DelegateCommand(new Action(this.ShapeModelCommandExecute));
            LineLeftCommand = new DelegateCommand(new Action(this.LineLeftCommandExecute));
            LineTopCommand = new DelegateCommand(new Action(this.LineTopCommandExecute));
            LineRightCommand = new DelegateCommand(new Action(this.LineRightCommandExecute));
            LineBottomCommand = new DelegateCommand(new Action(this.LineBottomCommandExecute));
            SaveCommand = new DelegateCommand(new Action(this.SaveCommandExecute));
            RecognizeCommand = new DelegateCommand(new Action(this.RecognizeCommandExecute));
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
                    DistanceDiffValue = ParamValue1.Distance;
                    AngleDiffValue = ParamValue1.Angle;
                    DistanceABSDiffValue = ParamValue1.DistanceABS;
                    AngleABSDiffValue = ParamValue1.AngleABS;
                    AddMessage("选择1号产品参数");
                    break;
                case "1":
                    SelectIndexValue = 1;
                    DistanceDiffValue = ParamValue2.Distance;
                    AngleDiffValue = ParamValue2.Angle;
                    DistanceABSDiffValue = ParamValue2.DistanceABS;
                    AngleABSDiffValue = ParamValue2.AngleABS;
                    AddMessage("选择2号产品参数");
                    break;
                case "2":
                    SelectIndexValue = 2;
                    DistanceDiffValue = ParamValue3.Distance;
                    AngleDiffValue = ParamValue3.Angle;
                    DistanceABSDiffValue = ParamValue3.DistanceABS;
                    AngleABSDiffValue = ParamValue3.AngleABS;
                    AddMessage("选择3号产品参数");
                    break;
                case "3":
                    SelectIndexValue = 3;
                    DistanceDiffValue = ParamValue4.Distance;
                    AngleDiffValue = ParamValue4.Angle;
                    DistanceABSDiffValue = ParamValue4.DistanceABS;
                    AngleABSDiffValue = ParamValue4.AngleABS;
                    AddMessage("选择4号产品参数");
                    break;
                default:
                    break;
            }
        }
        private async void ShapeModelCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画模板吗？",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确  认",
                    NegativeButtonText = "取  消",
                    ColorScheme = MetroDialogColorScheme.Accented,
                }) == MessageDialogResult.Affirmative;
            if (r)
            {
                string path = "";
                switch (SelectIndexValue)
                {
                    case 0:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                        break;
                    case 1:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                        break;
                    case 2:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                        break;
                    case 3:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                        break;
                    default:
                        break;
                }
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "green");
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_REGION);
                HObject ReduceDomainImage;
                HOperatorSet.ReduceDomain(CameraIamge, roi.getRegion(), out ReduceDomainImage);
                HObject modelImages, modelRegions;
                HOperatorSet.InspectShapeModel(ReduceDomainImage, out modelImages, out modelRegions, 7, 30);
                HObject objectSelected;
                HOperatorSet.SelectObj(modelRegions, out objectSelected, 1);               
                CameraAppendHObject = objectSelected;
                HOperatorSet.WriteRegion(objectSelected, Path.Combine(path, "ModelRegion.hobj"));
                HTuple ModelID;
                HOperatorSet.CreateShapeModel(ReduceDomainImage, 7, (new HTuple(-45)).TupleRad(), (new HTuple(90)).TupleRad(), (new HTuple(0.1)).TupleRad(), "no_pregeneration", "use_polarity", 30, 10, out ModelID);
                HOperatorSet.WriteShapeModel(ModelID, Path.Combine(path, "ShapeModel.shm"));
                CameraIamge.WriteImage("bmp", 0, Path.Combine(path, "ModelImage.bmp"));
                AddMessage("创建模板完成");
            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private async void LineLeftCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画左直线吗？",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确  认",
                    NegativeButtonText = "取  消",
                    ColorScheme = MetroDialogColorScheme.Accented,
                }) == MessageDialogResult.Affirmative;
            if (r)
            {
                string path = "";
                switch (SelectIndexValue)
                {
                    case 0:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                        break;
                    case 1:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                        break;
                    case 2:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                        break;
                    case 3:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                        break;
                    default:
                        break;
                }
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "red");
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE2);
                CameraAppendHObject = roi.getRegion();
                HOperatorSet.WriteRegion(roi.getRegion(), Path.Combine(path, "LeftLine.hobj"));
                AddMessage("画左直线完成");

            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private async void LineTopCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画上直线吗？",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确  认",
                    NegativeButtonText = "取  消",
                    ColorScheme = MetroDialogColorScheme.Accented,
                }) == MessageDialogResult.Affirmative;
            if (r)
            {
                string path = "";
                switch (SelectIndexValue)
                {
                    case 0:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                        break;
                    case 1:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                        break;
                    case 2:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                        break;
                    case 3:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                        break;
                    default:
                        break;
                }
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "red");
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE2);
                CameraAppendHObject = roi.getRegion();
                HOperatorSet.WriteRegion(roi.getRegion(), Path.Combine(path, "TopLine.hobj"));
                AddMessage("画上直线完成");

            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private async void LineRightCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画右直线吗？",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确  认",
                    NegativeButtonText = "取  消",
                    ColorScheme = MetroDialogColorScheme.Accented,
                }) == MessageDialogResult.Affirmative;
            if (r)
            {
                string path = "";
                switch (SelectIndexValue)
                {
                    case 0:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                        break;
                    case 1:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                        break;
                    case 2:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                        break;
                    case 3:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                        break;
                    default:
                        break;
                }
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "red");
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE2);
                CameraAppendHObject = roi.getRegion();
                HOperatorSet.WriteRegion(roi.getRegion(), Path.Combine(path, "RightLine.hobj"));
                AddMessage("画右直线完成");

            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private async void LineBottomCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画下直线吗？",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确  认",
                    NegativeButtonText = "取  消",
                    ColorScheme = MetroDialogColorScheme.Accented,
                }) == MessageDialogResult.Affirmative;
            if (r)
            {
                string path = "";
                switch (SelectIndexValue)
                {
                    case 0:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                        break;
                    case 1:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                        break;
                    case 2:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                        break;
                    case 3:
                        path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                        break;
                    default:
                        break;
                }
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "red");
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE2);
                CameraAppendHObject = roi.getRegion();
                HOperatorSet.WriteRegion(roi.getRegion(), Path.Combine(path, "BottomLine.hobj"));
                AddMessage("画下直线完成");

            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private void SaveCommandExecute()
        {
            string path = "";
            switch (SelectIndexValue)
            {
                case 0:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                    break;
                case 1:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                    break;
                case 2:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                    break;
                case 3:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                    break;
                default:
                    break;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            switch (SelectIndexValue)
            {
                case 0:
                    ParamValue1.Distance = DistanceDiffValue;
                    ParamValue1.Angle = AngleDiffValue;
                    ParamValue1.DistanceABS = DistanceABSDiffValue;
                    ParamValue1.AngleABS = AngleABSDiffValue;
                    WriteToJson(ParamValue1, Path.Combine(System.Environment.CurrentDirectory, @"Camera\1", "ParamValue.json"));
                    AddMessage("保存1号产品参数");
                    break;
                case 1:
                    ParamValue2.Distance = DistanceDiffValue;
                    ParamValue2.Angle = AngleDiffValue;
                    ParamValue2.DistanceABS = DistanceABSDiffValue;
                    ParamValue2.AngleABS = AngleABSDiffValue;
                    WriteToJson(ParamValue2, Path.Combine(System.Environment.CurrentDirectory, @"Camera\2", "ParamValue.json"));
                    AddMessage("保存2号产品参数");
                    break;
                case 2:
                    ParamValue3.Distance = DistanceDiffValue;
                    ParamValue3.Angle = AngleDiffValue;
                    ParamValue3.DistanceABS = DistanceABSDiffValue;
                    ParamValue3.AngleABS = AngleABSDiffValue;
                    WriteToJson(ParamValue3, Path.Combine(System.Environment.CurrentDirectory, @"Camera\3", "ParamValue.json"));
                    AddMessage("保存3号产品参数");
                    break;
                case 3:
                    ParamValue4.Distance = DistanceDiffValue;
                    ParamValue4.Angle = AngleDiffValue;
                    ParamValue4.DistanceABS = DistanceABSDiffValue;
                    ParamValue4.AngleABS = AngleABSDiffValue;
                    WriteToJson(ParamValue4, Path.Combine(System.Environment.CurrentDirectory, @"Camera\4", "ParamValue.json"));
                    AddMessage("保存4号产品参数");
                    break;
                default:
                    break;
            }
            

        }
        private void RecognizeCommandExecute()
        {
            RecognizeOpetate(SelectIndexValue);
        }
        #endregion
        #region 自定义函数
        private void Init()
        {
            WindowTitle = "SZFeedbarVisionTool20200612";
            HalconWindowVisibility = "Visible";
            LoginMenuItemHeader = "登录";
            MessageStr = "";
            IsLogin = false;
            StatusPLC = true;
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(System.Environment.CurrentDirectory, @"Camera\1", "ParamValue.json")))
                {
                    string json = reader.ReadToEnd();
                    ParamValue1 = JsonConvert.DeserializeObject<ParamValue>(json);
                }
            }
            catch (Exception ex)
            {
                ParamValue1 = new ParamValue();
                AddMessage(ex.Message);
            }
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(System.Environment.CurrentDirectory, @"Camera\2", "ParamValue.json")))
                {
                    string json = reader.ReadToEnd();
                    ParamValue2 = JsonConvert.DeserializeObject<ParamValue>(json);
                }
            }
            catch (Exception ex)
            {
                ParamValue2 = new ParamValue();
                AddMessage(ex.Message);
            }
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(System.Environment.CurrentDirectory, @"Camera\3", "ParamValue.json")))
                {
                    string json = reader.ReadToEnd();
                    ParamValue3 = JsonConvert.DeserializeObject<ParamValue>(json);
                }
            }
            catch (Exception ex)
            {
                ParamValue3 = new ParamValue();
                AddMessage(ex.Message);
            }
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(System.Environment.CurrentDirectory, @"Camera\4", "ParamValue.json")))
                {
                    string json = reader.ReadToEnd();
                    ParamValue4 = JsonConvert.DeserializeObject<ParamValue>(json);
                }
            }
            catch (Exception ex)
            {
                ParamValue4 = new ParamValue();
                AddMessage(ex.Message);
            }
            SelectIndexValue = 0;
            DistanceDiffValue = ParamValue1.Distance;
            AngleDiffValue = ParamValue1.Angle;
            DistanceABSDiffValue = ParamValue1.DistanceABS;
            AngleABSDiffValue = ParamValue1.AngleABS;
        }
        private void SystemRun()
        {
            while (SystemRunFlag)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    if (Fx5u.ReadM("M6000"))
                    {
                        Fx5u.SetMultiM("M6000", new bool[] { false, false, false });
                        CameraIamge = new HImage(newImage);
                        var rst = RecognizeOpetate(0);
                        Fx5u.SetM("M6002", rst);
                        Fx5u.SetM("M6001", true);
                        if (rst)
                        {
                            AddMessage("位置1检测成功");
                        }
                        else
                        {
                            AddMessage("位置1检测失败");
                        }
                    }
                    if (Fx5u.ReadM("M6003"))
                    {
                        Fx5u.SetMultiM("M6003", new bool[] { false, false, false });
                        CameraIamge = new HImage(newImage);
                        var rst = RecognizeOpetate(1);
                        Fx5u.SetM("M6005", rst);
                        Fx5u.SetM("M6004", true);
                        if (rst)
                        {
                            AddMessage("位置2检测成功");
                        }
                        else
                        {
                            AddMessage("位置2检测失败");
                        }
                    }
                    if (Fx5u.ReadM("M6006"))
                    {
                        Fx5u.SetMultiM("M6006", new bool[] { false, false, false });
                        CameraIamge = new HImage(newImage);
                        var rst = RecognizeOpetate(2);
                        Fx5u.SetM("M6008", rst);
                        Fx5u.SetM("M6007", true);
                        if (rst)
                        {
                            AddMessage("位置3检测成功");
                        }
                        else
                        {
                            AddMessage("位置3检测失败");
                        }
                    }
                    if (Fx5u.ReadM("M6009"))
                    {
                        Fx5u.SetMultiM("M6009", new bool[] { false, false, false });
                        CameraIamge = new HImage(newImage);
                        var rst = RecognizeOpetate(3);
                        Fx5u.SetM("M6011", rst);
                        Fx5u.SetM("M6010", true);
                        if (rst)
                        {
                            AddMessage("位置4检测成功");
                        }
                        else
                        {
                            AddMessage("位置4检测失败");
                        }
                    }
                }
                catch { }
                
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
                        HObject image;
                        HOperatorSet.GrabImageAsync(out image, Framegrabber, -1);
                        HObject image2, image3;
                        HOperatorSet.Decompose3(image, out newImage,out image2,out image3);
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
        private void WriteToJson(object p, string path)
        {
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, p);
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
        private bool RecognizeOpetate(int index)
        {
            isContinueGrab = false;
            string path = "";
            double _dist = 0,_angle = 0, _dist_abs = 0, _angle_abs = 0;
            switch (index)
            {
                case 0:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                    _dist = ParamValue1.Distance;
                    _angle = ParamValue1.Angle;
                    _dist_abs = ParamValue1.DistanceABS;
                    _angle_abs = ParamValue1.AngleABS;
                    break;
                case 1:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                    _dist = ParamValue2.Distance;
                    _angle = ParamValue2.Angle;
                    _dist_abs = ParamValue2.DistanceABS;
                    _angle_abs = ParamValue2.AngleABS;
                    break;
                case 2:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                    _dist = ParamValue3.Distance;
                    _angle = ParamValue3.Angle;
                    _dist_abs = ParamValue3.DistanceABS;
                    _angle_abs = ParamValue3.AngleABS;
                    break;
                case 3:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                    _dist = ParamValue4.Distance;
                    _angle = ParamValue4.Angle;
                    _dist_abs = ParamValue3.DistanceABS;
                    _angle_abs = ParamValue3.AngleABS;
                    break;
                default:
                    break;
            }
            try
            {
                #region 找模板
                HObject ModelImage;
                HOperatorSet.ReadImage(out ModelImage, Path.Combine(path, "ModelImage.bmp"));
                HTuple ModelID, row, column, angle, score, row1, column1, angle1, score1;
                HOperatorSet.ReadShapeModel(Path.Combine(path, "ShapeModel.shm"), out ModelID);
                HOperatorSet.FindShapeModel(ModelImage, ModelID, (new HTuple(-45)).TupleRad(), (new HTuple(90)).TupleRad(), 0.5, 1, 0, "least_squares", 0, 0.9, out row, out column, out angle, out score);
                HOperatorSet.FindShapeModel(CameraIamge, ModelID, (new HTuple(-45)).TupleRad(), (new HTuple(90)).TupleRad(), 0.5, 1, 0, "least_squares", 0, 0.9, out row1, out column1, out angle1, out score1);
                HTuple homMat2D;
                HOperatorSet.VectorAngleToRigid(row, column, angle, row1, column1, angle1, out homMat2D);
                HObject modelRegion;
                HOperatorSet.ReadRegion(out modelRegion, Path.Combine(path, "ModelRegion.hobj"));
                HObject regionAffineTrans;
                HOperatorSet.AffineTransRegion(modelRegion, out regionAffineTrans, homMat2D, "nearest_neighbor");
                CameraAppendHObject = null;
                CameraGCStyle = new Tuple<string, object>("Color", "green");
                CameraAppendHObject = regionAffineTrans;
                #endregion
                #region 找直线
                HObject lineRegion;
                HOperatorSet.ReadRegion(out lineRegion, Path.Combine(path, "LeftLine.hobj"));
                HObject regionLineAffineTrans;
                HOperatorSet.AffineTransRegion(lineRegion, out regionLineAffineTrans, homMat2D, "nearest_neighbor");
                HObject imageReduced1;
                HOperatorSet.ReduceDomain(CameraIamge, regionLineAffineTrans, out imageReduced1);
                HObject edges1;
                HOperatorSet.EdgesSubPix(imageReduced1, out edges1, "canny", 1, 20, 40);
                HObject contoursSplit1;
                HOperatorSet.SegmentContoursXld(edges1, out contoursSplit1, "lines_circles", 5, 4, 2);
                HObject selectedContours1;
                HOperatorSet.SelectContoursXld(contoursSplit1, out selectedContours1, "contour_length", 15, 500, -0.5, 0.5);
                HObject unionContours1;
                HOperatorSet.UnionAdjacentContoursXld(selectedContours1, out unionContours1, 10, 1, "attr_keep");
                HTuple rowBegin1, colBegin1, rowEnd1, colEnd1, nr1, nc1, dist1;
                HOperatorSet.FitLineContourXld(unionContours1, "tukey", -1, 0, 5, 2, out rowBegin1, out colBegin1, out rowEnd1, out colEnd1, out nr1, out nc1, out dist1);
                HObject regionLine;
                HOperatorSet.GenRegionLine(out regionLine, rowBegin1, colBegin1, rowEnd1, colEnd1);
                CameraAppendHObject = regionLine;
                index = FindMaxLine(regionLine);
                double lineAngle1 = Math.Atan2((nc1.DArr[index]), (nr1.DArr[index])) * 180 / Math.PI - 90;
                AddMessage("左直线距离:" + dist1.D.ToString("F1") + " 角度:" + lineAngle1.ToString("F1"));

                HOperatorSet.ReadRegion(out lineRegion, Path.Combine(path, "TopLine.hobj"));
                HOperatorSet.AffineTransRegion(lineRegion, out regionLineAffineTrans, homMat2D, "nearest_neighbor");
                HOperatorSet.ReduceDomain(CameraIamge, regionLineAffineTrans, out imageReduced1);
                HOperatorSet.EdgesSubPix(imageReduced1, out edges1, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges1, out contoursSplit1, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit1, out selectedContours1, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours1, out unionContours1, 10, 1, "attr_keep");
                HTuple dist2;
                HOperatorSet.FitLineContourXld(unionContours1, "tukey", -1, 0, 5, 2, out rowBegin1, out colBegin1, out rowEnd1, out colEnd1, out nr1, out nc1, out dist2);
                HOperatorSet.GenRegionLine(out regionLine, rowBegin1, colBegin1, rowEnd1, colEnd1);
                HTuple line1Row1 = rowBegin1, line1Column1 = colBegin1, line1Row2 = rowEnd1, line1Column2 = colEnd1;
                CameraAppendHObject = regionLine;
                index = FindMaxLine(regionLine);
                int line1Index = index;
                double lineAngle2 = Math.Atan2((nc1.DArr[index]), (nr1.DArr[index])) * 180 / Math.PI - 90;
                AddMessage("上直线距离:" + dist2.D.ToString("F1") + " 角度:" + lineAngle2.ToString("F1"));

                HOperatorSet.ReadRegion(out lineRegion, Path.Combine(path, "RightLine.hobj"));
                HOperatorSet.AffineTransRegion(lineRegion, out regionLineAffineTrans, homMat2D, "nearest_neighbor");
                HOperatorSet.ReduceDomain(CameraIamge, regionLineAffineTrans, out imageReduced1);
                HOperatorSet.EdgesSubPix(imageReduced1, out edges1, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges1, out contoursSplit1, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit1, out selectedContours1, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours1, out unionContours1, 10, 1, "attr_keep");
                HTuple dist3;
                HOperatorSet.FitLineContourXld(unionContours1, "tukey", -1, 0, 5, 2, out rowBegin1, out colBegin1, out rowEnd1, out colEnd1, out nr1, out nc1, out dist3);
                HOperatorSet.GenRegionLine(out regionLine, rowBegin1, colBegin1, rowEnd1, colEnd1);
                HTuple line2Row1 = rowBegin1, line2Column1 = colBegin1, line2Row2 = rowEnd1, line2Column2 = colEnd1;
                CameraAppendHObject = regionLine;
                index = FindMaxLine(regionLine);
                int line2Index = index;
                double lineAngle3 = Math.Atan2((nc1.DArr[index]), (nr1.DArr[index])) * 180 / Math.PI - 90;
                AddMessage("右直线距离:" + dist3.D.ToString("F1") + " 角度:" + lineAngle3.ToString("F1"));

                HOperatorSet.ReadRegion(out lineRegion, Path.Combine(path, "BottomLine.hobj"));
                HOperatorSet.AffineTransRegion(lineRegion, out regionLineAffineTrans, homMat2D, "nearest_neighbor");
                HOperatorSet.ReduceDomain(CameraIamge, regionLineAffineTrans, out imageReduced1);
                HOperatorSet.EdgesSubPix(imageReduced1, out edges1, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges1, out contoursSplit1, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit1, out selectedContours1, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours1, out unionContours1, 10, 1, "attr_keep");
                HTuple dist4;
                HOperatorSet.FitLineContourXld(unionContours1, "tukey", -1, 0, 5, 2, out rowBegin1, out colBegin1, out rowEnd1, out colEnd1, out nr1, out nc1, out dist4);
                HOperatorSet.GenRegionLine(out regionLine, rowBegin1, colBegin1, rowEnd1, colEnd1);
                CameraAppendHObject = regionLine;
                index = FindMaxLine(regionLine);
                double lineAngle4 = Math.Atan2((nc1.DArr[index]), (nr1.DArr[index])) * 180 / Math.PI - 90;
                AddMessage("下直线距离:" + dist4.D.ToString("F1") + " 角度:" + lineAngle4.ToString("F1"));

                HTuple insRow, insColum, isOverLapping ;
                HOperatorSet.IntersectionLines(line1Row1.DArr[line1Index], line1Column1.DArr[line1Index], line1Row2.DArr[line1Index], line1Column2.DArr[line1Index], line2Row1.DArr[line2Index], line2Column1.DArr[line2Index], line2Row2.DArr[line2Index], line2Column2.DArr[line2Index], out insRow, out insColum,out isOverLapping);
                #endregion
                #region 找模板上的直线
                HObject lineRegion0;
                HOperatorSet.ReadRegion(out lineRegion0, Path.Combine(path, "LeftLine.hobj"));
                HObject imageReduced0;
                HOperatorSet.ReduceDomain(ModelImage, lineRegion0, out imageReduced0);
                HObject edges0;
                HOperatorSet.EdgesSubPix(imageReduced0, out edges0, "canny", 1, 20, 40);
                HObject contoursSplit0;
                HOperatorSet.SegmentContoursXld(edges0, out contoursSplit0, "lines_circles", 5, 4, 2);
                HObject selectedContours0;
                HOperatorSet.SelectContoursXld(contoursSplit0, out selectedContours0, "contour_length", 15, 500, -0.5, 0.5);
                HObject unionContours0;
                HOperatorSet.UnionAdjacentContoursXld(selectedContours0, out unionContours0, 10, 1, "attr_keep");
                HTuple rowBegin0, colBegin0, rowEnd0, colEnd0, nr0, nc0, _dist1;
                HOperatorSet.FitLineContourXld(unionContours0, "tukey", -1, 0, 5, 2, out rowBegin0, out colBegin0, out rowEnd0, out colEnd0, out nr0, out nc0, out _dist1);
                HObject regionLine0;
                HOperatorSet.GenRegionLine(out regionLine0, rowBegin0, colBegin0, rowEnd0, colEnd0);                
                index = FindMaxLine(regionLine0);
                double _lineAngle1 = Math.Atan2((nc0.DArr[index]), (nr0.DArr[index])) * 180 / Math.PI - 90;

                HOperatorSet.ReadRegion(out lineRegion0, Path.Combine(path, "TopLine.hobj"));
                HOperatorSet.ReduceDomain(ModelImage, lineRegion0, out imageReduced0);
                HOperatorSet.EdgesSubPix(imageReduced0, out edges0, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges0, out contoursSplit0, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit0, out selectedContours0, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours0, out unionContours0, 10, 1, "attr_keep");
                HTuple _dist2;
                HOperatorSet.FitLineContourXld(unionContours0, "tukey", -1, 0, 5, 2, out rowBegin0, out colBegin0, out rowEnd0, out colEnd0, out nr0, out nc0, out _dist2);
                HOperatorSet.GenRegionLine(out regionLine0, rowBegin0, colBegin0, rowEnd0, colEnd0);
                HTuple _line1Row1 = rowBegin0, _line1Column1 = colBegin0, _line1Row2 = rowEnd0, _line1Column2 = colEnd0;
                index = FindMaxLine(regionLine0);
                int _line1Index = index;
                double _lineAngle2 = Math.Atan2((nc0.DArr[index]), (nr0.DArr[index])) * 180 / Math.PI - 90;

                HOperatorSet.ReadRegion(out lineRegion0, Path.Combine(path, "RightLine.hobj"));
                HOperatorSet.ReduceDomain(ModelImage, lineRegion0, out imageReduced0);
                HOperatorSet.EdgesSubPix(imageReduced0, out edges0, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges0, out contoursSplit0, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit0, out selectedContours0, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours0, out unionContours0, 10, 1, "attr_keep");
                HTuple _dist3;
                HOperatorSet.FitLineContourXld(unionContours0, "tukey", -1, 0, 5, 2, out rowBegin0, out colBegin0, out rowEnd0, out colEnd0, out nr0, out nc0, out _dist3);
                HOperatorSet.GenRegionLine(out regionLine0, rowBegin0, colBegin0, rowEnd0, colEnd0);
                HTuple _line2Row1 = rowBegin0, _line2Column1 = colBegin0, _line2Row2 = rowEnd0, _line2Column2 = colEnd0;
                index = FindMaxLine(regionLine0);
                int _line2Index = index;
                double _lineAngle3 = Math.Atan2((nc0.DArr[index]), (nr0.DArr[index])) * 180 / Math.PI - 90;

                HOperatorSet.ReadRegion(out lineRegion0, Path.Combine(path, "BottomLine.hobj"));
                HOperatorSet.ReduceDomain(ModelImage, lineRegion0, out imageReduced0);
                HOperatorSet.EdgesSubPix(imageReduced0, out edges0, "canny", 1, 20, 40);
                HOperatorSet.SegmentContoursXld(edges0, out contoursSplit0, "lines_circles", 5, 4, 2);
                HOperatorSet.SelectContoursXld(contoursSplit0, out selectedContours0, "contour_length", 15, 500, -0.5, 0.5);
                HOperatorSet.UnionAdjacentContoursXld(selectedContours0, out unionContours0, 10, 1, "attr_keep");
                HTuple _dist4;
                HOperatorSet.FitLineContourXld(unionContours0, "tukey", -1, 0, 5, 2, out rowBegin0, out colBegin0, out rowEnd0, out colEnd0, out nr0, out nc0, out _dist4);
                HOperatorSet.GenRegionLine(out regionLine0, rowBegin0, colBegin0, rowEnd0, colEnd0);
                index = FindMaxLine(regionLine0);
                double _lineAngle4 = Math.Atan2((nc0.DArr[index]), (nr0.DArr[index])) * 180 / Math.PI - 90;

                HTuple _insRow, _insColum, _isOverLapping;
                HOperatorSet.IntersectionLines(_line1Row1.DArr[_line1Index], _line1Column1.DArr[_line1Index], _line1Row2.DArr[_line1Index], _line1Column2.DArr[_line1Index], _line2Row1.DArr[_line2Index], _line2Column1.DArr[_line2Index], _line2Row2.DArr[_line2Index], _line2Column2.DArr[_line2Index], out _insRow, out _insColum, out _isOverLapping);
                #endregion
                #region 判定
                HOperatorSet.SetColor(Global.CameraImageViewer.viewController.viewPort.HalconWindow, "red");
                HOperatorSet.DispCross(Global.CameraImageViewer.viewController.viewPort.HalconWindow, insRow, insColum, 60, 0);
                HOperatorSet.SetColor(Global.CameraImageViewer.viewController.viewPort.HalconWindow, "green");
                HOperatorSet.DispCross(Global.CameraImageViewer.viewController.viewPort.HalconWindow, _insRow, _insColum, 60, 0);
                HTuple distance;
                HOperatorSet.DistancePp(insRow, insColum, _insRow, _insColum, out distance);
                var delta_d = Math.Abs(dist1.D - dist3.D - _dist1.D + _dist3.D);
                var delta_a = Math.Abs(lineAngle1 - lineAngle4 - _lineAngle1 + _lineAngle4);

                var delta_a_abs = Math.Abs(lineAngle1 - _lineAngle1);
                AddMessage("相对距离差:" + delta_d.ToString("F1") + " 相对角度差:" + delta_a.ToString("F1") + " 绝对距离差:" + distance.D.ToString("F1") + " 绝对角度差:" + delta_a_abs.ToString("F1"));
                if (delta_d > _dist || delta_a > _angle || distance.D > _dist_abs || delta_a_abs > _angle_abs)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                #endregion
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
                return false;
            }
        }
        private int FindMaxLine(HObject LineRegion)
        {
            HTuple area, row, column;
            HOperatorSet.AreaCenter(LineRegion, out area, out row, out column);
            HTuple max;
            HOperatorSet.TupleMax(area, out max);

            for (int i = 0; i < area.LArr.Length; i++)
            {
                if (area.LArr[i] == max)
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion
    }
    class ParamValue
    {
        public double Distance { get; set; }
        public double Angle { get; set; }
        public double DistanceABS { get; set; }
        public double AngleABS { get; set; }

    }
}
