using System.Windows.Controls;
using System.ComponentModel;
using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace WPF
{
    /// <summary>
    /// Interaction logic for UserControlWordMetro.xaml
    /// </summary>
    public partial class UserControlWordMetro : UserControl
    {
        private readonly BindingData Binding_Data;

        private readonly MainClass.ReferenceTypePackaging<MainClass.WordListStruct.WordSturct> Word_;

        public MainClass.WordListStruct.WordSturct Word
        {
            get
            {
                return Word_.Value;
            }
        }

        public readonly MainClass.WordListStruct WordList;

        private readonly MainWindow.BindingData FatherBindingData;

        public void SetTiptool()
        {
            if (!MenuItemViewedMark.IsEnabled)
            {
                Binding_Data.Information = "[Details]\n";
            }
            else
            {
                Binding_Data.Information = "Notice: You need to review this word!\n\n[Details]\n";
            }
            string Meanings = "";
            var ReturnMeanings = Word_.Value.Meanings;
            for (int i1 = 0; i1 < ReturnMeanings.Count; i1++)
            {

                Meanings += "[" + ReturnMeanings[i1].Meaning + "]";
                Meanings += "\nExamples: ";
                foreach (var i2 in ReturnMeanings[i1].Examples)
                {
                    Meanings += "{" + i2 + "} ";
                }
                Meanings += "\nDerivatives: ";
                foreach (var i2 in ReturnMeanings[i1].ReturnDerivativesList())
                {
                    Meanings += "{" + i2.Key + " " + i2.Value + "} ";
                }
                Meanings += "\nSynonyms: ";
                foreach (var i2 in ReturnMeanings[i1].ReturnSynonymsList())
                {
                    Meanings += "{" + i2.Key + " " + i2.Value + "} ";
                }
                Meanings += "\nAntonyms: ";
                foreach (var i2 in ReturnMeanings[i1].ReturnAntonymsList())
                {
                    Meanings += "{" + i2.Key + " " + i2.Value + "} ";
                }
                Meanings += "\nAdditions: ";
                foreach (var i2 in ReturnMeanings[i1].Additions)
                {
                    Meanings += "{" + i2 + "} ";
                }
                Meanings += "\nConfused: ";
                foreach (var i2 in ReturnMeanings[i1].ReturnConfusedList())
                {
                    Meanings += "{" + i2.Key + " " + i2.Value + "} ";
                }
                Meanings += "\n";
            }
            string Categorys = "";
            foreach (var h in Word_.Value.Categorys)
            {
                Categorys += "{" + h + "} ";
            }
            Binding_Data.Information +=
                "Meanings:\n" + Meanings + "\n\n" +
                "Review Level: " + Word_.Value.ViewLevel + "\n" +
                "Categorys: " + Categorys + "\n" +
                "Next Review Time: " + Word_.Value.NextViewTime + "\n\n\n\n" +
                "                                   ------ [" + Word_.Value.CreatedTime + "]";
        }

        private readonly Action<UserControlWordMetro> SortWords;

        private readonly Action<UserControlWordMetro> FilterWords;

        private readonly Func<bool> KeepFilterWords;

        public UserControlWordMetro(
            MainClass.ReferenceTypePackaging<MainClass.WordListStruct.WordSturct> Word,
            MainClass.WordListStruct InputWordList,
            MainWindow.BindingData InputBindingData,
            Action<UserControlWordMetro> InputSortWords,
            Action<UserControlWordMetro> InputFilterWords,
            Func<bool> InputKeepFilterWords)
        {
            InitializeComponent();
            WordList = InputWordList;
            Word_ = Word;
            SortWords = InputSortWords;
            FilterWords = InputFilterWords;
            KeepFilterWords = InputKeepFilterWords;
            #region 设置binding
            Binding_Data = new BindingData();
            MainClass.BindingData("Word", Binding_Data, LabelWord, Label.ContentProperty);
            MainClass.BindingData("Level", Binding_Data, LabelLevel, Label.ContentProperty);
            MainClass.BindingData("Information", Binding_Data, LabelWord, Label.ToolTipProperty);
            MainClass.BindingData("WordBackgroundColor", Binding_Data, GridWord, Grid.BackgroundProperty);
            MainClass.BindingData("WordForegroundColor", Binding_Data, LabelWord, Label.ForegroundProperty);
            MainClass.BindingData("WordBorderColor", Binding_Data, LabelWord, Label.BorderBrushProperty);
            MainClass.BindingData("Visible", Binding_Data, GridWord, Grid.VisibilityProperty);
            MainClass.BindingData("Height", Binding_Data, GridWord, Grid.HeightProperty);
            MainClass.BindingData("ControlHeight", Binding_Data, this, UserControl.HeightProperty);
            MainClass.BindingData("SelectedBorderColor", Binding_Data, this, UserControl.BorderBrushProperty);
            #endregion
            Binding_Data.Word = Word.Value.Spelling;
            Binding_Data.WordBackgroundColor = (Brush)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.Value.BackgroundColor);
            Binding_Data.WordBorderColor = (Brush)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.Value.BorderColor);
            Binding_Data.WordForegroundColor = (Brush)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.Value.ForegroundColor);
            Binding_Data.Level = Word.Value.ViewLevel.ToString();
            SetTiptool();
            FatherBindingData = InputBindingData;
            if (!Word.Value.Visible)
            {
                TurnBack();
            }
            else
            {
                TurnFront();
            }
        }

        private class BindingData : INotifyPropertyChanged
        {
            public BindingData()
            { }

            public event PropertyChangedEventHandler PropertyChanged;

            private string Word_;
            public string Word
            {
                get
                {
                    return Word_;
                }
                set
                {
                    Word_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Word"));
                    }
                }
            }

            private string Level_;
            public string Level
            {
                get
                {
                    return Level_;
                }
                set
                {
                    Level_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Level"));
                    }
                }
            }

            private object Information_;
            public object Information
            {
                get
                {
                    return Information_;
                }
                set
                {
                    Information_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Information"));
                    }
                }
            }

            private System.Windows.Media.Brush WordBackgroundColor_;
            public System.Windows.Media.Brush WordBackgroundColor
            {
                get
                {
                    return WordBackgroundColor_;
                }
                set
                {
                    WordBackgroundColor_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("WordBackgroundColor"));
                    }
                }
            }

            private System.Windows.Media.Brush WordForegroundColor_;
            public System.Windows.Media.Brush WordForegroundColor
            {
                get
                {
                    return WordForegroundColor_;
                }
                set
                {
                    WordForegroundColor_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("WordForegroundColor"));
                    }
                }
            }

            private System.Windows.Visibility Visible_;
            public System.Windows.Visibility Visible
            {
                get
                {
                    return Visible_;
                }
                set
                {
                    Visible_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Visible"));
                    }
                }
            }

            private bool Enable_;
            public bool Enable
            {
                get
                {
                    return Enable_;
                }
                set
                {
                    Enable_ = value;
                    if (Enable_)
                    {
                        Visible = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Visible = System.Windows.Visibility.Hidden;
                    }
                }
            }

            private System.Windows.Media.Brush SelectedBorderColor_;
            public System.Windows.Media.Brush SelectedBorderColor
            {
                get
                {
                    return SelectedBorderColor_;
                }
                set
                {
                    SelectedBorderColor_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SelectedBorderColor"));
                    }
                }
            }

            private System.Windows.Media.Brush WordBorderColor_;
            public System.Windows.Media.Brush WordBorderColor
            {
                get
                {
                    return WordBorderColor_;
                }
                set
                {
                    WordBorderColor_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("WordBorderColor"));
                    }
                }
            }

            private double Height_ = 60;
            public double Height
            {
                get
                {
                    return Height_;
                }
                set
                {
                    Height_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Height"));
                    }
                }
            }

            private double ControlHeight_ = 60;
            public double ControlHeight
            {
                get
                {
                    return ControlHeight_;
                }
                set
                {
                    ControlHeight_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ControlHeight"));
                    }
                }
            }
        }

        private void LabelWord_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    if (Binding_Data.Enable)
                    {
                        MainClass.Speak(Binding_Data.Word);
                    }
                    break;
            }
        }

        private void TextBlock_MouseUpDeleteWord(object sender, System.Windows.RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Delete this word?", "Question", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
            {
                var Temp = WordList[Binding_Data.Word];

                var RealtedWords = ReturnRelatedWords();

                WordList.DeleteWord(ref Temp);
                SyncOtherWords(RealtedWords);
                var TempParent = ((WrapPanel)Parent);
                TempParent.Children.Remove(this);
            }
        }

        private void TextBlock_MouseUpViewWord(object sender, System.Windows.RoutedEventArgs e)
        {
            var WindowWordmodify = new WindowWordModify(Word_, WordList);
            System.Windows.FrameworkElement K = this;
            while (K.Parent != null)
            {
                K = (System.Windows.FrameworkElement)K.Parent;
            }
            MainClass.OpenWindow(WindowWordmodify, (System.Windows.Window)K);
            Binding_Data.Word = Word_.Value.Spelling;
            Binding_Data.Enable = Word_.Value.Visible;
            var TempParent = ((WrapPanel)Parent);
            SetTiptool();
            SyncOtherWords(ReturnRelatedWords());
            SortWords(this);
            if (KeepFilterWords())
            {
                FilterWords(this);
            }
        }

        private void TextBlock_MouseUpHideShowWord(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!KeepFilterWords())
            {
                if (Word_.Value.Visible)
                {
                    TurnBack();
                }
                else
                {
                    TurnFront();
                }
            }
        }

        private void TextBlock_MouseUpViewedMark(object sender, System.Windows.RoutedEventArgs e)
        {
            Word_.Value.MarkReview();
            ReviewFinished();
        }

        private void MenuItem_MouseUpChangeWordBackgroundColor(object sender, System.Windows.RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<System.Windows.Media.Brush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);

            System.Windows.FrameworkElement K = this;
            while (K.Parent != null)
            {
                K = (System.Windows.FrameworkElement)K.Parent;
            }

            MainClass.OpenWindow(WindowChoiceColor, (System.Windows.Window)K);
            if (Color.Value != null)
            {
                Word_.Value.BackgroundColor = Color.Value.ToString(); ;
                Binding_Data.WordBackgroundColor = Color.Value;
            }
        }

        private void MenuItem_MouseUpChangeWordBorderColor(object sender, System.Windows.RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<System.Windows.Media.Brush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);

            System.Windows.FrameworkElement K = this;
            while (K.Parent != null)
            {
                K = (System.Windows.FrameworkElement)K.Parent;
            }

            MainClass.OpenWindow(WindowChoiceColor, (System.Windows.Window)K);
            if (Color.Value != null)
            {
                Word_.Value.BorderColor = Color.Value.ToString(); ;
                Binding_Data.WordBorderColor = Color.Value;
            }
        }

        private void MenuItem_MouseUpChangeWordForegroundColor(object sender, System.Windows.RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<System.Windows.Media.Brush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);

            System.Windows.FrameworkElement K = this;
            while (K.Parent != null)
            {
                K = (System.Windows.FrameworkElement)K.Parent;
            }

            MainClass.OpenWindow(WindowChoiceColor, (System.Windows.Window)K);
            if (Color.Value != null)
            {
                Word_.Value.ForegroundColor = Color.Value.ToString(); ;
                Binding_Data.WordForegroundColor = Color.Value;
            }
        }

        private void MenuItem_MouseUpNewWordMark(object sender, System.Windows.RoutedEventArgs e)
        {
            Word_.Value.NewWordMark();
            Word_.Value.MarkReview();
            NoticeReview();
            ReviewFinished();
        }

        private void MenuItem_MouseUpShowRelatedWords(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!KeepFilterWords())
            {
                List<string> RealtedWords = new List<string>();
                foreach (var l in Word.Meanings)
                {
                    foreach (var l1 in l.ReturnAntonymsList().Keys)
                    {
                        RealtedWords.Add(l1);
                    }
                    foreach (var l1 in l.ReturnConfusedList().Values)
                    {
                        RealtedWords.Add(l1);
                    }
                    foreach (var l1 in l.ReturnDerivativesList().Values)
                    {
                        RealtedWords.Add(l1);
                    }
                    foreach (var l1 in l.ReturnSynonymsList().Values)
                    {
                        RealtedWords.Add(l1);
                    }
                }
                foreach (UserControlWordMetro l in ((WrapPanel)Parent).Children)
                {
                    if (RealtedWords.Contains(l.Word.Spelling))
                    {
                        l.TurnFront();
                    }
                    else
                    {
                        l.TurnBack();
                    }
                }
                TurnFront();
            }
        }

        public void ReviewFinished()
        {
            if (MenuItemViewedMark.IsEnabled)
            {
                Binding_Data.Height = (Binding_Data.Height - 4) / 2;
                MenuItemViewedMark.IsEnabled = false;
                Binding_Data.Level = Word.ViewLevel.ToString();
                SetTiptool();
            }
        }

        public void NoticeReview()
        {
            if (!MenuItemViewedMark.IsEnabled)
            {
                Binding_Data.Height = Binding_Data.Height * 2 + 4;
                MenuItemViewedMark.IsEnabled = true;
                SetTiptool();
            }
        }

        public void TurnBack()
        {
            Binding_Data.Enable = false;
            Word_.Value.Visible = Binding_Data.Enable;
            if (FatherBindingData.CollapseRotatedWordPanel)
            {
                Binding_Data.ControlHeight = 0;
            }
            else
            {
                Binding_Data.ControlHeight = double.NaN;
            }
        }

        public void TurnFront()
        {
            Binding_Data.Enable = true;
            Word_.Value.Visible = Binding_Data.Enable;
            Binding_Data.ControlHeight = double.NaN;
        }

        public bool IsFaceFront
        {
            get
            {
                return Word_.Value.Visible;
            }
        }

        private void Got_Focus(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (UserControlWordMetro l in ((WrapPanel)Parent).Children)
            {
                l.Lost_Focus();
            }
            Binding_Data.SelectedBorderColor = Brushes.RoyalBlue;
        }

        public void Lost_Focus()
        {
            if (Binding_Data.SelectedBorderColor != Brushes.Transparent)
            {
                Binding_Data.SelectedBorderColor = Brushes.Transparent;
            }
        }

        private List<string> ReturnRelatedWords()
        {
            List<string> RealtedWords = new List<string>();
            foreach (var l in Word.Meanings)
            {
                foreach (var l1 in l.ReturnAntonymsList().Keys)
                {
                    RealtedWords.Add(l1);
                }
                foreach (var l1 in l.ReturnConfusedList().Values)
                {
                    RealtedWords.Add(l1);
                }
                foreach (var l1 in l.ReturnDerivativesList().Values)
                {
                    RealtedWords.Add(l1);
                }
                foreach (var l1 in l.ReturnSynonymsList().Values)
                {
                    RealtedWords.Add(l1);
                }
            }
            return RealtedWords;
            foreach (UserControlWordMetro l in ((WrapPanel)Parent).Children)
            {
                if (RealtedWords.Contains(l.Word.Spelling))
                {
                    l.SetTiptool();
                }

            }
        }

        private void SyncOtherWords(List<string> Words)
        {
            foreach (UserControlWordMetro l in ((WrapPanel)Parent).Children)
            {
                if (Words.Contains(l.Word.Spelling))
                {
                    l.SetTiptool();
                }

            }
        }
    }
}
