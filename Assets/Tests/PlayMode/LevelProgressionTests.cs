using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;

[TestFixture]
public class LevelProgressionTests
{
    private MapData level1;
    private MapData level2;
    private TerrainScob grassTerrain;
    private UnitSCOB warriorUnit;
    private CivilizationSCOB japanCiv;
    private CivilizationSCOB romeCiv;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    [UnityTest]
    public IEnumerator LevelManager_LoadsNextLevelCorrectly()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - add MapLoader GameObject to Game scene to enable this test");
            yield break;
        }

        // Load resources
        grassTerrain = Resources.Load<TerrainScob>("Grass");
        warriorUnit = Resources.Load<UnitSCOB>("Warrior");
        japanCiv = Resources.Load<CivilizationSCOB>("Japan");
        romeCiv = Resources.Load<CivilizationSCOB>("Rome");

        // Check if resources are available
        if (grassTerrain == null || warriorUnit == null)
        {
            Assert.Ignore("Required resources not found in Resources folder.");
            yield break;
        }

        // Create level 1 - small map
        level1 = ScriptableObject.CreateInstance<MapData>();
        level1.name = "TestLevel1";
        level1.size = new Vector2Int(3, 3);
        level1.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(1, 0), terrain = grassTerrain }
        };
        level1.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = warriorUnit },
            new UnitPlacement { position = new Vector2Int(1, 0), civilization = Civilization.Rome, unit = warriorUnit }
        };

        // Create level 2 - different layout
        level2 = ScriptableObject.CreateInstance<MapData>();
        level2.name = "TestLevel2";
        level2.size = new Vector2Int(5, 5);
        level2.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(2, 2), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(4, 4), terrain = grassTerrain }
        };
        level2.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = warriorUnit },
            new UnitPlacement { position = new Vector2Int(4, 4), civilization = Civilization.Rome, unit = warriorUnit }
        };

        // Load level 1
        MapLoader.Instance.LoadMap(level1);
        yield return null;

        // Verify level 1 loaded
        var unitManager = UnitManager.Instance;
        Assert.AreEqual(2, unitManager.units.Count, "Level 1 should have 2 units");
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(0, 0)), "Level 1 Japan unit at (0,0)");
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(1, 0)), "Level 1 Rome unit at (1,0)");

        Debug.Log("[LevelProgressionTests] Level 1 loaded successfully");

        // Load level 2
        MapLoader.Instance.LoadMap(level2);
        yield return null;

        // Verify level 2 loaded and level 1 was cleared
        Assert.AreEqual(2, unitManager.units.Count, "Level 2 should have 2 units");
        Assert.IsFalse(unitManager.HasUnitAt(new Vector2Int(1, 0)), "Level 1 unit should be cleared");
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(0, 0)), "Level 2 Japan unit at (0,0)");
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(4, 4)), "Level 2 Rome unit at (4,4)");

        Debug.Log("[LevelProgressionTests] Level 2 loaded successfully, Level 1 cleared");
    }

    [UnityTest]
    public IEnumerator LevelManager_RestartsCurrentLevel()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - add MapLoader GameObject to Game scene to enable this test");
            yield break;
        }

        // Load resources
        grassTerrain = Resources.Load<TerrainScob>("Grass");
        warriorUnit = Resources.Load<UnitSCOB>("Warrior");

        // Check if resources are available
        if (grassTerrain == null || warriorUnit == null)
        {
            Assert.Ignore("Required resources not found in Resources folder.");
            yield break;
        }

        // Create a test level
        var testLevel = ScriptableObject.CreateInstance<MapData>();
        testLevel.name = "RestartTest";
        testLevel.size = new Vector2Int(3, 3);
        testLevel.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = grassTerrain }
        };
        testLevel.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = warriorUnit }
        };

        // Load the level
        MapLoader.Instance.LoadMap(testLevel);
        yield return null;

        var unitManager = UnitManager.Instance;
        Assert.AreEqual(1, unitManager.units.Count, "Should have 1 unit");

        // Modify the unit state
        var unit = unitManager.GetUnitAt(new Vector2Int(0, 0));
        unit.actionsLeft = 0;
        unitManager.UpdateUnit(new Vector2Int(0, 0), unit);

        // Reload the level
        MapLoader.Instance.LoadMap(testLevel);
        yield return null;

        // Verify the level was reset
        Assert.AreEqual(1, unitManager.units.Count, "Should still have 1 unit");
        var reloadedUnit = unitManager.GetUnitAt(new Vector2Int(0, 0));
        Assert.Greater(reloadedUnit.actionsLeft, 0, "Unit should have fresh actions after reload");

        Debug.Log("[LevelProgressionTests] Level restart test passed");
    }
}

