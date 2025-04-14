using System.Collections;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    public bool isPlayerTurn = true;
    public Civilization playerCiv;
    public Civilization aiCiv;

    public void EndTurn() {
        isPlayerTurn = !isPlayerTurn;
        if (!isPlayerTurn) StartCoroutine(DoAITurn());
        else UnitManager.Instance.ResetMoves();
    }

    private IEnumerator DoAITurn() {
        yield return new WaitForSeconds(1.5f);
        EndTurn();
    }

    public bool AlltilemapMovedForCiv(Civilization civ) {
        foreach (var kvp in UnitManager.Instance.units)
        {
            if (kvp.Value.civ == civ && kvp.Value.movement > 0) return false;
        }
        return true;
    }
} 