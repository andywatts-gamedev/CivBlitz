using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MinMaxAI : MonoBehaviour
{
    public int maxDepth = 3;
    
    public (Vector2Int from, Vector2Int to) GetBestMove(Civilization aiCiv)
    {
        var bestScore = float.MinValue;
        var bestMove = (from: Vector2Int.zero, to: Vector2Int.zero);
        
        foreach (var unit in UnitManager.Instance.civUnits[aiCiv].Where(u => u.actionsLeft > 0))
        {
            var pos = unit.position;
            var possibleMoves = HexGrid.GetValidMoves(pos, unit.unit.movement, UnitManager.Instance.unitTilemap);
            
            foreach (var move in possibleMoves)
            {
                var score = EvaluateMove(pos, move, unit, aiCiv);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (pos, move);
                }
            }
        }
        
        return bestMove;
    }

    private float EvaluateMove(Vector2Int from, Vector2Int to, UnitInstance unit, Civilization aiCiv)
    {
        var score = 0f;
        
        var strength = (unit.unit.ranged > 0 ? unit.unit.ranged : unit.unit.melee) * 
                      (unit.health / (float)unit.unit.health);
        
        if (UnitManager.Instance.TryGetUnit(to, out var target) && target.civ != aiCiv)
        {
            var targetStrength = (target.unit.ranged > 0 ? target.unit.ranged : target.unit.melee) * 
                               (target.health / (float)target.unit.health);
            score += target.health * 0.5f;
            score += (strength - targetStrength) * 0.2f;
        }
            
        var nearestEnemy = UnitManager.Instance.units
            .Where(u => u.Value.civ != aiCiv)
            .OrderBy(u => HexGrid.GetDistance(to, u.Key))
            .FirstOrDefault();
            
        if (nearestEnemy.Key != default)
        {
            var distance = HexGrid.GetDistance(to, nearestEnemy.Key);
            score += 1f / (distance + 1);
            
            if (distance <= unit.unit.movement)
                score += 0.5f;
        }
            
        return score;
    }
} 