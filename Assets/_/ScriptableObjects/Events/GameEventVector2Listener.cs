using UnityEngine;
using UnityEngine.Events;
 
public class GameEventVector2Listener : MonoBehaviour
{
    [SerializeField]
    private GameEventVector2 gameEvent;
 
    [SerializeField]
    private UnityEvent response;
 
    private void OnEnable()
    {
        if (gameEvent != null)
            gameEvent.Handler += OnEventRaised;
    }
 
    private void OnDisable()
    {
        if (gameEvent != null)
            gameEvent.Handler -= OnEventRaised;
    }
    
    public void OnEventRaised(Vector2 vector2)
    {
        response.Invoke();
    } 
}