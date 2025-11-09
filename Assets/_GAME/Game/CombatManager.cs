using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Linq;

public struct CombatEvent
{
    public string attackerName;
    public Vector2Int attackerPos;
    public Civilization attackerCiv;
    public int attackerStrength;
    public int attackerHealthBefore;
    public int attackerHealthAfter;
    public bool isRanged;
    
    public string defenderName;
    public Vector2Int defenderPos;
    public Civilization defenderCiv;
    public int defenderStrength;
    public int defenderHealthBefore;
    public int defenderHealthAfter;
    
    public int strengthDiff;
    public int attackDamage;
    public int retaliationDamage;
    
    public bool defenderKilled;
    public bool attackerKilled;
}

public class CombatManager : Singleton<CombatManager>
{
    private const float COMBAT_DURATION = 1f;
    public bool isCombatMoving;
    
    public event System.Action<CombatEvent> OnCombatResolved;
    public event System.Action<Civilization> OnGameOver;

    public bool TryCombat(Vector2Int attackerPos, Vector2Int defenderPos) {
        if (!UnitManager.Instance.TryGetUnit(attackerPos, out var attacker) || 
            !UnitManager.Instance.TryGetUnit(defenderPos, out var defender)) return false;
        
        if (attacker.civ != defender.civ) {
            var distance = HexGrid.GetDistance(attackerPos, defenderPos);
            var isRanged = attacker.unit.type == UnitType.Ranged;
            
            // Check if target is in range
            if (isRanged && distance > attacker.unit.range) return false;
            
            // Use ranged or melee strength based on unit type
            var attackerBaseStrength = isRanged ? attacker.unit.ranged : attacker.unit.melee;
            var defenderBaseStrength = defender.unit.melee;
            
            // Terrain modifiers for defender
            var defenderTerrainBonus = 0;
            var defenderTile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)defenderPos) as TerrainTile;
            if (defenderTile != null)
            {
                var terrain = defenderTile.terrainScob.terrain;
                if (terrain.height == TerrainHeight.Hill) defenderTerrainBonus += 3;
                // TODO: Woods/Rainforest bonuses will come from features tilemap
            }
            
            // Combat strength scales with health percentage (Civ 6 style)
            var attackerStrength = Mathf.RoundToInt(attackerBaseStrength * (attacker.health / (float)attacker.unit.health));
            var defenderStrength = Mathf.RoundToInt((defenderBaseStrength + defenderTerrainBonus) * (defender.health / (float)defender.unit.health));
            var strengthDiff = attackerStrength - defenderStrength;
            
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
                
                var defenderKilled = defender.health - attackDamage <= 0;
                var attackerKilled = attacker.health - retaliationDamage <= 0;
                
                var combatEvent = new CombatEvent {
                    attackerName = attacker.unit.name,
                    attackerPos = attackerPos,
                    attackerCiv = attacker.civ,
                    attackerStrength = attackerStrength,
                    attackerHealthBefore = attacker.health,
                    attackerHealthAfter = attacker.health - retaliationDamage,
                    isRanged = isRanged,
                    defenderName = defender.unit.name,
                    defenderPos = defenderPos,
                    defenderCiv = defender.civ,
                    defenderStrength = defenderStrength,
                    defenderHealthBefore = defender.health,
                    defenderHealthAfter = defender.health - attackDamage,
                    strengthDiff = strengthDiff,
                    attackDamage = attackDamage,
                    retaliationDamage = retaliationDamage,
                    defenderKilled = defenderKilled,
                    attackerKilled = attackerKilled
                };
                
                defender.health -= attackDamage;
                attacker.health -= retaliationDamage;
                attacker.actionsLeft = 0;
                
                UnitManager.Instance.UpdateUnit(attackerPos, attacker);
                UnitManager.Instance.UpdateUnit(defenderPos, defender);
                UnitManager.Instance.EmitMovesConsumed();
                
