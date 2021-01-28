using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CreatingFloatRange))]
public class CreatingFloatRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        EditorGUI.BeginProperty(position,label,property) ;
        EditorGUI.PropertyField(position,property.FindPropertyRelative("min"));
        EditorGUI.EndProperty();
    }
}
