using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MapExporter : EditorWindow
{
    private string assetName = "ExportedMap";
    private string savePath = "Assets/_GAME/Map/MapData";
    private MapData mapDataToImport;
    private MapData mapDataToExportTo;

    [MenuItem("Tools/Map Exporter")]
    public static void ShowWindow()
    {
        GetWindow<MapExporter>("Map Exporter");
    }

    void OnGUI()
    {
        GUILayout.Label("Map Import/Export", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // IMPORT SECTION
        EditorGUILayout.LabelField("Import MapData to Scene", EditorStyles.boldLabel);
        mapDataToImport = (MapData)EditorGUILayout.ObjectField("MapData:", mapDataToImport, typeof(MapData), false);
        
        EditorGUI.BeginDisabledGroup(mapDataToImport == null);
        if (GUILayout.Button("Import to Scene", GUILayout.Height(30)))
        {
            ImportMapData();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Import will load a MapData asset into the scene tilemaps.\n" +
            "WARNING: This will clear existing tiles!",
            MessageType.Warning
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // UPDATE EXISTING MAP SECTION
        EditorGUILayout.LabelField("Update Existing Map", EditorStyles.boldLabel);
        mapDataToExportTo = (MapData)EditorGUILayout.ObjectField("MapData:", mapDataToExportTo, typeof(MapData), false);
        
        EditorGUI.BeginDisabledGroup(mapDataToExportTo == null);
        if (GUILayout.Button("Update Map", GUILayout.Height(30)))
        {
            UpdateExistingMap();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Updates the selected MapData with current scene tilemaps.",
            MessageType.Info
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // CREATE NEW MAP SECTION
        EditorGUILayout.LabelField("Create New Map", EditorStyles.boldLabel);
        assetName = EditorGUILayout.TextField("Asset Name:", assetName);
        savePath = EditorGUILayout.TextField("Save Path:", savePath);

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(assetName));
        if (GUILayout.Button("Create New Map", GUILayout.Height(30)))
        {
            CreateNewMap();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Creates a new MapData asset from current scene tilemaps.\n\n" +
            "Exports:\n" +
            "- Terrain tiles from Terrain tilemap\n" +
            "- Unit placements from CivilizationTilemap components\n" +
            "- Unit overrides from Overrides tilemap (optional)",
            MessageType.Info
        );
    }

    private void ImportMapData()
    {
        if (mapDataToImport == null)
        {
            EditorUtility.DisplayDialog("Error", "No MapData selected!", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Import MapData",
            $"This will clear all existing tiles and import:\n\n" +
            $"Map: {mapDataToImport.name}\n" +
            $"Size: {mapDataToImport.size.x} x {mapDataToImport.size.y}\n" +
            $"Terrain: {mapDataToImport.terrainTiles.Length} tiles\n" +
            $"Units: {mapDataToImport.unitPlacements.Length} units\n\n" +
            $"Continue?",
            "Yes", "Cancel"))
        {
            return;
        }

        // Find scene components
        var terrainTilemap = GameObject.Find("Terrain")?.GetComponent<Tilemap>();
        if (terrainTilemap == null)
        {
            EditorUtility.DisplayDialog("Error", "No Terrain tilemap found in scene!", "OK");
            return;
        }

        var civTilemaps = FindObjectsOfType<CivilizationTilemap>();
        if (civTilemaps.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No CivilizationTilemap components found in scene!", "OK");
            return;
        }
        
        // Find overrides tilemap (optional)
        var overridesTilemap = GameObject.Find("Overrides")?.GetComponent<Tilemap>();

        // Build tile caches using AssetDatabase
        var terrainTileGuids = AssetDatabase.FindAssets("t:TerrainTile");
        var terrainTileCache = new Dictionary<TerrainScob, TerrainTile>();
        foreach (var guid in terrainTileGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tile = AssetDatabase.LoadAssetAtPath<TerrainTile>(path);
            if (tile != null && tile.terrainScob != null)
                terrainTileCache[tile.terrainScob] = tile;
        }

        var unitTileGuids = AssetDatabase.FindAssets("t:UnitTile");
        var unitTileCache = new Dictionary<UnitSCOB, UnitTile>();
        foreach (var guid in unitTileGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tile = AssetDatabase.LoadAssetAtPath<UnitTile>(path);
            if (tile != null && tile.unitSCOB != null)
                unitTileCache[tile.unitSCOB] = tile;
        }

        // Find UnitFlag using AssetDatabase (works in editor)
        var flagTileGuids = AssetDatabase.FindAssets("UnitFlag t:Tile");
        Tile flagTile = null;
        if (flagTileGuids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(flagTileGuids[0]);
            flagTile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        }
        
        if (flagTile == null)
        {
            EditorUtility.DisplayDialog("Error", "UnitFlag tile not found!\nSearched for asset named 'UnitFlag' of type Tile.", "OK");
            return;
        }

        var civTilemapLookup = civTilemaps.ToDictionary(ct => ct.civ.civilization, ct => ct);

        // Build override tile cache
        var overrideTileGuids = AssetDatabase.FindAssets("t:UnitOverrideTile");
        var overrideTilesByHealth = new Dictionary<int, UnitOverrideTile>();
        foreach (var guid in overrideTileGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tile = AssetDatabase.LoadAssetAtPath<UnitOverrideTile>(path);
            if (tile != null && tile.healthOverride > 0)
            {
                overrideTilesByHealth[tile.healthOverride] = tile;
            }
        }

        // Clear existing tiles
        terrainTilemap.ClearAllTiles();
        if (overridesTilemap != null) overridesTilemap.ClearAllTiles();
        foreach (var civTilemap in civTilemaps)
        {
            if (civTilemap.flags != null) civTilemap.flags.ClearAllTiles();
            if (civTilemap.units != null) civTilemap.units.ClearAllTiles();
        }

        // Import terrain
        int terrainCount = 0;
        foreach (var terrainData in mapDataToImport.terrainTiles)
        {
            if (terrainData.terrain != null && terrainTileCache.TryGetValue(terrainData.terrain, out var tile))
            {
                terrainTilemap.SetTile((Vector3Int)terrainData.position, tile);
                terrainCount++;
            }
            else
            {
                Debug.LogWarning($"[MapImporter] No tile found for terrain: {terrainData.terrain?.name} at {terrainData.position}");
            }
        }

        // Import units
        int unitCount = 0;
        foreach (var unitPlacement in mapDataToImport.unitPlacements)
        {
            if (!civTilemapLookup.TryGetValue(unitPlacement.civilization, out var civTilemap))
            {
                Debug.LogWarning($"[MapImporter] No tilemap for civilization: {unitPlacement.civilization}");
                continue;
            }

            // Place flag
            if (civTilemap.flags != null)
            {
                civTilemap.flags.SetTile((Vector3Int)unitPlacement.position, flagTile);
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(2f, 2f, 2f));
                civTilemap.flags.SetTransformMatrix((Vector3Int)unitPlacement.position, matrix);
            }

            // Place unit
            if (unitPlacement.unit != null && unitTileCache.TryGetValue(unitPlacement.unit, out var unitTile) && civTilemap.units != null)
            {
                civTilemap.units.SetTile((Vector3Int)unitPlacement.position, unitTile);
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.8f, 0.8f, 0.8f));
                civTilemap.units.SetTransformMatrix((Vector3Int)unitPlacement.position, matrix);
                unitCount++;
            }
            else
            {
                Debug.LogWarning($"[MapImporter] No tile found for unit: {unitPlacement.unit?.name} at {unitPlacement.position}");
            }
            
            // Place override tile if unit has health override
            if (unitPlacement.healthOverride > 0 && overridesTilemap != null)
            {
                if (overrideTilesByHealth.TryGetValue(unitPlacement.healthOverride, out var overrideTile))
                {
                    overridesTilemap.SetTile((Vector3Int)unitPlacement.position, overrideTile);
                    Debug.Log($"[MapImporter] Placed override tile at {unitPlacement.position}: {unitPlacement.healthOverride}%");
                }
                else
                {
                    Debug.LogWarning($"[MapImporter] No override tile found for {unitPlacement.healthOverride}% health. Create a UnitOverrideTile with healthOverride={unitPlacement.healthOverride}");
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[MapImporter] Imported {mapDataToImport.name}:\n" +
                  $"  - {terrainCount} terrain tiles\n" +
                  $"  - {unitCount} units");

        EditorUtility.DisplayDialog("Import Complete",
            $"Map imported successfully!\n\n" +
            $"Terrain: {terrainCount} tiles\n" +
            $"Units: {unitCount} units",
            "OK");
    }

    private void UpdateExistingMap()
    {
        if (mapDataToExportTo == null) return;
        
        var mapData = ExportSceneToMapData();
        if (mapData == null) return;
        
        var fullPath = AssetDatabase.GetAssetPath(mapDataToExportTo);
        var assetFileName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
        
        // Copy data to existing asset
        EditorUtility.CopySerialized(mapData, mapDataToExportTo);
        
        // Fix name to match filename
        mapDataToExportTo.name = assetFileName;
        
        EditorUtility.SetDirty(mapDataToExportTo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Verify the data was copied
        Debug.Log($"[MapExporter] Verification - MapData now has {mapDataToExportTo.unitPlacements.Length} units");
        foreach (var unit in mapDataToExportTo.unitPlacements)
        {
            if (unit.healthOverride > 0)
            {
                Debug.Log($"[MapExporter] Verification - Unit at {unit.position} has {unit.healthOverride}% health in saved asset");
            }
        }
        
        // Clean up temporary MapData
        DestroyImmediate(mapData);
        
        // Select the updated asset
        EditorGUIUtility.PingObject(mapDataToExportTo);
        Selection.activeObject = mapDataToExportTo;

        Debug.Log($"[MapExporter] Updated {mapDataToExportTo.name}:\n" +
                  $"  - Size: {mapDataToExportTo.size.x}x{mapDataToExportTo.size.y}\n" +
                  $"  - {mapDataToExportTo.terrainTiles.Length} terrain tiles\n" +
                  $"  - {mapDataToExportTo.unitPlacements.Length} units");

        var unitsWithOverrides = mapDataToExportTo.unitPlacements.Count(u => u.healthOverride > 0);
        EditorUtility.DisplayDialog("Success",
            $"Map updated successfully!\n\n" +
            $"Updated: {mapDataToExportTo.name}\n" +
            $"Size: {mapDataToExportTo.size.x} x {mapDataToExportTo.size.y}\n" +
            $"Terrain: {mapDataToExportTo.terrainTiles.Length}\n" +
            $"Units: {mapDataToExportTo.unitPlacements.Length}\n" +
            $"Units with overrides: {unitsWithOverrides}",
            "OK");
    }

    private void CreateNewMap()
    {
        if (string.IsNullOrEmpty(assetName)) return;
        
        var mapData = ExportSceneToMapData();
        if (mapData == null) return;
        
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            var folders = savePath.Split('/');
            var currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                var nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }

        var fullPath = $"{savePath}/{assetName}.asset";
        mapData.name = assetName; // Set object name to match filename
        AssetDatabase.CreateAsset(mapData, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select the created asset
        EditorGUIUtility.PingObject(mapData);
        Selection.activeObject = mapData;

        Debug.Log($"[MapExporter] Created {fullPath}:\n" +
                  $"  - Size: {mapData.size.x}x{mapData.size.y}\n" +
                  $"  - {mapData.terrainTiles.Length} terrain tiles\n" +
                  $"  - {mapData.unitPlacements.Length} units");

        var unitsWithOverrides = mapData.unitPlacements.Count(u => u.healthOverride > 0);
        EditorUtility.DisplayDialog("Success",
            $"Map created successfully!\n\n" +
            $"Name: {assetName}\n" +
            $"Size: {mapData.size.x} x {mapData.size.y}\n" +
            $"Terrain: {mapData.terrainTiles.Length}\n" +
            $"Units: {mapData.unitPlacements.Length}\n" +
            $"Units with overrides: {unitsWithOverrides}\n\n" +
            $"Saved to: {fullPath}",
            "OK");
    }

    private MapData ExportSceneToMapData()
    {
        // Find the terrain tilemap
        var map = FindObjectOfType<Map>();
        if (map == null)
        {
            EditorUtility.DisplayDialog("Error", "No Map component found in scene!", "OK");
            return null;
        }

        var terrainTilemap = GameObject.Find("Terrain")?.GetComponent<Tilemap>();
        if (terrainTilemap == null)
        {
            EditorUtility.DisplayDialog("Error", "No Terrain tilemap found in scene!", "OK");
            return null;
        }

        // Find all civilization tilemaps
        var civTilemaps = FindObjectsOfType<CivilizationTilemap>();
        if (civTilemaps.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No CivilizationTilemap components found in scene!", "OK");
            return null;
        }
        
        // Find overrides tilemap (optional)
        var overridesTilemap = GameObject.Find("Overrides")?.GetComponent<Tilemap>();
        if (overridesTilemap != null)
        {
            Debug.Log($"[MapExporter] Found Overrides tilemap");
        }
        else
        {
            Debug.Log($"[MapExporter] No Overrides tilemap found (optional)");
        }

        // Create MapData
        var mapData = ScriptableObject.CreateInstance<MapData>();

        // Export terrain (collect first to find bounds)
        var terrainDataList = new List<TerrainData>();
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        
        foreach (var pos in terrainTilemap.cellBounds.allPositionsWithin)
        {
            var tile = terrainTilemap.GetTile(pos) as TerrainTile;
            if (tile != null && tile.terrainScob != null)
            {
                terrainDataList.Add(new TerrainData
                {
                    position = (Vector2Int)pos,
                    terrain = tile.terrainScob
                });
                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxX = Mathf.Max(maxX, pos.x);
                maxY = Mathf.Max(maxY, pos.y);
            }
        }

        // Export units (also track bounds)
        var unitPlacementList = new List<UnitPlacement>();
        foreach (var civTilemap in civTilemaps)
        {
            if (civTilemap.flags == null || civTilemap.units == null)
                continue;

            foreach (var pos in civTilemap.flags.cellBounds.allPositionsWithin)
            {
                var flagTile = civTilemap.flags.GetTile(pos);
                if (flagTile != null)
                {
                    var unitTile = civTilemap.units.GetTile(pos) as UnitTile;
                    if (unitTile != null && unitTile.unitSCOB != null)
                    {
                        // Check for override tile at this position
                        int healthOverride = 0;
                        if (overridesTilemap != null)
                        {
                            var anyTile = overridesTilemap.GetTile(pos);
                            if (anyTile != null)
                            {
                                Debug.Log($"[MapExporter] Found tile at {pos}: {anyTile.name} (Type: {anyTile.GetType().Name})");
                            }
                            
                            var overrideTile = anyTile as UnitOverrideTile;
                            if (overrideTile != null)
                            {
                                healthOverride = overrideTile.healthOverride;
                                Debug.Log($"[MapExporter] Found health override at {pos}: {healthOverride}%");
                            }
                            else if (anyTile != null)
                            {
                                Debug.LogWarning($"[MapExporter] Tile at {pos} is type '{anyTile.GetType().Name}', not UnitOverrideTile. Change the tile's Script field to UnitOverrideTile.");
                            }
                        }
                        
                        unitPlacementList.Add(new UnitPlacement
                        {
                            position = (Vector2Int)pos,
                            civilization = civTilemap.civ.civilization,
                            unit = unitTile.unitSCOB,
                            healthOverride = healthOverride
                        });
                        minX = Mathf.Min(minX, pos.x);
                        minY = Mathf.Min(minY, pos.y);
                        maxX = Mathf.Max(maxX, pos.x);
                        maxY = Mathf.Max(maxY, pos.y);
                    }
                }
            }
        }

        // Calculate size and normalize coordinates to (0,0) origin
        if (terrainDataList.Count == 0 && unitPlacementList.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No tiles or units found to export!", "OK");
            return null;
        }

        var offset = new Vector2Int(minX, minY);
        mapData.size = new Vector2Int(maxX - minX + 1, maxY - minY + 1);

        // Normalize terrain positions
        for (int i = 0; i < terrainDataList.Count; i++)
        {
            var terrain = terrainDataList[i];
            terrain.position -= offset;
            terrainDataList[i] = terrain;
        }
        mapData.terrainTiles = terrainDataList.ToArray();

        // Normalize unit positions
        for (int i = 0; i < unitPlacementList.Count; i++)
        {
            var unit = unitPlacementList[i];
            unit.position -= offset;
            unitPlacementList[i] = unit;
        }
        mapData.unitPlacements = unitPlacementList.ToArray();
        
        // Debug: Log final unit placements with overrides
        foreach (var unit in mapData.unitPlacements)
        {
            if (unit.healthOverride > 0)
            {
                Debug.Log($"[MapExporter] Final MapData has unit at {unit.position} with {unit.healthOverride}% health");
            }
        }
        
        return mapData;
    }
}

