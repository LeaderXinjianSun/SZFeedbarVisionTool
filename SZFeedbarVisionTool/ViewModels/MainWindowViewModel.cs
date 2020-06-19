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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewROI;
using System.Runtime.Serialization.Formatters.Binary;

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
        private int threshold;

        public int Threshold
        {
            get { return threshold; }
            set
            {
                threshold = value;
                this.RaisePropertyChanged("Threshold");
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
        private ObservableCollection<ROI> rOIList;

        public ObservableCollection<ROI> ROIList
        {
            get { return rOIList; }
            set
            {
                rOIList = value;
                this.RaisePropertyChanged("ROIList");
            }
        }
        private int activeIndex;

        public int ActiveIndex
        {
            get { return activeIndex; }
            set
            {
                activeIndex = value;
                this.RaisePropertyChanged("ActiveIndex");
            }
        }
        private bool repaint;

        public bool Repaint
        {
            get { return repaint; }
            set
            {
                repaint = value;
                this.RaisePropertyChanged("Repaint");
            }
        }
        private int selectIndexValue;
        public int SelectIndexValue
        {
            get { return selectIndexValue; }
            set
            {
                selectIndexValue = value;
                this.RaisePropertyChanged("SelectIndexValue");
            }
        }
        private ObservableCollection<bool> radioButtonIsChecked;
        public ObservableCollection<bool> RadioButtonIsChecked
        {
            get { return radioButtonIsChecked; }
            set
            {
                radioButtonIsChecked = value;
                this.RaisePropertyChanged("RadioButtonIsChecked");
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
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand RecognizeCommand { get; set; }
        public DelegateCommand RangeCommand { get; set; }
        public DelegateCommand BlockCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        #endregion
        #region 变量
        bool SystemRunFlag = true;
        private HFramegrabber Framegrabber;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Fx5u Fx5u;
        HObject newImage;
        bool isContinueGrab = false;
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
            RangeCommand = new DelegateCommand(new Action(this.RangeCommandExecute));
            BlockCommand = new DelegateCommand(new Action(this.BlockCommandExecute));
            DeleteCommand = new DelegateCommand(new Action(this.DeleteCommandExecute));
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
                    Threshold = ParamValue1.Threshold;
                    ActiveIndex = 0;
                    ROIList = ParamValue1.ROIList;
                    ActiveIndex = ROIList.Count > 0 ? ROIList.Count - 1 : 0;
                    AddMessage("选择1号产品参数");
                    break;
                case "1":
                    SelectIndexValue = 1;
                    Threshold = ParamValue2.Threshold;
                    ActiveIndex = 0;
                    ROIList = ParamValue2.ROIList;
                    ActiveIndex = ROIList.Count > 0 ? ROIList.Count - 1 : 0;
                    AddMessage("选择2号产品参数");
                    break;
                case "2":
                    SelectIndexValue = 2;
                    Threshold = ParamValue3.Threshold;
                    ActiveIndex = 0;
                    ROIList = ParamValue3.ROIList;
                    ActiveIndex = ROIList.Count > 0 ? ROIList.Count - 1 : 0;
                    AddMessage("选择3号产品参数");
                    break;
                case "3":
                    SelectIndexValue = 3;
                    Threshold = ParamValue4.Threshold;
                    ActiveIndex = 0;
                    ROIList = ParamValue4.ROIList;
                    ActiveIndex = ROIList.Count > 0 ? ROIList.Count - 1 : 0;
                    AddMessage("选择4号产品参数");
                    break;
                default:
                    break;
            }
        }
       
        private async void RangeCommandExecute()
        {
            isContinueGrab = false;
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Red");
            HalconWindowVisibility = "Collapsed";
            bool r = await((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("确认",
                "请确认要重新画范围吗？",
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
                ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE1);
                CameraAppendHObject = roi.getRegion();
                HOperatorSet.WriteRegion(roi.getRegion(), Path.Combine(path, "Range.hobj"));
                AddMessage("画范围完成");

            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                HalconWindowVisibility = "Visible";
            }
        }
        private void BlockCommandExecute()
        {
            isContinueGrab = false;          
            ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_LINE);
            roi.ROIColor = "magenta";
            roi.SizeEnable = true;
            ROIList.Add(roi);
            ActiveIndex = ROIList.Count - 1;

        }
        private void DeleteCommandExecute()
        {
            isContinueGrab = false;
            if (ROIList.Count > 0)
            {
                ROIList.RemoveAt(ActiveIndex);
                ActiveIndex = ROIList.Count > 0 ? ROIList.Count - 1 : 0;
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
                    ParamValue1.Threshold = Threshold;
                    ParamValue1.ROIList = ROIList;
                    SaveParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\1\Param.par"), ParamValue1);
                    AddMessage("保存1号产品参数");
                    break;
                case 1:
                    ParamValue2.Threshold = Threshold;
                    ParamValue2.ROIList = ROIList;
                    SaveParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\2\Param.par"), ParamValue2);
                    AddMessage("保存2号产品参数");
                    break;
                case 2:
                    ParamValue3.Threshold = Threshold;
                    ParamValue3.ROIList = ROIList;
                    SaveParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\3\Param.par"), ParamValue3);
                    AddMessage("保存3号产品参数");
                    break;
                case 3:
                    ParamValue4.Threshold = Threshold;
                    ParamValue4.ROIList = ROIList;
                    SaveParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\4\Param.par"), ParamValue4);
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
            WindowTitle = "SZFeedbarVisionTool20200619";
            HalconWindowVisibility = "Visible";
            LoginMenuItemHeader = "登录";
            MessageStr = "";
            IsLogin = false;
            StatusPLC = true;
            RadioButtonIsChecked = new ObservableCollection<bool>();
            for (int i = 0; i < 4; i++)
            {
                RadioButtonIsChecked.Add(false);
            }
            ParamValue1 = LoadParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\1", "Param.par"));
            ParamValue2 = LoadParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\2", "Param.par"));
            ParamValue3 = LoadParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\3", "Param.par"));
            ParamValue4 = LoadParam(Path.Combine(System.Environment.CurrentDirectory, @"Camera\4", "Param.par"));
            SelectIndexValue = 0;
            RadioButtonIsChecked[SelectIndexValue] = true;
            Threshold = ParamValue1.Threshold;
            ROIList = ParamValue1.ROIList;
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
                        SelectIndexValue = 0;
                        RadioButtonIsChecked[SelectIndexValue] = true;
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
                        SelectIndexValue = 1;
                        RadioButtonIsChecked[SelectIndexValue] = true;
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
                        SelectIndexValue = 2;
                        RadioButtonIsChecked[SelectIndexValue] = true;
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
                        SelectIndexValue = 3;
                        RadioButtonIsChecked[SelectIndexValue] = true;
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
            double _threshold = 0;
            switch (index)
            {
                case 0:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\1");
                    _threshold = ParamValue1.Threshold;
                    ROIList = ParamValue1.ROIList;
                    break;
                case 1:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\2");
                    _threshold = ParamValue2.Threshold;
                    ROIList = ParamValue2.ROIList;
                    break;
                case 2:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\3");
                    _threshold = ParamValue3.Threshold;
                    ROIList = ParamValue3.ROIList;
                    break;
                case 3:
                    path = Path.Combine(System.Environment.CurrentDirectory, @"Camera\4");
                    _threshold = ParamValue4.Threshold;
                    ROIList = ParamValue4.ROIList;
                    break;
                default:
                    break;
            }
            ActiveIndex = -1;
            foreach (var item in ROIList)
            {
                item.ROIColor = "magenta";
            }
            //Repaint = !Repaint;
            try
            {
                #region 图像处理
                HObject range;
                HOperatorSet.ReadRegion(out range, Path.Combine(path, "Range.hobj"));
                HObject imageReduced;
                HOperatorSet.ReduceDomain(CameraIamge, range, out imageReduced);
                HObject pregion;
                HOperatorSet.Threshold(imageReduced, out pregion, 0, _threshold);
                HObject regionFillUp;
                HOperatorSet.FillUp(pregion,out regionFillUp);
                HObject connectedRegions;
                HOperatorSet.Connection(regionFillUp,out connectedRegions);
                HObject selectedRegions;
                HOperatorSet.SelectShapeStd(connectedRegions,out selectedRegions, "max_area", 70);

                CameraGCStyle = new Tuple<string, object>("DrawMode", "fill");
                CameraGCStyle = new Tuple<string, object>("Color", "blue");
                CameraAppendHObject = null;
                CameraAppendHObject = selectedRegions;
                bool result = true;
                foreach (var item in ROIList)
                {
                    HObject regionIntersection;
                    HOperatorSet.Intersection(selectedRegions, item.getRegion(), out regionIntersection);
                    HTuple area, row, column;
                    HOperatorSet.AreaCenter(regionIntersection,out area,out row,out column);
                    
                    if (area > 0)
                    {
                        item.ROIColor = "red";
                        result = false;
                    }
                    else
                    {
                        item.ROIColor = "green";
                    }
                }
                Repaint = !Repaint;
                #endregion
                #region 判定

                return result;
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
        private ParamValue LoadParam(string filePath)
        {
            ParamValue _DefaultJob = null;
            if (File.Exists(filePath))
            {
                try
                {
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryFormatter mBinFmat = new BinaryFormatter();
                    _DefaultJob = mBinFmat.Deserialize(fileStream) as ParamValue;
                    fileStream.Close();
                }
                catch (Exception ex)
                {
                    _DefaultJob = new ParamValue();
                    AddMessage(ex.Message);
                }
            }
            else
            {
                _DefaultJob = new ParamValue();
            }
            return _DefaultJob;
        }
        private void SaveParam(string filePath, ParamValue param)
        {
            try
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Create);
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(fileStream, param);
                fileStream.Close();
            }
            catch { }
        }
        #endregion
    }
    [Serializable]
    public class ParamValue
    {
        public ParamValue()
        {
            Threshold = 128;
            ROIList = new ObservableCollection<ROI>();
        }
        public int Threshold { get; set; }
        public ObservableCollection<ROI> ROIList { get; set; }
    }
}
