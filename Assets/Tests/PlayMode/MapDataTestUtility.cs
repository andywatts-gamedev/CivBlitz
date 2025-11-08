using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class for creating MapData programmatically in tests
/// </summary>
public static class MapDataTestUtility
{
    private static TerrainScob cachedGrassTerrain;
    private static TerrainScob cachedOceanTerrain;
    private static UnitSCOB cachedWarriorUnit;
    private static UnitSCOB cachedSlingerUnit;

    /// <summary>
    /// Loads commonly used resources (call once at test setup)
    /// </summary>
    public static void LoadCommonResources()
    {
        cachedGrassTerrain = UnityEngine.Resources.Load<TerrainScob>("Terrain/Grass");
        cachedOceanTerrain = UnityEngine.Resources.Load<TerrainScob>("Terrain/Ocean");
        cachedWarriorUnit = UnityEngine.Resources.Load<UnitSCOB>("Units/Warrior");
        cachedSlingerUnit = UnityEngine.Resources.Load<UnitSCOB>("Units/Slinger");
    }

    /// <summary>
    /// Creates a simple test map with grass terrain and one unit per civilization
    /// </summary>
    public static MapData CreateSimpleTestMap(string name = "TestMap")
    {
        LoadCommonResources();
        
        var mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.name = name;
        mapData.size = new Vector2Int(5, 5);
        
        // Create a small grass field
        mapData.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = cachedGrassTerrain },
            new TerrainData { position = new Vector2Int(1, 0), terrain = cachedGrassTerrain },
            new TerrainData { position = new Vector2Int(0, 1), terrain = cachedGrassTerrain },
            new TerrainData { position = new Vector2Int(1, 1), terrain = cachedGrassTerrain }
        };
        
        // Place one warrior per civ
        mapData.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = cachedWarriorUnit },
            new UnitPlacement { position = new Vector2Int(1, 1), civilization = Civilization.Rome, unit = cachedWarriorUnit }
        };
        
        return mapData;
    }

    /// <summary>
    /// Creates a map with specified size and unit positions
    /// </summary>
    public static MapData CreateCustomMap(
        string name,
        Vector2Int size,
        Vector2Int[] terrainPositions,
        (Vector2Int pos, Civilization civ, UnitSCOB unit)[] units)
    {
        LoadCommonResources();
        
        var mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.name = name;
        mapData.size = size;
        
        // Create terrain
        var terrainList = new List<TerrainData>();
        foreach (var pos in terrainPositions)
        {
            terrainList.Add(new TerrainData { position = pos, terrain = cachedGrassTerrain });
        }
        mapData.terrainTiles = terrainList.ToArray();
        
        // Create units
        var unitList = new List<UnitPlacement>();
        foreach (var (pos, civ, unit) in units)
        {
            unitList.Add(new UnitPlacement { position = pos, civilization = civ, unit = unit });
        }
        mapData.unitPlacements = unitList.ToArray();
        
        return mapData;
    }

    /// <summary>
    /// Creates a combat test scenario with two units close together
    /// </summary>
    public static MapData CreateCombatTestMap(string name = "CombatTest")
    {
        LoadCommonResources();
        
        var mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.name = name;
        mapData.size = new Vector2Int(3, 3);
        
        // Small terrain
        mapData.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = cachedGrassTerrain },
            new TerrainData { position = new Vector2Int(1, 0), terrain = cachedGrassTerrain }
        };
        
        // Adjacent units for combat testing
        mapData.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = cachedWarriorUnit },
            new UnitPlacement { position = new Vector2Int(1, 0), civilization = Civilization.Rome, unit = cachedWarriorUnit }
        };
        
        return mapData;
    }

    /// <summary>
    /// Creates an empty map with just terrain, no units
    /// </summary>
    public static MapData CreateEmptyMap(string name = "EmptyMap", Vector2Int size = default)
    {
        LoadCommonResources();
        
        if (size == default) size = new Vector2Int(5, 5);
        
        var mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.name = name;
        mapData.size = size;
        
        // Create basic terrain grid
        var terrainList = new List<TerrainData>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                terrainList.Add(new TerrainData 
                { 
                    position = new Vector2Int(x, y), 
                    terrain = cachedGrassTerrain 
                });
            }
        }
        mapData.terrainTiles = terrainList.ToArray();
        mapData.unitPlacements = new UnitPlacement[0];
        
        return mapData;
    }

    /// <summary>
    /// Gets commonly used resources
    /// </summary>
    public static class Resources
    {
        public static TerrainScob GrassTerrain => cachedGrassTerrain ?? (cachedGrassTerrain = UnityEngine.Resources.Load<TerrainScob>("Terrain/Grass"));
        public static TerrainScob OceanTerrain => cachedOceanTerrain ?? (cachedOceanTerrain = UnityEngine.Resources.Load<TerrainScob>("Terrain/Ocean"));
        public static UnitSCOB WarriorUnit => cachedWarriorUnit ?? (cachedWarriorUnit = UnityEngine.Resources.Load<UnitSCOB>("Units/Warrior"));
        public static UnitSCOB SlingerUnit => cachedSlingerUnit ?? (cachedSlingerUnit = UnityEngine.Resources.Load<UnitSCOB>("Units/Slinger"));
    }
}

