using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Variable/Int2")]
public class Int2Variable : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif
    public int2 Value;

    public void SetValue(int2 value)
    {
        Value = value;
    }

    public void SetValue(Int2Variable value)
    {
        Value = value.Value;
    }

}
