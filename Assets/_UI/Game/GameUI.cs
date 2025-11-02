using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameEvent onTurnChanged;
    
    void Start()
    {
        if (onTurnChanged != null) onTurnChanged.Handler += UpdateTurnLabels;
        UpdateTurnLabels();
    }

    void OnDisable() 
    {
        if (onTurnChanged != null) onTurnChanged.Handler -= UpdateTurnLabels;
    }

    void UpdateTurnLabels()
    {
        var player = Game.Instance.player;
        if (player == null || UnitManager.Instance.civUnits.Count < 2) return;
        var aiCiv = UnitManager.Instance.civUnits.Keys.First(c => c != player.civilization);
    }
}

