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

    public GameObject[,] TileGameObjects { get; set; }

    public Tile[,] Tiles { get; set; }

    void Start()
    {
        Debug.Log(tilemap.size);
        TileGameObjects = new GameObject[size.x, size.y];
        Tiles = new Tile[size.x, size.y];

        var tileObjects = tilemap.GetComponentsInChildren<Transform>()
            .Where(t => t.GetComponent<Tile>() != null)
            .Select(t => t.gameObject);

        // foreach (var tileObject in tileObjects)
        // {
        //     var tile = tileObject.GetComponent<Tile>();
        //     TileGameObjects[tile.xy.x, tile.xy.y] = tileObject;
        //     Tiles[tile.xy.x, tile.xy.y] = tile;
        // }
    }

    // public bool HasUnit(int2 xy) => TileGameObjects[xy.x, xy.y]?.GetComponentInChildren<UnitMono>() != null;

    // public GameObject GetUnitGO(int2 xy) => 
    //     !IsWithinBounds(xy) ? null : TileGameObjects[xy.x, xy.y]?.GetComponentInChildren<UnitMono>()?.gameObject;

    // public UnitMono GetUnitMono(int2 xy) => 
    //     !IsWithinBounds(xy) ? null : TileGameObjects[xy.x, xy.y]?.GetComponentInChildren<UnitMono>();

    // public void MoveUnit(int2 source, int2 target)
    // {
    //     var unit = GetUnitGO(source);
    //     var targetTile = TileGameObjects[target.x, target.y];
    //     unit.transform.parent = targetTile.transform;
    // }

    public bool IsWithinBounds(int2 xy) => xy.x >= 0 && xy.y >= 0 && xy.x < size.x && xy.y < size.y;
    public bool IsWithinBounds(Vector3 worldPos) => worldPos.x >= 0 && worldPos.y >= 0 && worldPos.x < size.x && worldPos.y < size.y;
}

