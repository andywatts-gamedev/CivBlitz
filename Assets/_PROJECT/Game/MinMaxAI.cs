// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;

// public class MinMaxAI : MonoBehaviour
// {
//     public int maxDepth = 3;
    
//     public (Vector2Int from, Vector2Int to) GetBestMove()
//     {
//         var bestScore = float.MinValue;
//         var bestMove = (from: Vector2Int.zero, to: Vector2Int.zero);
        
//         // Only consider units that can still move
//         foreach (var unit in UnitManager.Instance.units
//             .Where(u => u.Value.civ == TurnManager.Instance.aiCiv && u.Value.movesLeft > 0))
//         {
//             var pos = unit.Key;
//             var possibleMoves = HexGrid.GetValidMoves(pos, unit.Value.unitData.movement, UnitManager.Instance.playerFlagsTilemap);
            
//             foreach (var move in possibleMoves)
//             {
//                 var score = EvaluateMove(pos, move, unit.Value);
//                 if (score > bestScore)
//                 {
//                     bestScore = score;
//                     bestMove = (pos, move);
//                 }
//             }
//         }
        
//         return bestMove;
//     }

//     private float EvaluateMove(Vector2Int from, Vector2Int to, UnitInstance unit)
//     {
//         var score = 0f;
        
//         // Consider unit's own strength (use melee or ranged based on range)
//         var strength = (unit.unitData.ranged > 0 ? unit.unitData.ranged : unit.unitData.melee) * 
//                       (unit.health / (float)unit.unitData.health);
        
//         // Prefer moves that attack enemy units
//         if (UnitManager.Instance.TryGetUnit(to, out var target) && target.civ != TurnManager.Instance.aiCiv)
//         {
//             var targetStrength = (target.unitData.ranged > 0 ? target.unitData.ranged : target.unitData.melee) * 
//                                (target.health / (float)target.unitData.health);
//             score += target.health * 0.5f;  // Base score for attacking
//             score += (strength - targetStrength) * 0.2f;  // Favor advantageous fights
//         }
            
//         // Prefer moves that get closer to enemy units
//         var nearestEnemy = UnitManager.Instance.units
//             .Where(u => u.Value.civ != TurnManager.Instance.aiCiv)
//             .OrderBy(u => HexGrid.GetDistance(to, u.Key))
//             .FirstOrDefault();
            
//         if (nearestEnemy.Key != default)
//         {
//             var distance = HexGrid.GetDistance(to, nearestEnemy.Key);
//             score += 1f / (distance + 1);  // Closer is better
            
//             // Consider if we can reach the enemy next turn
//             if (distance <= unit.unitData.movement)
//                 score += 0.5f;
//         }
            
//         return score;
//     }
// } 