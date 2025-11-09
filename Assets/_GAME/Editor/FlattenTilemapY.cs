using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FlattenTilemapY : EditorWindow
{
    [MenuItem("Tools/Flatten Tilemap Y Positions")]
    static void Flatten()
    {
        var tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (var tilemap in tilemaps)
        {
            var bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (int z = bounds.zMin; z < bounds.zMax; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        if (tilemap.HasTile(pos))
                        {
                            var tile = tilemap.GetTile(pos);
                            tilemap.SetTile(pos, null);
                            // Convert XY→XZ: old (x,y,z) becomes new (x, oldZ, oldY)
                            tilemap.SetTile(new Vector3Int(x, z, y), tile);
                        }
                    }
                }
            }
            tilemap.RefreshAllTiles();
        }
        Debug.Log($"Flattened {tilemaps.Length} tilemaps to Y=0");
    }
}

