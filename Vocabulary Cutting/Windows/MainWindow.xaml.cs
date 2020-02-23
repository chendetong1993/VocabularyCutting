using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using System.Net;
using System.Windows.Controls;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string MemorizeTitle = "[记忆] ";
        public const string RootTitle = "[词根缀] ";
        public const string WordsPackage = "Words Package\\";
        public const string ConditionsPath = "Conditions\\";
        public const string ConstTitle = " - Vocabulary Cutting";
        public const string BackupPath = "Backup\\";
        public const string VocabularyListsPath = "Vocabulary Lists\\";
        public const string ListExtension = ".list";
        public const string WordsBackground = "Pictures\\Background\\Background.jpg";
        public const string MemorizeMethod1Path = "Memory\\List1.txt";
        public const string MemorizeMethod2Path = "Memory\\List2.txt";
        private int? SequenceType = null;
        private bool SequenceTypeReverse = false;
        private FilterTypes FilterType = FilterTypes.CancelFilter;
        private readonly string[] SpellingMeaningsCategorysType = new string[] { "StartsWith", "EndsWith", "Equals", "Contains" };
        public readonly string[] InputCommand = new string[] { "!1", "!2", "!3", "#1 ", "#2 ", "#3 ", "#4 ", "#5 ", "#6 " };

        private readonly static List<string> OpenedList = new List<string>();
        public readonly List<UserControlWordCard> WordsControls = new List<UserControlWordCard>();
        public WordListClass WordsList;//全局单词列表

        public readonly BindingData Binding_Data;
        private string ListPath_ = null;
        public string ListPath
        {
            get
            {
                return ListPath_;
            }
            set
            {
                if (value == null)
                {
                    MenuItemCloseList.IsEnabled = false;
                    MenuItemOpenList.IsEnabled = true;
                    MenuItemChangeName.IsEnabled = false;
                    MenuItemCloseList.IsEnabled = false;
                    MenuItemChangePassword.IsEnabled = false;
                    MenuItemManageBackup.IsEnabled = false;
                    UserControlSplitToolsList.IsEnabled = false;
                    UserControlSplitWordList.IsEnabled = false;
                    MenuItemSaveList.IsEnabled = false;
                    foreach (System.Windows.Controls.Expander i in StackPanelToolsList.Children)
                    {
                        i.IsExpanded = false;
                    }
                }
                else
                {
                    MenuItemCloseList.IsEnabled = true;
                    MenuItemOpenList.IsEnabled = false;
                    MenuItemChangeName.IsEnabled = true;
                    MenuItemChangePassword.IsEnabled = true;
                    MenuItemCloseList.IsEnabled = true;
                    MenuItemManageBackup.IsEnabled = true;
                    UserControlSplitToolsList.IsEnabled = true;
                    UserControlSplitWordList.IsEnabled = true;
                    MenuItemSaveList.IsEnabled = true;
                }
                ListPath_ = value;
            }
        }

        [Serializable]
        public enum ConditionTypes
        {
            Tag_Equal,

            Review_Status,

            Spelling_StartsWith,
            Spelling_EndsWith,
            Spelling_Contain,
            Spelling_Equal,

            Meanings_StartsWith,
            Meanings_EndsWith,
            Meanings_Contain,
            Meanings_Equal,

            Categorys_StartsWith,
            Categorys_EndsWith,
            Categorys_Contain,
            Categorys_Equal,

            ReviewedLevel_Equal,
            ReviewedLevel_Less,
            ReviewedLevel_More,

            CreatedTime_Equal,
            CreatedTime_Less,
            CreatedTime_More,

            Opacity_Equal,
            Opacity_Less,
            Opacity_More,


            BackgroundColor_Equal,
            BorderColor_Equal,
            ForegroundColor_Equal,

            Meanings_Count_Equal,
            Meanings_Count_Less,
            Meanings_Count_More,

            Categorys_Count_Equal,
            Categorys_Count_Less,
            Categorys_Count_More
        }

        public enum ConditionFlipTypes
        {
            Follow,
            Reverse,
            Canel
        }

        [Serializable]
        public class FilterCondition
        {
            public FilterCondition(Action Action_, ConditionTypes SeletedItem__, string Value__, ConditionFlipTypes ListFlipIndex__)
            {
                Value_ = Value__;
                SeletedItem_ = SeletedItem__;
                ListFlipIndex_ = ListFlipIndex__;
                Action = Action_;
            }

            public FilterCondition(Action Action_)
            {
                Value_ = String.Empty;
                SeletedItem_ = ConditionTypes.Tag_Equal;
                ListFlipIndex_ = ConditionFlipTypes.Canel;
                Action = Action_;
            }

            private readonly Action Action;

            private static ConditionTypes[] List_ = null;
            public static ConditionTypes[] List
            {
                get
                {
                    if (List_ == null)
                    {
                        List_ = new ConditionTypes[Enum.GetNames(typeof(ConditionTypes)).Length];
                        for (int i = 0; i < List.Length; i++)
                        {
                            List_[i] = List_[0] + i;
                        }
                    }
                    return List_;
                }
            }

            private ConditionTypes SeletedItem_;
            public ConditionTypes SeletedItem
            {
                get { return SeletedItem_; }
                set { Action(); SeletedItem_ = value; }
            }

            private string Value_;
            public string Value
            {
                get { return Value_; }
                set { Action(); Value_ = value; }
            }

            private static ConditionFlipTypes[] ListFlip_ = null;
            public static ConditionFlipTypes[] ListFlip
            {
                get
                {
                    if (ListFlip_ == null)
                    {
                        ListFlip_ = new ConditionFlipTypes[Enum.GetNames(typeof(ConditionFlipTypes)).Length];
                        for (int i = 0; i < ListFlip.Length; i++)
                        {
                            ListFlip_[i] = ListFlip_[0] + i;
                        }
                    }
                    return ListFlip_;
                }
            }

            private ConditionFlipTypes ListFlipIndex_;
            public ConditionFlipTypes ListFlipIndex
            {
                get { return ListFlipIndex_; }
                set { Action(); ListFlipIndex_ = value; }
            }
        }

        public class BindingData : INotifyPropertyChanged
        {
            private readonly Action Filter;

            public BindingData(Action InputFilter)
            {
                Filter = InputFilter;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private bool CollapseRotatedWordPanel_ = false;
            public bool CollapseRotatedWordPanel
            {
                get
                {
                    return CollapseRotatedWordPanel_;
                }
                set
                {
                    CollapseRotatedWordPanel_ = value;
                    Filter();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("CollapseRotatedWordPanel"));
                    }
                }
            }

            private string Title_ = "";
            public string Title
            {
                get
                {
                    return Title_;
                }
                set
                {
                    Title_ = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }

        public enum FilterTypes
        {
            Filter,
            ReserveFilter,
            CancelFilter,
            FilterAll,
            ReserveFilterAll
        }

        public MainWindow()
        {
            InitializeComponent();
            MainPlatomEntrance.Show(this);
            using (MemoryStream byteStream = new MemoryStream(File.ReadAllBytes(MainPlatomEntrance.IconPath)))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = byteStream;
                image.EndInit();
                Icon = image;
            }
            foreach (var i in Enum.GetNames(typeof(WordListClass.OrderTypes)))
            {
                ComboBoxOrderTypes.Items.Add(i.Replace("_", " ") + "[Follow]");
                ComboBoxOrderTypes.Items.Add(i.Replace("_", " ") + "[Reverse]");
            }
            ComboBoxOrderTypes.Items.Add("Disorder");
            ComboBoxOrderTypes.Items.Add("Free");

            ComboBoxOrderTypes.SelectedIndex = ComboBoxOrderTypes.Items.Count - 1;

            GridMain.Children.Remove(ScrollViewerToolsList);
            GridMain.Children.Remove(ScrollViewerWordsList);
            UserControlSplitToolsList.Content = ScrollViewerToolsList;
            UserControlSplitWordList.Content = ScrollViewerWordsList;
            Title = ConstTitle;
            #region
            Binding_Data = new BindingData(
                new Action(() =>
                {
                    MainClass.ParallelTasks Tasks = new MainClass.ParallelTasks();
                    foreach (UserControlWordCard l in StackPanelWordsList.Children)
                    {
                        Tasks.AddTask(new Task((InputValue) =>
                        {
                            UserControlWordCard Value = (UserControlWordCard)InputValue;
                            FilterWord(Value);
                        }, l));
                    }
                    Tasks.WaitFinished();
                }));
            MainClass.BindingData("CollapseRotatedWordPanel", Binding_Data, CheckBoxCollapseRotatedWordPanel, System.Windows.Controls.CheckBox.IsCheckedProperty);
            MainClass.BindingData("Title", Binding_Data, this, Window.TitleProperty);
            Binding_Data.Title = ConstTitle;
            #endregion

            MainPlatomEntrance.SlowerTimer.Tick += SlowerTimer_Tick;
            MainPlatomEntrance.FasterTimer.Tick += FasterTimer_Tick;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized && (MainPlatomEntrance.Setting.MinInvisible == false))
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.ShowInTaskbar = false;
                this.Visibility = Visibility.Hidden;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Close Windows?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
            {
                CloseList(null, null);
                MainPlatomEntrance.SlowerTimer.Tick -= SlowerTimer_Tick;
                MainPlatomEntrance.FasterTimer.Tick -= FasterTimer_Tick;
                MainPlatomEntrance.Close(this);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void About(object sender, RoutedEventArgs e)
        {
            MainClass.OpenWindow(new WindowAbout(), this, true);
        }

        private void NewWindow(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
        }

        private void Contact(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject("chendetong1993@126.com");
            MainPlatomEntrance.SetNotify("Copying Author Emial to Clipboard Finished!", 2, this);
        }

        private void NewList(object sender, RoutedEventArgs e)
        {
            var Input = new MainClass.ReferenceTypePackaging<string>(null);
            var WindowInputInformation = new WindowInputInformation(out Input, "Add List Name", "New List", this);
            MainClass.OpenWindow(WindowInputInformation, this, true);
            if (Input.Value != null && Input.Value != String.Empty)
            {
                var Temp = VocabularyListsPath + Input.Value + ListExtension;
                if (File.Exists(Temp))
                {
                    MainPlatomEntrance.SetNotify("Create Failed!", 2, this);
                }
                else
                {
                    using (var i = new WordListClass(null, this))
                    {
                        File.WriteAllBytes(Temp, MainClass.CloneToBytes(i.Output()));
                        MainPlatomEntrance.SetNotify("Create Finished!", 2, this);
                    }
                }
            }
        }

        private void CloseList(object sender, RoutedEventArgs e)
        {
            if (ConstTitle != Binding_Data.Title)
            {
                //清理单词意思线程
                if (FillingPronunciationsTask != null)
                {
                    FillingPronunciationsTaskCanel = true;
                    FillingMeaningsTask.Abort();
                }
                //清理单词意思线程
                if (FillingMeaningsTask != null)
                {
                    Translation.Abort();
                    FillingMeaningsTaskCanel = true;
                    FillingMeaningsTask.Abort();
                }
                FillingControl.Clear();
                //清理例句线程
                if (FillingExamplesTask != null)
                {
                    Examples.Abort();
                    FillingExamplesTaskCanel = true;
                    FillingExamplesTask.Abort();
                }
                //清理图片线程
                if (FillingPicturesTask != null)
                {
                    Pictures.Abort();
                    FillingPicturesTaskCanel = true;
                    FillingPicturesTask.Abort();
                }
                NotifyMessage.Clear();
                NotifyMessage.TrimExcess();
                //备份列表
                Backup();
                SaveList();
                ListPath = null;

                OpenedList.Remove(Binding_Data.Title);
                OpenedList.TrimExcess();

                StackPanelWordsList.Children.Clear();
                WordsControls.Clear();
                WordsControls.TrimExcess();
                if (WordsList != null)
                {
                    WordsList.Dispose();
                    WordsList = null;
                }
                Binding_Data.Title = ConstTitle;
                ScrollViewerWordsList.Background = null;

                ActivedUserControlWordCard.Clear();
                ActivedUserControlWordCard.TrimExcess();

                GC.Collect();
            }
        }

        private void ChangeNameList(object sender, RoutedEventArgs e)
        {
            if (ListPath != null)
            {
                var Input = new MainClass.ReferenceTypePackaging<string>(null);
                var WindowInputInformation = new WindowInputInformation(out Input, "Change List Name", Path.GetFileNameWithoutExtension(ListPath), this);
                MainClass.OpenWindow(WindowInputInformation, this, true);
                if (Input.Value != null && Input.Value != String.Empty)
                {
                    var Temp = VocabularyListsPath + Input.Value + ListExtension;
                    if (File.Exists(Temp))
                    {
                        MainPlatomEntrance.SetNotify("Change Name Failed!", 2, this);
                    }
                    else
                    {
                        var TempDeletePath = ListPath;
                        ListPath = Temp;
                        File.Delete(TempDeletePath);
                        Binding_Data.Title = Input.Value + ConstTitle;
                        #region 更改备份文件
                        string FileName = null;
                        string FileNameTime = null;
                        foreach (FileInfo f in new DirectoryInfo(BackupPath).GetFiles())
                        {
                            FileName = f.ToString();
                            FileNameTime = FileName.Substring(0, FileName.IndexOf("]") + 1);
                            FileName = FileName.Substring(FileName.IndexOf("]") + 1);
                            if (FileName == Path.GetFileName(TempDeletePath))
                            {
                                try
                                {
                                    f.MoveTo(BackupPath + FileNameTime + Input.Value + ListExtension);
                                }
                                catch { }
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        public void SaveList()
        {
            if (ListPath != null)
            {
                File.WriteAllBytes(ListPath, MainClass.CloneToBytes(WordsList.Output()));
                MainPlatomEntrance.SetNotify("List is saved", 2, this);
            }
        }

        private void SelectListToOpen(object sender, RoutedEventArgs e)
        {
            List<string> FileLists = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(VocabularyListsPath).GetFiles())
            {
                if (f.ToString().EndsWith(ListExtension))
                {
                    FileLists.Add(Path.GetFileNameWithoutExtension(f.ToString()));
                }
            }
            var SelectIndex = new MainClass.ReferenceTypePackaging<int>(-1);
            var SelectItemWindow = new WindowSelectItem(out SelectIndex, FileLists.ToArray(), "Select a list!");
            MainClass.OpenWindow(SelectItemWindow, this, true);
            if (SelectIndex.Value != -1)
            {
                OpenList(FileLists[SelectIndex.Value]);
            }
        }

        private void ChangePassword(object sender, RoutedEventArgs e)
        {
            MainClass.ReferenceTypePackaging<string> Input;
            var InputWindow = new WindowInputInformation(out Input, "Input Password", "", this);
            MainClass.OpenWindow(InputWindow, this, true);
            if (Input.Value != null)
            {
                WordsList.Key = Input.Value;
                MainPlatomEntrance.SetNotify("Change Password Finished!", 2, this);
            }
        }

        public void OpenList(string ListName)
        {
            try
            {
                if (OpenedList.Contains(ListName + ConstTitle))
                {
                    MainPlatomEntrance.SetNotify("It has already opended!", 2, this);
                }
                else
                {
                    var Time = DateTime.Now;
                    ListPath = VocabularyListsPath + ListName + ListExtension;
                    WordsList = new WordListClass
                    (
                        (object)MainClass.BytesToClone(File.ReadAllBytes(ListPath)),
                        this
                    );
                    foreach (var i in WordsList.Words)
                    {
                        var UserControlWordCard = new UserControlWordCard(
                                i,
                                WordsList,
                                this);
                        StackPanelWordsList.Children.Add(UserControlWordCard);
                    }
                    SortWords();
                    FilterWords();
                    Binding_Data.Title = Path.GetFileNameWithoutExtension(ListPath) + ConstTitle;
                    SlowerTimer_Tick(null, null);
                    try
                    {
                        ScrollViewerWordsList.Background = MainClass.OpenPicture(File.ReadAllBytes(WordsBackground));
                    }
                    catch { }
                    OpenedList.Add(Binding_Data.Title);
                    MainPlatomEntrance.SetNotify(string.Format("Open List Finished!\nWords Count: {0}\nUsed: {1} seconds", WordsList.Words.Count, Math.Round((DateTime.Now - Time).TotalSeconds, 2)), 2, this);
                }
            }
            catch
            {
                ListPath = null;
                if (WordsList != null)
                {
                    WordsList.Dispose();
                    WordsList = null;
                }
                MainPlatomEntrance.SetNotify("Open List Failed!", 2, this);
            }
            GC.Collect();
        }

        private void ImportList(string FilePath)
        {
            try
            {
                string NewListPath = VocabularyListsPath + Path.GetFileName(FilePath);
                if (File.Exists(NewListPath))
                {
                    if (MessageBox.Show("Replace?", "List exists", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                if (NewListPath == ListPath_)
                {
                    CloseList(null, null);
                    File.Delete(NewListPath);
                    File.Copy(FilePath, NewListPath);
                    OpenList(Path.GetFileNameWithoutExtension(NewListPath));
                }
                else
                {
                    File.Copy(FilePath, NewListPath);
                }
                MainPlatomEntrance.SetNotify("Import List Finished!", 2, this);
            }
            catch
            {
                MainPlatomEntrance.SetNotify("Import List Failed!", 2, this);
            }
        }
        private void ImportList(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Please,select a list";
            fileDialog.Filter = "list type(*.list)|*.list";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportList(fileDialog.FileName);
            }
        }

        private void ExportList(object sender, RoutedEventArgs e)
        {
            SaveList();
            List<string> FileLists = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(VocabularyListsPath).GetFiles())
            {
                if (f.ToString().EndsWith(ListExtension))
                {
                    FileLists.Add(Path.GetFileNameWithoutExtension(f.ToString()));
                }
            }
            var SelectIndex = new MainClass.ReferenceTypePackaging<int>(-1);
            var SelectItemWindow = new WindowSelectItem(out SelectIndex, FileLists.ToArray(), "Select a list!");
            MainClass.OpenWindow(SelectItemWindow, this, true);
            if (SelectIndex.Value != -1)
            {
                System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                FolderBrowserDialog.Description = "Please,select a folder to save";
                if (FolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        SaveList(null, null);
                        string FileName = ListPath_.Substring(ListPath_.IndexOf("\\"));
                        File.Copy(ListPath_, FolderBrowserDialog.SelectedPath + FileName);
                        MainPlatomEntrance.SetNotify("Export List Finished!", 2, this);
                    }
                    catch
                    {
                        MainPlatomEntrance.SetNotify("Export List Failed!", 2, this);
                    }
                }
            }
        }

        private void DiscardList(object sender, RoutedEventArgs e)
        {
            List<string> FileLists = new List<string>();
            foreach (FileInfo f in new DirectoryInfo(VocabularyListsPath).GetFiles())
            {
                if (f.ToString().EndsWith(ListExtension))
                {
                    FileLists.Add(Path.GetFileNameWithoutExtension(f.ToString()));
                }
            }
            var SelectIndex = new MainClass.ReferenceTypePackaging<int>(-1);
            var SelectItemWindow = new WindowSelectItem(out SelectIndex, FileLists.ToArray(), "Select a list!");
            MainClass.OpenWindow(SelectItemWindow, this, true);
            if (SelectIndex.Value != -1)
            {
                try
                {
                    if (VocabularyListsPath + FileLists[SelectIndex.Value] + ListExtension == ListPath)
                    {
                        throw new Exception();
                    }
                    File.Delete(VocabularyListsPath + FileLists[SelectIndex.Value] + ListExtension);

                    #region 更改备份文件
                    var PathT = BackupPath + Path.GetFileNameWithoutExtension(ListPath);
                    if (Directory.Exists(PathT))
                    {
                        Directory.Delete(PathT, true);
                    }
                    #endregion
                    MainPlatomEntrance.SetNotify("Discard List Finished!", 2, this);
                }
                catch
                {
                    MainPlatomEntrance.SetNotify("Discard List Failed!", 2, this);
                }
            }
        }

        private void Setting(object sender, RoutedEventArgs e)
        {
            MainClass.OpenWindow(new WindowSetting(this), this, true);
            MainPlatomEntrance.SaveSetting();
            Backup();
        }

        private void WordsExport(object sender, RoutedEventArgs e)
        {
            var SelectionItems = new List<WindowExportTXTSelection.Selection>();
            SelectionItems.Add(new WindowExportTXTSelection.Selection("Export Index"));
            SelectionItems.Add(new WindowExportTXTSelection.Selection("Export Meanings"));
            SelectionItems.Add(new WindowExportTXTSelection.Selection("Export Categorys"));
            SelectionItems.Add(new WindowExportTXTSelection.Selection("Export Space"));
            var WindowExportTXTSelection = new WindowExportTXTSelection(new MainClass.ReferenceTypePackaging<List<WindowExportTXTSelection.Selection>>(SelectionItems));
            MainClass.OpenWindow(WindowExportTXTSelection, this, true);
            if (SelectionItems.Count != 0)
            {
                System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                FolderBrowserDialog.Description = "Please,select a folder to save";
                if (FolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        List<string> Text = new List<string>();
                        const string Space = "   ";
                        uint Index = 1;
                        foreach (UserControlWordCard l in ActivedUserControlWordCard)
                        {
                            //是否添加索引
                            if (SelectionItems[0].Enable)
                            {
                                Text.Add(Index + ". " + l.Word.Spelling);
                            }
                            else
                            {
                                Text.Add(l.Word.Spelling);
                            }
                            //是否添加解释
                            if (SelectionItems[1].Enable)
                            {
                                Text.Add("Meanings: ");
                                var ReturnMeanings = l.Word.Meanings;
                                foreach (var i1 in l.Word.Meanings)
                                {
                                    Text.Add(Space + i1);
                                }
                            }
                            //是否添加类别
                            if (SelectionItems[2].Enable)
                            {
                                Text.Add("Categorys: ");
                                foreach (var h in l.Word.Categorys)
                                {
                                    Text[Text.Count - 1] += "{" + h + "} ";
                                }
                            }
                            if (SelectionItems[3].Enable)
                            {
                                Text.Add("");
                            }
                            Index++;
                        }
                        string SavePath = FolderBrowserDialog.SelectedPath + "\\" + Path.GetFileNameWithoutExtension(ListPath) + ".txt";
                        File.WriteAllLines(SavePath, Text);
                        Process.Start(SavePath);
                        MainPlatomEntrance.SetNotify("Export Words Finished!", 2, this);
                    }
                    catch
                    {
                        MainPlatomEntrance.SetNotify("Export Words Failed!", 2, this);
                    }
                }
            }
        }

        private void RestoreList(object sender, RoutedEventArgs e)
        {
            var PathT = BackupPath + System.IO.Path.GetFileNameWithoutExtension(ListPath);
            if (Directory.Exists(PathT) == false)
            {
                Directory.CreateDirectory(PathT);
            }

            List<string> BackupList = new List<string>();
            var Files = new DirectoryInfo(PathT).GetFiles();
            foreach (FileInfo f in Files)
            {
                BackupList.Insert(0, System.IO.Path.GetFileNameWithoutExtension(f.Name).Replace(".", ":"));
            }
            var BoolStore = new MainClass.ReferenceTypePackaging<int>(-1);
            var RestoreBackup = new WindowSelectItem(out BoolStore, BackupList.ToArray(), "Please select a list to restore");
            MainClass.OpenWindow(RestoreBackup, this, true);
            if (BoolStore.Value != -1)
            {
                try
                {
                    string Path = ListPath_;
                    CloseList(null, null);
                    File.Copy(Files[BoolStore.Value].FullName, Path, true);
                    Path = System.IO.Path.GetFileNameWithoutExtension(Path);
                    OpenList(Path);
                    MainPlatomEntrance.SetNotify("Restore List Finished!", 2, this);
                }
                catch { }
            }
        }

        private void AddWord(object sender, RoutedEventArgs e)
        {
            if (MainPlatomEntrance.Setting.AutoFillPictures || MainPlatomEntrance.Setting.AutoFillMeanings || MainPlatomEntrance.Setting.AutoFillExamples)
            {
                MainPlatomEntrance.SetNotify("Can not delete due to \"Auto Filling\" is open", 2, this);
                return;
            }
            MainClass.ReferenceTypePackaging<string> Output = null;
            var WindowInputInformation = new WindowInputInformation(out Output, "Input a new word", "", this);
            MainClass.OpenWindow(WindowInputInformation, this, true);
            if (Output.Value != null && Output.Value != String.Empty)
            {
                WordListClass.WordClass Word;
                WordsList.CreateWord(out Word);
                try
                {
                    Word.Spelling = Output.Value;
                    Word.EnableBackup = true;
                }
                catch { }
                Backup();
                if (Word.Spelling != null && Word.Spelling != "")
                {
                    var UserControlWordCard = new UserControlWordCard(
                    Word,
                    WordsList,
                    this);
                    StackPanelWordsList.Children.Add(UserControlWordCard);
                    SortWord(UserControlWordCard);
                    FilterWord(UserControlWordCard);
                    MainPlatomEntrance.SetNotify("Add Word succeed!", 2, this);
                }
                else
                {
                    WordsList.DeleteWord(ref Word);
                    MainPlatomEntrance.SetNotify("Add word failed due to word Conflicts", 2, this);
                }
            }
        }

        private void AddWords(object sender, RoutedEventArgs e)
        {
            if (MainPlatomEntrance.Setting.AutoFillPictures || MainPlatomEntrance.Setting.AutoFillMeanings || MainPlatomEntrance.Setting.AutoFillExamples)
            {
                MainPlatomEntrance.SetNotify("Can not delete due to \"Auto Filling\" is open", 2, this);
                return;
            }
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Please,select a txt";
            fileDialog.Filter = "txt type(*.txt)|*.txt";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Backup();
                    int WordsSucceedCount = 0, TotalWordsCount = 0;
                    StringBuilder ExistWords = new StringBuilder();
                    foreach (var Item in File.ReadLines(fileDialog.FileName))
                    {
                        if (Item.Trim() != String.Empty)
                        {
                            WordListClass.WordClass Word;
                            WordsList.CreateWord(out Word);
                            try
                            {
                                Word.Spelling = Item;
                                Word.EnableBackup = true;
                            }
                            catch { }
                            if (Word.Spelling != null && Word.Spelling != "")
                            {
                                var UserControlWordCard = new UserControlWordCard(
                                Word,
                                WordsList,
                                this);
                                StackPanelWordsList.Children.Add(UserControlWordCard);
                                WordsSucceedCount++;
                                SortWord(UserControlWordCard);
                                FilterWord(UserControlWordCard);
                            }
                            else
                            {
                                WordsList.DeleteWord(ref Word);
                                ExistWords.AppendLine(Item);
                            }
                            TotalWordsCount++;
                        }
                    }

                    MainPlatomEntrance.SetNotify(string.Format("Add Words Succeed({0}/{1})!", WordsSucceedCount, TotalWordsCount), 2, this);
                    if (WordsSucceedCount != TotalWordsCount)
                    {
                        MainPlatomEntrance.SetNotify(string.Format("{0} Words Conflict\n\n{1}", TotalWordsCount - WordsSucceedCount, ExistWords.ToString()), 2, this);
                    }
                }
                catch
                {
                    MainPlatomEntrance.SetNotify("Fail to open the file", 2, this);
                }
            }
        }

        public void HideShowWord(object sender, System.Windows.RoutedEventArgs e)
        {
            HideShowWord(ActivedUserControlWordCard.ToArray());
        }

        public void HideShowWord(UserControlWordCard[] Words)
        {
            foreach (var m in Words)
            {
                m.TextBlock_MouseUpHideShowWord();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void FillWordMeanings(UserControlWordCard Word, Translation.SymbolsBaidu Result)
        {
            string Temp = null;
            if (Result != null && Word != null)
            {
                Word.Word.Meanings.Add(string.Format("/{0}/", Result.Ph_am));
                foreach (var Mean0 in Result.Parts)
                {
                    Temp = "# " + Mean0.Part;
                    Temp = Mean0.Part + " ";
                    foreach (var Mean1 in Mean0.Means)
                    {
                        Word.Word.Meanings.Add(Temp + Mean1);
                    }
                }
                SortWord(Word);
                FilterWord(Word);
            }
        }

        private void FillWordMeanings(UserControlWordCard Word, Translation.SymbolsYoudao Result)
        {
            if (Result != null && Word != null)
            {
                Word.Word.Meanings.Add(string.Format("/{0}/", Result.phonetic));
                foreach (var Mean0 in Result.explains)
                {
                    Word.Word.Meanings.Add("# " + Mean0);
                }
                SortWord(Word);
                FilterWord(Word);
            }
        }

        private void CopyWord(object sender, RoutedEventArgs e)
        {
            if (ActivedUserControlWordCard.Count - 1 >= 0)
            {
                Clipboard.SetDataObject(ActivedUserControlWordCard[ActivedUserControlWordCard.Count - 1].Word.Spelling);
            }
        }

        private void WordsCount(object sender, RoutedEventArgs e)
        {
            int Numerator = 0, Denominator = 0;
            Denominator = WordsControls.Count;
            foreach (var l in WordsControls)
            {
                if (l.Word.Visible)
                {
                    Numerator++;
                }
            }
            double Percentage = Math.Round(((double)Numerator) / Denominator * 100, 2);
            MainPlatomEntrance.SetNotify(string.Format("({0}/{1}) => {2}% words are visible", Numerator, Denominator, Percentage), 2, this);
        }

        private void SetWordTag(object sender, RoutedEventArgs e)
        {
            SetWordTag(ActivedUserControlWordCard.ToArray());
        }

        private void SetWordTag(UserControlWordCard[] Words)
        {
            MainClass.ReferenceTypePackaging<string> Value = null;
            var WindowsInput = new WindowInputInformation(out Value, "Please, input a string", "Tag", this);
            MainClass.OpenWindow(WindowsInput, this, true);
            if (Value.Value != null)
            {
                foreach (var l in Words)
                {
                    if (Value.Value == string.Empty)
                    {
                        l.Word.Tag = null;
                    }
                    else
                    {
                        l.Word.Tag = Value.Value;
                    }
                    SortWord(l);
                    FilterWord(l);
                }
                UserControlWordCard.WaitDrawRefresh();
            }
        }

        private void ChangeBackground(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Multiselect = false;
                fileDialog.Title = "Please,select a picture";
                fileDialog.Filter = "picture type(*.jpg)|*.jpg";
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    ScrollViewerWordsList.Background = null;
                    ScrollViewerWordsList.Background = MainClass.OpenPicture(File.ReadAllBytes(fileDialog.FileName));
                    File.Copy(fileDialog.FileName, WordsBackground, true);
                    MainPlatomEntrance.SetNotify("Change Background Finished!", 2, this);
                }
            }
            catch
            {
                MainPlatomEntrance.SetNotify("Change Background Failed!", 2, this);
            }
        }

        private void DeleteBackground(object sender, RoutedEventArgs e)
        {
            try
            {
                ScrollViewerWordsList.Background = null;
                File.Delete(WordsBackground);
                MainPlatomEntrance.SetNotify("Delete Background Finished!", 2, this);
            }
            catch
            {
                MainPlatomEntrance.SetNotify("Delete Background Failed!", 2, this);
            }
        }
        private void SlowerTimer_Tick(object sender, EventArgs e)
        {
            if (ListPath != null)
            {
                #region 提醒复习
                uint Count = 0;
                foreach (var i in WordsControls)
                {
                    if (i.Word.NeedReview)
                    {
                        if (i.Word.ReviewLevel != 0)
                        {
                            Count++;
                        }
                    }
                    if (i.Word.ReviewStatusChanged)
                    {
                        SortWord(i);
                        FilterWord(i);
                    }
                }
                if (Count != 0)
                {
                    MainPlatomEntrance.SetNotify(Count + " words need to review!", 10, this);
                }
                #endregion
                #region 自动备份
                Backup();
                #endregion
            }
            GC.Collect();
        }

        public void Backup()
        {
            if (MainPlatomEntrance.Setting.AutoBackup == false)
            {
                var PathT = BackupPath + Path.GetFileNameWithoutExtension(ListPath);
                if (Directory.Exists(PathT))
                {
                    Directory.Delete(PathT, true);
                }
            }
            else
            {
                if (ListPath != null)
                {
                    Thread thread = new Thread(() =>
                        {
                            try
                            {
                                byte[] Write = new byte[0];
                                lock (WordsList)
                                {
                                    Write = MainClass.CloneToBytes(WordsList.Output());
                                }

                                var PathT = BackupPath + System.IO.Path.GetFileNameWithoutExtension(ListPath);
                                if (Directory.Exists(PathT) == false)
                                {
                                    Directory.CreateDirectory(PathT);
                                }
                                var Files = new DirectoryInfo(PathT).GetFiles();
                                byte[] Compare = new byte[0];
                                if (Files.Length > 0)
                                {
                                    Compare = File.ReadAllBytes(Files[Files.Length - 1].FullName);
                                }
                                if (Write.SequenceEqual(Compare) == false)
                                {
                                    int FileIndex = 0;
                                    while (Files.Length - FileIndex > MainPlatomEntrance.Setting.MaxBackupTimes - 1)
                                    {
                                        File.Delete(Files[FileIndex++].FullName);
                                    }
                                    var Time = DateTime.Now;
                                    File.WriteAllBytes(
                                        string.Format("{0}\\{1}-{2}-{3} {4}.{5}.{6}.list",
                                        PathT,
                                        Time.Year.ToString("D4"),
                                        Time.Month.ToString("D2"),
                                        Time.Day.ToString("D2"),
                                        Time.Hour.ToString("D2"),
                                        Time.Minute.ToString("D2"),
                                        Time.Second.ToString("D2")),
                                        Write);
                                    lock (NotifyMessage)
                                    {
                                        NotifyMessage.Add("List is backuped");
                                    }
                                }
                            }
                            catch { }
                        });
                    thread.Start();
                }
            }
        }

        private void Button_FilterAll(object sender, RoutedEventArgs e)
        {
            FilterType = FilterTypes.FilterAll;
            FilterWords();
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
        }

        private void Button_ReverseFilterAll(object sender, RoutedEventArgs e)
        {
            FilterType = FilterTypes.ReserveFilterAll;
            FilterWords();
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
        }

        private void Button_Filter(object sender, RoutedEventArgs e)
        {
            FilterType = FilterTypes.Filter;
            FilterWords();
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
        }

        private void Button_ReverseFilter(object sender, RoutedEventArgs e)
        {
            FilterType = FilterTypes.ReserveFilter;
            FilterWords();
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
        }

        private void Button_FreeFilter(object sender, RoutedEventArgs e)
        {
            FilterType = FilterTypes.CancelFilter;
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
        }

        private void SortWords()
        {
            if (StackPanelWordsList == null)
            {
                return;
            }
            if (SequenceType != null)
            {
                StackPanelWordsList.Children.Clear();
                if (SequenceType != -1)
                {
                    foreach (var i in WordsList.OrderList[(WordListClass.OrderTypes)SequenceType].Item1)
                    {
                        if (SequenceTypeReverse)
                        {
                            StackPanelWordsList.Children.Insert(0, WordsControls[i]);
                        }
                        else
                        {
                            StackPanelWordsList.Children.Add(WordsControls[i]);
                        }
                    }
                }
                else
                {
                    StackPanelWordsList.Children.Clear();
                    Random Random = new Random();
                    int Index = 0;
                    if (WordsControls.Count != 0)
                    {
                        foreach (var i in WordsControls)
                        {
                            Index = Random.Next(0, StackPanelWordsList.Children.Count);
                            StackPanelWordsList.Children.Insert(Index, i);
                        }
                    }
                }
            }
            MainPlatomEntrance.SetNotify("Sort Finished!", 2, this);
            GC.Collect();
        }

        public static bool FilterWordCheck(List<FilterCondition> Conditions, WordListClass.WordClass Word)
        {
            bool Flip;
            foreach (FilterCondition i in Conditions)
            {
                try
                {
                    if (i.ListFlipIndex == ConditionFlipTypes.Canel)
                    {
                        continue;
                    }
                    Flip = (i.ListFlipIndex == ConditionFlipTypes.Reverse);
                    switch (i.SeletedItem)
                    {
                        case ConditionTypes.Tag_Equal:
                            if ((Word.Tag == i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        //Spelling
                        case ConditionTypes.Spelling_StartsWith:
                            if (Word.Spelling.StartsWith(i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Spelling_EndsWith:
                            if (Word.Spelling.EndsWith(i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Spelling_Contain:
                            if (Word.Spelling.Contains(i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Spelling_Equal:
                            if ((Word.Spelling == i.Value) == Flip)
                            {
                                return false;
                            }
                            break;

                        //Meanings
                        case ConditionTypes.Meanings_StartsWith:
                            bool ContainsCheck = false;
                            foreach (var n in Word.Meanings)
                            {
                                if (n.StartsWith(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Meanings_EndsWith:
                            ContainsCheck = false;
                            foreach (var n in Word.Meanings)
                            {
                                if (n.EndsWith(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Meanings_Contain:
                            ContainsCheck = false;
                            foreach (var n in Word.Meanings)
                            {
                                if (n.Contains(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Meanings_Equal:
                            ContainsCheck = false;
                            foreach (var n in Word.Meanings)
                            {
                                if (n == i.Value)
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;

                        //Categorys
                        case ConditionTypes.Categorys_StartsWith:
                            ContainsCheck = false;
                            foreach (var n in Word.Categorys)
                            {
                                if (n.StartsWith(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Categorys_EndsWith:
                            ContainsCheck = false;
                            foreach (var n in Word.Categorys)
                            {
                                if (n.EndsWith(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Categorys_Contain:
                            ContainsCheck = false;
                            foreach (var n in Word.Categorys)
                            {
                                if (n.Contains(i.Value))
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Categorys_Equal:
                            ContainsCheck = false;
                            foreach (var n in Word.Categorys)
                            {
                                if (n == i.Value)
                                {
                                    ContainsCheck = true;
                                }
                            }
                            if (ContainsCheck == Flip)
                            {
                                return false;
                            }
                            break;

                        //Review Level
                        case ConditionTypes.ReviewedLevel_Equal:
                            if ((Word.ReviewLevel == Convert.ToUInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.ReviewedLevel_Less:
                            if ((Word.ReviewLevel < Convert.ToUInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.ReviewedLevel_More:
                            if ((Word.ReviewLevel > Convert.ToUInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;

                        //CreatedTime
                        case ConditionTypes.CreatedTime_Equal:
                            if ((Word.CreatedTime.CompareTo(Convert.ToDateTime(i.Value)) == 0) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.CreatedTime_Less:
                            if ((Word.CreatedTime.CompareTo(Convert.ToDateTime(i.Value)) < 0) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.CreatedTime_More:
                            if ((Word.CreatedTime.CompareTo(Convert.ToDateTime(i.Value)) > 0) == Flip)
                            {
                                return false;
                            }
                            break;
                        //复习状态
                        case ConditionTypes.Review_Status:
                            switch (Convert.ToUInt16(i.Value))
                            {
                                //需要复习
                                case 0:
                                    if (Word.NeedReview == Flip)
                                    {
                                        return false;
                                    }
                                    break;
                                //不需要复习
                                case 1:
                                    if (Word.NeedReview != Flip)
                                    {
                                        return false;
                                    }
                                    break;
                            }
                            break;

                        //Review Level
                        case ConditionTypes.Opacity_Equal:
                            if ((Word.Opacity == Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Opacity_Less:
                            if ((Word.Opacity < Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Opacity_More:
                            if ((Word.Opacity > Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;

                        //Color
                        case ConditionTypes.BackgroundColor_Equal:
                            if ((Word.BackgroundColor == i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.BorderColor_Equal:
                            if ((Word.BorderColor == i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.ForegroundColor_Equal:
                            if ((Word.ForegroundColor == i.Value) == Flip)
                            {
                                return false;
                            }
                            break;
                        //Meanings Count
                        case ConditionTypes.Meanings_Count_Equal:
                            if ((Word.Meanings.Count == Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Meanings_Count_Less:
                            if ((Word.Meanings.Count < Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Meanings_Count_More:
                            if ((Word.Meanings.Count > Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;

                        //Categorys Count
                        case ConditionTypes.Categorys_Count_Equal:
                            if ((Word.Categorys.Count == Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Categorys_Count_Less:
                            if ((Word.Categorys.Count < Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                        case ConditionTypes.Categorys_Count_More:
                            if ((Word.Categorys.Count > Convert.ToInt32(i.Value)) == Flip)
                            {
                                return false;
                            }
                            break;
                    }
                }
                catch { }
            }
            return true;
        }

        public void FilterWord(UserControlWordCard WordsControl)
        {
            bool FilterFront = WordsControl.Word.Visible;
            List<FilterCondition> Conditions = new List<FilterCondition>();
            foreach (FilterCondition i in ListBoxFliterConditions.Items)
            {
                Conditions.Add(i);
            }
            if (FilterType == FilterTypes.CancelFilter)
            {
            }
            else if (FilterType == FilterTypes.FilterAll)
            {
                FilterFront = true;
            }
            else if (FilterType == FilterTypes.ReserveFilterAll)
            {
                FilterFront = false;
            }
            else
            {
                FilterFront = FilterWordCheck(Conditions, WordsControl.Word);
                if (FilterType == FilterTypes.ReserveFilter)
                {
                    FilterFront = !FilterFront;
                }
            }
            WordsControl.Word.Visible = FilterFront;
            if (FilterFront == false && Binding_Data.CollapseRotatedWordPanel == true)
            {
                WordsControl.IsVisible = false;
            }
            else
            {
                WordsControl.IsVisible = true;
                if (WordsControl.Word.ReviewStatusChanged)
                {
                    WordsControl.Word.ResetReviewStatusChanged();
                }
            }
            if (WordsControl.IsVisible == false)
            {
                lock (ActivedUserControlWordCard)
                {
                    if (ActivedUserControlWordCard.Contains(WordsControl))
                    {
                        WordsControl.Focus = false;
                        ActivedUserControlWordCard.Remove(WordsControl);
                    }
                }
            }
        }

        public void SortWord(UserControlWordCard WordsControl)
        {
            if (SequenceType != null)
            {
                if (SequenceType != -1)
                {
                    if (!SequenceTypeReverse)
                    {
                        StackPanelWordsList.Children.Remove(WordsControl);
                        StackPanelWordsList.Children.Insert(WordsList.OrderList[(WordListClass.OrderTypes)SequenceType].Item1.IndexOf(WordsControls.IndexOf(WordsControl)), WordsControl);
                    }
                    else
                    {
                        StackPanelWordsList.Children.Remove(WordsControl);
                        StackPanelWordsList.Children.Insert(WordsControls.Count - WordsList.OrderList[(WordListClass.OrderTypes)SequenceType].Item1.IndexOf(WordsControls.IndexOf(WordsControl)) - 1, WordsControl);
                    }
                }
                else
                {
                    StackPanelWordsList.Children.Remove(WordsControl);
                    Random Random = new Random();
                    int Index = Random.Next(0, StackPanelWordsList.Children.Count);
                    StackPanelWordsList.Children.Insert(Index, WordsControl);

                }
            }
        }

        private List<UserControlWordCard> ActivedUserControlWordCard = new List<UserControlWordCard>();
        private void DeleteWord(object sender, RoutedEventArgs e)
        {
            if (MainPlatomEntrance.Setting.AutoFillPictures || MainPlatomEntrance.Setting.AutoFillMeanings || MainPlatomEntrance.Setting.AutoFillExamples)
            {
                MainPlatomEntrance.SetNotify("Can not delete due to \"Auto Filling\" is open", 2, this);
                return;
            }
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].TextBlock_MouseUpDeleteWord();
            }
            UserControlWordCard.WaitDrawRefresh();
            ActivedUserControlWordCard.Clear();
        }

        private void ViewedMark(object sender, RoutedEventArgs e)
        {
            //MainPlatomEntrance.SetNotify(string.Format("Reviewed: {0} words", ActivedUserControlWordCard.Count), 2, this);
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].TextBlock_MouseUpViewedMark();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void ChangeWordBackgroundColor(object sender, RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<SolidColorBrush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);
            MainClass.OpenWindow(WindowChoiceColor, this, true);
            if (Color.Value != null)
            {
                for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
                {
                    ActivedUserControlWordCard[l].MenuItem_MouseUpChangeWordBackgroundColor(Color.Value);
                }
                UserControlWordCard.WaitDrawRefresh();
            }
        }

        private void ChangeWordBorderColor(object sender, RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<SolidColorBrush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);
            MainClass.OpenWindow(WindowChoiceColor, this, true);
            if (Color.Value != null)
            {
                for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
                {
                    ActivedUserControlWordCard[l].MenuItem_MouseUpChangeWordBorderColor(Color.Value);
                }
                UserControlWordCard.WaitDrawRefresh();
            }
        }

        private void ChangeWordForegroundColor(object sender, RoutedEventArgs e)
        {
            var Color = new MainClass.ReferenceTypePackaging<SolidColorBrush>(null);
            var WindowChoiceColor = new WindowChoiceColor(Color);
            MainClass.OpenWindow(WindowChoiceColor, this, true);
            if (Color.Value != null)
            {
                for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
                {
                    ActivedUserControlWordCard[l].MenuItem_MouseUpChangeWordForegroundColor(Color.Value);
                }
            }
        }

        private void NewWordMark(object sender, RoutedEventArgs e)
        {
            MainPlatomEntrance.SetNotify(string.Format("Forgotten: {0} words", ActivedUserControlWordCard.Count), 2, this);
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].MenuItem_MouseUpNewWordMark();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void RerememberWord(object sender, RoutedEventArgs e)
        {
            MainPlatomEntrance.SetNotify(string.Format("Reremember: {0} words", ActivedUserControlWordCard.Count), 2, this);
            List<UserControlWordCard> Temp = new List<UserControlWordCard>();
            Temp.AddRange(ActivedUserControlWordCard);
            for (int l = Temp.Count - 1; l >= 0; l--)
            {
                Temp[l].MenuItem_MouseUpNewWordMark();
                Temp[l].TextBlock_MouseUpViewedMark();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void ScrollViewerWordsList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!MutiChoiceWords)
                {
                    foreach (var k in ActivedUserControlWordCard)
                    {
                        k.Focus = false;
                    }
                    ActivedUserControlWordCard.Clear();
                }
                foreach (UserControlWordCard l in StackPanelWordsList.Children)
                {
                    Point MouseLocation = Mouse.GetPosition(l);
                    if (MouseLocation.X >= 0 &&
                        MouseLocation.Y >= 0 &&
                        MouseLocation.X <= l.Width &&
                        MouseLocation.Y <= l.Height &&
                        l.IsVisible)
                    {
                        if (!ActivedUserControlWordCard.Contains(l))
                        {
                            l.Focus = true;
                            ActivedUserControlWordCard.Add(l);
                            TextboxWordSelected.Text = l.Tiptool;
                        }
                        else if (MutiChoiceWords)
                        {
                            l.Focus = false;
                            ActivedUserControlWordCard.Remove(l);
                        }
                        break;
                    }
                }
                UserControlWordCard.WaitDrawRefresh();
            }
        }

        private void SaveList(object sender, RoutedEventArgs e)
        {
            SaveList();
            MainPlatomEntrance.SetNotify("Save List Finished!", 2, this);
        }

        private bool MutiChoiceWords = false;

        private bool KeyDownFinished = true;

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (MainClass.MouseInControl(UserControlSplitWordList))
            {
                if (KeyDownFinished)
                {
                    KeyDownFinished = false;
                    if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                    {
                        MutiChoiceWords = false;
                    }
                    KeyDownFinished = true;
                }
            }
        }

        private DateTime KeyDownTime = DateTime.Now;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (MainClass.MouseInControl(UserControlSplitWordList))
            {
                if (KeyDownFinished)
                {
                    if ((DateTime.Now - KeyDownTime).TotalMilliseconds < 100)
                    {
                        switch (e.Key)
                        {
                            case Key.Q:
                                WordDarker(null, null);
                                break;
                            case Key.W:
                                WordLighter(null, null);
                                break;
                            case Key.E:
                                WordRandomLighterDarker(null, null);
                                break;
                        }
                    }
                    else
                    {
                        UserControlSplitWordList.Focus();
                        KeyDownFinished = false;
                        bool Catch = false;
                        for (int i = ActivedUserControlWordCard.Count - 1; i >= 0; i--)
                        {
                            Catch |= ActivedUserControlWordCard[i].FrameworkElement_KeyDown(ActivedUserControlWordCard.Count > 1, e);
                        }
                        if (Catch && ActivedUserControlWordCard.Count == 0)
                        {
                            foreach (UserControlWordCard i in StackPanelWordsList.Children)
                            {
                                if (i.IsVisible)
                                {
                                    ActivedUserControlWordCard.Add(i);
                                    i.Focus = true;
                                    InternetSpeakeRecognize.Speak(i.Word.Spelling);
                                    break;
                                }
                            }
                        }

                        switch (e.Key)
                        {
                            case Key.LeftCtrl:
                                MutiChoiceWords = true;
                                break;
                            case Key.RightCtrl:
                                MutiChoiceWords = true;
                                break;
                            case Key.A:
                                if (MutiChoiceWords == true)
                                {
                                    foreach (UserControlWordCard i in StackPanelWordsList.Children)
                                    {
                                        if (i.IsVisible)
                                        {
                                            if (!ActivedUserControlWordCard.Contains(i))
                                            {
                                                i.Focus = true;
                                                ActivedUserControlWordCard.Add(i);
                                            }
                                        }
                                    }
                                }
                                break;
                            case Key.Z:
                                if (MutiChoiceWords == true)
                                {
                                    foreach (var i in ActivedUserControlWordCard)
                                    {
                                        i.Focus = false;
                                    }
                                    ActivedUserControlWordCard.Clear();
                                }
                                break;
                            case Key.S:
                                ShowRelatedCategorysGroup(null, null);
                                break;
                            case Key.Q:
                                WordDarker(null, null);
                                break;
                            case Key.W:
                                WordLighter(null, null);
                                break;
                            case Key.E:
                                WordRandomLighterDarker(null, null);
                                break;
                            case Key.U:
                                WordsUndo(null, null);
                                break;
                            case Key.C:
                                WordsCount(null, null);
                                break;
                            case Key.Left:
                                ScrollViewerWordsList.ScrollToHorizontalOffset(ScrollViewerWordsList.HorizontalOffset - 1);
                                break;
                            case Key.Right:
                                ScrollViewerWordsList.ScrollToHorizontalOffset(ScrollViewerWordsList.HorizontalOffset + 1);
                                break;
                            case Key.Up:
                                try
                                {
                                    List<UserControlWordCard> TempWordsList = new List<UserControlWordCard>();
                                    foreach (UserControlWordCard i in StackPanelWordsList.Children)
                                    {
                                        if (i.IsVisible)
                                        {
                                            TempWordsList.Add(i);
                                        }
                                    }
                                    List<int> TempL = new List<int>();
                                    foreach (var i in ActivedUserControlWordCard)
                                    {
                                        TempL.Add(TempWordsList.IndexOf(i));
                                    }
                                    TempL.Sort();
                                    if (TempL[0] != 0)
                                    {
                                        UserControlWordCard M = null;
                                        foreach (var i in TempL)
                                        {
                                            M = TempWordsList[i];
                                            M.Focus = false;
                                        }
                                        ActivedUserControlWordCard.Clear();
                                        foreach (var i in TempL)
                                        {
                                            M = TempWordsList[i - 1];
                                            M.Focus = true;
                                            ActivedUserControlWordCard.Add(M);
                                        }
                                        UserControlWordCard.WaitDrawRefresh();
                                        //TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                catch
                                {
                                    MainPlatomEntrance.SetNotify("Failed to switch selecting word", 2, this);
                                }
                                break;
                            case Key.Down:
                                try
                                {
                                    List<UserControlWordCard> TempWordsList = new List<UserControlWordCard>();
                                    foreach (UserControlWordCard i in StackPanelWordsList.Children)
                                    {
                                        if (i.IsVisible)
                                        {
                                            TempWordsList.Add(i);
                                        }
                                    }
                                    List<int> TempL = new List<int>();
                                    foreach (var i in ActivedUserControlWordCard)
                                    {
                                        TempL.Add(TempWordsList.IndexOf(i));
                                    }
                                    TempL.Sort();
                                    if (TempL[TempL.Count - 1] != TempWordsList.Count - 1)
                                    {
                                        UserControlWordCard M = null;
                                        foreach (var i in TempL)
                                        {
                                            M = TempWordsList[i];
                                            M.Focus = false;
                                        }
                                        ActivedUserControlWordCard.Clear();
                                        foreach (var i in TempL)
                                        {
                                            M = TempWordsList[i + 1];
                                            M.Focus = true;
                                            ActivedUserControlWordCard.Add(M);
                                        }
                                        //TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                catch
                                {
                                    MainPlatomEntrance.SetNotify("Failed to switch selecting word", 2, this);
                                }
                                break;
                            case Key.T:
                                if (UserControlWordCard.HiddenTiptool)
                                {
                                    MainPlatomEntrance.SetNotify("Enable Word Tiptool", 2, this);
                                    UserControlWordCard.HiddenTiptool = !UserControlWordCard.HiddenTiptool;
                                }
                                else
                                {
                                    MainPlatomEntrance.SetNotify("Disable Word Tiptood", 2, this);
                                    UserControlWordCard.HiddenTiptool = !UserControlWordCard.HiddenTiptool;
                                }
                                break;
                        }
                        UserControlWordCard.WaitDrawRefresh();
                    }
                    KeyDownFinished = true;
                }
            }
            KeyDownTime = DateTime.Now;
            try
            {
                TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
            }
            catch { }
        }


        //创建一个组
        private void EstablishCategoryGroup(object sender, RoutedEventArgs e)
        {
            EstablishCategoryGroup(ActivedUserControlWordCard.ToArray());
        }

        private void EstablishCategoryGroup(UserControlWordCard[] Words)
        {
            MainClass.ReferenceTypePackaging<string> TypeName;
            var WindowInputInformation = new WindowInputInformation(out TypeName, "Please, Input a Name", "", this);
            MainClass.OpenWindow(WindowInputInformation, this, true);
            if (TypeName.Value != null && TypeName.Value != String.Empty)
            {
                foreach (var m in MainPlatomEntrance.WindowList)
                {
                    foreach (var i in m.WordsControls)
                    {
                        if (i.Word.Categorys.Contains(TypeName.Value))
                        {
                            MainPlatomEntrance.SetNotify("Category Group Exists", 2, this);
                            return;
                        }
                    }
                }
                foreach (var t in Words)
                {
                    t.Word.Categorys.Add(TypeName.Value);
                    foreach (var g in MainPlatomEntrance.WindowList)
                    {
                        if (g.WordsControls.Contains(t))
                        {
                            g.SortWord(t);
                            g.FilterWord(t);
                            UserControlWordCard.WaitDrawRefresh();
                            MainPlatomEntrance.SetNotify("Establish Categorys Group Finished!", 2, g);
                        }
                    }
                }
            }
        }

        //加入一个组
        private void JoinCategorysGroup(object sender, RoutedEventArgs e)
        {
            JoinCategorysGroup(ActivedUserControlWordCard.ToArray());
        }

        private void JoinCategorysGroup(UserControlWordCard[] Words)
        {
            MainClass.ReferenceTypePackaging<int> TypeIndex;
            List<string> Group = new List<string>();
            foreach (var m in MainPlatomEntrance.WindowList)
            {
                foreach (var i in m.WordsControls)
                {
                    foreach (var j in i.Word.Categorys)
                    {
                        if (!Group.Contains(j))
                        {
                            Group.Add(j);
                        }
                    }
                }
            }
            Group.Sort();
            var WindowSelectItem = new WindowSelectItem(out TypeIndex, Group.ToArray(), "Please, Select a Group");
            MainClass.OpenWindow(WindowSelectItem, this, true);
            if (TypeIndex.Value != -1)
            {
                foreach (var t in Words)
                {
                    if (!t.Word.Categorys.Contains(Group[TypeIndex.Value]))
                    {
                        t.Word.Categorys.Add(Group[TypeIndex.Value]);
                        foreach (var g in MainPlatomEntrance.WindowList)
                        {
                            if (g.WordsControls.Contains(t))
                            {
                                g.SortWord(t);
                                g.FilterWord(t);
                                UserControlWordCard.WaitDrawRefresh();
                                MainPlatomEntrance.SetNotify("Join Categorys Group Finished!", 2, g);
                            }
                        }
                    }
                }
            }
        }

        private string[] CommonCategory(UserControlWordCard[] Words)
        {
            List<string> GroupName = new List<string>();
            List<int> GroupNameCount = new List<int>();
            int Index = 0;
            foreach (var l in Words)
            {
                foreach (var j in l.Word.Categorys)
                {
                    Index = GroupName.IndexOf(j);
                    if (Index != -1)
                    {
                        GroupNameCount[Index]++;
                    }
                    else
                    {
                        GroupName.Add(j);
                        GroupNameCount.Add(1);
                    }
                }
            }
            for (int i = GroupNameCount.Count - 1; i >= 0; i--)
            {
                if (GroupNameCount[i] != Words.Length)
                {
                    GroupName.RemoveAt(i);
                }
            }
            return GroupName.ToArray();
        }
        //离开一个组
        private void LeaveCategorysGroup(object sender, RoutedEventArgs e)
        {
            LeaveCategorysGroup(ActivedUserControlWordCard.ToArray());
        }

        private void LeaveCategorysGroup(UserControlWordCard[] Words)
        {
            var GroupName = CommonCategory(Words.ToArray());
            MainClass.ReferenceTypePackaging<int> TypeIndex;
            var WindowSelectItem = new WindowSelectItem(out TypeIndex, GroupName, "Please, Select a Group");
            MainClass.OpenWindow(WindowSelectItem, this, true);
            if (TypeIndex.Value != -1)
            {
                foreach (var t in Words)
                {
                    t.Word.Categorys.Remove(GroupName[TypeIndex.Value]);
                    foreach (var g in MainPlatomEntrance.WindowList)
                    {
                        if (g.WordsControls.Contains(t))
                        {
                            g.SortWord(t);
                            g.FilterWord(t);
                            UserControlWordCard.WaitDrawRefresh();
                            MainPlatomEntrance.SetNotify("Leave Categorys Group Finished!", 2, g);
                        }
                    }
                }
            }
        }

        //解散一个组
        private void DismissCategorysGroup(object sender, RoutedEventArgs e)
        {
            DismissCategorysGroup(ActivedUserControlWordCard.ToArray());
        }

        private void DismissCategorysGroup(UserControlWordCard[] Words)
        {
            var GroupName = CommonCategory(Words.ToArray());
            MainClass.ReferenceTypePackaging<int> TypeIndex;
            var WindowSelectItem = new WindowSelectItem(out TypeIndex, GroupName, "Please, Select a Group");
            MainClass.OpenWindow(WindowSelectItem, this, true);
            if (TypeIndex.Value != -1)
            {
                int Index = 0;
                foreach (var m in MainPlatomEntrance.WindowList)
                {
                    foreach (var l in m.WordsControls)
                    {
                        Index = l.Word.Categorys.IndexOf(GroupName[TypeIndex.Value]);
                        if (Index != -1)
                        {
                            l.Word.Categorys.RemoveAt(Index);
                            m.SortWord(l);
                            m.FilterWord(l);
                        }
                    }
                    UserControlWordCard.WaitDrawRefresh();
                    MainPlatomEntrance.SetNotify("Dismiss Categorys Group Finished!", 2, m);
                }
            }
        }

        private void RenameCategorysGroup(object sender, RoutedEventArgs e)
        {
            RenameCategorysGroup(ActivedUserControlWordCard.ToArray());
        }

        private void RenameCategorysGroup(UserControlWordCard[] Words)
        {
            var GroupName = CommonCategory(Words.ToArray());
            MainClass.ReferenceTypePackaging<int> TypeIndex;
            var WindowSelectItem = new WindowSelectItem(out TypeIndex, GroupName, "Please, Select a Group");
            MainClass.OpenWindow(WindowSelectItem, this, true);
            if (TypeIndex.Value != -1)
            {
                MainClass.ReferenceTypePackaging<string> TypeName;
                var WindowInputInformation = new WindowInputInformation(out TypeName, "Please, Input a Name", GroupName[TypeIndex.Value], this);
                MainClass.OpenWindow(WindowInputInformation, this, true);
                if (TypeName.Value != null && TypeName.Value != String.Empty)
                {
                    foreach (var m in MainPlatomEntrance.WindowList)
                    {
                        foreach (var i in m.WordsControls)
                        {
                            if (i.Word.Categorys.Contains(TypeName.Value))
                            {
                                MainPlatomEntrance.SetNotify("Category Group Exists", 2, this);
                                return;
                            }
                        }
                    }
                    int Index = 0;
                    foreach (var m in MainPlatomEntrance.WindowList)
                    {
                        foreach (var i in m.WordsControls)
                        {
                            Index = i.Word.Categorys.IndexOf(GroupName[TypeIndex.Value]);
                            if (Index != -1)
                            {
                                i.Word.Categorys[Index] = TypeName.Value;
                                m.SortWord(i);
                                m.FilterWord(i);
                            }
                        }
                        UserControlWordCard.WaitDrawRefresh();
                        MainPlatomEntrance.SetNotify("Rename Categorys Group Finished!", 2, m);
                    }
                }
            }
        }

        private void ShowRelatedCategorysGroup(object sender, RoutedEventArgs e)
        {
            ShowRelatedCategorysGroup(ActivedUserControlWordCard.ToArray());
        }

        private void ShowRelatedCategorysGroup(UserControlWordCard[] Words)
        {
            var GroupName = CommonCategory(Words.ToArray());
            MainClass.ReferenceTypePackaging<int> TypeIndex;
            var WindowSelectItem = new WindowSelectItem(out TypeIndex, GroupName, "Please, Select a Group");
            MainClass.OpenWindow(WindowSelectItem, this, true);
            if (TypeIndex.Value != -1)
            {
                foreach (var m in MainPlatomEntrance.WindowList)
                {
                    foreach (var k in Words)
                    {
                        if (m.WordsControls.Contains(k))
                        {
                            m.BackupConditions();
                            m.ListBoxFliterConditions.Items.Clear();
                            var FilterCondition = new FilterCondition(m.BackupConditions);
                            FilterCondition.SeletedItem = ConditionTypes.Categorys_Equal;
                            FilterCondition.Value = GroupName[TypeIndex.Value];
                            FilterCondition.ListFlipIndex = (int)ConditionFlipTypes.Follow;
                            ListBoxFliterConditions.Items.Add(FilterCondition);
                            FilterWords();
                            UserControlWordCard.WaitDrawRefresh();
                            MainPlatomEntrance.SetNotify("Conditions Establishing Finished!", 2, m);
                            continue;
                        }
                    }
                }
            }
        }

        public void FilterWords()
        {
            MainClass.ParallelTasks Tasks = new MainClass.ParallelTasks();
            foreach (UserControlWordCard l in StackPanelWordsList.Children)
            {
                Tasks.AddTask(new Task((InputValue) =>
                {
                    FilterWord((UserControlWordCard)InputValue);
                }, l));
            }
            Tasks.WaitFinished();
            MainPlatomEntrance.SetNotify("Filter Finished!", 2, this);
            //启动同步
            /*
            var Copy = new Tuple<ConditionTypes, string, ConditionFlipTypes>[ListBoxFliterConditions.Items.Count];
            FilterCondition Temp = null;
            for (int i = 0; i < ListBoxFliterConditions.Items.Count; i++)
            {
                Temp = (FilterCondition)ListBoxFliterConditions.Items[i];
                Copy[i] = new Tuple<ConditionTypes, string, ConditionFlipTypes>(Temp.SeletedItem, Temp.Value, Temp.ListFlipIndex);
            }
            foreach (var i in MainPlatomEntrance.WindowList)
            {
                if (i != this)
                {
                    i.InputCondition(Copy);
                }
            }
            */
        }

        public void InputCondition(Tuple<ConditionTypes, string, ConditionFlipTypes>[] List)
        {
            BackupConditions();
            ListBoxFliterConditions.Items.Clear();
            foreach(var i in List)
            {
                ListBoxFliterConditions.Items.Add(new FilterCondition(BackupConditions, i.Item1, i.Item2, i.Item3));
            }
            FilterWords();
        }

        private void AddCondition(object sender, RoutedEventArgs e)
        {
            BackupConditions();
            ListBoxFliterConditions.Items.Add(new FilterCondition(BackupConditions));
            FilterWords();
        }

        private void DeleteCondition(object sender, RoutedEventArgs e)
        {
            BackupConditions();
            int Index = ListBoxFliterConditions.SelectedIndex;
            if (Index != -1)
            {
                ListBoxFliterConditions.Items.RemoveAt(Index);
                FilterWords();
            }
        }

        private void ClearConditions(object sender, RoutedEventArgs e)
        {
            BackupConditions();
            ListBoxFliterConditions.Items.Clear();
            FilterWords();
        }

        private void FliterConditonIndexChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FilterWords();
        }

        private void CheckBox_FliterConditonReverseChanged(object sender, RoutedEventArgs e)
        {
            FilterWords();
        }

        private void WordDarker(object sender, RoutedEventArgs e)
        {
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].SwitchOpacity(true);
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void WordRandomLighterDarker(object sender, RoutedEventArgs e)
        {
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].SwitchOpacity(null);
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void WordLighter(object sender, RoutedEventArgs e)
        {
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].SwitchOpacity(false);
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        Dictionary<UserControlWordCard, MainClass.ReferenceTypePackaging<Translation.SymbolsBaidu>> TranslationResults = new Dictionary<UserControlWordCard, MainClass.ReferenceTypePackaging<Translation.SymbolsBaidu>>();

        //下载意思多线程
        private readonly Translation Translation = new Translation();
        private bool FillingMeaningsTaskCanel = false;
        private Thread FillingMeaningsTask = null;

        private List<string> NotifyMessage = new List<string>();
        private List<Tuple<WordListClass.WordClass, object>> FillingControl = new List<Tuple<WordListClass.WordClass, object>>();
        private void FindAllEmptyMeanings()
        {
            FillingMeaningsTaskCanel = false;
            if (FillingMeaningsTask == null)
            {
                FillingMeaningsTask = new Thread(() =>
                {
                    foreach (var i in WordsList.Words)
                    {
                        if (i.Meanings.Count < 2)
                        {
                            var Item = Translation.GetTranslation(i.Spelling);
                            if (Item != null)
                            {
                                lock (FillingControl)
                                {
                                    FillingControl.Add(new Tuple<WordListClass.WordClass, object>(i, Item));
                                }
                            }
                        }
                        if ((FillingMeaningsTaskCanel) || (MainPlatomEntrance.Setting.AutoFillMeanings == false))
                        {
                            return;
                        }
                    }
                    lock (NotifyMessage)
                    {
                        NotifyMessage.Add("Finshed All Meanings Filling");
                    }
                });
                FillingMeaningsTask.Start();
            }
            else if (FillingMeaningsTask.ThreadState != System.Threading.ThreadState.Running)
            {
                FillingMeaningsTask = null;
            }
            lock (FillingControl)
            {
                if (FillingControl.Count != 0)
                {
                    foreach (var i in FillingControl)
                    {
                        foreach (var ii in WordsControls)
                        {
                            if (ii.Word == i.Item1)
                            {
                                if(i.Item2 is Translation.SymbolsBaidu)
                                {
                                    FillWordMeanings(ii, (Translation.SymbolsBaidu)i.Item2);
                                }
                                else if(i.Item2 is Translation.SymbolsYoudao)
                                {
                                    FillWordMeanings(ii, (Translation.SymbolsYoudao)i.Item2);
                                }
                                break;
                            }
                        }
                    }
                    FillingControl.Clear();
                    FillingControl.TrimExcess();
                }
            }
        }

        //下载例句发音
        private bool FillingPronunciationsTaskCanel = false;
        private Thread FillingPronunciationsTask = null;
        private void FindAllEmptyPronunciations()
        {
            FillingPronunciationsTaskCanel = false;
            if (FillingPronunciationsTask == null)
            {
                FillingPronunciationsTask = new Thread(() =>
                {
                    string WordPath = null;
                    string WordDirectory = null;
                    byte[] Data = null;
                    foreach (var i in WordsList.Words)
                    {
                        WordPath = MainWindow.WordsPackage + i.Spelling[0].ToString() + "\\" + i.Spelling + "\\Pronunciation.kl";
                        if (File.Exists(WordPath) == false)
                        {
                            Data = InternetSpeakeRecognize.DownloadSpeak(i.Spelling);
                            if (!(Data == null || Data.Length == 0))
                            {
                                WordDirectory = Path.GetDirectoryName(WordPath);
                                if (!Directory.Exists(WordDirectory))
                                {
                                    Directory.CreateDirectory(WordDirectory);
                                }
                                File.WriteAllBytes(WordPath, Data);
                            }
                        }
                        if ((FillingPronunciationsTaskCanel) || (MainPlatomEntrance.Setting.AutoFillPronunciations == false))
                        {
                            return;
                        }
                    }
                    lock (NotifyMessage)
                    {
                        NotifyMessage.Add("Finshed All Pronunciations Filling");
                    }
                });
                FillingPronunciationsTask.Start();
            }
            else if (FillingPronunciationsTask.ThreadState != System.Threading.ThreadState.Running)
            {
                FillingPronunciationsTask = null;
            }
        }

        //下载例句多线程
        private bool FillingExamplesTaskCanel = false;
        private Thread FillingExamplesTask = null;
        public readonly DownloadExamples Examples = new DownloadExamples(WordsPackage);
        private void FindAllEmptyExamples()
        {
            FillingExamplesTaskCanel = false;
            if (FillingExamplesTask == null)
            {
                FillingExamplesTask = new Thread(() =>
                {
                    foreach (var i in WordsList.Words)
                    {
                        if (Examples.Download(i.Spelling) == false)
                        {
                        }
                        if ((FillingExamplesTaskCanel) || (MainPlatomEntrance.Setting.AutoFillExamples == false))
                        {
                            return;
                        }
                    }
                    lock (NotifyMessage)
                    {
                        NotifyMessage.Add("Finshed All Examples Filling");
                    }
                });
                FillingExamplesTask.Start();
            }
            else if (FillingExamplesTask.ThreadState != System.Threading.ThreadState.Running)
            {
                FillingExamplesTask = null;
            }
        }

        //下载图片多线程
        private bool FillingPicturesTaskCanel = false;
        private Thread FillingPicturesTask = null;
        public readonly DownloadPictures Pictures = new DownloadPictures(WordsPackage);
        private void FindAllEmptyPictures()
        {
            FillingPicturesTaskCanel = false;
            if (FillingPicturesTask == null)
            {
                FillingPicturesTask = new Thread(() =>
                {
                    foreach (var i in WordsList.Words)
                    {
                        if (Pictures.Download(i.Spelling) == false)
                        {
                            lock (NotifyMessage)
                            {
                                NotifyMessage.Add(string.Format("Fail to Filling Pictures on [{0}]", i.Spelling));
                            }
                        }
                        if ((FillingPicturesTaskCanel) || (MainPlatomEntrance.Setting.AutoFillPictures == false))
                        {
                            return;
                        }
                    }
                    lock (NotifyMessage)
                    {
                        NotifyMessage.Add("Finshed All Pictures Filling");
                    }
                });
                FillingPicturesTask.Start();
            }
            else if (FillingPicturesTask.ThreadState != System.Threading.ThreadState.Running)
            {
                FillingPicturesTask = null;
            }
        }

        private void FasterTimer_Tick(object sender, EventArgs e)
        {
            if (ListPath != null)
            {
                if (MainPlatomEntrance.Setting.AutoFillMeanings)
                {
                    FindAllEmptyMeanings();
                }
                if(MainPlatomEntrance.Setting.AutoFillPronunciations)
                {
                    FindAllEmptyPronunciations();
                }
                if (MainPlatomEntrance.Setting.AutoFillExamples)
                {
                    FindAllEmptyExamples();
                }
                if (MainPlatomEntrance.Setting.AutoFillPictures)
                {
                    FindAllEmptyPictures();
                }
                lock (NotifyMessage)
                {
                    foreach (var i in NotifyMessage)
                    {
                        MainPlatomEntrance.SetNotify(i, 2, this);
                    }
                    NotifyMessage.Clear();
                }
            }
        }

        private void ReviewWords(object sender, RoutedEventArgs e)
        {
            ReviewWords(ActivedUserControlWordCard.ToArray());
        }

        private void ReviewWords(UserControlWordCard[] Words)
        {
            if (Words.Length == 0)
            {
                MainPlatomEntrance.SetNotify("No Words can be reviewed!", 2, this);
            }
            else
            {
                var Lists = new List<WordListClass.WordClass>();
                foreach (var i in Words)
                {
                    Lists.Add(i.Word);
                }
                var WindowVocabularyReview = new WindowVocabularyReview(Lists, this);
                MainClass.OpenWindow(WindowVocabularyReview, this, false);
            }
        }

        private void TestWordsMeanings(object sender, RoutedEventArgs e)
        {
            var Lists = new List<UserControlWordCard>();
            foreach (var i in ActivedUserControlWordCard)
            {
                Lists.Add(i);
            }
            if (Lists.Count == 0)
            {
                MainPlatomEntrance.SetNotify("No Words can be Testd!", 2, this);
            }
            else
            {
                var WindowVocabularyTestMeanings = new WindowVocabularyTestMeanings(Lists, WordsList, this);
                MainClass.OpenWindow(WindowVocabularyTestMeanings, this, false);
            }
        }

        private void TestWordsExampless(object sender, RoutedEventArgs e)
        {
            var Lists = new List<UserControlWordCard>();
            foreach (var i in ActivedUserControlWordCard)
            {
                Lists.Add(i);
            }
            if (Lists.Count == 0)
            {
                MainPlatomEntrance.SetNotify("No Words can be Testd!", 2, this);
            }
            else
            {
                var WindowVocabularyTestExamples = new WindowVocabularyTestFilling(Lists, WordsList, this);
                MainClass.OpenWindow(WindowVocabularyTestExamples, this, false);
            }
        }

        private void CheckBox_FliterConditonFlipChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterWords();
        }

        private void RadioButton_SortWords(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxOrderTypes.SelectedIndex == ComboBoxOrderTypes.Items.Count - 1)
            {
                SequenceType = null;
            }
            else if (ComboBoxOrderTypes.SelectedIndex == ComboBoxOrderTypes.Items.Count - 2)
            {
                SequenceType = -1;
            }
            else
            {
                SequenceType = ComboBoxOrderTypes.SelectedIndex / 2;
                SequenceTypeReverse = ((ComboBoxOrderTypes.SelectedIndex % 2) == 1);
            }
            if (ListPath != null)
            {
                SortWords();
            }
        }

        private const string InstructionSign = ">>";
        private void TextBoxFreeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyDownFinished)
            {
                KeyDownFinished = false;
                if (e.Key == Key.Enter)
                {
                    TextBoxFreeInputFindWord();
                }
                KeyDownFinished = true;
            }
        }


        private void TextBoxFreeInputFindWord()
        {
            int Index = TextboxWordSearch.Text.LastIndexOf(InstructionSign);
            bool Found = false;
            string Instruction = null;
            if (Index != -1 && (Instruction = TextboxWordSearch.Text.Substring(Index + InstructionSign.Length).Trim()) != String.Empty)
            {
                bool Command = false;
                int CommandIndex = 0;
                foreach (var l in InputCommand)
                {
                    if(Instruction.ToLower().StartsWith(l))
                    {
                        Instruction = Instruction.Substring(l.Length);
                        List<string> SpellingList = new List<string>();
                        List<UserControlWordCard> WordCard = new List<UserControlWordCard>();
                        int SpaceIndex = 0;
                        while ((SpaceIndex = Instruction.IndexOf(" "))!=-1)
                        {
                            SpellingList.Add(Instruction.Substring(0, SpaceIndex));
                            Instruction = Instruction.Substring(SpaceIndex + 1).Trim();
                        }
                        SpellingList.Add(Instruction);
                        foreach(var m in MainPlatomEntrance.WindowList)
                        {
                            foreach(var k in m.WordsControls)
                            {
                                foreach (var g in SpellingList)
                                {
                                    if (k.Word.Spelling == g)
                                    {
                                        WordCard.Add(k);
                                        break;
                                    }
                                }
                            }
                        }
                        switch (CommandIndex)
                        {
                            case 0:
                                TextboxWordSearch.Text += "\n\nReview Words";
                                ReviewWords(WordCard.ToArray());
                                break;
                            case 1:
                                TextboxWordSearch.Text += "\n\nSet Tag";
                                SetWordTag(WordCard.ToArray());
                                break;
                            case 2:
                                TextboxWordSearch.Text += "\n\nFlip Words";
                                HideShowWord(WordCard.ToArray());
                                break;


                            case 0 + 3:
                                TextboxWordSearch.Text += "\n\nEstablish A Category";
                                EstablishCategoryGroup(WordCard.ToArray());
                                break;
                            case 1 + 3:
                                TextboxWordSearch.Text += "\n\nJoin A Category";
                                JoinCategorysGroup(WordCard.ToArray());
                                break;
                            case 2 + 3:
                                TextboxWordSearch.Text += "\n\nLeave A Category";
                                LeaveCategorysGroup(WordCard.ToArray());
                                break;
                            case 3 + 3:
                                TextboxWordSearch.Text += "\n\nDismiss A Category";
                                DismissCategorysGroup(WordCard.ToArray());
                                break;
                            case 4 + 3:
                                TextboxWordSearch.Text += "\n\nShow Related Category";
                                ShowRelatedCategorysGroup(WordCard.ToArray());
                                break;
                            case 5 + 3:
                                TextboxWordSearch.Text += "\n\nShow Related Category";
                                ShowRelatedCategorysGroup(WordCard.ToArray());
                                break;
                            case 6 + 3:
                                TextboxWordSearch.Text += "\n\nShow Related Category";
                                ShowRelatedCategorysGroup(WordCard.ToArray());
                                break;
                        }
                        TextboxWordSearch.Text += "\n\n" + InstructionSign;
                        Command = true;
                        break;
                    }
                    CommandIndex++;
                }
                if (Command == false)
                {
                    StringBuilder Text = new StringBuilder();
                    Text.AppendLine();
                    Text.AppendLine();
                    bool Chinese = MainClass.CheckEncode(Instruction);
                    foreach (var m in MainPlatomEntrance.WindowList)
                    {
                        foreach (var i in m.WordsControls)
                        {
                            if ((Chinese == false && i.Word.Spelling == Instruction) || (Chinese == true && i.Word.Meanings.Contains(Instruction)))
                            {
                                Text.AppendLine(i.Tiptool);
                                TextboxWordSearch.Text += Text.ToString();
                                if (m == this)
                                {
                                    for (int n = ActivedUserControlWordCard.Count - 1; n >= 0; n--)
                                    {
                                        ActivedUserControlWordCard[n].Focus = false;
                                    }
                                    ActivedUserControlWordCard.Clear();
                                    i.Word.Tag = "Searched";
                                    FilterWord(i);
                                    if (i.Word.Visible ||
                                        i.Word.Visible == false && Binding_Data.CollapseRotatedWordPanel == false)
                                    {
                                        ActivedUserControlWordCard.Add(i);
                                        i.Focus = true;
                                        UserControlWordCard.WaitDrawRefresh();
                                        ScrollViewerWordsList.ScrollToHorizontalOffset(VisualTreeHelper.GetOffset(i).X);
                                        //MainPlatomEntrance.SetNotify("Jump to Word", 2, this);
                                        MainClass.PlaySoundPath(WordsPackage + Instruction[0].ToString() + "\\" + Instruction + "\\" + "Pronunciation.kl", false);
                                    }
                                    else
                                    {
                                        //MainPlatomEntrance.SetNotify("Word is collapsed", 2, this);
                                        MainClass.PlaySoundPath(WordsPackage + Instruction[0].ToString() + "\\" + Instruction + "\\" + "Pronunciation.kl", false);
                                    }
                                }
                                else
                                {
                                    //MainPlatomEntrance.SetNotify("Word is outside", 2, this);
                                    MainClass.PlaySoundPath(WordsPackage + Instruction[0].ToString() + "\\" + Instruction + "\\" + "Pronunciation.kl", false);
                                }
                                Found = true;
                                goto End;
                            }
                        }
                    }
                End:
                    if (Found == false)
                    {
                        int WordCount = 0;
                        foreach (var m in MainPlatomEntrance.WindowList)
                        {
                            WordCount += m.WordsControls.Count;
                        }
                        decimal[] Similarity = new decimal[WordCount];
                        string[] Spell = new string[WordCount];
                        int IndexT = 0;
                        string Spelling = null;
                        foreach (var m in MainPlatomEntrance.WindowList)
                        {
                            foreach (var i in m.WordsControls)
                            {
                                Spelling = i.Word.Spelling;
                                Spell[IndexT] = Spelling;
                                if (!Chinese)
                                {
                                    Similarity[IndexT] = MainClass.ComputeStringSimilarity(Spelling, Instruction);
                                }
                                else
                                {
                                    foreach (var k in i.Word.Meanings)
                                    {
                                        if (k.Contains(Instruction))
                                        {
                                            Similarity[IndexT] = 1;
                                            break;
                                        }
                                    }
                                }
                                IndexT++;
                            }
                        }
                        Array.Sort(Similarity, Spell);
                        Text.AppendLine("[Similar]");
                        for (int i = 0; (Chinese == false && i < 10) || (Chinese == true && Similarity[Spell.Length - 1 - i] == 1) && (i < Spell.Length); i++)
                        {
                            Text.AppendLine(Spell[Spell.Length - 1 - i]);
                        }
                        TextboxWordSearch.Text += Text.ToString();
                    }
                    TextboxWordSearch.Text += "\n" + InstructionSign;
                }
            }
            else
            {
                TextboxWordSearch.Text = InstructionSign;
            }
            TextboxWordSearch.SelectionStart = TextboxWordSearch.Text.Length;
            TextboxWordSearch.ScrollToEnd();
        }

        private void ScrollViewerWordsList_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewerWordsList.ScrollToHorizontalOffset(ScrollViewerWordsList.HorizontalOffset - e.Delta);
        }

        private void WordsUndo(object sender, RoutedEventArgs e)
        {
            string[] Spelling = WordsList.UndoWord();
            if (Spelling.Length != 0)
            {
                foreach (var j in Spelling)
                {
                    foreach (var i in WordsControls)
                    {
                        if (i.Word.Spelling == j)
                        {
                            SortWord(i);
                            FilterWord(i);
                            break;
                        }
                    }
                }
                //MainPlatomEntrance.SetNotify("Undo Succeed!", 2, this);
            }
            else
            {
                //MainPlatomEntrance.SetNotify("No history to undo!", 2, this);
            }
        }

        private void ScrollViewerWordsList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (MainClass.MouseInControl(UserControlSplitWordList))
            {
                if (KeyDownFinished)
                {
                    KeyDownFinished = false;
                    switch (e.Key)
                    {
                        case Key.Left:
                            ScrollViewerWordsList.ScrollToHorizontalOffset(ScrollViewerWordsList.HorizontalOffset - 1);
                            break;
                        case Key.Right:
                            ScrollViewerWordsList.ScrollToHorizontalOffset(ScrollViewerWordsList.HorizontalOffset + 1);
                            break;
                        case Key.Up:
                            try
                            {
                                List<int> TempL = new List<int>();
                                List<UserControlWordCard> VisibleWords = new List<UserControlWordCard>();
                                foreach (UserControlWordCard i in StackPanelWordsList.Children)
                                {
                                    if (i.IsVisible || Binding_Data.CollapseRotatedWordPanel == false)
                                    {
                                        VisibleWords.Add(i);
                                    }
                                }
                                foreach (var i in ActivedUserControlWordCard)
                                {
                                    TempL.Add(VisibleWords.IndexOf(i));
                                }
                                if (TempL.Count != 0)
                                {
                                    TempL.Sort();
                                    if (TempL[0] != 0)
                                    {
                                        UserControlWordCard M = null;
                                        foreach (var i in TempL)
                                        {
                                            M = VisibleWords[i];
                                            M.Focus = false;
                                        }
                                        ActivedUserControlWordCard.Clear();
                                        foreach (var i in TempL)
                                        {
                                            M = VisibleWords[i - 1];
                                            M.Focus = true;
                                            ActivedUserControlWordCard.Add(M);
                                        }
                                        UserControlWordCard.WaitDrawRefresh();

                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    ActivedUserControlWordCard.Add(VisibleWords[VisibleWords.Count - 1]);
                                    ActivedUserControlWordCard[0].Focus = true;
                                }
                                //TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
                            }
                            catch
                            {
                                //MainPlatomEntrance.SetNotify("Failed to switch selecting word", 2, this);
                            }
                            break;
                        case Key.Down:
                            try
                            {
                                List<int> TempL = new List<int>();
                                List<UserControlWordCard> VisibleWords = new List<UserControlWordCard>();
                                foreach (UserControlWordCard i in StackPanelWordsList.Children)
                                {
                                    if (i.IsVisible || Binding_Data.CollapseRotatedWordPanel == false)
                                    {
                                        VisibleWords.Add(i);
                                    }
                                }
                                foreach (var i in ActivedUserControlWordCard)
                                {
                                    TempL.Add(VisibleWords.IndexOf(i));
                                }
                                if (TempL.Count != 0)
                                {
                                    TempL.Sort();
                                    if (TempL[TempL.Count - 1] != VisibleWords.Count - 1)
                                    {
                                        UserControlWordCard M = null;
                                        foreach (var i in TempL)
                                        {
                                            M = VisibleWords[i];
                                            M.Focus = false;
                                        }
                                        ActivedUserControlWordCard.Clear();
                                        foreach (var i in TempL)
                                        {
                                            M = VisibleWords[i + 1];
                                            M.Focus = true;
                                            ActivedUserControlWordCard.Add(M);
                                        }
                                        UserControlWordCard.WaitDrawRefresh();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    ActivedUserControlWordCard.Add(VisibleWords[0]);
                                    ActivedUserControlWordCard[0].Focus = true;
                                }
                                //TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
                            }
                            catch
                            {
                                //MainPlatomEntrance.SetNotify("Failed to switch selecting word", 2, this);
                            }
                            break;
                    }
                    KeyDownFinished = true;
                }
            }
            try
            {
                TextboxWordSelected.Text = ActivedUserControlWordCard[0].Tiptool;
            }
            catch { }
        }

        private void WordsSkipForever(object sender, RoutedEventArgs e)
        {
            //MainPlatomEntrance.SetNotify(string.Format("Reviewed: {0} words", ActivedUserControlWordCard.Count), 2, this);
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].TextBlock_MouseUpMaxViewedMark();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void ExportConditions(object sender, RoutedEventArgs e)
        {
            var SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            SaveFileDialog.Title = "Please,input a name";
            SaveFileDialog.InitialDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\" + ConditionsPath;
            SaveFileDialog.Filter = "co type(*.co)|*.co";
            if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var Lists = new Tuple<ConditionTypes, string, ConditionFlipTypes>[ListBoxFliterConditions.Items.Count];
                    FilterCondition Temp = null;
                    for (int i = 0; i < ListBoxFliterConditions.Items.Count; i++)
                    {
                        Temp = (FilterCondition)ListBoxFliterConditions.Items[i];
                        Lists[i] = new Tuple<ConditionTypes, string, ConditionFlipTypes>(Temp.SeletedItem, Temp.Value, Temp.ListFlipIndex);
                    }
                    File.WriteAllBytes(SaveFileDialog.FileName, MainClass.CloneToBytes(Lists));
                }
                catch { }
            }
        }

        private void ImportConditions(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Please,select a file";
            fileDialog.InitialDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\" + ConditionsPath;
            fileDialog.Filter = "co type(*.co)|*.co";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    BackupConditions();
                    ListBoxFliterConditions.Items.Clear();
                    FilterCondition Add = null;
                    foreach (var Item in (Tuple<ConditionTypes, string, ConditionFlipTypes>[])MainClass.BytesToClone(File.ReadAllBytes(fileDialog.FileName)))
                    {
                        Add = new FilterCondition(BackupConditions, Item.Item1, Item.Item2, Item.Item3);
                        ListBoxFliterConditions.Items.Add(Add);
                    }
                    FilterWords();
                }
                catch { }
            }
        }

        private void TextBox_FliterConditonTextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            FilterWords();
        }

        private readonly BackupWordsList<Tuple<ConditionTypes, string, ConditionFlipTypes>[]> ConditionsBackUp = new BackupWordsList<Tuple<ConditionTypes, string, ConditionFlipTypes>[]>();
        private void UndoConditions(object sender, RoutedEventArgs e)
        {
            if (ConditionsBackUp.Count > 0)
            {
                ListBoxFliterConditions.Items.Clear();
                foreach(var i in ConditionsBackUp.Pop().Item2)
                {
                    ListBoxFliterConditions.Items.Add(new FilterCondition(BackupConditions, i.Item1, i.Item2, i.Item3));
                }
                FilterWords();
                //MainPlatomEntrance.SetNotify("Undo Successfully", 2, this);
            }
            else
            {
                //MainPlatomEntrance.SetNotify("No Action to Undo", 2, this);
            }
        }

        private void BackupConditions()
        {
            var Backup = new Tuple<ConditionTypes, string, ConditionFlipTypes>[ListBoxFliterConditions.Items.Count];
            FilterCondition Temp = null;
            for (int i = 0; i < ListBoxFliterConditions.Items.Count; i++)
            {
                Temp = (FilterCondition)ListBoxFliterConditions.Items[i];
                Backup[i] = new Tuple<ConditionTypes, string, ConditionFlipTypes>(Temp.SeletedItem, Temp.Value, Temp.ListFlipIndex);
            }
            ConditionsBackUp.Push(Backup);
        }

        private void CancelStrongReview(object sender, RoutedEventArgs e)
        {
            //MainPlatomEntrance.SetNotify(string.Format("Cancel Strong Review: {0} words", ActivedUserControlWordCard.Count), 2, this);
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].MenuItem_MouseUpCancelStrongReview();
            }
            UserControlWordCard.WaitDrawRefresh();
        }

        private void StrongReview(object sender, RoutedEventArgs e)
        {
            //MainPlatomEntrance.SetNotify(string.Format("Strong Review: {0} words", ActivedUserControlWordCard.Count), 2, this);
            for (int l = ActivedUserControlWordCard.Count - 1; l >= 0; l--)
            {
                ActivedUserControlWordCard[l].MenuItem_MouseUpStrongReview();
            }
            UserControlWordCard.WaitDrawRefresh();
        }
    }
}
