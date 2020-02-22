using System.Windows;
using System;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowInputInformation.xaml
    /// </summary>
    public partial class WindowSelectItem : Window
    {

        private MainClass.ReferenceTypePackaging<int> Input_;
        public WindowSelectItem(out MainClass.ReferenceTypePackaging<int> Input, string[] Items, string InputTitle)
        {
            InitializeComponent();
            Input = new MainClass.ReferenceTypePackaging<int>(-1);
            Input_ = Input;
            #region 设置binding
            ComboBoxInput.ItemsSource = Items;
            ComboBoxInput.SelectedIndex = 0;
            this.Title = InputTitle;
            #endregion
        }

        private void Button_ClickOK(object sender, RoutedEventArgs e)
        {
            Input_.Value = ComboBoxInput.SelectedIndex;
            Close();
        }

        private void TextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ComboBoxInput.Items.Count > 0)
            {
                string Text = TextBoxSearch.Text;

                decimal[] Similarity = new decimal[ComboBoxInput.Items.Count];
                int[] Index = new int[ComboBoxInput.Items.Count];

                int i = 0;
                foreach (string n in ComboBoxInput.ItemsSource)
                {
                    Index[i] = i;
                    Similarity[i++] = MainClass.ComputeStringSimilarity(Text, n);
                }
                Array.Sort(Similarity, Index);

                ComboBoxInput.SelectedIndex = Index[Index.Length - 1];
            }
        }
    }
}
