using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }
    private const float MOVE_DURATION = 1f;
    
    public bool isMoving;

    [Sirenix.OdinInspector.ShowInInspector] public Dictionary<Vector2Int, UnitInstance> units = new();
    [Sirenix.OdinInspector.ShowInInspector] public Dictionary<Civilization, List<UnitInstance>> civUnits = new();

    public Dictionary<Civilization, Tilemap> flags = new();
    public Tilemap unitTilemap;

    void Awake() => Instance = this;

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
        if (isMoving) return;
        if (units.TryGetValue(from, out var unit))
        {
            units.Remove(from);
            units[to] = unit;
            
            // Flag
            var flagsTilemap = flags[unit.civ];
            var flagTile = flagsTilemap.GetTile((Vector3Int)from) as Tile;
            flagsTilemap.SetTile((Vector3Int)from, null);
            
            var movingFlag = SpriteUtils.CreateMovingSprite(
                "FlagMove",
                flagTile.sprite,
                flagsTilemap.color,
                10,
                flagsTilemap.CellToWorld((Vector3Int)from),
                Game.Instance.flagScale
            );

            // Unit
            var unitTile = unitTilemap.GetTile((Vector3Int)from) as Tile;
            unitTilemap.SetTile((Vector3Int)from, null);
            
            var movingUnit = SpriteUtils.CreateMovingSprite(
                "UnitMove",
                unitTile.sprite,
                unitTile.color,
                20,
                unitTilemap.CellToWorld((Vector3Int)from),
                Game.Instance.unitScale
            );

            StartCoroutine(MoveCoroutine(flagsTilemap, movingFlag, (Vector3Int)from, (Vector3Int)to, flagTile, Game.Instance.flagScale));
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
    }

    public bool HasUnitAt(Vector2Int position) => units.ContainsKey(position);
    public UnitInstance GetUnitAt(Vector2Int position) => units.TryGetValue(position, out var unit) ? unit : null;

    public void ResetMoves() {
        var positions = new List<Vector2Int>(units.Keys);
        foreach (var pos in positions)
        {
            var unit = units[pos];
            unit.movesLeft = unit.unit.movement;
            units[pos] = unit;
        }
    }

    public void UpdateUnit(Vector2Int pos, UnitInstance unit)
    {
        units[pos] = unit;
    }

    public void RemoveUnit(Vector2Int position)
    {
        if (units.TryGetValue(position, out var unit))
        {
            civUnits[unit.civ].Remove(unit);
            units.Remove(position);
        }
    }
} 
