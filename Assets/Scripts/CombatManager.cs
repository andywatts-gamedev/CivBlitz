using UnityEngine;
using UnityEngine.Tilemaps;

public class CombatManager : Singleton<CombatManager>
{
    public Tilemap tilemap;

    public bool TryCombat(Vector2Int attackerPos, Vector2Int defenderPos) {
        if (!UnitManager.Instance.TryGetUnit(attackerPos, out var attacker) || 
            !UnitManager.Instance.TryGetUnit(defenderPos, out var defender)) return false;
        
        if (attacker.civ != defender.civ) {
            defender.health -= attacker.unitData.attack;
            attacker.movement = 0;
            
            UnitManager.Instance.UpdateUnit(attackerPos, attacker);
            UnitManager.Instance.UpdateUnit(defenderPos, defender);
            
            if (defender.health <= 0) {
                UnitManager.Instance.RemoveUnit(defenderPos);
            }
            
            return true;
        }
        return false;
    }
} 