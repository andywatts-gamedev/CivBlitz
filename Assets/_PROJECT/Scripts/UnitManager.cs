using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : Singleton<UnitManager>
{
    public Tilemap tilemap;
    private const float MOVE_DURATION = 1f;
    
    public bool isMoving;

    [Sirenix.OdinInspector.ShowInInspector]
    public Dictionary<Vector2Int, UnitInstance> units = new();

    void Start()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos) as UnitTile;
            if (tile != null)
            {
                var unit = new UnitInstance
                {
                    unitData = tile.unitData,
                    health = tile.unitData.health,
                    movement = tile.unitData.movement,
                    civ = tile.civ
                };
                units[(Vector2Int)pos] = unit;
            }
        }
    }

    public bool TryGetUnit(Vector2Int pos, out UnitInstance unit)
    {
        var hasUnit = units.TryGetValue(pos, out unit);
        return hasUnit;
    }

    public void MoveUnit(UnitInstance unit, Vector2Int from, Vector2Int to)
    {
        if (isMoving) return;
        
        var tile = tilemap.GetTile((Vector3Int)from) as UnitTile;
        tilemap.SetTile((Vector3Int)from, null);
        
        units.Remove(from);
        units[to] = unit;
        
        var movingUnit = new GameObject("UnitMove");
        var sprite = movingUnit.AddComponent<SpriteRenderer>();
        sprite.sprite = tile.sprite;
        sprite.color = tile.color;
        sprite.sortingOrder = 1;
        sprite.transform.position = tilemap.CellToWorld((Vector3Int)from);
        
        StartCoroutine(MoveCoroutine(movingUnit, (Vector3Int)from, (Vector3Int)to, tile));
    }

    private IEnumerator MoveCoroutine(GameObject movingUnit, Vector3Int from, Vector3Int to, UnitTile tile)
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

    public void ResetMoves() {
        var positions = new List<Vector2Int>(units.Keys);
        foreach (var pos in positions)
        {
            var unit = units[pos];
            unit.movement = unit.unitData.movement;
            units[pos] = unit;
        }
    }

    public bool HasUnitAt(Vector2Int pos)
    {
        var hasUnit = units.ContainsKey(pos);
        return hasUnit;
    }

    public void UpdateUnit(Vector2Int pos, UnitInstance unit)
    {
        units[pos] = unit;
    }

    public void RemoveUnit(Vector2Int pos)
    {
        if (units.ContainsKey(pos)) {
            units.Remove(pos);
            tilemap.SetTile((Vector3Int)pos, null);
        }
    }

} 