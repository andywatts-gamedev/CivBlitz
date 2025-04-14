using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class UnitManager : Singleton<UnitManager>
{
    public Tilemap tilemap;

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
        // Update dictionary
        units.Remove(from);
        units[to] = unit;
        
        // Update tilemap
        var tile = tilemap.GetTile((Vector3Int)from);
        tilemap.SetTile((Vector3Int)from, null);
        tilemap.SetTile((Vector3Int)to, tile);
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