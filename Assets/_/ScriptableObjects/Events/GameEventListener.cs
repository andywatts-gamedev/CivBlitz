using UnityEngine;
using UnityEngine.Events;
 
public class GameEventListener : MonoBehaviour
{
    [SerializeField]
    private GameEvent gameEvent;
 
    [SerializeField]
    private UnityEvent response;
 
    private void OnEnable()
    {
        gameEvent.Handler += OnEventRaised;
    }
 
    private void OnDisable()
    {
        gameEvent.Handler -= OnEventRaised;
    }
    
    public void OnEventRaised()
    {
        response.Invoke();
    } 
}