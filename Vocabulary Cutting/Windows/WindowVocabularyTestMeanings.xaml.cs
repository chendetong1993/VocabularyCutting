using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Input;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowVocabularyTestMeanings.xaml
    /// </summary>
    public partial class WindowVocabularyTestMeanings : Window
    {
        public class WordStruct
        {
            public WordStruct(string Spell_, string Meanings_, ImageBrush Image_)
            {
                Spell = Spell_;
                Meanings = Meanings_;
                Image = Image_;
            }

            public string Spell { get; set; }
            public string Meanings { get; set; }
            public ImageBrush Image { get; set; }
        }

        private readonly List<UserControlWordCard> NeedReviewWordsList;
        private readonly WordListClass WordsList;
        private UserControlWordCard CorrectSpelling = null;
        private const int MultiItemsCount = 4;
        private List<MainClass.TupleC<int, List<ImageBrush>>> CorrectPicture = new List<MainClass.TupleC<int, List<ImageBrush>>>();

        private MainWindow Father = null;

        public WindowVocabularyTestMeanings(List<UserControlWordCard> InputNeedReviewWordsList, WordListClass InputWordsList,MainWindow Father_)
        {
            InitializeComponent();
            Father = Father_;
            if (InputWordsList.Words.Count < MultiItemsCount == false)
            {
                NeedReviewWordsList = new List<UserControlWordCard>(InputNeedReviewWordsList);
                WordsList = InputWordsList;
                WordCounts = NeedReviewWordsList.Count;
                WordIndex = 0;
                Reload();
            }
        }

        private List<ImageBrush> ReloadPicture(string Spelling)
        {
            List<ImageBrush> Return = new List<ImageBrush>();
            foreach (var fi in Father.Pictures.Read(Spelling))
            {
                try
                {
                    Return.Add((MainClass.OpenPicture(fi)));
                }
                catch { }
            }
            return Return;
        }

        private string GetMeanings(List<string> Meanings)
        {
            string Meaning = "";
            uint i = 0;
            foreach (var k1 in Meanings)
            {
                if (i != 0)
                {
                    Meaning += k1 + "\n";
                }
                i++;
            }
            return Meaning;
        }

        private int WordIndex = 0;
        private int WordCounts = 0;
        private void Reload()
        {
            if (0 == NeedReviewWordsList.Count)
            {
                MainPlatomEntrance.SetNotify("Review and test finished!", 2, Father);
                Close();
                return;
            }
            ListBoxMeanings.SelectedIndex = -1;
            CorrectSpelling = null;
            ListBoxMeanings.Items.Clear();
            CorrectPicture.Clear();
            TextBoxMainWordSpelling.Text = "";

            Random TempRandom = new Random(DateTime.Now.Millisecond);
            var TotalWordsLists = new List<WordListClass.WordClass>(WordsList.Words);

            int Index = TempRandom.Next(0, NeedReviewWordsList.Count - 1);
            TotalWordsLists.Remove(NeedReviewWordsList[Index].Word);

            for (int i = 0; i < MultiItemsCount - 1; i++)
            {
                System.Threading.Thread.Sleep(20);
                int IndexTemp = TempRandom.Next(0, TotalWordsLists.Count - 1);
                var M = ReloadPicture(TotalWordsLists[IndexTemp].Spelling);
                CorrectPicture.Add(new MainClass.TupleC<int, List<ImageBrush>>(0, M));
                if (M.Count > 0)
                {
                    ListBoxMeanings.Items.Add(new WordStruct(TotalWordsLists[IndexTemp].Spelling, GetMeanings(TotalWordsLists[IndexTemp].Meanings), M[0]));
                }
                else
                {
                    ListBoxMeanings.Items.Add(new WordStruct(TotalWordsLists[IndexTemp].Spelling, GetMeanings(TotalWordsLists[IndexTemp].Meanings), null));
                }
                TotalWordsLists.RemoveAt(IndexTemp);
            }
            {
                CorrectSpelling = NeedReviewWordsList[Index];

                var IndexR = (TempRandom.Next(0, ListBoxMeanings.Items.Count * 40) + 9) / 40;
                var M = ReloadPicture(NeedReviewWordsList[Index].Word.Spelling);
                CorrectPicture.Insert(IndexR, new MainClass.TupleC<int, List<ImageBrush>>(0, M));
                if (M.Count > 0)
                {
                    ListBoxMeanings.Items.Insert(IndexR, new WordStruct(NeedReviewWordsList[Index].Word.Spelling, GetMeanings(NeedReviewWordsList[Index].Word.Meanings), M[0]));
                }
                else
                {
                    ListBoxMeanings.Items.Insert(IndexR, new WordStruct(NeedReviewWordsList[Index].Word.Spelling, GetMeanings(NeedReviewWordsList[Index].Word.Meanings), null));
                }
                NeedReviewWordsList.RemoveAt(Index);
            }
            ListBoxMeanings.Items.Refresh();
            Title = "Vocabulary Test[" + ((WordIndex++) + 1) + " - " + WordCounts + "]";
            MainPlatomEntrance.SetNotify("Test a word", 2, Father);
            string WordSpelling = CorrectSpelling.Word.Spelling;
            MainClass.PlaySoundPath(MainWindow.WordsPackage + WordSpelling[0].ToString() + "\\" + WordSpelling + "\\" + "Pronunciation.kl", false);
            if(CheckBoxMainWordSpelling.IsChecked == true)
            {
                TextBoxMainWordSpelling.Text = CorrectSpelling.Word.Spelling;
            }
        }

        private void Button_ClickPronounce(object sender, RoutedEventArgs e)
        {
            string WordSpelling = ((Button)sender).Tag.ToString();
            MainClass.PlaySoundPath(MainWindow.WordsPackage + WordSpelling[0].ToString() + "\\" + WordSpelling + "\\" + "Pronunciation.kl", false);
        }

        private void Button_ClickCheck(object sender, RoutedEventArgs e)
        {
            if (ListBoxMeanings.SelectedIndex != -1 &&
                TextBoxMainWordSpelling.Text == CorrectSpelling.Word.Spelling &&
                ((WordStruct)ListBoxMeanings.Items[ListBoxMeanings.SelectedIndex]).Spell == CorrectSpelling.Word.Spelling)
            {
                CorrectSpelling.Word.MarkReview();
                Father.SortWord(CorrectSpelling);
                Father.FilterWord(CorrectSpelling);
                Reload();
            }
            else
            {
                MainPlatomEntrance.SetNotify("The answer is incorrect!", 2, Owner);
            }
        }

        private void Button_ClickReview(object sender, RoutedEventArgs e)
        {
            CorrectSpelling.Word.NewWordMark();
            Father.SortWord(CorrectSpelling);
            Father.FilterWord(CorrectSpelling);
            Reload();
        }

        private void Button_ClickSkip(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void Button_ClickPronounceMainWord(object sender, System.Windows.RoutedEventArgs e)
        {
            string Spelling = CorrectSpelling.Word.Spelling;
            MainClass.PlaySoundPath(MainWindow.WordsPackage + Spelling[0].ToString() + "\\" + Spelling + "\\" + "Pronunciation.kl", false);
        }

        private void Picture_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (ListBoxMeanings.SelectedIndex != -1 &&
                    CorrectPicture[ListBoxMeanings.SelectedIndex].Item2.Count > 0)
                {
                    CorrectPicture[ListBoxMeanings.SelectedIndex].Item1++;
                    if (CorrectPicture[ListBoxMeanings.SelectedIndex].Item1 >= CorrectPicture[ListBoxMeanings.SelectedIndex].Item2.Count)
                    {
                        CorrectPicture[ListBoxMeanings.SelectedIndex].Item1 = 0;
                    }
                    ((WordStruct)ListBoxMeanings.Items[ListBoxMeanings.SelectedIndex]).Image = CorrectPicture[ListBoxMeanings.SelectedIndex].Item2[CorrectPicture[ListBoxMeanings.SelectedIndex].Item1];
                    ListBoxMeanings.Items.Refresh();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WordsList.Words.Count < MultiItemsCount)
            {
                MainPlatomEntrance.SetNotify("No enough words to start a test", 2, this);
                Close();
            }
        }

        private void Meanings_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (ListBoxMeanings.SelectedIndex != -1)
                {
                    var Words = new List<WordListClass.WordClass>();
                    Words.Add(WordsList[((WordStruct)ListBoxMeanings.Items[ListBoxMeanings.SelectedIndex]).Spell]);
                    var WindowVocabularyReview = new WindowVocabularyReview(Words, Father);
                    MainClass.OpenWindow(WindowVocabularyReview, this, true);
                }
            }
        }

        private void PictureLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Button_ClickCheck(null, null);
            }
        }

        private void CheckBoxMainWordSpelling_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxMainWordSpelling.Text = CorrectSpelling.Word.Spelling;
        }

        private bool KeyDownFinished = true;
        private void ListBoxMeanings_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyDownFinished)
            {
                KeyDownFinished = false;
                if (MainClass.MouseInControl(ListBoxMeanings))
                {
                    switch (e.Key)
                    {
                        case Key.S:
                            Button_ClickSkip(null, null);
                            break;
                        case Key.C:
                            Button_ClickCheck(null, null);
                            break;
                        case Key.F:
                            Button_ClickReview(null, null);
                            break;
                        case Key.P:
                            Button_ClickPronounce(null, null);
                            break;
                    }
                }
                KeyDownFinished = true;
            }
        }
    }
}
