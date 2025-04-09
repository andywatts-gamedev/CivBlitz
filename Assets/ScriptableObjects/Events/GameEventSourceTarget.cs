using System;
using Unity.Mathematics;
using UnityEngine;
 
[Serializable]
[CreateAssetMenu(fileName = "SourceTargetEvent", menuName = "ScriptableObject/Event/SourceTarget")]
public class GameEventSourceTarget : ScriptableObject
{
    public event Action<int2, int2> Handler;
 
    public void Invoke(int2 source, int2 target) => Handler?.Invoke(source, target);
}