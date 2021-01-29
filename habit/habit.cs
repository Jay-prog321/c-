using UnityEngine;
using System.Collections.Generic;

public class habit : MonoBehaviour
{
    [SerializeField]
    habitUI ui;
    List<habitItem> habitItems;
    int habitCount;
    void Update()
    {
        ui.topArea.text = "请输入第一个目标";
        ui.buttomArea.text = ui.inputComment;
    }
}
