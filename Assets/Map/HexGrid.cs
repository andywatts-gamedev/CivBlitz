using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public static class HexGrid
{
    public static readonly Vector2Int[] OddRowOffsets = new[] {
        new Vector2Int(0,1),  // Upper left
        new Vector2Int(1,1),   // Upper right
        new Vector2Int(1,0),    // Right
        new Vector2Int(-1,0),    // Left
        new Vector2Int(0,-1),    // Lower right
        new Vector2Int(1,-1),    // Lower left
    };

    public static readonly Vector2Int[] EvenRowOffsets = new[] {
        new Vector2Int(-1,1),  // Upper left
        new Vector2Int(0,1),   // Upper right
        new Vector2Int(1,0),    // Right
        new Vector2Int(-1,0),    // Left
        new Vector2Int(-1,-1),    // Lower left
        new Vector2Int(0,-1),    // Lower right
    };

    public static Vector2Int[] GetNeighbors(Vector2Int pos)
    {
        return pos.y % 2 == 0 ? EvenRowOffsets : OddRowOffsets;
    }

    public static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        var offsets = a.y % 2 == 0 ? EvenRowOffsets : OddRowOffsets;
        return offsets.Any(o => a + o == b);
    }

    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        var ax = a.x - (a.y - (a.y & 1)) / 2;
        var bx = b.x - (b.y - (b.y & 1)) / 2;
        var az = a.y;
        var bz = b.y;
        var ay = -ax - az;
        var by = -bx - bz;
        
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    public static List<Vector2Int> GetValidMoves(Vector2Int pos, int movement, Tilemap tilemap)
    {
        var offsets = pos.y % 2 == 0 ? EvenRowOffsets : OddRowOffsets;
        var moves = new List<Vector2Int>();
        
        foreach (var offset in offsets)
        {
            var newPos = pos + offset;
            if (tilemap.cellBounds.Contains((Vector3Int)newPos))
                moves.Add(newPos);
        }
        
        return moves;
    }
} 