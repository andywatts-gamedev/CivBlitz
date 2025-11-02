using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Automated unit tests for UnitManager core functionality
/// </summary>
public class UnitManagerTests
{
    private GameObject unitManagerObj;
    private UnitManager unitManager;

    [SetUp]
    public void Setup()
    {
        unitManagerObj = new GameObject("UnitManager");
        unitManager = unitManagerObj.AddComponent<UnitManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(unitManagerObj);
    }

    [Test]
    public void TryGetUnit_ReturnsTrue_WhenUnitExists()
    {
        var pos = new Vector2Int(0, 0);
        var unit = new Unit { name = "TestUnit", melee = 10, health = 100 };
        var civ = Civilization.Rome;
        
        unitManager.RegisterUnit(civ, unit, pos);
        
        var found = unitManager.TryGetUnit(pos, out var result);
        
        Assert.IsTrue(found);
        Assert.AreEqual("TestUnit", result.unit.name);
        Assert.AreEqual(pos, result.position);
    }

    [Test]
    public void TryGetUnit_ReturnsFalse_WhenPositionEmpty()
    {
        var emptyPos = new Vector2Int(10, 10);
        
        var found = unitManager.TryGetUnit(emptyPos, out var result);
        
        Assert.IsFalse(found);
    }

    [Test]
    public void HasUnitAt_ReturnsTrue_WhenUnitExists()
    {
        var pos = new Vector2Int(5, 5);
        var unit = new Unit { name = "Warrior", melee = 20 };
        var civ = Civilization.Japan;
        
        unitManager.RegisterUnit(civ, unit, pos);
        
        Assert.IsTrue(unitManager.HasUnitAt(pos));
    }

    [Test]
    public void HasUnitAt_ReturnsFalse_WhenPositionEmpty()
    {
        var emptyPos = new Vector2Int(20, 20);
        
        Assert.IsFalse(unitManager.HasUnitAt(emptyPos));
    }

    [Test]
    public void RegisterUnit_AddsUnitToDictionary()
    {
        var pos = new Vector2Int(3, 7);
        var unit = new Unit { name = "Warrior", melee = 30 };
        var civ = Civilization.Rome;
        
        unitManager.RegisterUnit(civ, unit, pos);
        
        Assert.IsTrue(unitManager.units.ContainsKey(pos));
        Assert.AreEqual("Warrior", unitManager.units[pos].unit.name);
        Assert.AreEqual(30, unitManager.units[pos].unit.melee);
    }

    [Test]
    public void RemoveUnit_RemovesFromDictionary()
    {
        var pos = new Vector2Int(1, 2);
        var unit = new Unit { name = "Scout" };
        var civ = Civilization.Japan;
        
        unitManager.RegisterUnit(civ, unit, pos);
        Assert.IsTrue(unitManager.HasUnitAt(pos));
        
        unitManager.RemoveUnit(pos);
        
        Assert.IsFalse(unitManager.HasUnitAt(pos));
    }

    [Test]
    public void UpdateUnit_ModifiesExistingUnit()
    {
        var pos = new Vector2Int(4, 4);
        var unit = new Unit { name = "Archer", movement = 3 };
        var civ = Civilization.Rome;
        
        unitManager.RegisterUnit(civ, unit, pos);
        
        var updated = unitManager.units[pos];
        updated.actionsLeft = 1;
        unitManager.UpdateUnit(pos, updated);
        
        Assert.AreEqual(1, unitManager.units[pos].actionsLeft);
    }

    [Test]
    public void RegisterUnit_AddsToCivUnits()
    {
        var pos = new Vector2Int(2, 2);
        var unit = new Unit { name = "Spearman" };
        var civ = Civilization.Japan;
        
        unitManager.RegisterUnit(civ, unit, pos);
        
        Assert.IsTrue(unitManager.civUnits.ContainsKey(civ));
        Assert.AreEqual(1, unitManager.civUnits[civ].Count);
        Assert.AreEqual(unit.name, unitManager.civUnits[civ][0].unit.name);
    }

    [Test]
    public void MultipleUnits_SameCiv_AllTracked()
    {
        var civ = Civilization.Rome;
        var unit1 = new Unit { name = "Legion" };
        var unit2 = new Unit { name = "Archer" };
        
        unitManager.RegisterUnit(civ, unit1, new Vector2Int(0, 0));
        unitManager.RegisterUnit(civ, unit2, new Vector2Int(1, 1));
        
        Assert.AreEqual(2, unitManager.civUnits[civ].Count);
        Assert.AreEqual(2, unitManager.units.Count);
    }
}

