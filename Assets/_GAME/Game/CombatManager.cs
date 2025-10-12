using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Linq;

public class CombatManager : Singleton<CombatManager>
{
    private const float COMBAT_DURATION = 1f;
    public bool isCombatMoving;

    public bool TryCombat(Vector2Int attackerPos, Vector2Int defenderPos) {
        if (!UnitManager.Instance.TryGetUnit(attackerPos, out var attacker) || 
            !UnitManager.Instance.TryGetUnit(defenderPos, out var defender)) return false;
        
        if (attacker.civ != defender.civ) {
            var distance = HexGrid.GetDistance(attackerPos, defenderPos);
            var isRanged = attacker.unit.type == UnitType.Ranged;
            
            // Check if target is in range
            if (isRanged && distance > attacker.unit.range) return false;
            
            // Use ranged or melee strength based on unit type
            var strengthDiff = isRanged ? 
                attacker.unit.ranged - defender.unit.melee :
                attacker.unit.melee - defender.unit.melee;
            
            // Attack damage
            var attackDamage = Mathf.RoundToInt(30 * Mathf.Exp(0.04f * strengthDiff) * Random.Range(0.8f, 1.2f));
            // Retaliation damage only for melee
            var retaliationDamage = !isRanged && distance <= 1 ? 
                Mathf.RoundToInt(30 * Mathf.Exp(0.04f * -strengthDiff) * Random.Range(0.8f, 1.2f)) : 
                0;
            
            if (attackDamage > 0)
            {
                var attackerFlagTilemap = UnitManager.Instance.flags[attacker.civ];
                var defenderFlagTilemap = UnitManager.Instance.flags[defender.civ];
                var unitTilemap = UnitManager.Instance.unitTilemap;

                var attackerFlagTile = attackerFlagTilemap.GetTile((Vector3Int)attackerPos) as Tile;
                var defenderFlagTile = defenderFlagTilemap.GetTile((Vector3Int)defenderPos) as Tile;
                var attackerUnitTile = unitTilemap.GetTile((Vector3Int)attackerPos) as UnitTile;
                var defenderUnitTile = unitTilemap.GetTile((Vector3Int)defenderPos) as UnitTile;
                
                if (attackerFlagTile == null || defenderFlagTile == null || 
                    attackerUnitTile == null || defenderUnitTile == null) {
                    Debug.LogError($"Missing tiles at positions - Attacker: {attackerPos}, Defender: {defenderPos}");
                    return false;
                }
                
                defender.health -= attackDamage;
                attacker.health -= retaliationDamage;
                attacker.movesLeft = 0;
                
                UnitManager.Instance.UpdateUnit(attackerPos, attacker);
                UnitManager.Instance.UpdateUnit(defenderPos, defender);
                
                StartCoroutine(CombatCoroutine(
                    attackerPos, defenderPos, 
                    defender.health <= 0, attacker.health <= 0,
                    attackDamage, retaliationDamage,
                    attackerFlagTile, defenderFlagTile,
                    attackerUnitTile, defenderUnitTile,
                    attacker.civ, defender.civ
                ));
                return true;
            }
        }
        return false;
    }

