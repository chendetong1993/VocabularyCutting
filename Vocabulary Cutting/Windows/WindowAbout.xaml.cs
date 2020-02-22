using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WPF
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WindowAbout : Window
    {
        private readonly BindingData Binding_Data = new BindingData();
        private const string StoryPath = "About\\Story.txt";
        public WindowAbout()
        {
            InitializeComponent();
            #region 设置binding
            MainClass.BindingData("IntroduceText", Binding_Data, TextBlockIntroduction, TextBlock.TextProperty);
            MainClass.BindingData("StoryText", Binding_Data, TextBoxStory, TextBox.TextProperty);
            #endregion

            const int StartedData_Year = 2013;
            const int StartedData_Month = 3;
            const int StartedData_Day = 15;
            const string Author = "Detong Chen";

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);

            StringBuilder IntroduceText = new StringBuilder();
            //StartTime;
            IntroduceText.AppendLine("Product: " + info.ProductName);
            IntroduceText.AppendLine("Author: " + Author);
            IntroduceText.AppendLine("Company: " + info.CompanyName);
            IntroduceText.AppendLine("Version: " + info.ProductVersion);
            IntroduceText.AppendLine("Copyright: " + info.LegalCopyright);
            IntroduceText.AppendLine("Elapsed Time: " + (int)(DateTime.Now - new DateTime(StartedData_Year, StartedData_Month, StartedData_Day)).TotalDays + " days");
            var TempText = IntroduceText.ToString();
            TempText = TempText.Substring(0, TempText.Length - 2);
            Binding_Data.IntroduceText = TempText;
            Binding_Data.StoryText = File.ReadAllText(StoryPath);
            MainClass.LockPlaySound = true;
        }

        private class BindingData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string IntroduceText_;
            public string IntroduceText
            {
                get
                {
                    return IntroduceText_;
                }
                set
                {
                    IntroduceText_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IntroduceText"));
                    }
                }
            }

            private string StoryText_;
            public string StoryText
            {
                get
                {
                    return StoryText_;
                }
                set
                {
                    StoryText_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StoryText"));
                    }
                }
            }
        }

        private void ButtonSure_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainClass.LockPlaySound = false;
            MainClass.PlaySoundPath(null, false);
        }
    }
}
