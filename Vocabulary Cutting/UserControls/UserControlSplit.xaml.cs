using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF
{
    /// <summary>
    /// Interaction logic for UserControlSplit.xaml
    /// </summary>
    public partial class UserControlSplit : UserControl
    {
        public UserControlSplit()
        {
            InitializeComponent();
            #region 设置Binding
            MainClass.BindingData("Text", OwnSetBingdingData0, LabelM, TextBlock.TextProperty);
            #endregion
        }

        private DataBinding OwnSetBingdingData0 = new DataBinding();

        public string Header
        {
            get
            {
                return OwnSetBingdingData0.Text;
            }
            set
            {
                OwnSetBingdingData0.Text = value;
            }
        }

        public new FrameworkElement Content
        {
            get
            {
                return DockPanelM.Children[1] as FrameworkElement;
            }
            set
            {
                if (DockPanelM.Children.Count > 1)
                {
                    DockPanelM.Children.RemoveAt(DockPanelM.Children.Count - 1);
                }
                DockPanelM.Children.Add(value);
            }
        }

        public class DataBinding : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private string Text_;
            public string Text
            {
                get
                {
                    if (Text_ == null)
                    {
                        Text_ = "";
                    }
                    return Text_;
                }
                set
                {
                    Text_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Text"));
                    }
                }
            }
        }
    }
}
