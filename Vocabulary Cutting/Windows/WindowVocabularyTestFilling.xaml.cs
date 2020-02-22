using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Input;
using System.Text;
using System.Text.RegularExpressions;

namespace WPF
{
    /// <summary>
    /// Interaction logic for WindowVocabularyTestMeanings.xaml
    /// </summary>
    public partial class WindowVocabularyTestFilling : Window
    {
        public class WordStruct
        {
            public WordStruct(string Spell_)
            {
                Spelling = Spell_;
            }
            public string Spelling { get; set; }
        }

        private readonly List<UserControlWordCard> NeedReviewWordsList;
        private readonly WordListClass WordsList;
        private UserControlWordCard CorrectSpelling = null;
        private const int MultiItemsCount = 5;

        private MainWindow Father = null;

        public WindowVocabularyTestFilling(List<UserControlWordCard> InputNeedReviewWordsList, WordListClass InputWordsList, MainWindow Father_)
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

            Random TempRandom = new Random(DateTime.Now.Millisecond);
            var TotalWordsLists = new List<WordListClass.WordClass>(WordsList.Words);

            int Index = TempRandom.Next(0, NeedReviewWordsList.Count - 1);
            TotalWordsLists.Remove(NeedReviewWordsList[Index].Word);

            for (int i = 0; i < MultiItemsCount - 1; i++)
            {
                System.Threading.Thread.Sleep(20);
                int IndexTemp = TempRandom.Next(0, TotalWordsLists.Count - 1);
                ListBoxMeanings.Items.Add(new WordStruct(TotalWordsLists[IndexTemp].Spelling));
                TotalWordsLists.RemoveAt(IndexTemp);
            }
            {
                CorrectSpelling = NeedReviewWordsList[Index];

                var IndexR = (TempRandom.Next(0, ListBoxMeanings.Items.Count * 40) + 9) / 40;

                ListBoxMeanings.Items.Insert(IndexR, new WordStruct(NeedReviewWordsList[Index].Word.Spelling));

                NeedReviewWordsList.RemoveAt(Index);
            }
            ListBoxMeanings.Items.Refresh();
            Title = "Vocabulary Test[" + ((WordIndex++) + 1) + " - " + WordCounts + "]";
            MainPlatomEntrance.SetNotify("Test a word", 2, Father);

            var Result = Father.Examples.Read(CorrectSpelling.Word.Spelling);
            if (Result.Count > 0)
            {
                TextBoxExample.Content = Regex.Replace(Encoding.UTF8.GetString(Father.Examples.Read(CorrectSpelling.Word.Spelling)[new Random().Next(0, Result.Count - 1)]), CorrectSpelling.Word.Spelling, "[ ? ]", RegexOptions.IgnoreCase);
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
                ((WordStruct)ListBoxMeanings.Items[ListBoxMeanings.SelectedIndex]).Spelling == CorrectSpelling.Word.Spelling)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WordsList.Words.Count < MultiItemsCount)
            {
                MainPlatomEntrance.SetNotify("No enough words to start a test", 2, Father);
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
                    Words.Add(WordsList[((WordStruct)ListBoxMeanings.Items[ListBoxMeanings.SelectedIndex]).Spelling]);
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

        private void TextBoxExample_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var Result = Father.Examples.Read(CorrectSpelling.Word.Spelling);
                if (Result.Count > 0)
                {
                    TextBoxExample.Content = Regex.Replace(Encoding.UTF8.GetString(Father.Examples.Read(CorrectSpelling.Word.Spelling)[new Random().Next(0, Result.Count - 1)]), CorrectSpelling.Word.Spelling, "[ ? ]", RegexOptions.IgnoreCase);
                }
            }
        }
    }
}
