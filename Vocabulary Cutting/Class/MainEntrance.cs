using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace WPF
{
    class MainEntrance//定义入口
    {
        [STAThreadAttribute()]
        [DebuggerNonUserCodeAttribute()]
        public static void Main(string[] Parm)
        {
            new MainPlatomEntrance().Run(Parm);
        }
    }

    public class MainPlatomEntrance : System.Windows.Application
    {
        private static NotifyIcon notifyIcon;

        public readonly static List<MainWindow> WindowList = new List<MainWindow>();

        public static DispatcherTimer SlowerTimer = new DispatcherTimer();

        public static DispatcherTimer FasterTimer = new DispatcherTimer();

        public const string IconPath = "NotifyIcon\\NotifyIcon.ico";

        private const string SettingPath = "Setting\\Setting.ini";

        public const string StartVoice = "Sound\\Start.mp3";

        public const string EndVoice = "Sound\\End.mp3";

        public const string RemindVoice = "Sound\\Remind.mp3";

        private const string ErrorLogs = "Logs\\ErrorLog.txt";

        private static bool IsInDebugMode()
        {
            var Name = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileName;
            return Name.EndsWith("vshost.exe");
        }

        private static SettingStruct Setting_;
        public static SettingStruct Setting
        {
            get
            {
                return Setting_;
            }
        }

        public void Run(string[] Parm)// 传入参数
        {
            if (MainClass.ProcessStarted(Process.GetCurrentProcess().MainModule.FileName) > 1)
            {
                System.Windows.MessageBox.Show("It has already started！", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Action Action = new Action(() =>
                {
                    try
                    {
                        if (File.Exists(ErrorLogs))
                        {
                            File.WriteAllBytes(ErrorLogs, new byte[0]);
                        }
                    }
                    catch { }
                    try
                    {
                        Setting_ = (SettingStruct)MainClass.BytesToClone(File.ReadAllBytes(SettingPath));
                    }
                    catch
                    {
                        Setting_ = new SettingStruct();
                    }

                    if (Setting.EnableRemindVoice)
                    {
                        MainClass.PlaySoundPath(StartVoice, true);
                    }
                    #region 托盘
                    notifyIcon = new NotifyIcon();
                    notifyIcon.Icon = new System.Drawing.Icon(IconPath);
                    notifyIcon.Visible = true;

                    var item1 = new MenuItem("Show");
                    item1.Click += new EventHandler(NotifyIconShow_Click);

                    var item2 = new MenuItem("Exit");
                    item2.Click += new EventHandler(NotifyIconExit_Click);
                    var menuItems = new MenuItem[] { item1, item2 };
                    notifyIcon.ContextMenu = new ContextMenu(menuItems);
                    notifyIcon.DoubleClick += NotifyIconShow_Click;
                    #endregion
                    #region 定时
                    SlowerTimer = new DispatcherTimer();
                    SlowerTimer.Interval = new TimeSpan(0, 10, 0);
                    SlowerTimer.Tick += GCTimer_Tick;
                    //启动定时器
                    SlowerTimer.Start();

                    FasterTimer = new DispatcherTimer();
                    FasterTimer.Interval = new TimeSpan(0, 0, 0, 1);
                    FasterTimer.Tick += FasterTimer_Tick;
                    //启动定时器
                    FasterTimer.Start();
                    #endregion
                    if (Setting_.OpenLists.Count == 0)
                    {
                        MainWindow MainWindowT = new MainWindow();
                        MainWindowT.Show();
                    }
                    else
                    {
                        foreach (var j in Setting_.OpenLists)
                        {
                            MainWindow MainWindowT = new MainWindow();
                            MainWindowT.Show();
                            MainWindowT.OpenList(j);
                        }
                    }
                    Dispatcher.Run();
                    MainClass.LockPlaySound = false;
                    if (Setting.EnableRemindVoice)
                    {
                        MainClass.DoEvents();
                        MainClass.PlaySoundPath(EndVoice, true);
                    }
                    MainClass.PlaySoundPath(null, true);
                    InternetSpeakeRecognize.Speak(null);
                    notifyIcon.Visible = false;
                });
                if (!IsInDebugMode())
                {
                    try
                    {
                        Action();
                    }
                    catch (Exception Ex)
                    {
                        foreach (var l in WindowList)
                        {
                            try
                            {
                                l.SaveList();
                                l.Backup();
                            }
                            catch { }
                        }

                        List<string> Error = new List<string>();
                        while (Ex != null)
                        {
                            Error.Add("[Message]" + Ex.Message);
                            Error.Add("[Source]" + Ex.Source);
                            Error.Add("[TargetSite]" + Ex.TargetSite);
                            Error.Add("[HelpLink]" + Ex.HelpLink);
                            Error.Add("");
                            Error.Add("-----------------Inside-----------------");
                            Error.Add("----------------------------------------");
                            Ex = Ex.InnerException;
                        }
                        File.WriteAllLines(ErrorLogs, Error.ToArray());

                        System.Windows.MessageBox.Show("Dear Customer :\n\nI am sorry to inform you that this sofware crashed due to an unknown error. Fortunately, it automatically backed up(if you turn on it) and save the lists, you can restore them later.", "Notice", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                        Process.Start(ErrorLogs);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Visual Studio Debug Mode", "", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                    Action();
                }
            }
        }

        private static List<string> NotifyMessage = new List<string>();

        private static void FasterTimer_Tick(object sender, EventArgs e)
        {
            if (NotifyMessage.Count != 0)
            {
                foreach (var i in NotifyMessage)
                {
                    SetNotify(i, 2, null);
                }
                NotifyMessage.Clear();
            }
            notifyIcon.Text = string.Format("{0} [Windows: {1}]\n\nLast notification [{2} ago]", MainPlatformName, WindowList.Count, MainClass.ReturnVagueTimeString(DateTime.Now - NotifyTextTime));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new Exception(e.ToString());
        }

        //记录错误
        private static void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            throw new Exception("DispatcherUnhandledException", e.Exception);
        }

        private const string MainPlatformName = "Main Platform";
         
        private static DateTime NotifyTextTime = DateTime.Now;
        public static void SetNotify(string Text, uint Second, Window Window)
        {
            NotifyTextTime = DateTime.Now;
            string WindowName = MainPlatformName;
            if (Window != null)
            {
                WindowName = Window.Title;
            }

            string TempLog = string.Format("Notice from [{0}]\n\n{1}", WindowName, Text);
            switch(Environment.OSVersion.ToString())
            {
                case "Microsoft Windows NT 6.2.9200.0":
                    TempLog = TempLog.Replace("\n", "  ");
                    break;
            }
            notifyIcon.BalloonTipText = TempLog;
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip((int)Second * 1000);
            if (Setting.EnableRemindVoice)
            {
                MainClass.PlaySoundPath(RemindVoice, false);
            }
        }

        public static void Show(MainWindow sender)
        {
            WindowList.Add(sender);
        }

        public static void Close(MainWindow sender)
        {
            WindowList.Remove(sender);

            if (WindowList.Count == 0)
            {
                Dispatcher.ExitAllFrames();
            }
        }

        private static bool NotifyIconShow_ClickFInsihed = true;
        private static void NotifyIconShow_Click(object sender, EventArgs e)
        {
            if(NotifyIconShow_ClickFInsihed == false)
            {
                return;
            }
            NotifyIconShow_ClickFInsihed = false;
            MainClass.ReferenceTypePackaging<int> Index;
            var TempWindowsList = new List<string>();
            foreach (var i in WindowList)
            {
                TempWindowsList.Add(i.Title);
            }
            if (TempWindowsList.Count != 1)
            {
                var WindowSelectItem = new WindowSelectItem(out Index, TempWindowsList.ToArray(), "Select a Window to show");
                MainClass.OpenWindow(WindowSelectItem, null, true);
            }
            else
            {
                Index = new MainClass.ReferenceTypePackaging<int>(0);
            }
            if (Index.Value != -1)
            {
                WindowList[Index.Value].Visibility = Visibility.Visible;
                WindowList[Index.Value].WindowState = WindowState.Maximized;
                WindowList[Index.Value].ShowInTaskbar = true;
                WindowList[Index.Value].Focus();
            }
            NotifyIconShow_ClickFInsihed = true;
        }

        private static void NotifyIconExit_Click(object sender, EventArgs e)
        {
            var Temp = new List<MainWindow>(WindowList);
            foreach (var l in Temp)
            {
                l.Close();
            }
        }

        private static void GCTimer_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }

        public static void SaveSetting()
        {
            File.WriteAllBytes(SettingPath, MainClass.CloneToBytes(Setting_));
            SetNotify("Save Setting Finished!", 2, null);
        }
    }
}
