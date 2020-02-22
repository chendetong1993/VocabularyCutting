using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Net;
using System.Windows.Media;
using System.Text;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowVocabularyReview.xaml
    /// </summary>
    public partial class WindowVocabularyReview : Window
    {
        private readonly List<WordListClass.WordClass> Word;

        private class BindingData : INotifyPropertyChanged
        {
            public BindingData(Action<string, int> InputAction)
            {
                Word_ = "";
                Action = InputAction;
                CorrentSpelling_ = "";
            }

            private readonly Action<string, int> Action;

            private string CorrentSpelling_;
            public string CorrentSpelling
            {
                get
                {
                    return CorrentSpelling_;
                }
                set
                {
                    CorrentSpelling_ = value;
                    Word = "";
                }
            }

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
                    int Index = 0;
                    for (int l = 0; l < CorrentSpelling.Length && l < Word_.Length; l++)
                    {
                        if (CorrentSpelling[l] == Word_[l])
                        {
                            Index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (CorrentSpelling == Word_)
                    {
                        MainClass.PlaySoundPath(MainWindow.WordsPackage + CorrentSpelling[0].ToString() + "\\" + CorrentSpelling + "\\" + "Pronunciation.kl", false);
                    }
                    Action(CorrentSpelling, Index);
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Word"));
                    }
                }
            }
        }

        private readonly BindingData Binding_Data;

        private MainWindow Father = null;
        public WindowVocabularyReview(List<WordListClass.WordClass> InputWord, MainWindow Father_)
        {
            InitializeComponent();
            Binding_Data = new BindingData(new Action<string, int>(WordSpelling.Draw));
            MainClass.BindingData("Word", Binding_Data, TextBoxSpelling, TextBox.TextProperty);
            Word = InputWord;
            Father = Father_;
        }

        private int Index = 0;
        private void Reload()
        {
            ListBoxExamples.SelectedIndex = -1;
            const string Space = "   ";
            Title = "Vocabulary Review[" + (Index + 1) + " - " + Word.Count + "]";
            StringBuilder Meanings = new StringBuilder();

            int Count = 0, CountM = 0;
            var Categorys = Word[Index].Categorys;
            Meanings.AppendLine("Categorys [" + Categorys.Count + "]:");
            foreach (var h in Categorys)
            {
                Meanings.AppendLine(string.Format("{0}[{1}] {2}", Space, (++Count).ToString(), h));
                CountM = 0;
                foreach (var m in MainPlatomEntrance.WindowList)
                {
                    foreach (var k in m.WordsList.Words)
                    {
                        if (k.Spelling != Word[Index].Spelling && k.Categorys.Contains(h))
                        {
                            Meanings.AppendLine(string.Format("{0}({1}) {2}", Space + Space, (++CountM).ToString(), k.Spelling));
                        }
                    }
                }
            }
            Meanings.AppendLine();

            Count = 0;
            var ReturnMeanings = Word[Index].Meanings;
            Meanings.AppendLine("Meanings [" + ReturnMeanings.Count + "]: ");
            foreach (var h in ReturnMeanings)
            {
                Meanings.AppendLine(string.Format("{0}[{1}] {2}", Space, (++Count).ToString(), h));
            }
            Meanings.AppendLine();

            Meanings.AppendLine(string.Format("Review Level:\n{0}   >>   [{1}]\n", Space + Word[Index].ReviewLevel, MainClass.ReturnVagueTimeString(new TimeSpan(0, 0, WordListClass.WordClass.ReviewLevels[Word[Index].ReviewLevel], 0))));
            Meanings.AppendLine(string.Format("Created Time:\n{0}\n", Space + Word[Index].CreatedTime.ToString()));
            Meanings.AppendLine(string.Format("Next Review Time:\n{0}\n", Space + Word[Index].NextReviewTime.ToString()));
            Meanings.AppendLine(string.Format("Background Color:\n{0}\n", Space + Word[Index].BackgroundColor));
            Meanings.AppendLine(string.Format("Border Color:\n{0}\n", Space + Word[Index].BorderColor));
            Meanings.AppendLine(string.Format("Foreground Color:\n{0}\n", Space + Word[Index].ForegroundColor));

            Meanings.AppendLine(string.Format("Need Review:\n{0}\n", Space + Word[Index].NeedReview));
            Meanings.AppendLine(string.Format("Opacity:\n{0}\n", Space + Word[Index].Opacity));
            Meanings.AppendLine(string.Format("Visible:\n{0}\n", Space + Word[Index].Visible));
            TextBoxWord.Text = Meanings.ToString();

            Binding_Data.CorrentSpelling = Word[Index].Spelling;
            MainClass.PlaySoundPath(MainWindow.WordsPackage + Word[Index].Spelling[0].ToString() + "\\" + Word[Index].Spelling + "\\" + "Pronunciation.kl", false);
            MainPlatomEntrance.SetNotify("Review a word", 2, Father);

            ReloadPictures();
            ReloadExamples();
        }

        private void Button_ClickCheck(object sender, RoutedEventArgs e)
        {
            if (Word[Index].Spelling != Binding_Data.Word)
            {
                MainPlatomEntrance.SetNotify("Spelling is incorrent", 2, Owner);
            }
            else
            {
                Index++;
                if (Index >= Word.Count)
                {
                    MainPlatomEntrance.SetNotify("Reviewed Finished", 2, Owner);
                    Close();
                }
                else
                {
                    Reload();
                }
            }
        }

        private void Button_ClickSkipBack(object sender, RoutedEventArgs e)
        {
            Index++;
            if (Index >= Word.Count)
            {
                MainPlatomEntrance.SetNotify("Reviewed Finished", 2, Owner);
                Close();
            }
            else
            {
                Reload();
            }
        }

        private void Button_ClickSkipFront(object sender, RoutedEventArgs e)
        {
            if (Index > 0)
            {
                Index--;
                Reload();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();

        }

        private void StackPanelPcitures_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void ReloadPictures()
        {
            double Scroll = ScrollViewerPcitures.HorizontalOffset;
            StackPanelPcitures.Children.Clear();
            List<string> WordPaths = new List<string>();
            foreach (var fi in Father.Pictures.Read(Word[Index].Spelling))
            {
                UserControl Image = new UserControl();
                Image.Margin = new Thickness(2);
                Image.BorderThickness = new Thickness(2);
                Image.BorderBrush = Brushes.Blue;
                Image.Height = StackPanelPcitures.ActualHeight - 10;
                Image.Width = Image.Height;
                Image.Background = MainClass.OpenPicture(fi);
                StackPanelPcitures.Children.Add(Image);
            }
            ScrollViewerPcitures.ScrollToHorizontalOffset(Scroll);
        }


        private DateTime DateTime = DateTime.Now;
        private void StackPanelPcitures_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                Process.Start(MainWindow.WordsPackage + Word[Index].Spelling[0].ToString() + "\\" + Word[Index].Spelling);
                return;
            }
            if ((DateTime.Now - DateTime).TotalMilliseconds < 300)
            {
                int m = 0;
                foreach (Control i in StackPanelPcitures.Children)
                {
                    if (MainClass.MouseInControl(i))
                    {
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            Father.Pictures.Delete(m, Word[Index].Spelling);
                            ReloadPictures();
                        }
                        else if (e.ChangedButton == MouseButton.Right)
                        {
                            Father.Pictures.Clear(Word[Index].Spelling);
                            ReloadPictures();
                        }
                        break;
                    }
                    m++;
                }
            }
            DateTime = DateTime.Now;
        }

        private void StackPanelPcitures_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    foreach (var Path in (string[])e.Data.GetData(DataFormats.FileDrop))
                    {
                        var Reads = File.ReadAllBytes(Path);
                        Reads = MainClass.ScaleImage(Reads);
                        if (Reads != null)
                        {
                            Father.Pictures.Add(Word[Index].Spelling, Reads);
                        }
                    }
                    ReloadPictures();
                }
                else if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    var Website = (string)e.Data.GetData(DataFormats.Text);
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            var Reads = client.DownloadData(Website);
                            Reads = MainClass.ScaleImage(Reads);
                            if (Reads != null)
                            {
                                Father.Pictures.Add(Word[Index].Spelling, Reads);
                            }
                        }
                        catch { }
                    }
                    ReloadPictures();
                }
            }
            catch { }
        }

        private void ListBoxExamples_MouseDown(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxExamples.SelectedIndex == -1)
            {
                return;
            }
            string WordSpelling = ((ListBoxItem)ListBoxExamples.SelectedItem).Content.ToString();
            InternetSpeakeRecognize.Speak(WordSpelling);
        }

        private void ListBoxExamples_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxExamples.SelectedIndex == -1)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                Father.Examples.Delete(ListBoxExamples.SelectedIndex, Word[Index].Spelling);
                ReloadExamples();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                Father.Examples.Clear(Word[Index].Spelling);
                ReloadExamples();
            }
        }

        private void ListBoxExamples_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void ListBoxExamples_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    foreach (var Path in (string[])e.Data.GetData(DataFormats.FileDrop))
                    {
                        Father.Examples.Add(Word[Index].Spelling, File.ReadAllBytes(Path));
                    }
                    ReloadExamples();
                }
                else if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    var Website = (string)e.Data.GetData(DataFormats.Text);
                    Father.Examples.Add(Word[Index].Spelling, System.Text.Encoding.Default.GetBytes(Website));
                    ReloadExamples();
                }
            }
            catch { }
        }

        private void ReloadExamples()
        {
            int SIndex = ListBoxExamples.SelectedIndex;
            ListBoxExamples.Items.Clear();

            foreach (var fi in Father.Examples.Read(Word[Index].Spelling))
            {
                ListBoxItem ListBoxItem = new ListBoxItem();
                ListBoxItem.FontSize = 14;
                ListBoxItem.Content = Encoding.UTF8.GetString(fi);

                ListBoxItem.Margin = new Thickness(2);
                ListBoxItem.BorderThickness = new Thickness(2);
                ListBoxItem.BorderBrush = Brushes.Blue;
                ListBoxItem.Background = Brushes.WhiteSmoke;

                ListBoxExamples.Items.Add(ListBoxItem);
            }
            ListBoxExamples.SelectedIndex = SIndex;
        }
    }
}
