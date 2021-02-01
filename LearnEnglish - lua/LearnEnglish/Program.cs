using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
namespace LearnEnglish
{
    class Program
    {
        #region
        static List<Word> wordList = new List<Word>();
        static void Main(string[] args)
        {
            Console.WriteLine("本程序支持.xls或者.xlsx文件");
            try
            {
                string path = Directory.GetCurrentDirectory();
                DirectoryInfo root = new DirectoryInfo(path);
                foreach (FileInfo f in root.GetFiles("*.csv"))
                {
                    string fname = f.Name;
                    Console.WriteLine(fname);
                    FileStream fs = new FileStream(path + "/" + fname, FileMode.Open, FileAccess.Read, FileShare.None);
                    StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
                    string str = "";
                    bool _stop = true;

                    //int i = 1;
                    while (_stop)
                    {
                        str = sr.ReadLine();
                        _stop = str != null;
                        if (str == null)
                        {
                            _stop = false;
                            sr.Close();
                            continue;
                        }
                        //Console.WriteLine(str);
                        string[] Array;
                        Array = str.Split('，');
                        foreach (var item in Array)
                        {
                            Console.WriteLine(item);
                        }
                    }
                    IWorkbook workbook;
                    if (fname.Contains(".xls"))
                    {
                        workbook = new HSSFWorkbook(fs);
                    }
                    else
                    {
                        workbook = new XSSFWorkbook(fs);
                    }
                    var st = workbook.GetSheetAt(0);
                    List<string> chinese = new List<string>();//中文意思
                    List<string> english = new List<string>();//英文意思
                    List<Word> tempWordList = new List<Word>();
                    for (var i = 1; i < st.LastRowNum; i++)
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            if (a != 1 && a != 2)
                            {
                                var currentCell = st.GetRow(i).GetCell(a);
                                if (a == 0) { english.Add(currentCell.StringCellValue); }
                                if (a == 3) { chinese.Add(currentCell.StringCellValue); }
                            }
                        }
                    }
                    for (int i = 0; i < english.Count; i++)
                    {
                        tempWordList.Add(new Word(chinese[i], english[i]));
                    }

                    //Console.WriteLine(tempWordList.Count);
                    for (int i = 0; i < tempWordList.Count; i++)
                    {
                        string str1 = tempWordList[i].Chinese;
                        if (!str1.Contains("未识别") && !str1.Contains("或者地名"))
                        {
                            //Console.WriteLine(wordList[i].Chinese);
                            wordList.Add(tempWordList[i]);
                        }
                    }
                    //Console.WriteLine(wordList.Count);
                    //foreach (var item in wordList)
                    //{
                    //    Console.WriteLine("*************\n{0}\n{1}", item.English, item.Chinese);
                    //}
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            if (wordList.Count == 0) { Console.WriteLine("本次学习单词数量为0，请将文件放入netcoreapp3.1下");return; }
            Console.WriteLine("单词文件加载完毕");
            Console.WriteLine("************************");
            Console.WriteLine("请输入你要背诵的单词个数(0~{0})：", wordList.Count);
            string inputnumber = Console.ReadLine();
            int Num = Int32.Parse(inputnumber);
            List<Word> newList = GetRandomWordList(Num);
            //第一轮学习
            #region
            Console.WriteLine("开始背单词:(按y继续)学习过程中按Esc键退出学习");
            //bool IsFirstLearnNotOver = true;
            //string inputWord = Console.ReadLine();
            for (int i = 0; i < Num;)
            {
                if (Console.ReadKey().KeyChar == 'y')
                {
                    Console.WriteLine("\n{0} 中文意思为 {1}", newList[i].English, newList[i].Chinese);
                    Console.WriteLine("是否继续？（y）");
                    i++;
                }
                else if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    return;
                }
                    
            }

            #endregion
            //第一轮复习
            #region
            int score1=0,score2=0;
            List<Word> wrongWord = new List<Word>();
            Console.WriteLine("恭喜你全部学完了！下面开始检查学习效果");
            Console.WriteLine("请根据中文意思输入英文单词(按y开始)");
            if (Console.ReadKey().KeyChar == 'y')
            {
                Console.Clear();
            }
            Random random = new Random();
            for (int i = 0; i < Num;)
            {
                int tempIndex = random.Next(newList.Count);
                if (newList.Contains(newList[tempIndex]))
                {
                    Console.WriteLine("********************");
                    Console.WriteLine("{0}\n请输入正确的英文单词：", newList[tempIndex].Chinese);
                    string inputWord = Console.ReadLine();
                    if (inputWord == newList[tempIndex].English)
                    {
                        Console.WriteLine("答对了!看下一个");
                        newList.RemoveAt(tempIndex);
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("很遗憾！正确的英文单词为：{0}", newList[tempIndex].English);
                        score1++;
                        wrongWord.Add(newList[tempIndex]);
                        newList.RemoveAt(tempIndex);
                        i++;
                    }
                }
            }
            #endregion
            //错题复习
            #region
            Console.WriteLine("接下来对错误的单词进行复习");
            Console.WriteLine("按y开始");
            if (Console.ReadKey().KeyChar == 'y')
            {
                Console.Clear();
            }
            int Num2 = score1;
            for (int i = 0; i < Num2;)
            {
                int tempIndex = random.Next(wrongWord.Count);
                if (wrongWord.Contains(wrongWord[tempIndex]))
                {
                    Console.WriteLine("********************");
                    Console.WriteLine("{0}\n请输入正确的英文单词：", wrongWord[tempIndex].Chinese);
                    string inputWord = Console.ReadLine();
                    if (inputWord == wrongWord[tempIndex].English)
                    {
                        Console.WriteLine("答对了!看下一个");
                        wrongWord.RemoveAt(tempIndex);
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("很遗憾！正确的英文单词为：{0}", wrongWord[tempIndex].English);
                        score2++;
                        wrongWord.RemoveAt(tempIndex);
                        i++;
                    }
                }
            }
            #endregion
            Console.WriteLine("本次一共背了{0}个单词，在第一轮复习中答错{1}个单词，在第二轮错题复习中答错{2}个单词",Num,score1,score2);
            if(score1+score2<Num)
            {
                Console.WriteLine("学习效果佳，恭喜你！！");
            }
            else
            {
                Console.WriteLine("学习效果一般，再接再厉哦");
            }
            Console.WriteLine("按任意键退出程序");
            Console.ReadKey();
        }
        static List<Word> GetRandomWordList(int num)
        {
            List<Word> newList = new List<Word>();
            Random random = new Random();
            for (int i = 0; i < num;)
            {
                int tempIndex = random.Next(wordList.Count);
                if (!newList.Contains(wordList[tempIndex]))
                {
                    newList.Add(wordList[tempIndex]);
                    i++;
                }
            }
            return newList;
        }
        #endregion

    }
}  
            

