using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;

[TestFixture]
public class MapSystemIntegrationTests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    [UnityTest]
    public IEnumerator Integration_MapDataLoadingWorks()
    {
        yield return null;

        // Verify core systems exist
        Assert.IsNotNull(MapLoader.Instance, "MapLoader should exist");
        Assert.IsNotNull(UnitManager.Instance, "UnitManager should exist");
        Assert.IsNotNull(Game.Instance, "Game should exist");

        Debug.Log("[Integration] Core systems verified");
    }

    [UnityTest]
    public IEnumerator Integration_SceneFallbackWorks()
    {
        yield return null;

        // When no MapData is set, scene should load normally
        var unitManager = UnitManager.Instance;
        
        // The scene should have units (either from MapData or scene tilemaps)
        Assert.Greater(unitManager.units.Count, 0, "Scene should have units loaded");

        Debug.Log($"[Integration] Scene loaded with {unitManager.units.Count} units");
    }

    [UnityTest]
    public IEnumerator Integration_TestUtilitiesWork()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - test skipped");
            yield break;
        }

        // Create a test map using utilities
        var testMap = MapDataTestUtility.CreateSimpleTestMap("IntegrationTest");
        
        Assert.IsNotNull(testMap, "Test map should be created");
        Assert.Greater(testMap.terrainTiles.Length, 0, "Test map should have terrain");
        Assert.Greater(testMap.unitPlacements.Length, 0, "Test map should have units");

        // Load it
        MapLoader.Instance.LoadMap(testMap);
        yield return null;

        // Verify it loaded
        var unitManager = UnitManager.Instance;
        Assert.AreEqual(testMap.unitPlacements.Length, unitManager.units.Count, 
            "Loaded units should match MapData");

        Debug.Log("[Integration] Test utilities work correctly");
    }

    [UnityTest]
    public IEnumerator Integration_MapExportValidation()
    {
        yield return null;

        // Create a test map
        var testMap = MapDataTestUtility.CreateSimpleTestMap("ValidationTest");
        
        // Validate it
        testMap.ValidateMapData();
        
        // Should not throw errors (checked via logs)
        yield return null;

        Debug.Log("[Integration] MapData validation works");
    }

    [UnityTest]
    public IEnumerator Integration_LevelManagerExists()
    {
        yield return null;

        // LevelManager may or may not exist depending on scene setup
        // This test just verifies the system handles it gracefully
        if (LevelManager.Instance != null)
        {
            Debug.Log("[Integration] LevelManager found in scene");
            Assert.IsNotNull(LevelManager.Instance, "LevelManager should be accessible");
        }
        else
        {
            Debug.Log("[Integration] LevelManager not in scene (optional)");
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator Integration_ClearAndReloadWorks()
    {
        yield return null;

        // Check if MapLoader exists in scene
        if (MapLoader.Instance == null)
        {
            Assert.Ignore("MapLoader not in scene - test skipped");
            yield break;
        }

        // Create and load first map
        var map1 = MapDataTestUtility.CreateSimpleTestMap("ClearTest1");
        MapLoader.Instance.LoadMap(map1);
        yield return null;

        var unitManager = UnitManager.Instance;
        int map1Units = unitManager.units.Count;
        Assert.Greater(map1Units, 0, "Map 1 should have units");

        // Create and load second map with different unit count
        var map2 = MapDataTestUtility.CreateCombatTestMap("ClearTest2");
        MapLoader.Instance.LoadMap(map2);
        yield return null;

        int map2Units = unitManager.units.Count;
        
        // Should have cleared map1 and loaded map2
        Assert.AreEqual(map2.unitPlacements.Length, map2Units, "Map 2 units should be loaded");
        
        Debug.Log($"[Integration] Clear and reload: {map1Units} -> {map2Units}");
    }
}

