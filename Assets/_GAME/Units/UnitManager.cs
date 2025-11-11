using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnitManager : Singleton<UnitManager>
{
    private const float MOVE_DURATION = 1f;
    
    public bool isMoving;
    
    [SerializeField] private GameEvent onUnitMoved;
    [SerializeField] private GameEvent onMovesConsumed;
    [SerializeField] private GameEvent onUnitStateChanged;
    
    [Sirenix.OdinInspector.ShowInInspector] public Dictionary<Vector2Int, UnitInstance> units = new();
    [Sirenix.OdinInspector.ShowInInspector] public Dictionary<Civilization, List<UnitInstance>> civUnits = new();

    public Dictionary<Civilization, Tilemap> flags = new();
    public Tilemap terrainTilemap;
    public Tilemap unitTilemap;

    public void RegisterUnit(Civilization civ, Unit unit, Vector2Int pos)
    {
        var unitInstance = new UnitInstance(unit, civ, pos);
        units[pos] = unitInstance;
        if (!civUnits.ContainsKey(civ)) civUnits[civ] = new List<UnitInstance>();
        civUnits[civ].Add(unitInstance);
        Debug.Log("Registered " + civ + " " + unit + " at " + pos);
    }

    public bool TryGetUnit(Vector2Int pos, out UnitInstance unit)
    {
        var hasUnit = units.TryGetValue(pos, out unit);
        return hasUnit;
    }

    public void AddUnit(UnitInstance unit)
    {
        units[unit.position] = unit;
        if (!civUnits.ContainsKey(unit.civ))
            civUnits[unit.civ] = new List<UnitInstance>();
        civUnits[unit.civ].Add(unit);
    }

    public void MoveUnit(Vector2Int from, Vector2Int to)
    {
        Debug.Log("UnitManager#MoveUnit: Moving unit from " + from + " to " + to);
        if (isMoving || CombatManager.Instance.isCombatMoving) return;
        if (units.TryGetValue(from, out var unit))
        {
            units.Remove(from);
            unit.position = to;
            unit.actionsLeft--;
            unit.state = UnitState.Ready; // Un-fortify on move
            units[to] = unit;
            
            // State indicator (Ready after move)
            var stateTilemap = flags[unit.civ];
            var oldStateTile = stateTilemap.GetTile((Vector3Int)from) as Tile;
            stateTilemap.SetTile((Vector3Int)from, null);
            var movingStateIndicator = SpriteUtils.CreateMovingUnitSprite(oldStateTile, from, unit.civ, true);
            
            // Get Ready state tile for destination
            var readyStateTile = MapLoader.Instance.GetStateTileForState(UnitState.Ready);

            // Unit
            var unitTile = unitTilemap.GetTile((Vector3Int)from) as Tile;
            unitTilemap.SetTile((Vector3Int)from, null);
            var movingUnit = SpriteUtils.CreateMovingUnitSprite(unitTile, from, unit.civ, false);

            StartCoroutine(MoveCoroutine(stateTilemap, movingStateIndicator, (Vector3Int)from, (Vector3Int)to, readyStateTile, Game.Instance.flagScale));
            StartCoroutine(MoveCoroutine(unitTilemap, movingUnit, (Vector3Int)from, (Vector3Int)to, unitTile, Game.Instance.unitScale));
        }
    }

    private IEnumerator MoveCoroutine(Tilemap tilemap, GameObject movingUnit, Vector3Int from, Vector3Int to, Tile tile, Vector3 scale)
    {
        isMoving = true;
        var start = tilemap.CellToWorld(from);
        var end = tilemap.CellToWorld(to);
        var elapsed = 0f;
        
        while (elapsed < MOVE_DURATION)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / MOVE_DURATION);
            movingUnit.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        
        tilemap.SetTile(to, tile);
        var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        tilemap.SetTransformMatrix((Vector3Int)to, matrix);

        Destroy(movingUnit);
        isMoving = false;
        onUnitMoved?.Invoke();
    }

    public bool HasUnitAt(Vector2Int position) => units.ContainsKey(position);
    public UnitInstance GetUnitAt(Vector2Int position) => units.TryGetValue(position, out var unit) ? unit : null;

    public void ResetMoves() {
        var positions = new List<Vector2Int>(units.Keys);
        foreach (var pos in positions)
        {
            var unit = units[pos];
            if (unit.state != UnitState.Fortified)
            {
                unit.actionsLeft = unit.unit.movement;
                units[pos] = unit;
            }
        }
    }

    public void UpdateUnit(Vector2Int pos, UnitInstance unit)
    {
        units[pos] = unit;
        onUnitStateChanged?.Invoke();
    }

    public void RemoveUnit(Vector2Int position)
    {
        if (units.TryGetValue(position, out var unit))
        {
            civUnits[unit.civ].Remove(unit);
            units.Remove(position);
            onUnitStateChanged?.Invoke();
        }
    }

    public UnitInstance GetNextReadyUnit()
    {
        foreach (var kvp in units)
        {
            var unit = kvp.Value;
            if (unit.civ == Game.Instance.player.civilization && 
                unit.actionsLeft > 0 && 
                unit.state == UnitState.Ready)
            {
                return unit;
            }
        }
        return null;
    }

    public void ResetUnitStates()
    {
        // No-op: Fortified state persists across turns
        onUnitStateChanged?.Invoke();
    }

    public void SetUnitState(Vector2Int position, UnitState newState)
    {
        if (units.TryGetValue(position, out var unit))
        {
            unit.state = newState;
            units[position] = unit;
            
            // Update visual state indicator on tilemap
            var stateTile = MapLoader.Instance.GetStateTileForState(newState);
            if (stateTile != null && flags.TryGetValue(unit.civ, out var stateTilemap))
            {
                var cellPos = new Vector3Int(position.x, position.y, 0);
                stateTilemap.SetTile(cellPos, stateTile);
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale);
                stateTilemap.SetTransformMatrix(cellPos, matrix);
            }
            
            onUnitStateChanged?.Invoke();
        }
    }
    
    public void EmitMovesConsumed()
    {
        onMovesConsumed?.Invoke();
    }

    public bool HasUnitsForCivilization(Civilization civ)
    {
        return civUnits.ContainsKey(civ) && civUnits[civ].Count > 0;
    }

    public Civilization? CheckForGameOver()
    {
        var playerHasUnits = HasUnitsForCivilization(Game.Instance.player.civilization);
        var aiHasUnits = HasUnitsForCivilization(Game.Instance.ai.civilization);
        
        if (!playerHasUnits) return Game.Instance.ai.civilization;
        if (!aiHasUnits) return Game.Instance.player.civilization;
        return null;
    }
} 
