using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Threading;
using NAudio.Wave;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace WPF
{
    [Serializable]
    public class ListO<T> : List<T>
    {
        private T[] CopySelf()
        {
            return (T[])MainClass.BytesToClone(MainClass.CloneToBytes(base.ToArray()));
        }

        public new T this[int Index]
        {
            get
            {
                return base[Index];
            }
            set
            {
                var Old = CopySelf();
                base[Index] = value;
                Action.Invoke(Old);
            }
        }

        public ListO(Action<T[]> ValueChanged)
        {
            Action = ValueChanged;
        }

        private readonly Action<T[]> Action;

        public new void Add(T Item)
        {
            var Old = CopySelf();
            base.Add(Item);
            Action.Invoke(Old);
        }

        public new void AddRange(IEnumerable<T> Items)
        {
            var Old = CopySelf();
            base.AddRange(Items);
            Action.Invoke(Old);
        }

        public new void InsertRange(int Index, IEnumerable<T> Items)
        {
            var Old = CopySelf();
            base.InsertRange(Index, Items);
            Action.Invoke(Old);
        }

        public new void Insert(int Index, T Item)
        {
            var Old = CopySelf();
            base.Insert(Index, Item);
            Action.Invoke(Old);
        }

        public new void Remove(T Item)
        {
            var Old = CopySelf();
            base.Remove(Item);
            Action.Invoke(Old);
        }

        public new void RemoveAt(int Index)
        {
            var Old = CopySelf();
            base.RemoveAt(Index);
            Action.Invoke(Old);
        }

        public new void RemoveRange(int Index, int Count)
        {
            var Old = CopySelf();
            base.RemoveRange(Index, Count);
            Action.Invoke(Old);
        }


        public new void Clear()
        {
            var Old = CopySelf();
            base.Clear();
            Action.Invoke(Old);
        }

        public new void Sort()
        {
            var Old = CopySelf();
            base.Sort();
            Action.Invoke(Old);
        }

        public new void Reverse()
        {
            var Old = CopySelf();
            base.Reverse();
            Action.Invoke(Old);
        }
    }
    public class BackupWordsList<T> : IDisposable
    {
        public void Dispose()
        {
            foreach (var i in HistroyPathList)
            {
                try
                {
                    File.Delete(i);
                }
                catch { }
            }
        }

        private static bool Inited = false;
        public BackupWordsList()
        {
            try
            {
                if (Inited == false)
                {
                    string DirectoryName = Path.GetDirectoryName(HistroyPath);
                    if (Directory.Exists(DirectoryName))
                    {
                        Directory.Delete(DirectoryName, true);
                    }
                    Directory.CreateDirectory(DirectoryName);
                    Inited = true;
                }
            }
            catch { }
        }

        //最多记录100000条
        private readonly List<string> HistroyPathList = new List<string>();
        private const string HistroyPath = "History\\Words Status\\{0}.hi";
        private const int MaxCount = 1000;
        private int HistrotyCount = 0;
        private readonly List<Tuple<DateTime, T>> Items = new List<Tuple<DateTime, T>>();
        public void Push(T Value)
        {
            if (Items.Count > MaxCount)
            {
                string NewFile = null;
                int HistoryIndex = 0;
                do
                {
                    NewFile = string.Format(HistroyPath, HistoryIndex++);
                }
                while (File.Exists(NewFile));

                File.WriteAllBytes(NewFile, MainClass.CloneToBytes(Items.ToArray()));
                HistroyPathList.Add(NewFile);
                Items.Clear();
                Items.TrimExcess();
            }
            Items.Add(new Tuple<DateTime, T>(DateTime.Now, Value));
            HistrotyCount++;
        }

        public Tuple<DateTime, T> Pop()
        {
            var Return = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            HistrotyCount--;
            if (HistroyPathList.Count != 0 && Items.Count == 0)
            {
                string Path = HistroyPathList[HistroyPathList.Count - 1];
                HistroyPathList.RemoveAt(HistroyPathList.Count - 1);
                Items.AddRange((Tuple<DateTime, T>[])MainClass.BytesToClone(File.ReadAllBytes(Path)));
                File.Delete(Path);
            }
            return Return;
        }

        public Tuple<DateTime, T> Peek()
        {
            return Items[Items.Count - 1];
        }

        public int Count
        {
            get
            {
                return HistrotyCount;
            }
        }
    }

    public class WordListClass : IDisposable
    {
        public void Dispose()
        {
            WordsBackup.Dispose();
        }

        private readonly BackupWordsList<Tuple<string,string,object>> WordsBackup = new BackupWordsList<Tuple<string,string,object>>();
        public WordListClass(object InputValue, MainWindow InputMainWindow)
        {
            Words = new List<WordClass>();
            if (InputValue == null)
            {
                return;
            }
            var Value = (object[])InputValue;
            if ((string)Value[1] != "" && (string)Value[1] != null)
            {
                MainClass.ReferenceTypePackaging<string> Input;
                var InputWindow = new WindowInputInformation(out Input, "Input Password", "", InputMainWindow);
                MainClass.OpenWindow(InputWindow, InputMainWindow, true);
                if (Input.Value != (string)Value[1])
                {
                    throw new Exception();
                }
                Key = (string)Value[1];
            }
            #region 添加顺序判断类型
            //字母表
            OrderList.Add(OrderTypes.Alphabetical_A_To_Z, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                string Value1 = null;
                if (Input1 is string)
                {
                    Value1 = ((string)Input1).ToLower();
                }
                else
                {
                    Value1 = ((WordClass)Input1).Spelling.ToLower();
                }
                string Value2 = Words[Input2].Spelling.ToLower();
                int Length1 = Value1.Length;
                int Length2 = Value2.Length;
                int Length;
                if (Length1 > Length2)
                {
                    Length = Length2;
                }
                else
                {
                    Length = Length1;
                }

                for (int i = 0; i < Length; i++)
                {
                    if (Value1[i] > Value2[i])
                    {
                        return true;
                    }
                    else if (Value1[i] < Value2[i])
                    {
                        return false;
                    }
                }
                if (Length1 == Length2)
                {
                    return null;
                }
                else if (Length1 > Length2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            //单词长度
            OrderList.Add(OrderTypes.Word_Length_Short_To_High, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                int Value1 = 0;
                if (Input1 is uint)
                {
                    Value1 = (int)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).Spelling.Length;
                }
                int Value2 = Words[Input2].Spelling.Length;
                if (Value1 == Value2)
                {
                    return null;
                }
                else if (Value1 > Value2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            //单词复习等级
            OrderList.Add(OrderTypes.Review_Level_Low_To_High, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                uint Value1 = 0;
                if (Input1 is uint)
                {
                    Value1 = (uint)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).ReviewLevel;
                }
                uint Value2 = Words[Input2].ReviewLevel;
                if (Value1 == Value2)
                {
                    return null;
                }
                else if (Value1 > Value2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            //单词创建时间
            OrderList.Add(OrderTypes.Created_Time_Old_To_New, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                DateTime Value1;
                if (Input1 is DateTime)
                {
                    Value1 = (DateTime)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).CreatedTime;
                }
                DateTime Value2 = Words[Input2].CreatedTime;
                int Span = Value1.CompareTo(Value2);
                if (Span == 0)
                {
                    return null;
                }
                else if (Span < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            OrderList.Add(OrderTypes.Next_Review_Time_Old_To_New, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                DateTime Value1;
                if (Input1 is DateTime)
                {
                    Value1 = (DateTime)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).NextReviewTime;
                }
                DateTime Value2 = Words[Input2].NextReviewTime;
                int Span = Value1.CompareTo(Value2);
                if (Span == 0)
                {
                    return null;
                }
                else if (Span < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            OrderList.Add(OrderTypes.Opacity_Light_To_Dark, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                uint Value1;
                if (Input1 is uint)
                {
                    Value1 = (uint)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).Opacity;
                }
                uint Value2 = Words[Input2].Opacity;
                if (Value1 == Value2)
                {
                    return null;
                }
                else if (Value1 > Value2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            OrderList.Add(OrderTypes.Meanings_Count_Less_To_More, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                int Value1;
                if (Input1 is int)
                {
                    Value1 = (int)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).Meanings.Count;
                }
                int Value2 = Words[Input2].Meanings.Count;
                if (Value1 == Value2)
                {
                    return null;
                }
                else if (Value1 > Value2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            OrderList.Add(OrderTypes.Category_Count_Less_To_More, new Tuple<List<int>, Func<object, int, bool?>>(new List<int>(), (Input1, Input2) =>
            {
                int Value1;
                if (Input1 is int)
                {
                    Value1 = (int)Input1;
                }
                else
                {
                    Value1 = ((WordClass)Input1).Categorys.Count;
                }
                int Value2 = Words[Input2].Categorys.Count;
                if (Value1 == Value2)
                {
                    return null;
                }
                else if (Value1 > Value2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            #endregion
            #region 单词
            var TempWords = (Dictionary<string, Dictionary<string, object>>)Value[0];
            foreach (var k0 in TempWords)
            {
                WordClass WordClass;
                CreateWord(out WordClass);
                WordClass.Spelling = k0.Key;

                WordClass.Visible = (bool)k0.Value[OutputTitleVisible];
                WordClass.StrongReview = (bool)k0.Value[OutputTitleStrongReview];
                WordClass.NextReviewTime = (DateTime)k0.Value[OutputTitleNextReviewTime];
                WordClass.ReviewLevel = (uint)k0.Value[OutputTitleReviewLevel];
                WordClass.CreatedTime = (DateTime)k0.Value[OutputTitleCreatedTime];
                WordClass.BackgroundColor = (string)k0.Value[OutputTitleBackgroundColor];
                WordClass.BorderColor = (string)k0.Value[OutputTitleBorderColor];
                WordClass.ForegroundColor = (string)k0.Value[OutputTitleForegroundColor];
                WordClass.Categorys.AddRange(((string[])k0.Value[OutputTitleCategorys]));
                WordClass.Opacity = (uint)k0.Value[OutputTitleOpacity];

                WordClass.Meanings.AddRange((string[])k0.Value[OutputTitleMeanings]);

                WordClass.EnableBackup = true;
            }
            #endregion
        }

        public enum OrderTypes
        {
            Alphabetical_A_To_Z,
            Review_Level_Low_To_High,
            Created_Time_Old_To_New,
            Word_Length_Short_To_High,
            Next_Review_Time_Old_To_New,
            Opacity_Light_To_Dark,
            Meanings_Count_Less_To_More,
            Category_Count_Less_To_More
        }

        //二分法
        public readonly Dictionary<OrderTypes, Tuple<List<int>, Func<object, int, bool?>>> OrderList = new Dictionary<OrderTypes, Tuple<List<int>, Func<object, int, bool?>>>();

        //单词
        public class WordClass
        {
            private Action<WordClass> UpdateWord;
            private Func<string, bool> WordExist;
            private BackupWordsList<Tuple<string,string,object>> WordsBackup;
            public bool EnableBackup = false;

            public WordClass(
                List<WordClass> WordsList_,
                Action<WordClass> UpdateWord_,
                Func<string, bool> WordExist_,
                BackupWordsList<Tuple<string, string, object>> WordsBackup_)
            {
                WordsList_.Add(this);
                UpdateWord = UpdateWord_;

                WordExist = WordExist_;

                WordsBackup = WordsBackup_;

                Meanings_ = new ListO<string>(new Action<string[]>((value) =>
                {
                    if (EnableBackup && (MainClass.CompareList<string>(Meanings_.ToArray(), value) == false))
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleMeanings, value));
                        }
                    }
                    UpdateWord.Invoke(this);
                }));
                Categorys_ = new ListO<string>(new Action<string[]>((value) =>
                {
                    if (EnableBackup && MainClass.CompareList<string>(Categorys_.ToArray(), value) == false)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleCategorys, value));
                        }
                    }
                    UpdateWord.Invoke(this);
                }));

                Visible_ = true;
                NextReviewTime_ = DateTime.Now;
                ReviewLevel_ = 0;
                BackgroundColor_ = "Black";
                BorderColor_ = "Gray";
                ForegroundColor_ = "White";

                CreatedTime_ = DateTime.Now;
                Spelling_ = "";
                Opacity_ = 100;

                UpdateWord.Invoke(this);
            }

            private string Tag_ = null;
            public string Tag
            {
                get
                {
                    return Tag_;
                }
                set
                {
                    if (EnableBackup && Tag_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleTag, Tag_));
                        }
                    }
                    Tag_ = value;
                }
            }

            private string Spelling_ = "";
            public string Spelling
            {
                get
                {
                    return Spelling_;
                }
                set
                {
                    if (value != Spelling_)
                    {
                        if (value != Spelling_ && WordExist(value))
                        {
                            throw new Exception();
                        }
                        Spelling_ = value;

                        UpdateWord.Invoke(this);
                    }
                }
            }

            ListO<string> Meanings_ ;
            public ListO<string> Meanings
            {
                get
                {
                    return Meanings_;
                }
            }

            private bool Visible_;
            public bool Visible
            {
                get
                {
                    return Visible_;
                }
                set
                {
                    if (EnableBackup && (Visible_ != value))
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleVisible, Visible_));
                        }
                    }
                    Visible_ = value;
                }
            }

            private bool StrongReview_ = false;
            public bool StrongReview
            {
                get
                {
                    return StrongReview_;
                }
                set
                {
                    if(value)
                    {
                        ReviewLevel = 3;
                        NextReviewTime = DateTime.Now.AddMinutes(ReviewLevels[ReviewLevel]);
                    }
                    StrongReview_ = value;
                }
            }
            private DateTime NextReviewTime_;
            public DateTime NextReviewTime
            {
                get
                {
                    return NextReviewTime_;
                }
                set
                {
                    if (EnableBackup && NextReviewTime_.ToString() != value.ToString())
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleNextReviewTime, NextReviewTime_));
                        }
                    }
                    NextReviewTime_ = value;
                    NeedReview_ = false;
                    UpdateWord.Invoke(this);
                }
            }

            private uint ReviewLevel_;
            public uint ReviewLevel//回顾次数
            {
                get
                {
                    return ReviewLevel_;
                }
                set
                {
                    if (EnableBackup && ReviewLevel_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleReviewLevel, ReviewLevel_));
                        }
                    }
                    ReviewLevel_ = value;
                    UpdateWord.Invoke(this);
                }
            }

            private bool NeedReview_ = false;
            public bool NeedReview
            {
                get
                {
                    if (NeedReview_ == true)
                    {
                        return true;
                    }
                    else if (DateTime.Compare(NextReviewTime, DateTime.Now) > 0)
                    {
                        return false;
                    }
                    else
                    {
                        NeedReview_ = true;
                        return true;
                    }
                }
            }

            private bool OldReviewStatus = false;
            public bool ReviewStatusChanged
            {
                get
                {
                    return (OldReviewStatus != NeedReview);
                }
            }

            public void ResetReviewStatusChanged()
            {
                OldReviewStatus = NeedReview;
            }

            private DateTime CreatedTime_;
            public DateTime CreatedTime
            {
                get
                {
                    return CreatedTime_;
                }
                set
                {
                    if (EnableBackup && CreatedTime_.ToString() != value.ToString())
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleCreatedTime, CreatedTime_));
                        }
                    }
                    CreatedTime_ = value;
                    UpdateWord.Invoke(this);
                }
            }

            #region 遗忘曲线表
            //得到下一次回顾时间(20级：160年)
            public static readonly int[] ReviewLevels = new int[]
            {
                0, //0分钟                      0
                5, //5分钟                      1
                30, //30分钟                    2
                12 * 60, //12小时               3
                24 * 60, //1天                  4
                2 * 24 * 60,//2天               5
                3 * 24 * 60, //4天              6
                6 * 24 * 60,//7天               7
                12 * 24 * 60,//15天             8
                25 * 24 * 60,//15天             8
                1 * 30 * 24 * 60,//1个月        9
                2 * 30 * 24 * 60,//2个月        10
                4 * 30 * 24 * 60,//4个月        11
                8 * 30 * 24 * 60,//8个月        12
                16 * 30 * 24 * 60,//16个月      13
                32 * 30 * 24 * 60,//32个月      14
                64 * 30 * 24 * 60,//64个月      15
                128 * 30 * 24 * 60,//10年       16
                256 * 30 * 24 * 60,//20年       17
                512 * 30 * 24 * 60,//40年       18
                1024 * 30 * 24 * 60,//80年      19
                2048 * 30 * 24 * 60,//160年     20
            };
            #endregion

            public void MarkReview()
            {
                if (NeedReview && ReviewLevel < ReviewLevels.Length - 1) //需要复习
                {
                    double Interval = (DateTime.Now - NextReviewTime).TotalMinutes;
                    while (Interval > ReviewLevels[++ReviewLevel] && ReviewLevel != 1) //根据复习间隔 自动调整复习等级
                    {
                    }
                }
                NextReviewTime = DateTime.Now.AddMinutes(ReviewLevels[ReviewLevel]);
                StrongReview = StrongReview;
            }

            public void MarkMaxReview()
            {
                StrongReview = false;
                ReviewLevel = (uint)(ReviewLevels.Length - 1);
                try
                {
                    NextReviewTime = DateTime.Now.AddMinutes(ReviewLevels[ReviewLevel]);
                }
                catch
                {
                    NextReviewTime = DateTime.Now.AddMinutes(ReviewLevels[ReviewLevels.Length - 1]);
                }
            }

            public void NewWordMark()
            {
                ReviewLevel = 0;
                NextReviewTime = DateTime.Now.AddMinutes(ReviewLevels[ReviewLevel]);
            }

            private string BackgroundColor_;
            public string BackgroundColor//颜色
            {
                get
                {
                    return BackgroundColor_;
                }
                set
                {
                    if (EnableBackup && BackgroundColor_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleBackgroundColor, BackgroundColor_));
                        }
                    }
                    BackgroundColor_ = value;
                }
            }

            private string BorderColor_;
            public string BorderColor//颜色
            {
                get
                {
                    return BorderColor_;
                }
                set
                {
                    if (EnableBackup && BorderColor_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleBorderColor, BorderColor_));
                        }
                    }
                    BorderColor_ = value;
                }
            }

            private string ForegroundColor_;
            public string ForegroundColor//颜色
            {
                get
                {
                    return ForegroundColor_;
                }
                set
                {
                    if (EnableBackup && ForegroundColor_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleForegroundColor, ForegroundColor_));
                        }
                    }
                    ForegroundColor_ = value;
                }
            }

            private uint Opacity_;
            public uint Opacity
            {
                get
                {
                    return Opacity_;
                }
                set
                {
                    if (EnableBackup && Opacity_ != value)
                    {
                        lock (WordsBackup)
                        {
                            WordsBackup.Push(new Tuple<string, string, object>(Spelling_, OutputTitleOpacity, Opacity_));
                        }
                    }
                    Opacity_ = value;
                    UpdateWord.Invoke(this);
                }
            }

            private ListO<string> Categorys_;
            public ListO<string> Categorys
            {
                get
                {
                    return Categorys_;
                }
            }
        }

        //索引方便找单词(二分法)
        public WordClass this[string Spelling]
        {
            get
            {
                int Get = BinaryFindIndex(Spelling, OrderTypes.Alphabetical_A_To_Z);
                if (Get != -1 &&
                    Words[Get].Spelling == Spelling)
                {
                    return Words[Get];
                }
                throw new Exception();
            }
            set
            {
                int Get = BinaryFindIndex(Spelling, OrderTypes.Alphabetical_A_To_Z);
                if (Get != -1 &&
                    Words[Get].Spelling == Spelling)
                {
                    Words[Get] = value;
                }
                throw new Exception();
            }
        }

        private int BinaryFindIndex(object Value, OrderTypes Type)
        {
            var Item = OrderList[Type];
            var Lists = Item.Item1;
            var Compare = Item.Item2;
            if (Lists.Count == 0)
            {
                return -1;
            }
            int Start = 0, End = Lists.Count - 1, Middle = 0;
            while (true)
            {
                Middle = (Start + End) / 2;
                bool? CompareResult = Compare.Invoke(Value, Lists[Middle]);
                if (CompareResult == null) //就在这里
                {
                    return Lists[Middle];
                }
                else if (CompareResult == true) //后面去找
                {
                    Start = Middle + 1;
                }
                else //前面去找
                {
                    End = Middle - 1;
                }
                if (Start > End)
                {
                    return -1;
                }
            }
        }

        private int BinaryFindInsert(object Value, OrderTypes Type)
        {
            var Item = OrderList[Type];
            var Lists = Item.Item1;
            var Compare = Item.Item2;
            if (Lists.Count == 0)
            {
                return 0;
            }
            int Start = 0, End = Lists.Count - 1, Middle = 0;
            while (true)
            {
                Middle = (Start + End) / 2;
                bool? CompareResult = Compare.Invoke(Value, Lists[Middle]);
                if (CompareResult == true) //后面去找
                {
                    Start = Middle + 1;
                    if (Start > End)
                    {
                        if (Start < Lists.Count &&
                            Compare.Invoke(Value, Lists[Start]) == true)
                        {
                            Start++;
                        }
                        return Start;
                    }
                }
                else //前面去找
                {
                    End = Middle - 1;
                    if (Start > End)
                    {
                        if (End > 0 &&
                            Compare.Invoke(Value, Lists[End]) == false)
                        {
                            Middle--;
                        }
                        return Middle;
                    }
                }
            }
        }

        //生成一个新单词
        public void CreateWord(out WordClass InputWord)
        {
            InputWord = new WordClass
                (
                Words,
                new Action<WordClass>(UpdateWord),
                new Func<string, bool>(WordExsit),
                WordsBackup
            );
        }

        public string[] UndoWord()
        {
            List<string> Words = new List<string>();
            if (WordsBackup.Count > 0)
            {
                DateTime Time;
                string Spelling;
                string Action;
                object Value;
                do
                {
                    var ItemT = WordsBackup.Pop();
                    Time = ItemT.Item1;
                    Spelling = ItemT.Item2.Item1;
                    Action = ItemT.Item2.Item2;
                    Value = ItemT.Item2.Item3;
                    if (WordExsit(Spelling)) //拼写查看
                    {
                        this[Spelling].EnableBackup = false; //关闭备份
                        switch (Action) //行为查看
                        {
                            case OutputTitleStrongReview:
                                this[Spelling].StrongReview = (bool)Value;
                                break;
                            case OutputTitleVisible:
                                this[Spelling].Visible = (bool)Value;
                                break;
                            case OutputTitleOpacity:
                                this[Spelling].Opacity = (uint)Value;
                                break;
                            case OutputTitleNextReviewTime:
                                this[Spelling].NextReviewTime = (DateTime)Value;
                                break;
                            case OutputTitleReviewLevel:
                                this[Spelling].ReviewLevel = (uint)Value;
                                break;
                            case OutputTitleCreatedTime:
                                this[Spelling].CreatedTime = (DateTime)Value;
                                break;
                            case OutputTitleBackgroundColor:
                                this[Spelling].BackgroundColor = (string)Value;
                                break;
                            case OutputTitleBorderColor:
                                this[Spelling].BorderColor = (string)Value;
                                break;
                            case OutputTitleForegroundColor:
                                this[Spelling].ForegroundColor = (string)Value;
                                break;
                            case OutputTitleCategorys:
                                this[Spelling].Categorys.Clear();
                                this[Spelling].Categorys.AddRange((string[])Value);
                                break;
                            case OutputTitleMeanings:
                                this[Spelling].Meanings.Clear();
                                this[Spelling].Meanings.AddRange((string[])Value);
                                break;
                            case OutputTitleTag:
                                this[Spelling].Tag = (string)Value;
                                break;
                        }
                        this[Spelling].EnableBackup = true;
                        Words.Add(Spelling);
                    }
                }
                while (WordsBackup.Count > 0 && (Time - WordsBackup.Peek().Item1).TotalMilliseconds < 80);
            }
            return Words.ToArray();
        }

        //删除一个单词
        private bool WordExsit(string Spelling)
        {
            int Get = BinaryFindIndex(Spelling, OrderTypes.Alphabetical_A_To_Z);
            if (Get != -1 &&
                Words[Get].Spelling == Spelling)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void DeleteWord(ref WordClass InputWord)
        {
            //删除单词并释放资源
            int Index = Words.IndexOf(InputWord);
            Words.RemoveAt(Index);
            //索引更新
            int j;
            int Count = OrderList[OrderTypes.Alphabetical_A_To_Z].Item1.Count - 1;
            foreach (var i in OrderList)
            {
                i.Value.Item1.Remove(Index);
                for (j = 0; j < Count; j++)
                {
                    if (i.Value.Item1[j] > Index)
                    {
                        i.Value.Item1[j]--;
                    }
                }
            }
            InputWord = null;
        }
        private void UpdateWord(WordClass InputWord)
        {
            int WordIndex = Words.IndexOf(InputWord);
            int Index = 0;
            foreach (var i in OrderList)
            {
                if (!i.Value.Item1.Contains(WordIndex))
                {
                    i.Value.Item1.Insert(0, WordIndex);
                }
                Index = i.Value.Item1.IndexOf(WordIndex);
                if (Index > 0)
                {
                    //跟前面比
                    if (i.Value.Item2.Invoke(InputWord, i.Value.Item1[Index - 1]) == false) //比前面小
                    {
                        i.Value.Item1.RemoveAt(Index);
                        i.Value.Item1.Insert(BinaryFindInsert(InputWord, i.Key), WordIndex);
                        continue;
                    }
                }
                //跟后面比
                if (Index < i.Value.Item1.Count - 1)
                {
                    if (i.Value.Item2.Invoke(InputWord, i.Value.Item1[Index + 1]) == true) //比后面大
                    {
                        i.Value.Item1.RemoveAt(Index);
                        i.Value.Item1.Insert(BinaryFindInsert(InputWord, i.Key), WordIndex);
                        continue;
                    }
                }
            }
        }

        //返回单词列表
        public readonly List<WordClass> Words = new List<WordClass>();

        #region
        private const string OutputTitleVisible = "Visible";

        private const string OutputTitleStrongReview = "StrongReview";

        private const string OutputTitleOpacity = "Opacity";

        private const string OutputTitleNextReviewTime = "NextReviewTime";

        private const string OutputTitleReviewLevel = "ReviewLevel";

        private const string OutputTitleCreatedTime = "CreatedTime";

        private const string OutputTitleBackgroundColor = "BackgroundColor";

        private const string OutputTitleBorderColor = "BorderColor";

        private const string OutputTitleForegroundColor = "ForegroundColor";

        private const string OutputTitleCategorys = "Categorys";

        private const string OutputTitleMeanings = "Meanings";

        private const string OutputTitleTag = "Tag";
        #endregion

        public string Key
        {
            get;
            set;
        }

        public object Output()
        {
            Dictionary<string, Dictionary<string, object>> Return = new Dictionary<string, Dictionary<string, object>>();
            foreach (var l in Words)
            {
                Return.Add(l.Spelling, new Dictionary<string, object>());
                Return[l.Spelling].Add(OutputTitleStrongReview, l.StrongReview);
                Return[l.Spelling].Add(OutputTitleVisible, l.Visible);
                Return[l.Spelling].Add(OutputTitleReviewLevel, l.ReviewLevel);
                Return[l.Spelling].Add(OutputTitleNextReviewTime, l.NextReviewTime);
                Return[l.Spelling].Add(OutputTitleCreatedTime, l.CreatedTime);
                Return[l.Spelling].Add(OutputTitleBackgroundColor, l.BackgroundColor);
                Return[l.Spelling].Add(OutputTitleBorderColor, l.BorderColor);
                Return[l.Spelling].Add(OutputTitleForegroundColor, l.ForegroundColor);
                Return[l.Spelling].Add(OutputTitleCategorys, l.Categorys.ToArray());
                Return[l.Spelling].Add(OutputTitleOpacity, l.Opacity);
                Return[l.Spelling].Add(OutputTitleMeanings, l.Meanings.ToArray());
            }
            return new object[] { Return, Key };
        }
    }

    [Serializable]
    public class SettingStruct
    {
        public SettingStruct()
        {
            AutoBackup = true;
            Topmost = false;
            EnableRemindVoice = true;
            MaxBackupTimes = 5;
            AutoFillMeanings = false;
            MinInvisible = false;
        }

        public bool MinInvisible
        {
            get;
            set;
        }

        //自动备份
        public bool AutoBackup
        {
            get;
            set;
        }

        public bool AutoFillMeanings
        {
            get;
            set;
        }


        public bool AutoFillExamples
        {
            get;
            set;
        }

        public bool AutoFillPronunciations
        {
            get;
            set;
        }

        public bool AutoFillPictures
        {
            get;
            set;
        }

        //备份次数
        public int MaxBackupTimes
        {
            get;
            set;
        }

        //窗口定制
        public bool Topmost
        {
            get;
            set;
        }

        public bool EnableRemindVoice
        {
            get;
            set;
        }

        //开启开启列表
        public readonly List<string> OpenLists = new List<string>();
    }

    public class MainClass
    {
        public static int ProcessStarted(string Path_)
        {
            int Count = 0;
            string CurrentDirectory = Environment.CurrentDirectory;
            if (!Path_.StartsWith(CurrentDirectory))
            {
                Path_ = CurrentDirectory + "\\" + Path_;
            }
            foreach ( var P in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path_)))
            {
                if (P.MainModule.FileName == Path_)
                {
                    Count++;
                }
            }
            return Count;
        }

        public static bool CompareList<T>(T[] A, T[] B)
        {
            if (A.Length != B.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < A.Length; i++)
                {
                    if (A[i].ToString() != B[i].ToString())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string ReturnVagueTimeString(TimeSpan Span)
        {
            if (Span.Days > 0)
            {
                int Number = 0;
                if ((Number = Span.Days / 365) > 0)
                {
                    return Number + " years";
                }
                else if ((Number = Span.Days / 30) > 0)
                {
                    return Number + " months";
                }
                else
                {
                    return Span.Days + " days";
                }
            }
            else if (Span.Hours > 0)
            {
                return Span.Hours + " hours"; ;
            }
            else if (Span.Minutes > 0)
            {
                return Span.Minutes + " minutes";
            }
            else
            {
                return Span.Seconds + " seconds";
            }
        }

        public static byte[] ScaleImage(byte[] Data)
        {
            const int Size = 200;
            try
            {
                using (MemoryStream Memory = new MemoryStream(Data))
                {
                    using (var Image = System.Drawing.Image.FromStream(Memory))
                    {
                        if (Image.Width >= Size && Image.Height >= Size)
                        {
                            // 要保存到的图片
                            using (var Bitmap = new System.Drawing.Bitmap(Size, Size))
                            {
                                using (var Graphics = System.Drawing.Graphics.FromImage(Bitmap))
                                {
                                    Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                    Graphics.DrawImage(Image, new System.Drawing.Rectangle(0, 0, Size, Size));

                                    using (MemoryStream SaveMemory = new MemoryStream())
                                    {
                                        Bitmap.Save(SaveMemory, System.Drawing.Imaging.ImageFormat.Png);
                                        Data = SaveMemory.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }
                return Data;
            }
            catch
            {
                return null;
            }
        }

        #region
        [Serializable]
        public class TupleC<T0>
        {
            public TupleC(T0 SetItem1)
            {
                Item1 = SetItem1;
            }

            public T0 Item1;
        }

        [Serializable]
        public class TupleC<T0, T1>
        {
            public TupleC(T0 SetItem1, T1 SetItem2)
            {
                Item1 = SetItem1;
                Item2 = SetItem2;
            }
            public T0 Item1;
            public T1 Item2;
        }

        [Serializable]
        public class TupleC<T0, T1, T2>
        {
            public TupleC(T0 SetItem1, T1 SetItem2, T2 SetItem3)
            {
                Item1 = SetItem1;
                Item2 = SetItem2;
                Item3 = SetItem3;
            }
            public T0 Item1;
            public T1 Item2;
            public T2 Item3;
        }

        [Serializable]
        public class TupleC<T0, T1, T2, T3>
        {
            public TupleC(T0 SetItem1, T1 SetItem2, T2 SetItem3, T3 SetItem4)
            {
                Item1 = SetItem1;
                Item2 = SetItem2;
                Item3 = SetItem3;
                Item4 = SetItem4;
            }
            public T0 Item1;
            public T1 Item2;
            public T2 Item3;
            public T3 Item4;
        }

        [Serializable]
        public class TupleC<T0, T1, T2, T3, T4>
        {
            public TupleC(T0 SetItem1, T1 SetItem2, T2 SetItem3, T3 SetItem4, T4 SetRest)
            {
                Item1 = SetItem1;
                Item2 = SetItem2;
                Item3 = SetItem3;
                Item4 = SetItem4;
                Item5 = SetRest;
            }
            public T0 Item1;
            public T1 Item2;
            public T2 Item3;
            public T3 Item4;
            public T4 Item5;
        }
        #endregion

        public static ImageBrush OpenPicture(byte[] Data)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream(Data);
            bmp.EndInit();

            var ImageBrush = new ImageBrush();
            ImageBrush.ImageSource = bmp;
            return ImageBrush;
        }

        //Binding界面与后台代码
        public static System.Windows.Data.Binding BindingData(string Path, object Source, DependencyObject Target, DependencyProperty dp)//设置Binding
        {
            System.Windows.Data.Binding Binding = new System.Windows.Data.Binding(Path);
            Binding.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            Binding.Source = Source;
            Binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(Target, dp, Binding);
            return Binding;
        }

        public static bool MouseInControl(FrameworkElement Control)
        {
            var MouseLocation = Mouse.GetPosition(Control);
            if (MouseLocation.X == 0 && MouseLocation.Y == 0)
            {
                return false;
            }
            return 0 <= MouseLocation.X && MouseLocation.X <= Control.ActualWidth && 0 <= MouseLocation.Y && MouseLocation.Y <= Control.ActualHeight;
        }

        private class StringCompute
        {
            #region 私有变量
            /// <summary>
            /// 字符串1
            /// </summary>
            private char[] _ArrChar1;
            /// <summary>
            /// 字符串2
            /// </summary>
            private char[] _ArrChar2;
            /// <summary>
            /// 统计结果
            /// </summary>
            private Result _Result;
            /// <summary>
            /// 算法矩阵
            /// </summary>
            private int[,] _Matrix;
            /// <summary>
            /// 矩阵列数
            /// </summary>
            private int _Column;
            /// <summary>
            /// 矩阵行数
            /// </summary>
            private int _Row;
            #endregion
            #region 属性
            public Result ComputeResult
            {
                get { return _Result; }
            }
            #endregion
            #region 构造函数
            public StringCompute(string str1, string str2)
            {
                this.StringComputeInit(str1, str2);
            }
            public StringCompute()
            {
            }
            #endregion
            #region 算法实现
            /// <summary>
            /// 初始化算法基本信息
            /// </summary>
            /// <param name="str1">字符串1</param>
            /// <param name="str2">字符串2</param>
            private void StringComputeInit(string str1, string str2)
            {
                _ArrChar1 = str1.ToCharArray();
                _ArrChar2 = str2.ToCharArray();
                _Result = new Result();
                _Row = _ArrChar1.Length + 1;
                _Column = _ArrChar2.Length + 1;
                _Matrix = new int[_Row, _Column];
            }
            /// <summary>
            /// 计算相似度
            /// </summary>
            public void Compute()
            {
                //初始化矩阵的第一行和第一列
                this.InitMatrix();
                int intCost = 0;
                for (int i = 1; i < _Row; i++)
                {
                    for (int j = 1; j < _Column; j++)
                    {
                        if (_ArrChar1[i - 1] == _ArrChar2[j - 1])
                        {
                            intCost = 0;
                        }
                        else
                        {
                            intCost = 1;
                        }
                        //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                        //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                        _Matrix[i, j] = this.Minimum(_Matrix[i - 1, j] + 1, _Matrix[i, j - 1] + 1, _Matrix[i - 1, j - 1] + intCost);
                    }
                }
                //相似率 移动次数小于最长的字符串长度的20%算同一题
                int intLength = _Row > _Column ? _Row : _Column;

                _Result.Rate = (1 - (decimal)_Matrix[_Row - 1, _Column - 1] / intLength);
                _Result.Difference = _Matrix[_Row - 1, _Column - 1];
            }


            /// <summary>
            /// 计算相似度（不记录比较时间）
            /// </summary>
            public void SpeedyCompute()
            {
                //开始时间
                //_BeginTime = DateTime.Now;
                //初始化矩阵的第一行和第一列
                this.InitMatrix();
                int intCost = 0;
                for (int i = 1; i < _Row; i++)
                {
                    for (int j = 1; j < _Column; j++)
                    {
                        if (_ArrChar1[i - 1] == _ArrChar2[j - 1])
                        {
                            intCost = 0;
                        }
                        else
                        {
                            intCost = 1;
                        }
                        //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                        //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                        _Matrix[i, j] = this.Minimum(_Matrix[i - 1, j] + 1, _Matrix[i, j - 1] + 1, _Matrix[i - 1, j - 1] + intCost);
                    }
                }
                //结束时间
                //_EndTime = DateTime.Now;
                //相似率 移动次数小于最长的字符串长度的20%算同一题
                int intLength = _Row > _Column ? _Row : _Column;

                _Result.Rate = (1 - (decimal)_Matrix[_Row - 1, _Column - 1] / intLength);
                // _Result.UseTime = (_EndTime - _BeginTime).ToString();
                _Result.Difference = _Matrix[_Row - 1, _Column - 1];
            }

            /// <summary>
            /// 计算相似度
            /// </summary>
            /// <param name="str1">字符串1</param>
            /// <param name="str2">字符串2</param>
            public void SpeedyCompute(string str1, string str2)
            {
                this.StringComputeInit(str1, str2);
                this.SpeedyCompute();
            }
            /// <summary>
            /// 初始化矩阵的第一行和第一列
            /// </summary>
            private void InitMatrix()
            {
                for (int i = 0; i < _Column; i++)
                {
                    _Matrix[0, i] = i;
                }
                for (int i = 0; i < _Row; i++)
                {
                    _Matrix[i, 0] = i;
                }
            }
            /// <summary>
            /// 取三个数中的最小值
            /// </summary>
            /// <param name="First"></param>
            /// <param name="Second"></param>
            /// <param name="Third"></param>
            /// <returns></returns>
            private int Minimum(int First, int Second, int Third)
            {
                int intMin = First;
                if (Second < intMin)
                {
                    intMin = Second;
                }
                if (Third < intMin)
                {
                    intMin = Third;
                }
                return intMin;
            }
            #endregion
        }

        private struct Result
        {
            public decimal Rate;
            public int Difference;
        }

        private static readonly StringCompute String_Compute = new StringCompute();

        public static decimal ComputeStringSimilarity(string String1, string String2)
        {
            String_Compute.SpeedyCompute(String1, String2);    // 计算相似度， 不记录比较时间
            return String_Compute.ComputeResult.Rate;         // 相似度百分之几，完全匹配相似度为1
        }

        //public static Languages Language = new Languages();

        //序列化
        public static byte[] CloneToBytes(object Read)
        {
            if (Read == null)
            {
                return null;
            }
            BinaryFormatter bf = new BinaryFormatter();
            byte[] cache;
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, Read);
                ms.Seek(0, SeekOrigin.Begin);
                cache = new BinaryReader(ms).ReadBytes((int)ms.Length);
            }
            return cache;
        }

        //反序列化
        public static object BytesToClone(byte[] Read)
        {
            if (Read == null || Read.Length == 0)
            {
                return null;
            }
            object i;
            using (MemoryStream ms = new MemoryStream(Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                i = bf.Deserialize(ms);
            }
            return i;
        }

        //值类型封装为引用类型
        [Serializable]
        public class ReferenceTypePackaging<O>
        {
            public ReferenceTypePackaging(O SetValue)
            {
                Value = SetValue;
            }
            public O Value;
        }

        //打开子窗口
        private readonly static Dictionary<string, System.Windows.Size> WindowsSizeRecording = new Dictionary<string, System.Windows.Size>();
        public static void OpenWindow(Window InputWindow, Window MainWindow, bool Suspend)
        {
            InputWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            InputWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;


            if (MainWindow != null)
            {
                InputWindow.Owner = MainWindow;
                InputWindow.Icon = MainWindow.Icon;
            }
            string FullName = InputWindow.GetType().FullName;
            if (WindowsSizeRecording.ContainsKey(FullName))
            {
                InputWindow.Width = WindowsSizeRecording[FullName].Width;
                InputWindow.Height = WindowsSizeRecording[FullName].Height;
            }
            else
            {
                WindowsSizeRecording.Add(FullName, new System.Windows.Size());
            }
            if (Suspend)
            {
                InputWindow.ShowInTaskbar = false;
                InputWindow.ShowDialog();
                WindowsSizeRecording[FullName] = new System.Windows.Size(InputWindow.Width, InputWindow.Height);
            }
            else
            {
                InputWindow.ShowInTaskbar = true;
                InputWindow.Show();
                InputWindow.Closing += InputWindow_Closing;
            }
        }

        private static void InputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window Window = (Window)sender;
            string FullName = Window.GetType().FullName;
            WindowsSizeRecording[FullName] = new System.Windows.Size(Window.Width, Window.Height);
        }

        public class ParallelTasks<O>
        {
            private readonly List<Task<O>> Tasks = new List<Task<O>>();

            public void AddTask(Task<O> Action)
            {
                Tasks.Add(Action);
                Action.Start();
            }

            public O[] WaitFinished()
            {
                O[] Results = new O[Tasks.Count];
                int Index = 0;
                foreach (var l in Tasks)
                {
                    Results[Index] = l.Result;
                    l.Dispose();
                    Index++;
                }
                Tasks.Clear();
                return Results;
            }
        }

        public class ParallelTasks
        {
            private readonly Stack<Task> Tasks = new Stack<Task>();

            public void AddTask(Task Action)
            {
                lock (Tasks)
                {
                    Tasks.Push(Action);
                    Tasks.Peek().Start();
                }
            }

            public void WaitFinished()
            {
                lock (Tasks)
                {
                    foreach (var l in Tasks)
                    {
                        l.Wait();
                        l.Dispose();
                    }
                    Tasks.Clear();
                }
            }
        }

        //刷新
        public static void DoEvents()
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        public static bool CheckEncode(string srcString)
        {
            int strLen = srcString.Length;

            //字符串的长度，一个字母和汉字都算一个  

            int bytLeng = System.Text.Encoding.UTF8.GetBytes(srcString).Length;

            //字符串的字节数，字母占1位，汉字占2位,注意，一定要UTF8  

            bool chkResult = false;
            if (strLen < bytLeng)

            //如果字符串的长度比字符串的字节数小，当然就是其中有汉字啦^-^  
            {
                chkResult = true;
            }

            return chkResult;
        } 

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;
            frame.Continue = false;
            return null;
        }

        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);

        //播放声音
        public static bool LockPlaySound = false;
        private static Task SpeakTask = null;
        private static bool ExitSpeak = false;
        public static void PlaySoundPath(string Filename, bool Wait)
        {
            if (LockPlaySound)
            {
                return;
            }
            if (Filename != null)
            {
                PlaySoundFile(null, Wait);
                try
                {
                    PlaySoundFile(File.ReadAllBytes(Filename), Wait);
                }
                catch { }
            }
            else
            {
                PlaySoundFile(null, Wait);
            }
        }

        public static void PlaySoundFile(byte[] Data, bool Wait)
        {
            try
            {
                lock (SpeakTask)
                {
                    if (SpeakTask != null)
                    {
                        ExitSpeak = true;
                        while (!SpeakTask.IsCompleted)
                        {
                            Thread.Sleep(50);
                        }
                        SpeakTask.Dispose();
                        SpeakTask = null;
                    }
                }
            }
            catch { }
            if (Data != null && Data.Length!=0)
            {
                SpeakTask = new Task(() =>
                {
                    MemoryStream File = new MemoryStream(Data);
                    File.Position = 0;
                    File.Flush();
                    Mp3FileReader rdr = null;
                    WaveStream wavStream = null;
                    BlockAlignReductionStream baStream = null;
                    WaveOut waveOut = null;
                    try
                    {
                        rdr = new Mp3FileReader(File);
                        wavStream = WaveFormatConversionStream.CreatePcmStream(rdr);
                        baStream = new BlockAlignReductionStream(wavStream);
                        waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                        waveOut.Init(baStream);
                        waveOut.Play();
                        ExitSpeak = false;
                        waveOut.PlaybackStopped +=
                            (pbss, pbse) =>
                            {
                                ExitSpeak = true;
                            };
                        while (ExitSpeak == false)
                        {
                            Thread.Sleep(50);
                        }
                        waveOut.Stop();
                    }
                    catch
                    { }
                    finally
                    {
                        if (rdr != null)
                        {
                            try
                            {
                                rdr.Dispose();
                            }
                            catch { }
                        }
                        if (wavStream != null)
                        {
                            try
                            {
                                wavStream.Dispose();
                            }
                            catch { }
                        }
                        if (baStream != null)
                        {
                            try
                            {
                                baStream.Dispose();
                            }
                            catch { }
                        }
                        if (waveOut != null)
                        {
                            try
                            {
                                waveOut.Dispose();
                            }
                            catch { }
                        }
                        if (File != null)
                        {
                            try
                            {
                                File.Dispose();
                            }
                            catch { }
                        }
                    }
                });
                lock (SpeakTask)
                {
                    SpeakTask.Start();
                    if (Wait)
                    {
                        while (SpeakTask.IsCompleted == false)
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
            }
        }

        public class CNNWebClient : WebClient
        {
            private const int Timeout = 10;
            private const string UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            HttpWebRequest request = null;
            
            protected override WebRequest GetWebRequest(Uri address)
            {
                request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = UserAgent;
                request.Timeout = 1000 * Timeout;
                request.ReadWriteTimeout = 1000 * Timeout;
                request.ContinueTimeout = 1000 * Timeout;
                return request;
            }

            public void Abort()
            {
                if (request != null)
                {
                    request.Abort();
                    request = null;
                }
            }
        }
    }

    //百度翻译API
    public class Translation
    {
        public void Abort()
        {
            client.Abort();
        }

        private TranClassBaidu tranClass = new TranClassBaidu();
        private MainClass.CNNWebClient client = new MainClass.CNNWebClient();
        private const string BaiduAPI_ID = "uGGS7kIjF8YFD78Wx9oFwYqk";
        private const string BaiduServiceAddress = "http://openapi.baidu.com/public/2.0/translate/dict/simple?client_id={0}&q={1}&from={2}&to={3}";
        private const string YoudaoAPI_ID = "keyfrom=Ptihbkuhb&key=790054173";
        private const string YoudaoServiceAddress = "http://fanyi.youdao.com/openapi.do?{0}&type=data&doctype=json&version=1.1&q={1}";
        public object GetTranslation(string Text)
        {
            bool UsingBaidu = false;
            if (UsingBaidu)
            {
                TranClassBaidu r = null;
                string url = string.Format(BaiduServiceAddress, BaiduAPI_ID, Text, tranClass.From, tranClass.To);
                try
                {
                    using (StringReader sr = new StringReader(client.DownloadString(url)))
                    {
                        try
                        {
                            JsonTextReader jsonReader = new JsonTextReader(sr); //引用Newtonsoft.Json 自带
                            JsonSerializer serializer = new JsonSerializer();
                            r = serializer.Deserialize<TranClassBaidu>(jsonReader); //因为获取后的为json对象 ，实行转换
                        }
                        catch { }
                    }
                }
                catch { }
                if (r == null || r.Data == null || r.Errno != 0)
                {
                    return null;
                }
                return r.Data.Symbols[0];
            }
            else
            {
                var SymbolsYoudao = new SymbolsYoudao();
                SymbolsYoudao.phonetic = "";
                SymbolsYoudao.explains = new List<string>();
                string url = string.Format(YoudaoServiceAddress, YoudaoAPI_ID, Text);
                string DownloadString = Encoding.UTF8.GetString(client.DownloadData(url));
                if (DownloadString.Contains(@"""us-phonetic"":"""))
                {
                    SymbolsYoudao.phonetic = DownloadString.Substring(DownloadString.IndexOf(@"""us-phonetic"":""") + @"""us-phonetic"":""".Length);
                    SymbolsYoudao.phonetic = SymbolsYoudao.phonetic.Substring(0, SymbolsYoudao.phonetic.IndexOf(@""""));

                }
                if (DownloadString.Contains(@"""explains"":["))
                {
                    string Temp = DownloadString.Substring(DownloadString.IndexOf(@"""explains"":[") + @"""explains"":[".Length);
                    Temp = Temp.Substring(0, Temp.IndexOf("]}"));
                    while (Temp.Contains(@""""))
                    {
                        Temp = Temp.Substring(Temp.IndexOf(@"""") + 1);
                        SymbolsYoudao.explains.Add(Temp.Substring(0, Temp.IndexOf(@"""")));
                        Temp = Temp.Substring(Temp.IndexOf(@"""") + 1);
                    }
                }
                else
                {
                    return null;
                }
                return SymbolsYoudao;
                /*
                TranClassYoudao r = null;
                string url = string.Format(YoudaoServiceAddress, YoudaoAPI_ID, Text);
                try
                {
                    using (StringReader sr = new StringReader(client.DownloadString(url)))
                    {
                        try
                        {
                            JsonTextReader jsonReader = new JsonTextReader(sr); //引用Newtonsoft.Json 自带
                            JsonSerializer serializer = new JsonSerializer();
                            r = serializer.Deserialize<TranClassYoudao>(jsonReader); //因为获取后的为json对象 ，实行转换
                        }
                        catch (Exception E) { }
                    }
                }
                catch { }
                if (r == null || r.basic == null || r.errorCode != 0)
                {
                    return null;
                }
                return r.basic;
                */
            }
        }

        private class TranClassBaidu
        {
            public TranClassBaidu()
            {
                To = "zh";
                From = "en";
            }

            public byte Errno { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public DataBaidu Data { get; set; }
        }

        private class DataBaidu
        {
            public List<SymbolsBaidu> Symbols { get; set; }
        }

        public class SymbolsBaidu
        {
            public List<PartsBaidu> Parts { get; set; }
            public string Ph_am { get; set; }
            //public string Ph_en { get; set; }
        }

        public class PartsBaidu
        {
            public List<string> Means { get; set; }
            public string Part { get; set; }
        }

        public class TranClassYoudao
        {
            public List<string> translation { get; set; }
            public SymbolsYoudao basic { get; set; }
            public string query { get; set; }
            public int errorCode { get; set; }
            public SymbolsWebYoudao[] web { get; set; }
        }

        public class SymbolsWebYoudao
        {
            public List<string> value { get; set; }
            public string key { get; set; }
        }

        public class SymbolsYoudao
        {
            public string usphonetic { get; set; }
            public string phonetic { get; set; }
            public string ukphonetic { get; set; }
            public List<string> explains { get; set; }
        }
    }

    //金山句库网页HTML格式读取(有风险)
    public class DownloadExamples : FileList
    {
        public DownloadExamples(string Path)
        {
            TotalPath = Path;
            TypicalName = "Examples.kl";
        }

        public void Abort()
        {
            WebClient.Abort();
        }

        //非正式
        private MainClass.CNNWebClient WebClient = new MainClass.CNNWebClient();
        public bool Download(string Spelling)
        {
            try
            {
                if (GetCount(Spelling) != 0)
                {
                    return true;
                }

                byte[] htmlByte = WebClient.DownloadData(string.Format("http://dj.iciba.com/{0}-1.html", Spelling));
                string Result = Encoding.UTF8.GetString(htmlByte);
                int Index1 = 0;
                List<byte[]> AddLists = new List<byte[]>();
                int IndexF = -1, IndexR = -1;
                for (int m = 1; ; m++)
                {
                    //找序列号
                    Index1 = Result.IndexOf(m.ToString() + ". ");
                    if (Index1 == -1 || (Result.Substring(0, Index1).Contains("1. ")))
                    {
                        break;
                    }
                    else
                    {
                        //截取序列号
                        Result = Result.Substring(Index1 + (m.ToString() + ". ").Length);
                        //截取当行
                        string Stencence = Result.Substring(0, Result.IndexOf("\r\n"));
                        //去括号里面的内容
                        while (((IndexF = Stencence.IndexOf("<"))!=-1) &&
                               ((IndexR = Stencence.IndexOf(">"))!=-1) &&
                               IndexR > IndexF)
                        {
                            Stencence = Stencence.Substring(0, Stencence.IndexOf("<")) + Stencence.Substring(Stencence.IndexOf(">") + 1);
                        }
                        //截取中文
                        string Chinese = null;
                        if (Result.IndexOf("<span class=\"stc_cn_txt\">") != -1)
                        {
                            Chinese = Result.Substring(Result.IndexOf("<span class=\"stc_cn_txt\">") + "<span class=\"stc_cn_txt\">".Length + 2);
                            while (Chinese.StartsWith("\t"))
                            {
                                Chinese = Chinese.Substring(1);
                            }
                            Chinese = Chinese.Substring(0, Chinese.IndexOf("\r\n"));
                            while (((IndexF = Chinese.IndexOf("<")) != -1) &&
                                   ((IndexR = Chinese.IndexOf(">")) != -1) &&
                                   IndexR > IndexF)
                            {
                                Chinese = Chinese.Substring(0, Chinese.IndexOf("<")) + Chinese.Substring(Chinese.IndexOf(">") + 1);
                            }
                        }
                        AddLists.Add(System.Text.Encoding.UTF8.GetBytes(Stencence + "\r\n" + Chinese));
                    }
                }
                AddArray(Spelling, AddLists);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    //谷歌图片搜索网页HTML格式读取(有风险)
    public class DownloadPictures : FileList
    {
        public DownloadPictures(string Path)
        {
            TotalPath = Path;
            TypicalName = "Pictures.kl";
        }

        private bool Stop = false;
        public void Abort()
        {
            Stop = true;
            WebClient.Abort();
        }

        //非正式
        private MainClass.CNNWebClient WebClient = new MainClass.CNNWebClient();
        public bool Download(string Spelling)
        {
            Stop = false;
            try
            {
                if (GetCount(Spelling) != 0)
                {
                    return true; 
                }
                string html = WebClient.DownloadString(string.Format("https://www.google.com/search?tbm=isch&q={0} Clipart", Spelling ));
                string Picture = null;
                byte[] DownloadData = null;
                List<byte[]> AddLists = new List<byte[]>();
                for (int m = 0; m < 5 && Stop == false; m++)
                {
                    //找序列号
                    html = html.Substring(html.IndexOf("img ") + "img ".Length);
                    Picture = html.Substring(html.IndexOf("https:"));
                    Picture = Picture.Substring(0, Picture.IndexOf("\""));
                    DownloadData = MainClass.ScaleImage(WebClient.DownloadData(Picture));
                    if (DownloadData != null)
                    {
                        AddLists.Add(DownloadData);
                    }
                }
                AddArray(Spelling, AddLists);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    //文件保存列表列表
    public class FileList
    {
        public string TotalPath = null;
        public string TypicalName = null;

        public void Delete(int Index, string Spelling)
        {
            CreateDirectory(Spelling);
            try
            {
                List<byte[]> Temps = Read(Spelling);
                Temps.RemoveAt(Index);
                Write(Spelling, Temps);
            }
            catch
            {
            }
        }

        public void Clear(string Spelling)
        {
            try
            {
                CreateDirectory(Spelling);
                string BasePath = TotalPath + Spelling[0].ToString() + "\\" + Spelling + "\\" + TypicalName;
                File.Delete(BasePath);
            }
            catch
            {
            }
        }

        public void Write(string Spelling, List<byte[]> Data)
        {
            try
            {
                string BasePath = TotalPath + Spelling[0].ToString() + "\\" + Spelling + "\\" + TypicalName;
                File.WriteAllBytes(BasePath, MainClass.CloneToBytes(Data));
            }
            catch { }
        }

        public List<byte[]> Read(string Spelling)
        {
            CreateDirectory(Spelling);
            List<byte[]> Lists = null;
            try
            {
                string BasePath = TotalPath + Spelling[0].ToString() + "\\" + Spelling + "\\" + TypicalName;
                if (!File.Exists(BasePath))
                {
                    Lists = new List<byte[]>();
                }
                else
                {
                    Lists = (List<byte[]>)MainClass.BytesToClone(File.ReadAllBytes(BasePath));
                }
            }
            catch
            {
                Lists = new List<byte[]>();
            }
            return Lists;
        }

        public void Add(string Spelling, byte[] Data)
        {
            CreateDirectory(Spelling);
            try
            {
                List<byte[]> Temps = Read(Spelling);
                Temps.Add(Data);
                Write(Spelling, Temps);
            }
            catch
            {
            }
        }

        public void AddArray(string Spelling, IEnumerable<byte[]> Data)
        {
            CreateDirectory(Spelling);
            try
            {
                List<byte[]> Temps = Read(Spelling);
                Temps.AddRange(Data);
                Write(Spelling, Temps);
            }
            catch
            {
            }
        }

        public int GetCount(string Spelling)
        {
            return Read(Spelling).Count;
        }

        private void CreateDirectory(string Spelling)
        {
            string BasePath = TotalPath + Spelling[0].ToString() + "\\" + Spelling;
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }
    }

    public class GooglePictures
    {
        private const string Address = "https://www.google.com/search?tbm=isch&q={0} Clipart";
        public static void SearchPictures(string Spelling)
        {

            Process.Start(string.Format(Address, Spelling));
        }
    }

    //百度谷歌API
    public class InternetSpeakeRecognize
    {
        private const string SpeakUrl = @"http://translate.google.cn/translate_tts?ie=UTF-8&q={0}&tl=en&prev=input";

        private static List<Task> SpeakTaskLists = new List<Task>();
        private static HttpWebRequest SpeakRequest = null;
        public static byte[] DownloadSpeak(string text)
        {
            var ms = new List<byte>();
            try
            {
                //过滤中文
                List<char> NewText = new List<char>();
                foreach (var i in text)
                {
                    if (i < 127)
                    {
                        NewText.Add(i);
                    }
                }
                StringBuilder StringBuilder = new StringBuilder();
                foreach (var i in NewText)
                {
                    StringBuilder.Append(i);
                }
                text = StringBuilder.ToString();

                SpeakRequest = (HttpWebRequest)HttpWebRequest.Create(string.Format(SpeakUrl, text));
                SpeakRequest.KeepAlive = false;
                SpeakRequest.Method = "GET";
                using (var stream = SpeakRequest.GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[1];
                    int byteReader;
                    do
                    {
                        byteReader = stream.Read(buffer, 0, 1);
                        ms.Add(buffer[0]);
                    }
                    while (byteReader > 0);
                }
            }
            catch { }
            return ms.ToArray();
        }

        public static void Speak(string text)
        {
            MainClass.PlaySoundFile(null, false);
            if (SpeakRequest != null)
            {
                SpeakRequest.Abort();
            }
            foreach (var i in SpeakTaskLists)
            {
                while (!i.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                i.Dispose();
            }
            SpeakTaskLists.Clear();
            if (text == null)
            {
                return;
            }
            for (int i = SpeakTaskLists.Count - 1; i >= 0; i--)
            {
                if (SpeakTaskLists[i].IsCompleted)
                {
                    SpeakTaskLists[i].Dispose();
                    SpeakTaskLists.RemoveAt(i);
                }
            }
            SpeakTaskLists.Add(new Task(() =>
                {
                    try
                    {
                        var ms = DownloadSpeak(text);
                        if (ms.Length != 0)
                        {
                            MainClass.PlaySoundFile(ms, false);
                        }
                    }
                    catch { }
                }));
            SpeakTaskLists[SpeakTaskLists.Count - 1].Start();
        }
    }
}
