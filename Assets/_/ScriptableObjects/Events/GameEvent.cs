using System;
using UnityEngine;
 
[CreateAssetMenu(fileName = "New Event", menuName = "ScriptableObject/Event/Default")]
public class GameEvent : ScriptableObject
{
    public event Action Handler;
 
    public void Invoke()
    {
        Handler?.Invoke();
    }
}