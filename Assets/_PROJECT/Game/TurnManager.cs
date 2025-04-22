using System.Collections;
using UnityEngine;
using System;
using System.Linq;

public class TurnManager : Singleton<TurnManager>
{
    public bool isPlayerTurn = true;
    public Civilization playerCiv;
    public Civilization aiCiv;
    public event Action OnTurnChanged;
    private Texture2D waitCursor;
    private bool isAITurnInProgress;
    private MinMaxAI ai;

    void Start() {
        waitCursor = Resources.Load<Texture2D>("Cursors/Wait");
        if (waitCursor != null) {
            waitCursor.filterMode = FilterMode.Point;
        }
        ai = gameObject.AddComponent<MinMaxAI>();
    }

    public void EndTurn() {
        if (isAITurnInProgress) return;
        
        isPlayerTurn = !isPlayerTurn;
        OnTurnChanged?.Invoke();
        if (!isPlayerTurn) {
            StartCoroutine(DoAITurn());
        } else {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            UnitManager.Instance.ResetMoves();
        }
    }

    private IEnumerator DoAITurn() {
        isAITurnInProgress = true;
        if (waitCursor != null) {
            Cursor.SetCursor(waitCursor, new Vector2(waitCursor.width/2, waitCursor.height/2), CursorMode.Auto);
        }

        // Move each AI unit
        while (UnitManager.Instance.units.Any(u => u.Value.civ == aiCiv && u.Value.movesLeft > 0))
        {
            var (from, to) = ai.GetBestMove();
            if (from == Vector2Int.zero && to == Vector2Int.zero) break;
            
            // Skip if unit is already at destination
            if (from == to) continue;
            
            // Ensure the unit still exists and has movement
            if (!UnitManager.Instance.TryGetUnit(from, out var unit) || unit.movesLeft <= 0) continue;
            
            if (UnitManager.Instance.HasUnitAt(to))
                CombatManager.Instance.TryCombat(from, to);
            else
                UnitManager.Instance.MoveUnit(from, to);
                
            // Wait for movement to complete
            yield return new WaitForSeconds(0.5f);
            
            // Double check the unit's movement was properly updated
            if (UnitManager.Instance.TryGetUnit(to, out var movedUnit))
            {
                movedUnit.movesLeft = 0;
                UnitManager.Instance.UpdateUnit(to, movedUnit);
            }
        }

        yield return new WaitForSeconds(0.5f);
        
        isPlayerTurn = true;
        isAITurnInProgress = false;
        OnTurnChanged?.Invoke();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        UnitManager.Instance.ResetMoves();
    }
} 