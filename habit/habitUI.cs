using UnityEngine;
using UnityEngine.UI;

public class habitUI : MonoBehaviour
{
    [SerializeField]
    public Text topArea;
    [SerializeField]
    public Text buttomArea;
    [SerializeField]
    InputField input;
    [SerializeField]
    Button confirm;
    public string inputComment;
    void Start()
    {
        input.onEndEdit.AddListener(End_Value);
        confirm.onClick.AddListener(()=>Input.GetKeyDown(KeyCode.KeypadEnter));
    }
    public void End_Value(string input)
    {
        inputComment = input;
    }
}
