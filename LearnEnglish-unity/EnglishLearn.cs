using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class EnglishLearn : MonoBehaviour
{
    public EnglishUI UI;
    List<Word> wordList = new List<Word>();
    string fname;
    private void Start()
    {
        try
        {
            DirectoryInfo root = new DirectoryInfo(Application.streamingAssetsPath);
            foreach (FileInfo f in root.GetFiles("*.xls"))
            {
            fname = f.Name;
            var path = Path.Combine(Application.streamingAssetsPath,fname);
            FileStream fs =
#if UNITY_EDITOR
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
#elif UNITY_STANDALONE_WIN
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
#endif
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
                List<string> chinese = new List<string>();//中文意思
                List<string> Prounce_US = new List<string>();//音标1
                List<string> Prounce_UK = new List<string>();//音标2
                List<string> english = new List<string>();//英文意思
                List<Word> tempWordList = new List<Word>();
                if (fname.Contains("xls") || fname.Contains("xlsx"))
                {
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
                    for (var i = 1; i < st.LastRowNum; i++)
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            var currentCell = st.GetRow(i).GetCell(a);
                            if (a == 0) { english.Add(currentCell.StringCellValue); }
                            if (a == 1) { Prounce_US.Add(currentCell.StringCellValue); }
                            if (a == 2) { Prounce_UK.Add(currentCell.StringCellValue); }
                            if (a == 3) { chinese.Add(currentCell.StringCellValue); }
                        }
                    }
                }
                if (fname.Contains("csv"))//功能未写完
                {
                   
                    string str = "";
                    bool _stop = true;
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
                        string[] Array;
                        Array = str.Split('，');
                        Debug.Log(str);
                        for (int i = 0; i < Array.Length; i++)
                        {

                        }
                    }                   
                }

                for (int i = 0; i < english.Count; i++)
                {
                    tempWordList.Add(new Word(chinese[i], english[i], Prounce_US[i], Prounce_UK[i]));
                }

                //Debug.Log(tempWordList.Count);
                for (int i = 0; i < tempWordList.Count; i++)
                {
                    string str = tempWordList[i].Chinese;
                    if (!str.Contains("未识别") && !str.Contains("或者地名"))
                    {
                        wordList.Add(tempWordList[i]);
                    }
                }
                //Debug.Log(wordList.Count);
                //foreach (var item in wordList)
                //{
                //    Debug.Log("*************\n"+ item.English + "\n" + item.Prounce_US + "\n" + item.Prounce_UK + "\n"+ item.Chinese);
                //}
                sr.Close();
            }
        }
        catch(Exception e)
        {
            Debug.Log("The process failed:"+e.ToString());
        }

    }
    public bool GoNext=false;
    int NextIf = 0;
    int Num = 0;//需要学习单词
    int Num2 = 0;//错误单词
    List<Word> newList = null;
    List<Word> wrongWord = new List<Word>();
    int wordIndex1 = 0;//第一轮单词学习索引
    int wordIndex2 = 0;//第二轮单词学习索引
    int wordIndex3 = 0;//第三轮单词学习索引
    int tempIndex;
    int score1 = 0, score2 = 0;
    bool showWord=true;
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Y)) { GoNext = true; }
        if(GoNext&& NextIf == 0){
            if (wordList.Count == 0)
            {
                UI.topText.text = "未检测到excel文件！";
            }
            else
            {
                UI.topText.text = string.Format("请输入你要背诵的单词个数(0~{0})", wordList.Count);
                UI.wordArea.text =string.Format("检测到文件：{0}",fname);
                GoNext = false;
                UI.inputArea.contentType = UnityEngine.UI.InputField.ContentType.IntegerNumber;
                NextIf++;
            }
        }
        if (NextIf==0) return;
        
        if (Num == 0&&NextIf==1&& UI.inputComment != null&& UI.inputComment != "")
        {           
            if (int.Parse(UI.inputComment) > wordList.Count)
            {
                UI.buttomArea.text = "单词数超过单词本数量";
            }
            else
            {
                UI.wordArea.text = "";
                Num = int.Parse(UI.inputComment); newList = GetRandomWordList(Num); NextIf++;
                UI.inputArea.contentType = UnityEngine.UI.InputField.ContentType.Alphanumeric;
            }
        }
        if (NextIf == 1) return;
        if (NextIf == 2) { UI.InputAreaClear(); UI.inputComment = null; NextIf++; }       
        if (NextIf == 3) {
            UI.topText.text = string.Format("开始背单词(一共{0}个单词)", newList.Count);
            if (wordIndex1 < newList.Count)
            {
                UI.buttomArea.text = "按NEXT继续";
                if (GoNext)
                {
                    UI.wordArea.text = string.Format("{0}\n{1}\n{2}", newList[wordIndex1].English, newList[wordIndex1].Prounce_UK, newList[wordIndex1].Chinese);
                    wordIndex1++;
                    GoNext = false;
                }
            }
            else
            {
                UI.topText.text = "下面开始检查学习效果(按回车或Enter键输入)";
                NextIf++;
                GoNext = false;
            }
        }    
        if (GoNext&& NextIf==4) {
            UI.buttomArea.text = string.Format("请根据中文意思输入英文单词,点击Enter按钮或者按键盘Enter(按NEXT开始)");
            UI.wordArea.text = "";
            NextIf++;
            GoNext = false;
        }
        if (NextIf == 5)
        {
            if (GoNext && wordIndex2 < Num)
            {
                if (UI.buttomArea.text == string.Format("请根据中文意思输入英文单词(按NEXT开始)")) { UI.buttomArea.text = ""; }
                if (UI.inputComment == "" && !showWord) { UI.buttomArea.text = "请输入单词再继续！";return; }
                tempIndex = UnityEngine.Random.Range(0, newList.Count);
                UI.wordArea.text = string.Format("{0}\n请输入正确的英文单词：", newList[tempIndex].Chinese);
                UI.buttomArea.text = "";
                showWord = false;
                GoNext = false;
            }
            else if(wordIndex2 >= Num)
            {
                UI.topText.text = string.Format("接下来对错误的单词进行复习,一共{0}个单词(按回车或Enter键输入   )", wrongWord.Count);
                NextIf++;
                Num2 = wrongWord.Count;
                GoNext = false;
            }
        }
        if (NextIf == 5)
        {
            if (UI.inputComment == null || UI.inputComment == "" || showWord) {return; }
            if (UI.inputComment == newList[tempIndex].English)
            {
                if (wordIndex2 < Num-1) { UI.buttomArea.text = "答对了!看下一个"; } else { UI.buttomArea.text = "答对了!"; }
                if (showWord) { return; }
                UI.InputAreaClear();
                newList.RemoveAt(tempIndex);
                wordIndex2++;
                GoNext = true;
                UI.inputComment = null;
                showWord = true;
            }
            else if (UI.inputComment != newList[tempIndex].English && UI.inputComment != null)
            {
                UI.buttomArea.text = string.Format("很遗憾！正确的英文单词为：{0}(按NEXT继续)", newList[tempIndex].English);
                if (showWord) { return; }
                score1++;
                UI.InputAreaClear();
                wrongWord.Add(newList[tempIndex]);
                newList.RemoveAt(tempIndex);
                wordIndex2++;
                UI.inputComment = null;
                showWord = true;
            }
        }
        if (GoNext&&NextIf == 6)
        {
            UI.buttomArea.text = string.Format("请根据中文意思输入英文单词,点击Enter按钮或者按键盘Enter(按NEXT开始)");
            UI.wordArea.text = "";
            NextIf++;
            GoNext = false;
        }
        if (NextIf == 7)
        {
            if (GoNext && wordIndex3 < Num2)
            {
                if (UI.buttomArea.text == string.Format("请根据中文意思输入英文单词(按NEXT开始)")) { UI.buttomArea.text = ""; }
                if (UI.inputComment == "" && !showWord) { UI.buttomArea.text = "请输入单词再继续！"; return; }
                tempIndex = UnityEngine.Random.Range(0, wrongWord.Count);
                UI.wordArea.text = string.Format("{0}\n请输入正确的英文单词：", wrongWord[tempIndex].Chinese);
                UI.buttomArea.text = "";
                showWord = false;
                GoNext = false;
            }
            else if (wordIndex3 >= Num2)
            {
                UI.topText.text = "恭喜你完成学习！";
                UI.wordArea.text = string.Format("本次一共背了{0}个单词，在第一轮复习中答错{1}个单词，在第二轮错题复习中答错{2}个单词", Num, score1, score2);
                if (score1 + score2 < Num / 2)
                {
                    UI.buttomArea.text = "学习效果佳，恭喜你！！";
                }
                else 
                {
                    UI.buttomArea.text = "学习效果一般，再接再厉哦";
                }
                NextIf++;
                GoNext = false;
            }
        }
        if (NextIf == 7) 
        {
            if (UI.inputComment == null || UI.inputComment == ""||showWord) { return; }
            if (UI.inputComment == wrongWord[tempIndex].English)
            {
                if (wordIndex3 < Num - 1) { UI.buttomArea.text = "答对了!看下一个"; } else { UI.buttomArea.text = "答对了!"; }
                if (showWord) { return; }
                UI.InputAreaClear();
                wrongWord.RemoveAt(tempIndex);
                wordIndex3++;
                GoNext = true;
                UI.inputComment = null;
                showWord = true;
            }
            else if (UI.inputComment != wrongWord[tempIndex].English && UI.inputComment != null)
            {
                UI.buttomArea.text = string.Format("很遗憾！正确的英文单词为：{0}(按NEXT继续)", wrongWord[tempIndex].English);
                if (showWord) { return; }
                score2++;
                UI.InputAreaClear();
                wrongWord.RemoveAt(tempIndex);
                wordIndex3++;
                UI.inputComment = null;
                showWord = true;
            }
        }
    }

    List<Word> GetRandomWordList(int num)
    {
        List<Word> newList = new List<Word>();
        System.Random random = new System.Random();
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
}

