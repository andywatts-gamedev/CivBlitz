# Map Loading System Documentation

## Overview

The map loading system allows you to:
- **Author maps visually** in Unity Editor using tilemaps
- **Export maps** to MapData assets for reusability
- **Load maps programmatically** for level progression
- **Create test scenarios** in code for automated testing

## Architecture

### Key Components

1. **MapData** (`MapData.cs`) - ScriptableObject that stores map configuration
   - Terrain tile positions and types
   - Unit placements with civilizations
   - Map size and validation

2. **MapLoader** (`MapLoader.cs`) - Loads maps into the game
   - Clears existing map state
   - Populates tilemaps from MapData
   - Registers units with UnitManager
   - Supports fallback to scene-based loading

3. **LevelManager** (`LevelManager.cs`) - Manages level progression
   - Stores array of levels (MapData assets)
   - Loads next level on win
   - Restarts current level on loss
   - Handles level completion

4. **MapExporter** (`Editor/MapExporter.cs`) - Editor tool
   - Exports current scene tilemaps to MapData asset
   - Accessible via `Tools > Map Exporter` menu

## Usage

### For Level Design (Visual Authoring)

1. **Create/Edit a Map in Unity Editor**
   - Open the Game scene
   - Paint terrain on the Terrain tilemap
   - Paint units on the civilization tilemaps (Japan/Rome)

2. **Export to MapData**
   - Go to `Tools > Map Exporter`
   - Scroll to "Export Scene to MapData" section
   - Enter a name for your map (e.g., "Level1")
   - Click "Export Current Scene"
   - MapData asset saved to `Assets/_GAME/Map/MapData/`

3. **Import MapData Back to Scene (for editing)**
   - Go to `Tools > Map Exporter`
   - In "Import MapData to Scene" section
   - Drag your MapData asset to the "MapData" field
   - Click "Import to Scene"
   - ⚠️ WARNING: This clears existing tiles!
   - Now you can edit the imported map

4. **Set Up Level Progression**
   - Find the LevelManager GameObject in the scene
   - Add your MapData assets to the "Levels" array in order
   - The game will progress through them sequentially

### For Testing (Programmatic Creation)

```csharp
// Create a simple test map
var testMap = MapDataTestUtility.CreateSimpleTestMap("MyTest");

// Load it
MapLoader.Instance.LoadMap(testMap);

// Or create a custom map
var customMap = MapDataTestUtility.CreateCustomMap(
    "CustomTest",
    new Vector2Int(10, 10),
    terrainPositions: new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) },
    units: new[] {
        (pos: new Vector2Int(0, 0), civ: Civilization.Japan, unit: warriorUnit),
        (pos: new Vector2Int(1, 0), civ: Civilization.Rome, unit: slingerUnit)
    }
);
```

## Scene Setup

### Adding MapLoader to Scene

1. Create empty GameObject named "MapLoader"
2. Add MapLoader component
3. Assign references:
   - Terrain Tilemap: Reference to the terrain tilemap
   - Unit Tilemap: Not currently used, can leave null
   - Civilization Tilemaps: Add all CivilizationTilemap components
   - Flag Tile: Optional, will auto-find "UnitFlag" if not set
   - Initial Map Data: Optional MapData to load on start

### Adding LevelManager to Scene

1. Create empty GameObject named "LevelManager" under Managers
2. Add LevelManager component
3. Assign levels array with your MapData assets

## Workflow Examples

### Creating a 3-Level Game

1. **Create Level 1**
   - Design map in scene
   - Export as "Level1" MapData
   
2. **Create Level 2**
   - Import Level1 back (optional - for tweaking)
   - OR clear tilemaps manually
   - Design new map
   - Export as "Level2" MapData
   
3. **Create Level 3**
   - Import Level2 back (optional)
   - OR clear tilemaps
   - Design new map
   - Export as "Level3" MapData

4. **Configure LevelManager**
   - Assign [Level1, Level2, Level3] to levels array
   - Clear MapLoader's "Initial Map Data" field to use scene tilemaps for testing
   - Or set Initial Map Data to Level1 to test progression

5. **Play**
   - Game starts with Level1 (or scene map)
   - Win → loads Level2
   - Win → loads Level3
   - Win → loops back to Level1

### Iterating on Existing Levels

1. **Load Level for Editing**
   - Go to Tools > Map Exporter
   - Drag your MapData to "MapData" field
   - Click "Import to Scene"
   
2. **Edit the Map**
   - Modify terrain tiles
   - Add/remove/move units
   
3. **Re-export**
   - Same name will overwrite existing MapData
   - OR use new name to create variant

### Testing a Specific Combat Scenario