                StartCoroutine(CombatCoroutine(
                    combatEvent,
                    attackerFlagTile, defenderFlagTile,
                    attackerUnitTile, defenderUnitTile
                ));
                return true;
            }
        }
        return false;
    }

    private IEnumerator CombatCoroutine(
        CombatEvent e,
        Tile attackerFlagTile, Tile defenderFlagTile,
        UnitTile attackerUnitTile, UnitTile defenderUnitTile)
    {
        isCombatMoving = true;
        
        var attackerFlagTilemap = UnitManager.Instance.flags[e.attackerCiv];
        var defenderFlagTilemap = UnitManager.Instance.flags[e.defenderCiv];
        var unitTilemap = UnitManager.Instance.unitTilemap;
        
        // Hide tiles
        attackerFlagTilemap.SetTile((Vector3Int)e.attackerPos, null);
        defenderFlagTilemap.SetTile((Vector3Int)e.defenderPos, null);
        unitTilemap.SetTile((Vector3Int)e.attackerPos, null);
        unitTilemap.SetTile((Vector3Int)e.defenderPos, null);
        
        // Create moving sprites
        var attackerFlagSprite = CreateMovingSprite(attackerFlagTile, e.attackerPos, e.attackerCiv, true);
        var attackerUnitSprite = CreateMovingSprite(attackerUnitTile, e.attackerPos, e.attackerCiv, false);
        var defenderFlagSprite = CreateMovingSprite(defenderFlagTile, e.defenderPos, e.defenderCiv, true);
        var defenderUnitSprite = CreateMovingSprite(defenderUnitTile, e.defenderPos, e.defenderCiv, false);

        // Colors
        var attackerColor = Game.Instance.civilizations[e.attackerCiv].color;
        var defenderColor = Game.Instance.civilizations[e.defenderCiv].color;
        
        var start = attackerFlagTilemap.CellToWorld((Vector3Int)e.attackerPos);
        var end = defenderFlagTilemap.CellToWorld((Vector3Int)e.defenderPos);
        
        // For ranged: attacker stays put, defender reacts
        // For melee: both move toward each other
        var attackerMeetingPoint = e.isRanged ? start : Vector3.Lerp(start, end, 0.33f);
        var defenderMeetingPoint = e.isRanged ? end : Vector3.Lerp(end, start, 0.33f);
        
        // Move to meeting point
        var elapsed = 0f;
        while (elapsed < COMBAT_DURATION)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
            var lerpedAttackerPos = Vector3.Lerp(start, attackerMeetingPoint, t);
            var lerpedDefenderPos = Vector3.Lerp(end, defenderMeetingPoint, t);
            
            attackerFlagSprite.transform.position = lerpedAttackerPos;
            attackerUnitSprite.transform.position = lerpedAttackerPos;
            defenderFlagSprite.transform.position = lerpedDefenderPos;
            defenderUnitSprite.transform.position = lerpedDefenderPos;
            yield return null;
        }
        
        FloatingCombatText.Create(
            e.isRanged ? defenderMeetingPoint + Vector3.up * 0.5f : attackerMeetingPoint + Vector3.up * 0.5f,
            e.attackDamage, defenderColor, defenderFlagTilemap.CellToWorld((Vector3Int)e.defenderPos),
            e.retaliationDamage, attackerColor, attackerFlagTilemap.CellToWorld((Vector3Int)e.attackerPos)
        );
        
        if (e.defenderKilled)
        {
            UnitManager.Instance.RemoveUnit(e.defenderPos);
            Destroy(defenderFlagSprite);
            Destroy(defenderUnitSprite);
        }
        
        if (e.attackerKilled)
        {
            UnitManager.Instance.RemoveUnit(e.attackerPos);
            Destroy(attackerFlagSprite);
            Destroy(attackerUnitSprite);
        }
        
        // Check for game over after unit removal
        var winner = UnitManager.Instance.CheckForGameOver();
        if (winner.HasValue)
        {
            OnGameOver?.Invoke(winner.Value);
        }
        
        // Return animation for survivors
        if (!e.defenderKilled || !e.attackerKilled)
        {
            elapsed = 0f;
            while (elapsed < COMBAT_DURATION)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / COMBAT_DURATION);
                
                if (!e.attackerKilled) {
                    attackerFlagSprite.transform.position = Vector3.Lerp(attackerMeetingPoint, start, t);
                    attackerUnitSprite.transform.position = Vector3.Lerp(attackerMeetingPoint, start, t);
                }
                if (!e.defenderKilled) {
                    defenderFlagSprite.transform.position = Vector3.Lerp(defenderMeetingPoint, end, t);
                    defenderUnitSprite.transform.position = Vector3.Lerp(defenderMeetingPoint, end, t);
                }
                yield return null;
            }
        }
        
        // Restore tiles for survivors
        if (!e.attackerKilled) {
            attackerFlagTilemap.SetTile((Vector3Int)e.attackerPos, attackerFlagTile);
            attackerFlagTilemap.SetTransformMatrix((Vector3Int)e.attackerPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            unitTilemap.SetTile((Vector3Int)e.attackerPos, attackerUnitTile);
            unitTilemap.SetTransformMatrix((Vector3Int)e.attackerPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
            Destroy(attackerFlagSprite);
            Destroy(attackerUnitSprite);
        }
        
        if (!e.defenderKilled) {
            defenderFlagTilemap.SetTile((Vector3Int)e.defenderPos, defenderFlagTile);
            defenderFlagTilemap.SetTransformMatrix((Vector3Int)e.defenderPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            unitTilemap.SetTile((Vector3Int)e.defenderPos, defenderUnitTile);
            unitTilemap.SetTransformMatrix((Vector3Int)e.defenderPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
            Destroy(defenderFlagSprite);
            Destroy(defenderUnitSprite);
        }
        isCombatMoving = false;
        
        // Fire combat event for UI
        OnCombatResolved?.Invoke(e);
    }

    private GameObject CreateMovingSprite(Tile tile, Vector2Int pos, Civilization civ, bool isFlag)
    {
        return SpriteUtils.CreateMovingUnitSprite(tile, pos, civ, isFlag);
    }

    // Test helpers
    public void TriggerCombatResolvedForTest(CombatEvent e) => OnCombatResolved?.Invoke(e);
    public void TriggerGameOverForTest(Civilization winner) => OnGameOver?.Invoke(winner);

} 

