using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
//互动服务 

using System.Collections.Generic;
using System.IO;
using IWshRuntimeLibrary;


namespace WPF
{
    /// <summary>
    /// 插件设置窗口
    /// </summary>
    public partial class WindowSetting : Window
    {
        private readonly BindingData Binding_Data;
        private readonly MainWindow Father;
        public WindowSetting(MainWindow Father_)
        {
            InitializeComponent();
            Binding_Data = new BindingData(Owner);
            #region 设置Binding
            MainClass.BindingData("Topmost", Binding_Data, CheckBoxTopmost, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoStart", Binding_Data, CheckBoxAutoStart, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoBackup", Binding_Data, CheckBoxAutoBackup, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoBackup", Binding_Data, SliderMaxBackupTimes, Slider.IsEnabledProperty);
            MainClass.BindingData("MinInvisible", Binding_Data, CheckBoxMinVisible, CheckBox.IsCheckedProperty);
            MainClass.BindingData("MaxBackupTimes", Binding_Data, SliderMaxBackupTimes, Slider.ValueProperty);
            MainClass.BindingData("TextMaxBackupTimes", Binding_Data, LabelMaxBackupTimes, Label.ContentProperty);
            MainClass.BindingData("EnableRemindVoice", Binding_Data, CheckBoxEnableRemindVoice, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoFillingMeanings", Binding_Data, CheckBoxAutoFillMeanings, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoFillingExamples", Binding_Data, CheckBoxAutoFillExamples, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoFillingPictures", Binding_Data, CheckBoxAutoFillPictures, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoFillingPronunciations", Binding_Data, CheckBoxAutoFillPronunciations, CheckBox.IsCheckedProperty);
            MainClass.BindingData("AutoShowTips", Binding_Data, CheckBoxAutoShowTips, CheckBox.IsCheckedProperty);
            Father = Father_;
            #endregion
            foreach (var l in MainPlatomEntrance.Setting.OpenLists)
            {
                var Temp = new ListBoxItem();
                Temp.Content = l;
                ListBoxAutoStartLists.Items.Add(Temp);
            }
        }

        private class BindingData : INotifyPropertyChanged//binding结构
        {
            private readonly string InkName;
            private readonly Window Father;
            public BindingData(Window SetFather)
            {
                InkName = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "//Vocabulary Cutting.lnk";
                Father = SetFather;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public bool Topmost
            {
                get
                {
                    return MainPlatomEntrance.Setting.Topmost;
                }
                set
                {
                    MainPlatomEntrance.Setting.Topmost = value;
                    foreach (var t in MainPlatomEntrance.WindowList)
                    {
                        t.Topmost = MainPlatomEntrance.Setting.Topmost;
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Topmost"));
                    }
                }
            }

            public bool MinInvisible
            {
                get
                {
                    return MainPlatomEntrance.Setting.MinInvisible;
                }
                set
                {
                    MainPlatomEntrance.Setting.MinInvisible = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MinInvisible"));
                    }
                }
            }

            public bool AutoBackup
            {
                get
                {
                    return MainPlatomEntrance.Setting.AutoBackup;
                }
                set
                {
                    MainPlatomEntrance.Setting.AutoBackup = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoBackup"));
                    }
                }
            }

            public bool AutoFillingMeanings
            {
                get
                {
                    return MainPlatomEntrance.Setting.AutoFillMeanings;
                }
                set
                {
                    MainPlatomEntrance.Setting.AutoFillMeanings = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoFillingMeanings"));
                    }
                }
            }

            public bool AutoFillingPictures
            {
                get
                {
                    return MainPlatomEntrance.Setting.AutoFillPictures;
                }
                set
                {
                    MainPlatomEntrance.Setting.AutoFillPictures = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoFillingPictures"));
                    }
                }
            }

