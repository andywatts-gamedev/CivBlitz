using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class CombatManager : Singleton<CombatManager>
{
    private const float COMBAT_DURATION = 1f;
    public bool isCombatMoving;

    public bool TryCombat(Vector2Int attackerPos, Vector2Int defenderPos) {
        if (!UnitManager.Instance.TryGetUnit(attackerPos, out var attacker) || 
            !UnitManager.Instance.TryGetUnit(defenderPos, out var defender)) return false;
        
        if (attacker.civ != defender.civ) {
            var distance = HexGrid.GetDistance(attackerPos, defenderPos);
            var strengthDiff = attacker.unit.melee - defender.unit.melee;
            
            // Attack damage
            var attackDamage = Mathf.RoundToInt(30 * Mathf.Exp(0.04f * strengthDiff) * Random.Range(0.8f, 1.2f));
            // Retaliation damage (just flip the strength diff)
            var retaliationDamage = distance <= 1 ? 
                Mathf.RoundToInt(30 * Mathf.Exp(0.04f * -strengthDiff) * Random.Range(0.8f, 1.2f)) : 
                0;
            
            if (attackDamage > 0)
            {
                var attackerTilemap = UnitManager.Instance.flags[attacker.civ];
                var defenderTilemap = UnitManager.Instance.flags[defender.civ];
                var attackerTile = attackerTilemap.GetTile((Vector3Int)attackerPos) as UnitTile;
                var defenderTile = defenderTilemap.GetTile((Vector3Int)defenderPos) as UnitTile;
                
                Debug.Log($"Pre-combat tiles - Attacker: {attackerTile}, Defender: {defenderTile}");
                if (attackerTile == null || defenderTile == null) {
                    Debug.LogError($"Missing tiles at positions - Attacker: {attackerPos}, Defender: {defenderPos}");
                    return false;
                }
                
                defender.health -= attackDamage;
                attacker.health -= retaliationDamage;
                attacker.movesLeft = 0;
                
                UnitManager.Instance.UpdateUnit(attackerPos, attacker);
                UnitManager.Instance.UpdateUnit(defenderPos, defender);
                
                // Pass tiles to coroutine
                StartCoroutine(CombatCoroutine(
                    attackerPos, defenderPos, 
                    defender.health <= 0, 
                    attackDamage, retaliationDamage,
                    attackerTile, defenderTile,
                    attacker.civ, defender.civ
                ));
                return true;
            }
        }
        return false;
    }

    private IEnumerator CombatCoroutine(
        Vector2Int attackerPos, Vector2Int defenderPos, 
        bool defenderDies, int attackDamage, int retaliationDamage,
        UnitTile attackerTile, UnitTile defenderTile,
        Civilization attackerCiv, Civilization defenderCiv)
    {
        if (attackerTile == null || defenderTile == null) {
            Debug.LogError("Combat started with null tiles");
            yield break;
        }

        isCombatMoving = true;
        
        var attackerTilemap = UnitManager.Instance.flags[attackerCiv];
        var defenderTilemap = UnitManager.Instance.flags[defenderCiv];
        Debug.Log($"Combat tilemaps - Attacker: {attackerTilemap.name}, Defender: {defenderTilemap.name}");
        
        // Hide tiles
        attackerTilemap.SetTile((Vector3Int)attackerPos, null);
        defenderTilemap.SetTile((Vector3Int)defenderPos, null);
        
        // Create moving sprites
        var attackerSprite = CreateMovingSprite(attackerTile, attackerPos, attackerCiv);
        if (attackerSprite == null) {
            Debug.LogError("Failed to create attacker sprite");
            yield break;
        }
        
        var defenderSprite = CreateMovingSprite(defenderTile, defenderPos, defenderCiv);
        if (defenderSprite == null) {
            Destroy(attackerSprite);
            Debug.LogError("Failed to create defender sprite");
            yield break;
        }
        
        // Calculate meeting point (1/3 of the way)
        var attackerStart = attackerTilemap.CellToWorld((Vector3Int)attackerPos);
        var defenderStart = defenderTilemap.CellToWorld((Vector3Int)defenderPos);
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
        
        // Show damage text at meeting point
        FloatingCombatText.Create(meetingPoint + Vector3.up * 0.5f, 
                                attackDamage, 
                                defenderTile.color, 
                                retaliationDamage, 
                                attackerTile.color);
        
        // Update final positions
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
            
            attackerTilemap.SetTile((Vector3Int)defenderPos, attackerTile);
            UnitManager.Instance.MoveUnit(attackerPos, defenderPos);
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
            
            attackerTilemap.SetTile((Vector3Int)attackerPos, attackerTile);
            defenderTilemap.SetTile((Vector3Int)defenderPos, defenderTile);
        }
        // 
        Destroy(attackerSprite);
        if (!defenderDies) Destroy(defenderSprite);
        isCombatMoving = false;
    }

    private GameObject CreateMovingSprite(UnitTile tile, Vector2Int pos, Civilization civ)
    {
        if (tile == null) {
            Debug.LogError($"Attempted to create sprite with null tile at {pos}");
            return null;
        }

        var tilemap = UnitManager.Instance.flags[civ];
        
        return SpriteUtils.CreateMovingSprite(
            "CombatMove",
            tile.sprite,
            tile.color,
            20,
            tilemap.CellToWorld((Vector3Int)pos),
            Game.Instance.flagScale
        );
    }

} 