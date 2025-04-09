using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Variable/Transform")]
public class TransformVariable : ScriptableObject
{
    [SerializeField]
    private Transform value;

    public Transform Value
    {
        get { return value; }
        set { this.value = value; }
    }
}
