using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowExportTXTSelection.xaml
    /// </summary>
    public partial class WindowExportTXTSelection : Window
    {
        public class Selection
        {
            public Selection(string SetType)
            {
                Type = SetType;
                Enable = true;
            }

            public bool Enable { get; set; }
            public string Type { get; set; }
        }

        private BindingData Binding_Data;
        public WindowExportTXTSelection(MainClass.ReferenceTypePackaging<List<Selection>> InputSelection)
        {
            InitializeComponent();
            Binding_Data = new BindingData(InputSelection);
            MainClass.BindingData("Selection", Binding_Data, ListBoxSelection, ListBox.ItemsSourceProperty);
        }

        private class BindingData : INotifyPropertyChanged
        {
            public BindingData(MainClass.ReferenceTypePackaging<List<Selection>> InputSelection)
            {
                Selection_ = InputSelection;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private MainClass.ReferenceTypePackaging<List<Selection>> Selection_;
            public List<Selection> Selection
            {
                get
                {
                    return Selection_.Value;
                }
                set
                {
                    Selection_.Value = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Selection"));
                    }
                }
            }
        }

        private bool OK = false;
        private void Button_OK(object sender, RoutedEventArgs e)
        {
            OK = true;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!OK)
            {
                Binding_Data.Selection.Clear();
            }
        }
    }
}
