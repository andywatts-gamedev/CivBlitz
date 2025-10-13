using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class GameUI : MonoBehaviour
{
    void Start()
    {
        TurnManager.Instance.OnTurnChanged += UpdateTurnLabels;
        UpdateTurnLabels();
    }

    void OnDisable() 
    {
        TurnManager.Instance.OnTurnChanged -= UpdateTurnLabels;
    }

    void UpdateTurnLabels()
    {
        var player = Game.Instance.player;
        if (player == null || UnitManager.Instance.civUnits.Count < 2) return;
        var aiCiv = UnitManager.Instance.civUnits.Keys.First(c => c != player.civilization);
    }
}