    private IEnumerator CombatCoroutine(
        Vector2Int attackerPos, Vector2Int defenderPos, 
        bool defenderDies, bool attackerDies,
        int attackDamage, int retaliationDamage,
        Tile attackerFlagTile, Tile defenderFlagTile,
        UnitTile attackerUnitTile, UnitTile defenderUnitTile,
        Civilization attackerCiv, Civilization defenderCiv)
    {
        isCombatMoving = true;
        
        var attackerFlagTilemap = UnitManager.Instance.flags[attackerCiv];
        var defenderFlagTilemap = UnitManager.Instance.flags[defenderCiv];
        var unitTilemap = UnitManager.Instance.unitTilemap;
        
        // Hide tiles
        attackerFlagTilemap.SetTile((Vector3Int)attackerPos, null);
        defenderFlagTilemap.SetTile((Vector3Int)defenderPos, null);
        unitTilemap.SetTile((Vector3Int)attackerPos, null);
        unitTilemap.SetTile((Vector3Int)defenderPos, null);
        
        // Create moving sprites
        var attackerFlagSprite = CreateMovingSprite(attackerFlagTile, attackerPos, attackerCiv, true);
        var attackerUnitSprite = CreateMovingSprite(attackerUnitTile, attackerPos, attackerCiv, false);
        var defenderFlagSprite = CreateMovingSprite(defenderFlagTile, defenderPos, defenderCiv, true);
        var defenderUnitSprite = CreateMovingSprite(defenderUnitTile, defenderPos, defenderCiv, false);

        // Colors
        var attackerColor = Game.Instance.civilizations[attackerCiv].color;
        var defenderColor = Game.Instance.civilizations[defenderCiv].color;
        
        // Calculate meeting point
        var start = attackerFlagTilemap.CellToWorld((Vector3Int)attackerPos);
        var end = defenderFlagTilemap.CellToWorld((Vector3Int)defenderPos);
        var meetingPoint = Vector3.Lerp(start, end, 0.33f);
        
        // Move to meeting point
        var elapsed = 0f;
        while (elapsed < COMBAT_DURATION)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
            var lerpedAttackerPos = Vector3.Lerp(start, meetingPoint, t);
            var lerpedDefenderPos = Vector3.Lerp(end, meetingPoint, t);
            
            attackerFlagSprite.transform.position = lerpedAttackerPos;
            attackerUnitSprite.transform.position = lerpedAttackerPos;
            defenderFlagSprite.transform.position = lerpedDefenderPos;
            defenderUnitSprite.transform.position = lerpedDefenderPos;
            yield return null;
        }
        
        FloatingCombatText.Create(
            meetingPoint + Vector3.up * 0.5f, 
            attackDamage, defenderColor, defenderFlagTilemap.CellToWorld((Vector3Int)defenderPos),
            retaliationDamage, attackerColor, attackerFlagTilemap.CellToWorld((Vector3Int)attackerPos)
        );
        
        if (defenderDies)
        {
            UnitManager.Instance.RemoveUnit(defenderPos);
            Destroy(defenderFlagSprite);
            Destroy(defenderUnitSprite);
        }
        
        if (attackerDies)
        {
            UnitManager.Instance.RemoveUnit(attackerPos);
            Destroy(attackerFlagSprite);
            Destroy(attackerUnitSprite);
        }
        
        if (!defenderDies && !attackerDies)
        {
            elapsed = 0f;
            while (elapsed < COMBAT_DURATION)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
                attackerFlagSprite.transform.position = Vector3.Lerp(meetingPoint, start, t);
                attackerUnitSprite.transform.position = Vector3.Lerp(meetingPoint, start, t);
                defenderFlagSprite.transform.position = Vector3.Lerp(meetingPoint, end, t);
                defenderUnitSprite.transform.position = Vector3.Lerp(meetingPoint, end, t);
                yield return null;
            }
            
            // Only restore tiles if units survived
            attackerFlagTilemap.SetTile((Vector3Int)attackerPos, attackerFlagTile);
            attackerFlagTilemap.SetTransformMatrix((Vector3Int)attackerPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            
            defenderFlagTilemap.SetTile((Vector3Int)defenderPos, defenderFlagTile);
            defenderFlagTilemap.SetTransformMatrix((Vector3Int)defenderPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            
            unitTilemap.SetTile((Vector3Int)attackerPos, attackerUnitTile);
            unitTilemap.SetTransformMatrix((Vector3Int)attackerPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
            
            unitTilemap.SetTile((Vector3Int)defenderPos, defenderUnitTile);
            unitTilemap.SetTransformMatrix((Vector3Int)defenderPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
        }
        
        // Only destroy sprites that weren't already destroyed
        if (!attackerDies) Destroy(attackerFlagSprite);
        if (!attackerDies) Destroy(attackerUnitSprite);
        if (!defenderDies) Destroy(defenderFlagSprite);
        if (!defenderDies) Destroy(defenderUnitSprite);
        isCombatMoving = false;

        // Only check for turn end if player was the attacker
        if (attackerCiv == Game.Instance.player.civilization && 
            !UnitManager.Instance.units.Any(u => u.Value.civ == Game.Instance.player.civilization && u.Value.movesLeft > 0))
            TurnManager.Instance.EndTurn();
    }

    private GameObject CreateMovingSprite(Tile tile, Vector2Int pos, Civilization civ, bool isFlag)
    {
        return SpriteUtils.CreateMovingUnitSprite(tile, pos, civ, isFlag);
    }

} 