            public bool AutoFillingExamples
            {
                get
                {
                    return MainPlatomEntrance.Setting.AutoFillExamples;
                }
                set
                {
                    MainPlatomEntrance.Setting.AutoFillExamples = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoFillingExamples"));
                    }
                }
            }

            public bool AutoFillingPronunciations
            {
                get
                {
                    return MainPlatomEntrance.Setting.AutoFillPronunciations;
                }
                set
                {
                    MainPlatomEntrance.Setting.AutoFillPronunciations = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoFillingPronunciations"));
                    }
                }
            }
    
            public bool AutoStart
            {
                get
                {
                    return System.IO.File.Exists(InkName);
                }
                set
                {
                    try
                    {
                        switch (value)
                        {
                            case true:
                                //开机启动 
                                WshShell shell = new WshShell();
                                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(InkName);
                                shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                shortcut.WorkingDirectory = System.Environment.CurrentDirectory;
                                shortcut.WindowStyle = 1;
                                shortcut.Description = "Vocabulary Cutting";
                                shortcut.Save();
                                break;
                            case false:
                                //取消开机启动
                                System.IO.File.Delete(InkName);
                                break;
                        }
                        MainPlatomEntrance.SetNotify("Set AutoStart Finished!", 2, Father);
                    }
                    catch
                    {
                        MainPlatomEntrance.SetNotify("Set AutoStart Failed!", 2, Father);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AutoStart"));
                    }
                }
            }

            public bool EnableRemindVoice
            {
                get
                {
                    return MainPlatomEntrance.Setting.EnableRemindVoice;
                }
                set
                {
                    MainPlatomEntrance.Setting.EnableRemindVoice = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("EnableRemindVoice"));
                    }
                }
            }

            public int MaxBackupTimes
            {
                get
                {
                    return MainPlatomEntrance.Setting.MaxBackupTimes;
                }
                set
                {
                    MainPlatomEntrance.Setting.MaxBackupTimes = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MaxBackupTimes"));
                    }
                    TextMaxBackupTimes = TextMaxBackupTimes;
                }
            }

            public string TextMaxBackupTimes
            {
                get
                {
                    return "Maximum Backup Times (" + MaxBackupTimes + ")";
                }
                set
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("TextMaxBackupTimes"));
                    }
                }
            }
        }

        private void MenuItem_ClickAddList(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> FileLists = new List<string>();
                foreach (FileInfo f in new DirectoryInfo(MainWindow.VocabularyListsPath).GetFiles())
                {
                    if (f.ToString().EndsWith(MainWindow.ListExtension))
                    {
                        FileLists.Add(Path.GetFileNameWithoutExtension(f.ToString()));
                    }
                }
                var SelectIndex = new MainClass.ReferenceTypePackaging<int>(-1);
                var SelectItemWindow = new WindowSelectItem(out SelectIndex, FileLists.ToArray(), "Select a list!");
                MainClass.OpenWindow(SelectItemWindow, this, true);
                if (SelectIndex.Value != -1)
                {
                    if (MainPlatomEntrance.Setting.OpenLists.Contains(FileLists[SelectIndex.Value]))
                    {
                        throw new Exception();
                    }
                    var Temp = new ListBoxItem();
                    Temp.Content = FileLists[SelectIndex.Value];
                    ListBoxAutoStartLists.Items.Add(Temp);
                    MainPlatomEntrance.Setting.OpenLists.Add(FileLists[SelectIndex.Value]);
                    MainPlatomEntrance.SetNotify("Add AutoStart List Finished", 2, Owner);
                }
            }
            catch
            {
                MainPlatomEntrance.SetNotify("Add AutoStart List Failed", 2, Owner);
            }
        }

        private void MenuItem_ClickDeleteList(object sender, RoutedEventArgs e)
        {
            if (ListBoxAutoStartLists.SelectedIndex != -1)
            {
                MainPlatomEntrance.Setting.OpenLists.RemoveAt(ListBoxAutoStartLists.SelectedIndex);
                ListBoxAutoStartLists.Items.RemoveAt(ListBoxAutoStartLists.SelectedIndex);
            }
        }

        private void MenuItem_ClickClearList(object sender, RoutedEventArgs e)
        {
            ListBoxAutoStartLists.Items.Clear();
            MainPlatomEntrance.Setting.OpenLists.Clear();
        }

        private void Button_ChangeRemindVoice(object sender, RoutedEventArgs e)
        {
            ChangeVoice(MainPlatomEntrance.RemindVoice);
        }

        private void Button_ChangeStartVoice(object sender, RoutedEventArgs e)
        {
            ChangeVoice(MainPlatomEntrance.StartVoice);
        }

        private void Button_ChangeEndVoice(object sender, RoutedEventArgs e)
        {
            ChangeVoice(MainPlatomEntrance.EndVoice);
        }

        private void ChangeVoice(string Path)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Please,select a voice";
            fileDialog.Filter = "list type(*.mp3)|*.mp3";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    MainClass.PlaySoundPath(null, false);
                    if (System.IO.File.Exists(Path))
                    {
                        System.IO.File.Delete(Path);
                    }
                    System.IO.File.Copy(fileDialog.FileName, Path);
                    MainPlatomEntrance.SetNotify("Change Voice Finished!", 2, Owner);
                    MainClass.PlaySoundPath(Path, false);
                    MainClass.LockPlaySound = true;
                    MessageBox.Show("Click OK to stop", "Testing Voice", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    MainClass.LockPlaySound = false;
                    MainClass.PlaySoundPath(null, false);
                }
                catch
                {
                    MainPlatomEntrance.SetNotify("Change Voice Failed!", 2, Owner);
                }
            }
        }

        private void ButtonClearBuffer_Click(object sender, RoutedEventArgs e)
        {
            switch (ComboBoxClearBuffer.SelectedIndex)
            {
                case 0:
                    foreach (var m in MainPlatomEntrance.WindowList)
                    {
                        foreach (var v in m.WordsList.Words)
                        {
                            v.Meanings.Clear();
                        }
                    }
                    break;
                case 1:
                    //System.IO.File.Delete();
                    break;
                case 2:
                    //System.IO.File.Delete();
                    break;
                case 3:
                    //System.IO.File.Delete();
                    break;
                case 4:
                    foreach (var m in MainPlatomEntrance.WindowList)
                    {
                        foreach (var v in m.WordsList.Words)
                        {
                            for (int n = v.Categorys.Count - 1; n >= 0; n--)
                            {
                                if (v.Categorys[n].StartsWith(MainWindow.MemorizeTitle) || v.Categorys[n].StartsWith(MainWindow.RootTitle))
                                {
                                    v.Categorys.RemoveAt(n);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
