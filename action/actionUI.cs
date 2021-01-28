using UnityEngine;
using FairyGUI;
using UnityEngine.UI;

public class actionUI : MonoBehaviour
{
    public JoystickModule _joystick;
    [SerializeField]
    Text X,Y;
    public Transform _transform;
    void Start()
    {
        GComponent mainView = GetComponent<UIPanel>().ui;
        _joystick = new JoystickModule(mainView);
    }
    private void Update()
    {
        //_joystick.Control(_transform,0.0005f);
        //X.text = string.Format("X:{0}", _transform.position.x);
        //Y.text = string.Format("Y:{0}", _transform.position.z);
    }
}
