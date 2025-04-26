using System.Collections;
using UnityEngine;
using System;
using System.Linq;

public class TurnManager : Singleton<TurnManager>
{
    public bool isPlayerTurn = true;
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

    public void EndTurn()
    {
        if (isAITurnInProgress) return;
        if (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving) {
            StartCoroutine(DelayedEndTurn());
            return;
        }
        isPlayerTurn = false;
        OnTurnChanged?.Invoke();
        StartCoroutine(DoAITurn());
    }

    private IEnumerator DelayedEndTurn()
    {
        while (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving) {
            yield return null;
        }
        isPlayerTurn = false;
        OnTurnChanged?.Invoke();
        StartCoroutine(DoAITurn());
    }

    private IEnumerator DoAITurn() {
        isAITurnInProgress = true;
        if (waitCursor != null) {
            Cursor.SetCursor(waitCursor, new Vector2(waitCursor.width/2, waitCursor.height/2), CursorMode.Auto);
        }

        foreach (var aiCiv in UnitManager.Instance.civUnits.Keys.Where(c => c != Game.Instance.player)) {
            // Move each AI unit for this civ
            while (UnitManager.Instance.civUnits[aiCiv].Any(u => u.movesLeft > 0))
            {
                var (from, to) = ai.GetBestMove(aiCiv); // Assuming ai.GetBestMove takes a civ param now
                if (from == Vector2Int.zero && to == Vector2Int.zero) break;
                
                if (from == to) continue;
                
                if (!UnitManager.Instance.TryGetUnit(from, out var unit) || unit.movesLeft <= 0) continue;
                
                if (UnitManager.Instance.HasUnitAt(to))
                    CombatManager.Instance.TryCombat(from, to);
                else
                    UnitManager.Instance.MoveUnit(from, to);
                    
                yield return new WaitForSeconds(0.5f);
                
                if (UnitManager.Instance.TryGetUnit(to, out var movedUnit))
                {
                    movedUnit.movesLeft = 0;
                    UnitManager.Instance.UpdateUnit(to, movedUnit);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        isPlayerTurn = true;
        isAITurnInProgress = false;
        OnTurnChanged?.Invoke();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        UnitManager.Instance.ResetMoves();
    }
} 