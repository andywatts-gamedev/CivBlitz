using System.Collections;
using UnityEngine;
using System;
using System.Linq;

public class TurnManager : Singleton<TurnManager>
{
    public bool isPlayerTurn = true;
    
    [SerializeField] private GameEvent onTurnChanged;
    
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
        onTurnChanged?.Invoke();
        // Don't auto-start AI turn - wait for user to click Next Turn button
    }
    
    public void StartAITurn()
    {
        if (isAITurnInProgress || isPlayerTurn) return;
        StartCoroutine(DoAITurn());
    }

    private IEnumerator DelayedEndTurn()
    {
        while (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving) {
            yield return null;
        }
        isPlayerTurn = false;
        onTurnChanged?.Invoke();
        // Don't auto-start AI turn - wait for user to click Next Turn button
    }

    private IEnumerator DoAITurn() {
        isAITurnInProgress = true;
        if (waitCursor != null) {
            Cursor.SetCursor(waitCursor, new Vector2(waitCursor.width/2, waitCursor.height/2), CursorMode.Auto);
        }

        foreach (var aiCiv in UnitManager.Instance.civUnits.Keys.Where(c => c != Game.Instance.player.civilization)) {
            // Move each AI unit for this civ
            while (UnitManager.Instance.civUnits[aiCiv].Any(u => u.actionsLeft > 0))
            {
                var (from, to) = ai.GetBestMove(aiCiv); // Assuming ai.GetBestMove takes a civ param now
                if (from == Vector2Int.zero && to == Vector2Int.zero) break;
                
                if (from == to) continue;
                
                if (!UnitManager.Instance.TryGetUnit(from, out var unit) || unit.actionsLeft <= 0) continue;
                
                if (UnitManager.Instance.HasUnitAt(to))
                    CombatManager.Instance.TryCombat(from, to);
                else
                    UnitManager.Instance.MoveUnit(from, to);
                
                // Wait for any movement/combat animations to complete
                while (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving)
                {
                    yield return null;
                }
                    
                yield return new WaitForSeconds(0.5f);
                
                if (UnitManager.Instance.TryGetUnit(to, out var movedUnit))
                {
                    movedUnit.actionsLeft = 0;
                    UnitManager.Instance.UpdateUnit(to, movedUnit);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        UnitManager.Instance.ResetMoves();
        UnitManager.Instance.ResetUnitStates();
        
        isPlayerTurn = true;
        isAITurnInProgress = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        onTurnChanged?.Invoke();
    }
} 