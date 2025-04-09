using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Variable/Quaternion")]
public class QuaternionVariable : ScriptableObject
{
    public Quaternion Value;

    public void SetValue(Quaternion value)
    {
        Value = value;
    }

}
