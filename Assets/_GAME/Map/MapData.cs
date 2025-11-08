using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct TerrainData
{
    [HorizontalGroup("Split")]
    [LabelWidth(60)]
    public Vector2Int position;
    
    [HorizontalGroup("Split")]
    [Required]
    public TerrainScob terrain;
}

[Serializable]
public struct UnitPlacement
{
    [HorizontalGroup("Split", Width = 100)]
    [LabelWidth(60)]
    public Vector2Int position;
    
    [HorizontalGroup("Split")]
    [Required]
    public Civilization civilization;
    
    [HorizontalGroup("Split")]
    [Required]
    public UnitSCOB unit;
}

[CreateAssetMenu(menuName = "ScriptableObject/MapData")]
public class MapData : ScriptableObject
{
    [Title("Map Configuration")]
    [LabelWidth(100)]
    public Vector2Int size;
    
    [Title("Terrain")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "position")]
    public TerrainData[] terrainTiles = new TerrainData[0];
    
    [Title("Units")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "position")]
    public UnitPlacement[] unitPlacements = new UnitPlacement[0];
    
    [Button(ButtonSizes.Large), PropertySpace(10)]
    [InfoBox("Validates terrain positions are within bounds and no duplicate positions exist")]
    public void ValidateMapData()
    {
        int errors = 0;
        
        // Check terrain positions
        for (int i = 0; i < terrainTiles.Length; i++)
        {
            var tile = terrainTiles[i];
            if (tile.position.x < 0 || tile.position.y < 0 || 
                (size.x > 0 && tile.position.x >= size.x) || 
                (size.y > 0 && tile.position.y >= size.y))
            {
                Debug.LogError($"Terrain[{i}] at {tile.position} is outside map bounds {size}");
                errors++;
            }
        }
        
        // Check unit positions
        for (int i = 0; i < unitPlacements.Length; i++)
        {
            var unit = unitPlacements[i];
            if (unit.position.x < 0 || unit.position.y < 0 || 
                (size.x > 0 && unit.position.x >= size.x) || 
                (size.y > 0 && unit.position.y >= size.y))
            {
                Debug.LogError($"Unit[{i}] at {unit.position} is outside map bounds {size}");
                errors++;
            }
        }
        
        if (errors == 0)
        {
            Debug.Log($"✓ MapData validation passed: {terrainTiles.Length} terrain tiles, {unitPlacements.Length} units");
        }
        else
        {
            Debug.LogError($"✗ MapData validation failed with {errors} errors");
        }
    }
    
    [ShowInInspector, ReadOnly]
    [PropertySpace(SpaceBefore = 10)]
    public string Summary => $"{terrainTiles.Length} terrain tiles, {unitPlacements.Length} units, {size.x}x{size.y} map";
}

