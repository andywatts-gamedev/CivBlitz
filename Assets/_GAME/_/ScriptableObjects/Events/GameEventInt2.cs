using System;
using Unity.Mathematics;
using UnityEngine;
 
[Serializable]
[CreateAssetMenu(fileName = "New Int2 Event", menuName = "ScriptableObject/Event/Int2")]
public class GameEventInt2 : ScriptableObject
{
    public event Action<int2?> Handler;
 
    public void Invoke(int2? i)
    {
        Handler?.Invoke(i);
    }
}