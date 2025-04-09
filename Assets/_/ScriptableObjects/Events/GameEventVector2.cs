using System;
using UnityEngine;
 
[Serializable]
[CreateAssetMenu(fileName = "New Vector2 Event", menuName = "ScriptableObject/Event/Vector2")]
public class GameEventVector2 : ScriptableObject
{
    public event Action<Vector2> Handler;
 
    public void Invoke(Vector2 v)
    {
        Handler?.Invoke(v);
    }
}