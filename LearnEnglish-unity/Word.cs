using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word
{
        public string Chinese;
        public string English;
        public string Prounce_US;
        public string Prounce_UK;
    public Word(string chinese, string english,string p_US,string p_UK)
      {
            Chinese = chinese;
            English = english;
        Prounce_US = p_US;
        Prounce_UK = p_UK;
      }
}
