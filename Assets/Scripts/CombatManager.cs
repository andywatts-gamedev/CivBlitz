using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class CombatManager : Singleton<CombatManager>
{
    public Tilemap tilemap;
    private const float COMBAT_DURATION = 1f;
    public bool isCombatMoving;

    public bool TryCombat(Vector2Int attackerPos, Vector2Int defenderPos) {
        if (!UnitManager.Instance.TryGetUnit(attackerPos, out var attacker) || 
            !UnitManager.Instance.TryGetUnit(defenderPos, out var defender)) return false;
        
        if (attacker.civ != defender.civ) {
            var distance = HexGrid.GetDistance(attackerPos, defenderPos);
            var damage = attacker.unitData.ranged > 0 ? 
                        (distance <= attacker.unitData.range ? attacker.unitData.ranged : 0) :
                        (distance <= 1 ? attacker.unitData.melee : 0);
            
            if (damage > 0) {
                defender.health -= damage;
                attacker.movement = 0;
                
                UnitManager.Instance.UpdateUnit(attackerPos, attacker);
                UnitManager.Instance.UpdateUnit(defenderPos, defender);
                
                // Start combat animation
                StartCoroutine(CombatCoroutine(attackerPos, defenderPos, defender.health <= 0));
                return true;
            }
        }
        return false;
    }

    private IEnumerator CombatCoroutine(Vector2Int attackerPos, Vector2Int defenderPos, bool defenderDies)
    {
        isCombatMoving = true;
        
        // Get tiles and hide them
        var attackerTile = tilemap.GetTile((Vector3Int)attackerPos) as UnitTile;
        var defenderTile = tilemap.GetTile((Vector3Int)defenderPos) as UnitTile;
        tilemap.SetTile((Vector3Int)attackerPos, null);
        tilemap.SetTile((Vector3Int)defenderPos, null);
        
        // Create moving sprites
        var attackerSprite = CreateMovingSprite(attackerTile, attackerPos);
        var defenderSprite = CreateMovingSprite(defenderTile, defenderPos);
        
        // Calculate meeting point (1/3 of the way)
        var attackerStart = tilemap.CellToWorld((Vector3Int)attackerPos);
        var defenderStart = tilemap.CellToWorld((Vector3Int)defenderPos);
        var meetingPoint = Vector3.Lerp(attackerStart, defenderStart, 0.33f);
        
        // Move to meeting point
        var elapsed = 0f;
        while (elapsed < COMBAT_DURATION)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
            attackerSprite.transform.position = Vector3.Lerp(attackerStart, meetingPoint, t);
            defenderSprite.transform.position = Vector3.Lerp(defenderStart, meetingPoint, t);
            yield return null;
        }
        
        // Handle combat result
        if (defenderDies)
        {
            UnitManager.Instance.RemoveUnit(defenderPos);
            Destroy(defenderSprite);
            
            // Winner moves to destination
            elapsed = 0f;
            while (elapsed < COMBAT_DURATION)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
                attackerSprite.transform.position = Vector3.Lerp(meetingPoint, defenderStart, t);
                yield return null;
            }
            
            tilemap.SetTile((Vector3Int)defenderPos, attackerTile);
            UnitManager.Instance.MoveUnit(UnitManager.Instance.units[attackerPos], attackerPos, defenderPos);
        }
        else
        {
            // Both return to start
            elapsed = 0f;
            while (elapsed < COMBAT_DURATION)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
                attackerSprite.transform.position = Vector3.Lerp(meetingPoint, attackerStart, t);
                defenderSprite.transform.position = Vector3.Lerp(meetingPoint, defenderStart, t);
                yield return null;
            }
            
            tilemap.SetTile((Vector3Int)attackerPos, attackerTile);
            tilemap.SetTile((Vector3Int)defenderPos, defenderTile);
        }
        
        Destroy(attackerSprite);
        if (!defenderDies) Destroy(defenderSprite);
        isCombatMoving = false;
    }

    private GameObject CreateMovingSprite(UnitTile tile, Vector2Int pos)
    {
        var go = new GameObject("CombatMove");
        var sprite = go.AddComponent<SpriteRenderer>();
        sprite.sprite = tile.sprite;
        sprite.color = tile.color;
        sprite.sortingOrder = 1;
        sprite.transform.position = tilemap.CellToWorld((Vector3Int)pos);
        return go;
    }
} 