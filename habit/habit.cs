using UnityEngine;
using System.Collections.Generic;

public class habit : MonoBehaviour
{
    [SerializeField]
    habitUI ui;
    List<habitItem> habitItems;
    int habitCount=0;
    bool Next;
    int State = 0;
    void Update()
    {
        if (State == 0)
        {
            ui.topArea.text = "请输入一共要完成几个目标";
            habitCount = int.Parse(ui.inputComment);
            State++;
        }
        ui.buttomArea.text = ui.inputComment;
    }
}
