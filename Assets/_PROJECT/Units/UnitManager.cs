using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }
    public Tilemap playerFlagsTilemap;
    public Tilemap enemyFlagsTilemap;
    public Tilemap playerUnitTilemap;
    public Tilemap enemyUnitTilemap;
    public Civilization playerCiv;
    public Civilization enemyCiv;
    private const float MOVE_DURATION = 1f;
    
    public bool isMoving;

    [Sirenix.OdinInspector.ShowInInspector]
    public Dictionary<Vector2Int, UnitInstance> units = new();

    [Sirenix.OdinInspector.ShowInInspector]
    public Dictionary<Civilization, List<UnitInstance>> civUnits = new();

    void Awake() => Instance = this;

    public void RegisterUnit(Civilization civ, UnitData unitData, Vector2Int pos)
    {
        var unit = new UnitInstance(unitData, civ, pos);
        units[pos] = unit;
        if (!civUnits.ContainsKey(civ)) civUnits[civ] = new List<UnitInstance>();
        civUnits[civ].Add(unit);
        Debug.Log("Registered " + civ.name + " " + unit.unitData.name + " at " + pos);
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
        if (isMoving) return;
        if (units.TryGetValue(from, out var unit))
        {
            units.Remove(from);
            units[to] = unit;
            
            // Flag
            var flagsTilemap = unit.civ == playerCiv ? playerFlagsTilemap : enemyFlagsTilemap;
            var flagTile = flagsTilemap.GetTile((Vector3Int)from) as Tile;
            flagsTilemap.SetTile((Vector3Int)from, null);
            
            var movingFlag = new GameObject("FlagMove");
            var sprite = movingFlag.AddComponent<SpriteRenderer>();
            sprite.sprite = flagTile.sprite;
            sprite.color = flagsTilemap.color;
            sprite.sortingOrder = 10;
            sprite.transform.position = flagsTilemap.CellToWorld((Vector3Int)from);
            StartCoroutine(MoveCoroutine(flagsTilemap, movingFlag, (Vector3Int)from, (Vector3Int)to, flagTile));

            // Unit
            var unitTilemap = unit.civ == playerCiv ? playerUnitTilemap : enemyUnitTilemap;
            var unitTile = unitTilemap.GetTile((Vector3Int)from) as Tile;
            unitTilemap.SetTile((Vector3Int)from, null);
            
            var movingUnit = new GameObject("UnitMove");
            var unitSprite = movingUnit.AddComponent<SpriteRenderer>();
            unitSprite.sprite = unitTile.sprite;
            unitSprite.sortingOrder = 20;
            unitSprite.transform.position = unitTilemap.CellToWorld((Vector3Int)from);
            StartCoroutine(MoveCoroutine(unitTilemap, movingUnit, (Vector3Int)from, (Vector3Int)to, unitTile));
        }
    }

    private IEnumerator MoveCoroutine(Tilemap tilemap, GameObject movingUnit, Vector3Int from, Vector3Int to, Tile tile)
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
            unit.movesLeft = unit.unitData.movement;
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
