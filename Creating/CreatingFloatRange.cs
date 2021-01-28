using UnityEngine;

[System.Serializable]
public struct CreatingFloatRange 
{
    public float min, max;
    public float RandomValueInRange {
        get {
            return Random.Range(min,max);
        }
    }
}
