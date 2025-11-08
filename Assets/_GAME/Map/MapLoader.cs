using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class MapLoader : MonoBehaviour
{
    public static MapLoader Instance { get; private set; }
    
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap unitTilemap;
    [SerializeField] private List<CivilizationTilemap> civilizationTilemaps;
    [SerializeField] private Tile flagTile;
    [SerializeField] private MapData initialMapData;

    private Dictionary<TerrainScob, TerrainTile> terrainTileCache;
    private Dictionary<UnitSCOB, UnitTile> unitTileCache;
    private Dictionary<Civilization, CivilizationTilemap> civTilemapCache;
    private bool mapLoadedFromData = false;

    void Awake()
    {
        Instance = this;
        BuildTileCaches();
    }

    void Start()
    {
        // If MapData is provided, load from it instead of scene tilemaps
        if (initialMapData != null)
        {
            Debug.Log("[MapLoader] Loading from MapData");
            LoadMap(initialMapData);
            mapLoadedFromData = true;
        }
        else
        {
            Debug.Log("[MapLoader] No MapData provided, using scene tilemaps");
            // Scene-based loading will happen via CivilizationTilemap.Start()
            mapLoadedFromData = false;
        }
    }

    private void BuildTileCaches()
    {
        // Load all terrain tiles
        var terrainTiles = Resources.LoadAll<TerrainTile>("");
        terrainTileCache = new Dictionary<TerrainScob, TerrainTile>();
        foreach (var tile in terrainTiles)
        {
            if (tile.terrainScob != null)
            {
                terrainTileCache[tile.terrainScob] = tile;
            }
        }
        Debug.Log($"[MapLoader] Loaded {terrainTileCache.Count} terrain tiles");

        // Load all unit tiles
        var unitTiles = Resources.LoadAll<UnitTile>("");
        unitTileCache = new Dictionary<UnitSCOB, UnitTile>();
        foreach (var tile in unitTiles)
        {
            if (tile.unitSCOB != null)
            {
                unitTileCache[tile.unitSCOB] = tile;
            }
        }
        Debug.Log($"[MapLoader] Loaded {unitTileCache.Count} unit tiles");

        // Build civilization tilemap lookup
        civTilemapCache = new Dictionary<Civilization, CivilizationTilemap>();
        if (civilizationTilemaps != null)
        {
            foreach (var civTilemap in civilizationTilemaps)
            {
                if (civTilemap != null && civTilemap.civ != null)
                {
                    civTilemapCache[civTilemap.civ.civilization] = civTilemap;
                }
            }
        }
        
        // Load flag tile
        if (flagTile == null)
        {
            var flags = Resources.LoadAll<Tile>("").Where(t => t.name == "UnitFlag").ToArray();
            if (flags.Length > 0)
            {
                flagTile = flags[0];
                Debug.Log("[MapLoader] Loaded flag tile");
            }
        }
    }

    public void ClearMap()
    {
        Debug.Log("[MapLoader] Clearing map...");
        
        // Clear terrain tilemap
        if (terrainTilemap != null)
        {
            terrainTilemap.ClearAllTiles();
        }
        
        // Clear unit tilemap
        if (unitTilemap != null)
        {
            unitTilemap.ClearAllTiles();
        }
        
        // Clear civilization tilemaps (flags and units)
        if (civilizationTilemaps != null)
        {
            foreach (var civTilemap in civilizationTilemaps)
            {
                if (civTilemap != null && civTilemap.flags != null)
                {
                    civTilemap.flags.ClearAllTiles();
                }
                if (civTilemap != null && civTilemap.units != null)
                {
                    civTilemap.units.ClearAllTiles();
                }
            }
        }
        
        // Clear UnitManager state
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.units.Clear();
            UnitManager.Instance.civUnits.Clear();
            UnitManager.Instance.flags.Clear();
        }
        
        Debug.Log("[MapLoader] Map cleared");
    }

    public void LoadMap(MapData mapData)
    {
        if (mapData == null)
        {
            Debug.LogError("[MapLoader] MapData is null!");
            return;
        }

        Debug.Log($"[MapLoader] Loading map: {mapData.name}");
        
        ClearMap();

        // Load terrain
        foreach (var terrainData in mapData.terrainTiles)
        {
            if (terrainData.terrain != null && terrainTileCache.TryGetValue(terrainData.terrain, out var tile))
            {
                terrainTilemap.SetTile((Vector3Int)terrainData.position, tile);
            }
            else
            {
                Debug.LogWarning($"[MapLoader] No tile found for terrain at {terrainData.position}");
            }
        }
        Debug.Log($"[MapLoader] Loaded {mapData.terrainTiles.Length} terrain tiles");

        // Load units
        foreach (var unitPlacement in mapData.unitPlacements)
        {
            if (unitPlacement.unit == null)
            {
                Debug.LogWarning($"[MapLoader] Unit placement at {unitPlacement.position} has null unit");
                continue;
            }

            // Get the civilization tilemap
            if (!civTilemapCache.TryGetValue(unitPlacement.civilization, out var civTilemap))
            {
                Debug.LogWarning($"[MapLoader] No tilemap found for civilization {unitPlacement.civilization}");
                continue;
            }

            // Place flag
            if (flagTile != null && civTilemap.flags != null)
            {
                civTilemap.flags.SetTile((Vector3Int)unitPlacement.position, flagTile);
                // Scale the flag
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale);
                civTilemap.flags.SetTransformMatrix((Vector3Int)unitPlacement.position, matrix);
            }

            // Place unit sprite
            if (unitTileCache.TryGetValue(unitPlacement.unit, out var unitTile) && civTilemap.units != null)
            {
                civTilemap.units.SetTile((Vector3Int)unitPlacement.position, unitTile);
                // Scale the unit
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale);
                civTilemap.units.SetTransformMatrix((Vector3Int)unitPlacement.position, matrix);
            }

            // Register unit with UnitManager
            UnitManager.Instance.RegisterUnit(
                unitPlacement.civilization,
                unitPlacement.unit.unit,
                unitPlacement.position
            );
        }
        Debug.Log($"[MapLoader] Loaded {mapData.unitPlacements.Length} units");

        // Register flag tilemaps with UnitManager
        foreach (var kvp in civTilemapCache)
        {
            if (kvp.Value.flags != null)
            {
                UnitManager.Instance.flags[kvp.Key] = kvp.Value.flags;
            }
        }

        Debug.Log($"[MapLoader] Map loaded successfully");
    }

    public bool IsMapLoadedFromData()
    {
        return mapLoadedFromData;
    }
}

