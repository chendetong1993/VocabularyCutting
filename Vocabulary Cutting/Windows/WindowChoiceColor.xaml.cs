using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowChoiceColor.xaml
    /// </summary>
    public partial class WindowChoiceColor : Window
    {
        private MainClass.ReferenceTypePackaging<SolidColorBrush> Color_;
        private BindingData Binding_Data;
        public WindowChoiceColor(MainClass.ReferenceTypePackaging<SolidColorBrush> InputColor)
        {
            InitializeComponent();
            Binding_Data = new BindingData();
            InputColor.Value = null;
            Color_ = InputColor;
            MainClass.BindingData("List", Binding_Data, ListBoColorList, ListBox.ItemsSourceProperty);
            foreach (var pi in typeof(Brushes).GetProperties())
            {
                Binding_Data.List.Add(new Item(pi.Name, (SolidColorBrush)pi.GetValue(null, null)));
            }
        }

        public class Item
        {
            public Item(string InputColorName, SolidColorBrush InputColor)
            {
                ColorName = InputColorName;
                Color = InputColor;
            }
            public string ColorName { get; set; }
            public SolidColorBrush Color { get; set; }
        }

        private class BindingData : INotifyPropertyChanged
        {
            public BindingData()
            {
                List = new ObservableCollection<Item>();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private ObservableCollection<Item> List_;
            public ObservableCollection<Item> List
            {
                get
                {
                    return List_;
                }
                set
                {
                    List_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("List"));
                    }
                }
            }
        }

        private void ListBoColorList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ListBoColorList.SelectedIndex != -1)
            {
                Color_.Value = Binding_Data.List[ListBoColorList.SelectedIndex].Color;
                Close();
            }
        }
    }
}
