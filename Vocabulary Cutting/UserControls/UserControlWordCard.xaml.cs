using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Threading;
using System.Text;


namespace WPF
{
    /// <summary>
    /// Interaction logic for UserControlWordCard.xaml
    /// </summary>
    public partial class UserControlWordCard : FrameworkElement
    {
        private class BindingData : INotifyPropertyChanged
        {
            public BindingData()
            { }

            public event PropertyChangedEventHandler PropertyChanged;

            private double Height_ = 70;
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

            private Visibility Visibility_ = Visibility.Visible;
            public Visibility Visibility
            {
                get
                {
                    return Visibility_;
                }
                set
                {
                    Visibility_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Visibility"));
                    }
                }
            }

            private string ToolTip_ = "";
            public string ToolTip
            {
                get
                {
                    return ToolTip_;
                }
                set
                {
                    ToolTip_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTip"));
                    }
                }
            }

            private bool ToolTipIsEnable_ = false;
            public bool ToolTipIsEnable
            {
                get
                {
                    return ToolTipIsEnable_;
                }
                set
                {
                    ToolTipIsEnable_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTipIsEnable"));
                    }
                }
            }

            private double Opacity_ = 1;
            public double Opacity
            {
                get
                {
                    return Opacity_;
                }
                set
                {
                    Opacity_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Opacity"));
                    }
                }
            }
        }

        private readonly BindingData Binding_Data;

        private readonly DrawingVisual _drawingVisual = new DrawingVisual();

        public UserControlWordCard(
            WordListClass.WordClass InputWord,
            WordListClass InputWordList,
            MainWindow InputFather)
        {
            InitializeComponent();
            // 必须加入到VisualTree中才能显示
            this.AddVisualChild(_drawingVisual);
            Binding_Data = new BindingData();
            MainClass.BindingData("Height", Binding_Data, this, FrameworkElement.HeightProperty);
            MainClass.BindingData("ToolTip", Binding_Data, this, FrameworkElement.ToolTipProperty);
            MainClass.BindingData("ToolTipIsEnable", Binding_Data, this, ToolTipService.IsEnabledProperty);
            MainClass.BindingData("Visibility", Binding_Data, this, FrameworkElement.VisibilityProperty);
            MainClass.BindingData("Opacity", Binding_Data, this, FrameworkElement.OpacityProperty);
            Binding_Data.Height = 142;
            Binding_Data.Opacity = ((double)InputWord.Opacity) / 100.0;
            WordList = InputWordList;
            Word = InputWord;
            Father = InputFather;
            Father.WordsControls.Add(this);
            DrawRefresh();
        }

        // 重载自己的VisualTree的孩子的个数，由于只有一个DrawingVisual，返回1
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        // 重载当WPF框架向自己要孩子的时候，返回返回DrawingVisual
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return _drawingVisual;
            }
            throw new IndexOutOfRangeException();
        }

        public WordListClass.WordClass Word;

        private readonly MainWindow Father;

        private readonly WordListClass WordList;

        private bool ReDraw = false;
        public new bool IsVisible
        {
            get
            {
                return Binding_Data.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (Binding_Data.Visibility != Visibility.Visible)
                    {
                        Binding_Data.Visibility = Visibility.Visible;
                    }
                    if (Word.Visible != LastVisible ||
                        Word.NeedReview != LastNeedReview ||
                        LastForegroundColor != Word.ForegroundColor ||
                        LastBorderColor != Word.BorderColor ||
                        LastBackgroundColor != Word.BackgroundColor ||
                        LastReviewLevel != Word.ReviewLevel ||
                        ReDraw)
                    {
                        DrawRefresh();
                        ReDraw = false;
                    }
                }
                else
                {
                    if (Binding_Data.Visibility != Visibility.Collapsed)
                    {
                        ReDraw = true;
                        Binding_Data.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public string Tiptool
        {
            get
            {
                StringBuilder Information = new StringBuilder();
                if (Word.Tag != null)
                {
                    Information.AppendLine(string.Format("[Tag: {0}]", Word.Tag));
                }
                var ReturnMeanings = Word.Meanings;
                if (ReturnMeanings.Count > 0)
                {
                    Information.AppendLine("[Meanings]");
                    if (ReturnMeanings.Count > 0)
                    {
                        for (int i1 = 0; i1 < ReturnMeanings.Count; i1++)
                        {
                            Information.AppendLine((i1 + 1) + ". " + ReturnMeanings[i1]);
                        }
                    }
                    Information.AppendLine();
                }

                var ReturnCategorys = Word.Categorys;
                if (ReturnCategorys.Count > 0)
                {
                    Information.AppendLine("[Categorys]");
                    if (ReturnCategorys.Count > 0)
                    {
                        int Index = 0;

                        for (int i1 = 0; i1 < ReturnCategorys.Count; i1++)
                        {
                            Index = 0;
                            Information.AppendLine((i1 + 1) + ". " + ReturnCategorys[i1]);
                            foreach (var m in MainPlatomEntrance.WindowList)
                            {
                                foreach (var k in m.WordsList.Words)
                                {
                                    if (k.Spelling != Word.Spelling && k.Categorys.Contains(ReturnCategorys[i1]))
                                    {
                                        Information.AppendLine(string.Format("{0}({1}) {2}", "  ", (++Index).ToString(), k.Spelling));
                                    }
                                }
                            }
                            Information.AppendLine();
                        }
                    }
                    Information.AppendLine();
                }
                var String = Information.ToString();
                while (String.EndsWith("\r\n"))
                {
                    String = String.Substring(0, String.Length - 2);
                }
                return String;
            }
        }
        private void SetTiptool(bool Clear)
        {
            if (!Clear)
            {
                Binding_Data.ToolTip = Tiptool;
                Binding_Data.ToolTipIsEnable = true;
            }
            else
            {
                Binding_Data.ToolTip = "";
                Binding_Data.ToolTipIsEnable = false;
            }
        }

        public void LabelWord_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                MainClass.PlaySoundPath(MainWindow.WordsPackage + Word.Spelling[0].ToString() + "\\" + Word.Spelling + "\\" + "Pronunciation.kl", false);
            }
            else if(e.ChangedButton == System.Windows.Input.MouseButton.Middle)
            {
                Clipboard.SetDataObject(Word.Spelling);
            }
        }

        private bool? OpacityGoal = null;
        public void SwitchOpacity(bool? UpDown)
        {
            if (Word.Opacity > 100)
            {
                Word.Opacity = 100;
            }
            switch (UpDown)
            {
                case true:
                    if (Word.Opacity < 100)
                    {
                        Word.Opacity++;
                        Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                    }
                    OpacityGoal = null;
                    break;
                case false:
                    if (Word.Opacity > 0)
                    {
                        Word.Opacity--;
                        Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                    }
                    OpacityGoal = null;
                    break;
                case null:
                    if (OpacityGoal == null)
                    {
                        Word.Opacity = (uint)new Random(WordList.Words.IndexOf(Word)).Next(0, 100);
                        Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                        OpacityGoal = true;
                    }
                    if (OpacityGoal == true)
                    {
                        if (Word.Opacity < 100)
                        {
                            Word.Opacity++;
                            Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                        }
                        else
                        {
                            OpacityGoal = false;
                        }
                    }
                    else
                    {
                        if (Word.Opacity > 0)
                        {
                            Word.Opacity--;
                            Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                        }
                        else
                        {
                            OpacityGoal = true;
                        }
                    }
                    break;
            }
            Father.SortWord(this);
            Father.FilterWord(this);
        }
        public void TextBlock_MouseUpDeleteWord()
        {
            WordList.DeleteWord(ref Word);
            var TempParent = ((WrapPanel)Parent);
            TempParent.Children.Remove(this);
            Father.WordsControls.Remove(this);
        }

        public void TextBlock_MouseUpHideShowWord()
        {
            Word.Visible = !Word.Visible;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void TextBlock_MouseUpViewedMark()
        {
            Word.MarkReview();
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void TextBlock_MouseUpMaxViewedMark()
        {
            Word.MarkMaxReview();
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpChangeWordBackgroundColor(SolidColorBrush Color)
        {
            Word.BackgroundColor = Color.ToString(); ;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpChangeWordBorderColor(SolidColorBrush Color)
        {
            Word.BorderColor = Color.ToString(); ;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpChangeWordForegroundColor(SolidColorBrush Color)
        {
            Word.ForegroundColor = Color.ToString(); ;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpNewWordMark()
        {
            Word.NewWordMark();
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpStrongReview()
        {
            Word.StrongReview = true;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        public void MenuItem_MouseUpCancelStrongReview()
        {
            Word.StrongReview = false;
            DrawRefresh();
            Father.SortWord(this);
            Father.FilterWord(this);
        }

        private bool LastVisible = false;
        private bool LastNeedReview = false;
        private string LastForegroundColor = null;
        private string LastBorderColor = null;
        private string LastBackgroundColor = null;
        private uint LastReviewLevel = 0;

        private static int DrawRefreshCount = 0;
        private void DrawRefresh()
        {
            LastVisible = Word.Visible;
            LastNeedReview = Word.NeedReview;
            LastForegroundColor = Word.ForegroundColor;
            LastBorderColor = Word.BorderColor;
            LastBackgroundColor = Word.BackgroundColor;
            LastReviewLevel = Word.ReviewLevel;

            int Height = 0;
            if (!Word.NeedReview)
            {
                Height = 142/ 2;
                Binding_Data.Height = Height;
            }
            else
            {
                Height = 142;
                Binding_Data.Height = Height;
            }
            (this).GetBindingExpression(UserControl.HeightProperty).UpdateSource();
            Action Action = new System.Action(() =>
            {
                DrawRefreshCount++;
                if (Binding_Data.Visibility == Visibility.Visible)
                {
                    var dc = _drawingVisual.RenderOpen();
                    Brush ForegroundColor = (Brush)TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.ForegroundColor);
                    Brush BorderColor = (Brush)TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.BorderColor);
                    Brush BackgroundColor = (Brush)TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString(Word.BackgroundColor);

                    if (Focus)
                    {
                        dc.DrawRectangle(Brushes.RoyalBlue, null, new Rect(0, 0, Width, Height));
                    }
                    if (Word.Visible)
                    {
                        dc.DrawRectangle(BorderColor, null, new Rect(3, 3, Width - 6, Height - 6));
                        dc.DrawRectangle(BackgroundColor, null, new Rect(8, 8, Width - 16, Height - 16));
                        FormattedText FormattedText = null;
                        Typeface Typeface = new Typeface("Verdana");
                        if (Word.StrongReview)
                        {
                            FormattedText = new FormattedText(
                                Word.ReviewLevel.ToString(),
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                Typeface,
                                14,
                                Brushes.Orange);
                        }
                        else
                        {
                            if (Word.NeedReview)
                            {
                                FormattedText = new FormattedText(
                                    Word.ReviewLevel.ToString(),
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    Typeface,
                                    14,
                                    Brushes.Red);
                            }
                            else
                            {
                                FormattedText = new FormattedText(
                                    Word.ReviewLevel.ToString(),
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    Typeface,
                                    12,
                                    ForegroundColor);
                            }
                        }
                        dc.DrawText(FormattedText, new Point(10, 10));
                        FormattedText = new FormattedText(
                                Word.Spelling,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                Typeface,
                                14,
                                ForegroundColor);
                        dc.DrawText(FormattedText,
                                new Point(Width / 2 - FormattedText.Width / 2, Height / 2 - FormattedText.Height / 2));
                    }
                    else
                    {
                        dc.DrawRectangle(Brushes.Gray, null, new Rect(3, 3, Width - 6, Height - 6));
                    }
                    dc.Close();
                }
                DrawRefreshCount--;
            });
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, Action);
        }

        public static void WaitDrawRefresh()
        {
            while(DrawRefreshCount!=0)
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        private bool Focus_ = false;
        public new bool Focus
        {
            get
            {
                return Focus_;
            }
            set
            {
                if (value != Focus_)
                {
                    Focus_ = value;
                    DrawRefresh();
                }
            }
        }

        public static bool HiddenTiptool = false;
        private void FrameworkElement_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (HiddenTiptool == false)
            {
                SetTiptool(false);
            }
        }

        private void FrameworkElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetTiptool(true);
        }

        private bool KeyDownFinished = true;
        public bool FrameworkElement_KeyDown(bool Multi, System.Windows.Input.KeyEventArgs e)
        {
            if (KeyDownFinished)
            {
                KeyDownFinished = false;
                if (Focus)
                {
                    switch (e.Key)
                    {
                        case System.Windows.Input.Key.K:
                            if (Word.NeedReview)
                            {
                                if (!Multi)
                                {
                                    Binding_Data.Opacity = 1;
                                    for (int i = 0; i < 80 && Word.NeedReview; i++)
                                    {
                                        Binding_Data.Opacity = 1.0 - i * 0.01;
                                        MainClass.DoEvents();
                                        System.Threading.Thread.Sleep(5);
                                    }
                                }
                                Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                                TextBlock_MouseUpViewedMark();
                            }
                            goto TRUE;
                        case System.Windows.Input.Key.F:
                            if (!Multi)
                            {
                                Binding_Data.Opacity = 0;
                                for (int i = 0; i < 80 && (!(Word.NeedReview == false && Word.ReviewLevel == 1)); i++)
                                {
                                    Binding_Data.Opacity = i * 0.01;
                                    MainClass.DoEvents();
                                    System.Threading.Thread.Sleep(5);
                                }
                            }
                            Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                            MenuItem_MouseUpNewWordMark();
                            goto TRUE;
                        case System.Windows.Input.Key.R:
                            if (!Multi)
                            {
                                Binding_Data.Opacity = 0;
                                for (int i = 0; i < 80 && (!(Word.NeedReview == false && Word.ReviewLevel == 1)); i++)
                                {
                                    Binding_Data.Opacity = i * 0.01;
                                    MainClass.DoEvents();
                                    System.Threading.Thread.Sleep(5);
                                }
                            }
                            Binding_Data.Opacity = ((double)Word.Opacity) / 100.0;
                            MenuItem_MouseUpNewWordMark();
                            TextBlock_MouseUpViewedMark();
                            goto TRUE;
                        case System.Windows.Input.Key.Space:
                            TextBlock_MouseUpHideShowWord();
                            goto TRUE;
                    }
                }
            }
            KeyDownFinished = true;
            return false;
        TRUE:
            KeyDownFinished = true;
            return true;
        }
    }
}
