using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;

[TestFixture]
public class MapLoaderTests
{
    private MapData testMapData;
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
    public IEnumerator MapLoader_LoadsMapDataCorrectly()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - add MapLoader GameObject to Game scene to enable this test");
            yield break;
        }

        // Load resources
        grassTerrain = Resources.Load<TerrainScob>("Terrain/Grass");
        warriorUnit = Resources.Load<UnitSCOB>("Units/Warrior");
        japanCiv = Resources.Load<CivilizationSCOB>("Civilizations/Japan");
        romeCiv = Resources.Load<CivilizationSCOB>("Civilizations/Rome");

        // Check if resources are available (need to be in Resources folder)
        if (grassTerrain == null || warriorUnit == null || japanCiv == null || romeCiv == null)
        {
            Assert.Ignore("Required resources not found in Resources folder. " +
                         "Create Assets/_GAME/Resources/ and move Grass.asset, Warrior.asset, Japan.asset, Rome.asset there.");
            yield break;
        }

        // Create test MapData
        testMapData = ScriptableObject.CreateInstance<MapData>();
        testMapData.size = new Vector2Int(5, 5);
        testMapData.terrainTiles = new TerrainData[]
        {
            new TerrainData { position = new Vector2Int(0, 0), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(1, 0), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(0, 1), terrain = grassTerrain },
            new TerrainData { position = new Vector2Int(1, 1), terrain = grassTerrain }
        };
        testMapData.unitPlacements = new UnitPlacement[]
        {
            new UnitPlacement { position = new Vector2Int(0, 0), civilization = Civilization.Japan, unit = warriorUnit },
            new UnitPlacement { position = new Vector2Int(1, 1), civilization = Civilization.Rome, unit = warriorUnit }
        };

        // Load the map
        MapLoader.Instance.LoadMap(testMapData);

        yield return null;

        // Verify units were registered
        var unitManager = UnitManager.Instance;
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(0, 0)), "Japan unit should be at (0,0)");
        Assert.IsTrue(unitManager.HasUnitAt(new Vector2Int(1, 1)), "Rome unit should be at (1,1)");

        var japanUnit = unitManager.GetUnitAt(new Vector2Int(0, 0));
        Assert.AreEqual(Civilization.Japan, japanUnit.civ, "Unit at (0,0) should be Japan");

        var romeUnit = unitManager.GetUnitAt(new Vector2Int(1, 1));
        Assert.AreEqual(Civilization.Rome, romeUnit.civ, "Unit at (1,1) should be Rome");

        Debug.Log("[MapLoaderTests] Test passed: Map loaded correctly");
    }

    [UnityTest]
    public IEnumerator MapLoader_ClearsMapCorrectly()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - add MapLoader GameObject to Game scene to enable this test");
            yield break;
        }

        // Clear the map
        MapLoader.Instance.ClearMap();

        yield return null;

        // Verify all units were cleared
        var unitManager = UnitManager.Instance;
        Assert.AreEqual(0, unitManager.units.Count, "All units should be cleared");
        Assert.AreEqual(0, unitManager.civUnits.Count, "All civ units should be cleared");

        Debug.Log("[MapLoaderTests] Test passed: Map cleared correctly");
    }
}

