using UnityEngine;
using UnityEngine.UI;

public class EnglishUI : MonoBehaviour
{
    public Text topText;
    public Text wordArea;
    public Text buttomArea;
    public InputField inputArea;
    public string inputComment;
    public Button Button1;
    public Button Button2;
    public EnglishLearn englishLearn;
    private void Start()
    {
        inputArea.transform.GetComponent<InputField>().onEndEdit.AddListener(End_Value);
        Button2.transform.GetComponent<Button>().onClick.AddListener(Confirm);
        Button1.transform.GetComponent<Button>().onClick.AddListener(PressEnter);
    }

    public void End_Value(string input)
    {
        inputComment = input;
    }

    public void InputAreaClear()
    {
        inputArea.Select();
        inputArea.text = "";
    }
    void Confirm()
    {
        englishLearn.GoNext = true;
    }
    void PressEnter()
    {
        Input.GetKeyDown(KeyCode.KeypadEnter);
    }
}

