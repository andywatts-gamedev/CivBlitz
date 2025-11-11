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
    
    private System.Collections.Generic.Dictionary<Vector2Int, Vector3> combatPositions = new();

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
            var defenderTile = UnitManager.Instance.terrainTilemap.GetTile(new Vector3Int(defenderPos.x, defenderPos.y, 0)) as TerrainTile;
            if (defenderTile != null)
            {
                defenderTerrainBonus = defenderTile.terrainScob.terrain.defenseBonus;
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

                var attackerCellPos = new Vector3Int(attackerPos.x, attackerPos.y, 0);
                var defenderCellPos = new Vector3Int(defenderPos.x, defenderPos.y, 0);
                
                var attackerFlagTile = attackerFlagTilemap.GetTile(attackerCellPos) as Tile;
                var defenderFlagTile = defenderFlagTilemap.GetTile(defenderCellPos) as Tile;
                var attackerUnitTile = unitTilemap.GetTile(attackerCellPos) as UnitTile;
                var defenderUnitTile = unitTilemap.GetTile(defenderCellPos) as UnitTile;
                
                if (attackerFlagTile == null || defenderFlagTile == null || 
                    attackerUnitTile == null || defenderUnitTile == null) {
                    Debug.LogError($"Missing tiles - Attacker: flag={attackerFlagTile}, unit={attackerUnitTile} at {attackerPos}, Defender: flag={defenderFlagTile}, unit={defenderUnitTile} at {defenderPos}");
                    
                    // Wait for move animation to complete
                    if (UnitManager.Instance.isMoving)
                    {
                        Debug.Log("Unit moving - deferring combat");
                        return false;
                    }
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

    public bool TryGetCombatPosition(Vector2Int pos, out Vector3 worldPos)
    {
        return combatPositions.TryGetValue(pos, out worldPos);
    }

    private IEnumerator CombatCoroutine(
        CombatEvent e,
        Tile attackerFlagTile, Tile defenderFlagTile,
        UnitTile attackerUnitTile, UnitTile defenderUnitTile)
    {
        isCombatMoving = true;
        combatPositions.Clear();
        
        var attackerFlagTilemap = UnitManager.Instance.flags[e.attackerCiv];
        var defenderFlagTilemap = UnitManager.Instance.flags[e.defenderCiv];
        var unitTilemap = UnitManager.Instance.unitTilemap;
        
        var attackerCellPos = new Vector3Int(e.attackerPos.x, e.attackerPos.y, 0);
        var defenderCellPos = new Vector3Int(e.defenderPos.x, e.defenderPos.y, 0);
        
        // Hide tiles
        attackerFlagTilemap.SetTile(attackerCellPos, null);
        defenderFlagTilemap.SetTile(defenderCellPos, null);
        unitTilemap.SetTile(attackerCellPos, null);
        unitTilemap.SetTile(defenderCellPos, null);
        
        // Create moving sprites
        var attackerFlagSprite = CreateMovingSprite(attackerFlagTile, e.attackerPos, e.attackerCiv, true);
        var attackerUnitSprite = CreateMovingSprite(attackerUnitTile, e.attackerPos, e.attackerCiv, false);
        var defenderFlagSprite = CreateMovingSprite(defenderFlagTile, e.defenderPos, e.defenderCiv, true);
        var defenderUnitSprite = CreateMovingSprite(defenderUnitTile, e.defenderPos, e.defenderCiv, false);

        // Colors
        var attackerColor = Game.Instance.civilizations[e.attackerCiv].color;
        var defenderColor = Game.Instance.civilizations[e.defenderCiv].color;
        
        var start = attackerFlagTilemap.CellToWorld(attackerCellPos);
        var end = defenderFlagTilemap.CellToWorld(defenderCellPos);
        
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
            
            combatPositions[e.attackerPos] = lerpedAttackerPos;
            combatPositions[e.defenderPos] = lerpedDefenderPos;
            
            yield return null;
        }
        
        FloatingCombatText.Create(
            e.isRanged ? defenderMeetingPoint + Vector3.up * 0.5f : attackerMeetingPoint + Vector3.up * 0.5f,
            e.attackDamage, defenderColor, defenderFlagTilemap.CellToWorld(defenderCellPos),
            e.retaliationDamage, attackerColor, attackerFlagTilemap.CellToWorld(attackerCellPos)
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
                    var pos = Vector3.Lerp(attackerMeetingPoint, start, t);
                    attackerFlagSprite.transform.position = pos;
                    attackerUnitSprite.transform.position = pos;
                    combatPositions[e.attackerPos] = pos;
                }
                if (!e.defenderKilled) {
                    var pos = Vector3.Lerp(defenderMeetingPoint, end, t);
                    defenderFlagSprite.transform.position = pos;
                    defenderUnitSprite.transform.position = pos;
                    combatPositions[e.defenderPos] = pos;
                }
                yield return null;
            }
        }
        
        // Restore tiles for survivors
        if (!e.attackerKilled) {
            attackerFlagTilemap.SetTile(attackerCellPos, attackerFlagTile);
            attackerFlagTilemap.SetTransformMatrix(attackerCellPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            unitTilemap.SetTile(attackerCellPos, attackerUnitTile);
            unitTilemap.SetTransformMatrix(attackerCellPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
            Destroy(attackerFlagSprite);
            Destroy(attackerUnitSprite);
        }
        
        if (!e.defenderKilled) {
            defenderFlagTilemap.SetTile(defenderCellPos, defenderFlagTile);
            defenderFlagTilemap.SetTransformMatrix(defenderCellPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale));
            unitTilemap.SetTile(defenderCellPos, defenderUnitTile);
            unitTilemap.SetTransformMatrix(defenderCellPos, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale));
            Destroy(defenderFlagSprite);
            Destroy(defenderUnitSprite);
        }
        
        // Update health after animation
        if (!e.attackerKilled && UnitManager.Instance.TryGetUnit(e.attackerPos, out var attacker))
        {
            attacker.health -= e.retaliationDamage;
            attacker.actionsLeft = 0;
            UnitManager.Instance.UpdateUnit(e.attackerPos, attacker);
        }
        
        if (!e.defenderKilled && UnitManager.Instance.TryGetUnit(e.defenderPos, out var defender))
        {
            defender.health -= e.attackDamage;
            UnitManager.Instance.UpdateUnit(e.defenderPos, defender);
        }
        
        UnitManager.Instance.EmitMovesConsumed();
        
        combatPositions.Clear();
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

