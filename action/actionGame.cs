using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class actionGame : MonoBehaviour
{
    [SerializeField]
    actionRole role=default;

    Vector2 Pos = new Vector2(0, 1.73f);
    private void Awake()
    {
        role.Init(Pos);
    }
}
