using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class MapLoader : MonoBehaviour
{
    public static MapLoader Instance { get; private set; }
    
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap unitTilemap;
    [SerializeField] private Tilemap overridesTilemap;
    [SerializeField] private List<CivilizationTilemap> civilizationTilemaps;
    [SerializeField] private MapData initialMapData;
    [SerializeField] private GameEvent onMapLoaded;
    [SerializeField] private Tile fallbackStateTile; // Temporary fallback while migrating to UnitStateTiles

    private Dictionary<TerrainScob, TerrainTile> terrainTileCache;
    private Dictionary<UnitSCOB, UnitTile> unitTileCache;
    private Dictionary<UnitState, UnitStateTile> unitStateTileCache;
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
            StartCoroutine(CenterAfterSceneLoad());
        }
    }

    private System.Collections.IEnumerator CenterAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        CenterCameraOnMap();
        onMapLoaded?.Invoke();
    }

    private void BuildTileCaches()
    {
        // Load all terrain tiles from Resources
        var terrainTiles = Resources.LoadAll<TerrainTile>("Tiles");
        terrainTileCache = new Dictionary<TerrainScob, TerrainTile>();
        foreach (var tile in terrainTiles)
        {
            if (tile.terrainScob != null)
            {
                terrainTileCache[tile.terrainScob] = tile;
            }
        }
        Debug.Log($"[MapLoader] Loaded {terrainTileCache.Count} terrain tiles");

        // Load all unit tiles from Resources
        var unitTiles = Resources.LoadAll<UnitTile>("Units");
        unitTileCache = new Dictionary<UnitSCOB, UnitTile>();
        foreach (var tile in unitTiles)
        {
            if (tile.unitSCOB != null)
            {
                unitTileCache[tile.unitSCOB] = tile;
            }
        }
        Debug.Log($"[MapLoader] Loaded {unitTileCache.Count} unit tiles");

        // Load unit state tiles from Resources (for Ready/Fortified state indicators)
        var unitStateTiles = Resources.LoadAll<UnitStateTile>("States");
        unitStateTileCache = new Dictionary<UnitState, UnitStateTile>();
        foreach (var tile in unitStateTiles)
        {
            Debug.Log($"[MapLoader] Found UnitStateTile: {tile.name}, state={tile.unitState}");
            unitStateTileCache[tile.unitState] = tile;
        }
        Debug.Log($"[MapLoader] Loaded {unitStateTileCache.Count} unit state tiles");
        
        if (unitStateTileCache.Count == 0)
        {
            Debug.LogWarning("[MapLoader] No UnitStateTile assets found in Resources/States. Using fallback tile. Create UnitStateTile assets for Ready and Fortified states.");
        }

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
        
        // Clear civilization tilemaps (state indicators and units)
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

    public void CenterCameraOnMap()
    {
        if (terrainTilemap == null) return;

        terrainTilemap.CompressBounds();
        var bounds = terrainTilemap.cellBounds;
        
        var sum = Vector3.zero;
        var count = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (terrainTilemap.GetTile(pos) != null)
            {
                sum += terrainTilemap.GetCellCenterWorld(pos);
                count++;
            }
        }
        
        if (count == 0) return;
        var centerWorld = sum / count;
        
        var vcam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null)
        {
            vcam.transform.position = new Vector3(centerWorld.x, vcam.transform.position.y, centerWorld.z);
        }
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
                // Grid uses XZY swizzle: position.x=worldX, position.y=worldZ, z=0
                var cellPos = new Vector3Int(terrainData.position.x, terrainData.position.y, 0);
                terrainTilemap.SetTile(cellPos, tile);
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

            // Grid uses XZY swizzle: position.x=worldX, position.y=worldZ, z=0
            var cellPos = new Vector3Int(unitPlacement.position.x, unitPlacement.position.y, 0);
            
            // Place unit state indicator (Ready by default)
            if (civTilemap.flags != null)
            {
                Tile stateTileToPlace = null;
                if (unitStateTileCache.TryGetValue(UnitState.Ready, out var stateTile))
                {
                    stateTileToPlace = stateTile;
                }
                else if (fallbackStateTile != null)
                {
                    stateTileToPlace = fallbackStateTile;
                    Debug.LogWarning($"[MapLoader] Using fallback state tile for unit at {unitPlacement.position}");
                }
                
                if (stateTileToPlace != null)
                {
                    civTilemap.flags.SetTile(cellPos, stateTileToPlace);
                    var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.flagScale);
                    civTilemap.flags.SetTransformMatrix(cellPos, matrix);
                }
            }

            // Place unit sprite
            if (unitTileCache.TryGetValue(unitPlacement.unit, out var unitTile) && civTilemap.units != null)
            {
                civTilemap.units.SetTile(cellPos, unitTile);
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Game.Instance.unitScale);
                civTilemap.units.SetTransformMatrix(cellPos, matrix);
            }

            // Register unit with UnitManager
            UnitManager.Instance.RegisterUnit(
                unitPlacement.civilization,
                unitPlacement.unit.unit,
                unitPlacement.position
            );
            
            // Apply health override if set (percentage)
            if (unitPlacement.healthOverride > 0)
            {
                var registeredUnit = UnitManager.Instance.GetUnitAt(unitPlacement.position);
                var maxHealth = unitPlacement.unit.unit.health;
                registeredUnit.health = Mathf.RoundToInt(maxHealth * unitPlacement.healthOverride / 100f);
                UnitManager.Instance.UpdateUnit(unitPlacement.position, registeredUnit);
                Debug.Log($"[MapLoader] Applied {unitPlacement.healthOverride}% health to unit at {unitPlacement.position}: {registeredUnit.health} HP");
            }
        }
        Debug.Log($"[MapLoader] Loaded {mapData.unitPlacements.Length} units");

        // Register state indicator tilemaps with UnitManager
        foreach (var kvp in civTilemapCache)
        {
            if (kvp.Value.flags != null)
            {
                UnitManager.Instance.flags[kvp.Key] = kvp.Value.flags;
            }
        }

        Debug.Log($"[MapLoader] Map loaded successfully");
        CenterCameraOnMap();
        onMapLoaded?.Invoke();
    }

    public bool IsMapLoadedFromData()
    {
        return mapLoadedFromData;
    }

    public UnitStateTile GetStateTileForState(UnitState state)
    {
        if (unitStateTileCache.TryGetValue(state, out var tile))
        {
            return tile;
        }
        
        // Fallback: return the fallback tile as UnitStateTile (will be cast to Tile)
        Debug.LogWarning($"[MapLoader] No UnitStateTile found for state {state}, using fallback");
        return fallbackStateTile as UnitStateTile;
    }
}