```csharp
[UnityTest]
public IEnumerator TestRangedVsMelee()
{
    yield return null;
    
    // Create specific scenario
    var map = MapDataTestUtility.CreateCustomMap(
        "RangedTest",
        new Vector2Int(5, 5),
        terrainPositions: new[] { new Vector2Int(0, 0), new Vector2Int(3, 0) },
        units: new[] {
            (pos: new Vector2Int(0, 0), civ: Civilization.Japan, 
             unit: MapDataTestUtility.Resources.SlingerUnit),
            (pos: new Vector2Int(3, 0), civ: Civilization.Rome, 
             unit: MapDataTestUtility.Resources.WarriorUnit)
        }
    );
    
    MapLoader.Instance.LoadMap(map);
    yield return null;
    
    // Test combat behavior
    // ...
}
```

## API Reference

### MapData

```csharp
public class MapData : ScriptableObject
{
    public Vector2Int size;
    public TerrainData[] terrainTiles;
    public UnitPlacement[] unitPlacements;
    
    public void ValidateMapData(); // Checks positions are in bounds
    public string Summary { get; } // Human-readable summary
}
```

### MapLoader

```csharp
public class MapLoader : MonoBehaviour
{
    public static MapLoader Instance { get; }
    
    public void LoadMap(MapData mapData); // Load a map
    public void ClearMap(); // Clear current map
    public bool IsMapLoadedFromData(); // Check if using MapData vs scene
}
```

### LevelManager

```csharp
public class LevelManager : Singleton<LevelManager>
{
    public void LoadNextLevel(); // Progress to next level
    public void RestartCurrentLevel(); // Reload current level
    public void LoadLevel(int index); // Load specific level
    public MapData GetCurrentLevel(); // Get current MapData
    public bool HasNextLevel(); // Check if more levels exist
}
```

### MapDataTestUtility

```csharp
public static class MapDataTestUtility
{
    public static void LoadCommonResources();
    public static MapData CreateSimpleTestMap(string name = "TestMap");
    public static MapData CreateCombatTestMap(string name = "CombatTest");
    public static MapData CreateEmptyMap(string name = "EmptyMap", Vector2Int size);
    public static MapData CreateCustomMap(string name, Vector2Int size, 
                                          Vector2Int[] terrainPositions,
                                          (Vector2Int pos, Civilization civ, UnitSCOB unit)[] units);
    
    // Quick resource access
    public static class Resources
    {
        public static TerrainScob GrassTerrain { get; }
        public static TerrainScob OceanTerrain { get; }
        public static UnitSCOB WarriorUnit { get; }
        public static UnitSCOB SlingerUnit { get; }
    }
}
```

## Migration Guide

### Existing Scene → MapData System

If you have an existing game scene with tilemaps:

**Option 1: Keep Scene-Based (No Change)**
- Don't set Initial Map Data on MapLoader
- Game continues loading from scene tilemaps
- Good for development/testing

**Option 2: Export to MapData**
- Use Map Exporter tool to create MapData asset
- Set Initial Map Data on MapLoader to your exported map
- Scene tilemaps will be ignored
- Good for production/progression

**Option 3: Hybrid**
- Keep scene tilemaps for testing in editor
- Create MapData assets for each level
- Configure LevelManager with your levels
- In production, set Initial Map Data to Level 1

## Testing

All tests are in `Assets/Tests/PlayMode/`:

- `MapLoaderTests.cs` - Core map loading functionality
- `LevelProgressionTests.cs` - Level progression and restart
- `MapSystemIntegrationTests.cs` - End-to-end integration tests
- `GameOverTests.cs` - Existing game over tests (still work)

Run tests via Unity Test Runner (Window > General > Test Runner).

## Troubleshooting

**Q: Units not appearing after LoadMap?**
- Check MapLoader has Civilization Tilemaps assigned
- Verify UnitSCOB references in MapData match available tiles
- Check console for "[MapLoader]" warnings

**Q: Map export creates empty MapData or size is 0x0?**
- Ensure scene has terrain tiles placed
- Verify Terrain tilemap named "Terrain"
- Check CivilizationTilemap components exist
- The exporter calculates size from actual tile bounds

**Q: Map import doesn't place tiles correctly?**
- Verify tile assets are in Resources folder
- Check TerrainTile and UnitTile assets reference correct ScriptableObjects
- Ensure UnitFlag tile exists in Resources
- Look for warnings in console about missing tiles

**Q: Can't import MapData back to scene?**
- Make sure scene has Terrain tilemap
- Verify CivilizationTilemap components exist for both Japan and Rome
- Check that MapData references valid terrain/unit ScriptableObjects

**Q: Level progression not working?**
- Verify LevelManager in scene
- Check levels array is populated
- Ensure GameOverUI is triggering properly

**Q: Tests fail with null references?**
- Call MapDataTestUtility.LoadCommonResources() in test setup
- Verify Resources folder has required assets (Grass, Warrior, etc.)

## Next Steps

1. **Export your current map** using Map Exporter
2. **Create 2-3 more levels** with different layouts
3. **Configure LevelManager** with your levels
4. **Test progression** by playing and winning levels
5. **Add custom test scenarios** for edge cases

