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

        // EXPORT SECTION
        EditorGUILayout.LabelField("Export Scene to MapData", EditorStyles.boldLabel);
        assetName = EditorGUILayout.TextField("Asset Name:", assetName);
        savePath = EditorGUILayout.TextField("Save Path:", savePath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Export Current Scene", GUILayout.Height(30)))
        {
            ExportScene();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Export will scan the current scene's tilemaps and create a MapData asset.\n\n" +
            "It will export:\n" +
            "- Terrain tiles from the Terrain tilemap\n" +
            "- Unit placements from CivilizationTilemap components",
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

        // Clear existing tiles
        terrainTilemap.ClearAllTiles();
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

    private void ExportScene()
    {
        // Find the terrain tilemap
        var map = FindObjectOfType<Map>();
        if (map == null)
        {
            EditorUtility.DisplayDialog("Error", "No Map component found in scene!", "OK");
            return;
        }

        var terrainTilemap = GameObject.Find("Terrain")?.GetComponent<Tilemap>();
        if (terrainTilemap == null)
        {
            EditorUtility.DisplayDialog("Error", "No Terrain tilemap found in scene!", "OK");
            return;
        }

        // Find all civilization tilemaps
        var civTilemaps = FindObjectsOfType<CivilizationTilemap>();
        if (civTilemaps.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No CivilizationTilemap components found in scene!", "OK");
            return;
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
                        unitPlacementList.Add(new UnitPlacement
                        {
                            position = (Vector2Int)pos,
                            civilization = civTilemap.civ.civilization,
                            unit = unitTile.unitSCOB
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
            mapData.size = Vector2Int.zero;
            mapData.terrainTiles = terrainDataList.ToArray();
            mapData.unitPlacements = unitPlacementList.ToArray();
        }
        else
        {
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
        }

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

        // Save asset
        var fullPath = $"{savePath}/{assetName}.asset";
        AssetDatabase.CreateAsset(mapData, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select the created asset
        EditorGUIUtility.PingObject(mapData);
        Selection.activeObject = mapData;

        Debug.Log($"[MapExporter] Exported map to {fullPath}:\n" +
                  $"  - Size: {mapData.size.x}x{mapData.size.y}\n" +
                  $"  - {mapData.terrainTiles.Length} terrain tiles\n" +
                  $"  - {mapData.unitPlacements.Length} units");

        EditorUtility.DisplayDialog("Success",
            $"Map exported successfully!\n\n" +
            $"Size: {mapData.size.x} x {mapData.size.y}\n" +
            $"Terrain tiles: {mapData.terrainTiles.Length}\n" +
            $"Units: {mapData.unitPlacements.Length}\n\n" +
            $"Coordinates normalized to (0,0) origin.\n" +
            $"Saved to: {fullPath}",
            "OK");
    }
}

