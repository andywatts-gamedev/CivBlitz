using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Map : MonoBehaviour
{
    public static Map Instance { get; private set; }
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;

    public int2 size;

    void Awake() => Instance = this;

    public bool HasTerrainAt(Vector2Int position) => tilemap != null && tilemap.GetTile((Vector3Int)position) != null;
    public TerrainTile GetTerrainAt(Vector2Int position) => tilemap?.GetTile((Vector3Int)position) as TerrainTile;
    
    // public bool IsWithinBounds(int2 xy) => xy.x >= 0 && xy.y >= 0 && xy.x < size.x && xy.y < size.y;
    // public bool IsWithinBounds(Vector3 worldPos) => worldPos.x >= 0 && worldPos.y >= 0 && worldPos.x < size.x && worldPos.y < size.y;
}

