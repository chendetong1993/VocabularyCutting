using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;
using System;
using System.Windows.Threading;
using System.Collections.Generic;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowInputInformation.xaml
    /// </summary>
    public partial class WindowInputInformation : Window
    {
        private readonly BindingData Binding_Data;
        private readonly MainWindow Father;

        private readonly MainClass.ReferenceTypePackaging<string> Input_ = null;
        public WindowInputInformation(out MainClass.ReferenceTypePackaging<string> Input,string InputTitle,string Text, MainWindow Father_)
        {
            InitializeComponent();
            Father = Father_;
            Input = new MainClass.ReferenceTypePackaging<string>(null);
            Input_ = Input;
            #region 设置binding
            Binding_Data = new BindingData();
            MainClass.BindingData("Input", Binding_Data, TextBoxInput, TextBox.TextProperty);
            Title = InputTitle;
            Binding_Data.Input = Text;
            Binding_Data.Image = System.Windows.Media.Brushes.Transparent;
            #endregion
            TextBoxInput.Focus();
            TextBoxInput.SelectAll();
        }

        private void Button_ClickOK(object sender, RoutedEventArgs e)
        {
            if (Binding_Data.Input != null)
            {
                if (Binding_Data.Input.Trim() != string.Empty)
                {
                    Input_.Value = Binding_Data.Input;
                }
                else
                {
                    Input_.Value = string.Empty;
                }
            }
            else
            {
                Input_.Value = string.Empty;
            }
            Close();
        }

        private class BindingData : INotifyPropertyChanged
        {
            private string Input_;
            public BindingData()
            {
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public string Input
            {
                get
                {
                    return Input_;
                }
                set
                {
                    Input_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Input"));
                    }
                }
            }

            private System.Windows.Media.Brush Image_;
            public System.Windows.Media.Brush Image
            {
                get
                {
                    return Image_;
                }
                set
                {
                    Image_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Image"));
                    }
                }
            }
        }
    }
}
