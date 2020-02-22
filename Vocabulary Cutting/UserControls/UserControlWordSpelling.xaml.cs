using System;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Diagnostics;

namespace WPF
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControlWordCardSpelling : FrameworkElement
    {
        public UserControlWordCardSpelling()
        {
            InitializeComponent();
            // 必须加入到VisualTree中才能显示
            this.AddVisualChild(_drawingVisual);
        }

        private DrawingVisual _drawingVisual = new DrawingVisual();

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

        // 绘制代码
        private string WordSpell = null;
        private int Index = 0;
        public void Draw(string Text, int Index_)
        {
            if(Text == null)
            {
                return;
            }
            WordSpell = Text;
            Index = Index_;
            if (ActualWidth >= 4)
            {
                var dc = _drawingVisual.RenderOpen();
                dc.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, ActualWidth, ActualHeight));
                dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(2, 2, ActualWidth - 4, ActualHeight - 4));

                var TempMeasureFormattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 22, Brushes.Red);
                double TempHeight = (ActualHeight - TempMeasureFormattedText.Height) / 2;
                double TempWidth = (ActualWidth - TempMeasureFormattedText.Width) / 2;

                TempMeasureFormattedText = new FormattedText(
                    "A",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    22,
                    Brushes.Red);
                double TempSpaceWidth = TempMeasureFormattedText.Width;

                for (int l = 0; l < Text.Length; l++)
                {
                    Brush BrushColor;
                    if (Index_ == l)
                    {
                        BrushColor = Brushes.Red;
                    }
                    else if (Index_ > l)
                    {
                        BrushColor = Brushes.Green;
                    }
                    else
                    {
                        BrushColor = Brushes.Gray;
                    }
                    if (Text[l] == ' ')
                    {
                        TempWidth += TempSpaceWidth;
                    }
                    else
                    {
                        var FormattedText = new FormattedText(
                        Text[l].ToString(),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        22,
                        BrushColor);
                        dc.DrawText(FormattedText, new Point(TempWidth, TempHeight));
                        TempWidth += FormattedText.Width;
                    }
                }
                dc.Close();
            }
        }

        private void FrameworkElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WordSpell != null)
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    GooglePictures.SearchPictures(WordSpell);
                }
                else if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    MainClass.PlaySoundPath(MainWindow.WordsPackage + WordSpell[0].ToString() + "\\" + WordSpell + "\\" + "Pronunciation.kl", false);
                }
            }
        }

        private void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw(WordSpell, Index);
        }
    }
}
