using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Automated integration tests that verify UI behavior in the Game scene
/// Standard Unity test pattern with scene-based isolation
/// </summary>
public class SelectedTileUIIntegrationTests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }


    [UnityTest]
    public IEnumerator UnitRow_DisplaysFlex_WhenUnitExists()
    {
        var unitManager = UnitManager.Instance;
        Assert.IsNotNull(unitManager);
        
        Vector2Int? unitPos = null;
        UnitInstance unit = null;
        foreach (var kvp in unitManager.units)
        {
            unitPos = kvp.Key;
            unit = kvp.Value;
            break;
        }
        
        if (!unitPos.HasValue)
        {
            Assert.Inconclusive("No units in scene");
            yield break;
        }
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        Assert.IsTrue(gameStateEvents.Length > 0);
        
        gameStateEvents[0].EmitTileSelected(unitPos.Value);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        
        var container = root.Q("SelectedTileContainer");
        Assert.IsNotNull(container, "Container should exist");
        Assert.AreEqual(DisplayStyle.Flex, container.style.display.value, "Container should be visible");
        
        var unitRow = container.Q("UnitRow");
        Assert.IsNotNull(unitRow, "UnitRow should exist");
        Assert.AreEqual(DisplayStyle.Flex, unitRow.style.display.value, "UnitRow should be visible when unit exists");

        var unitName = container.Q<Label>("UnitName");
        var unitAttack = container.Q<Label>("UnitAttack");
        var unitDefense = container.Q<Label>("UnitDefense");
        
        Assert.IsNotNull(unitName, "UnitName label should exist");
        Assert.IsNotNull(unitAttack, "UnitAttack label should exist");
        Assert.IsNotNull(unitDefense, "UnitDefense label should exist");
        
        Assert.AreEqual(unit.unit.name, unitName.text, "Unit name should match");
        Assert.IsFalse(string.IsNullOrEmpty(unitAttack.text), "Unit attack should have a value");
        Assert.IsFalse(string.IsNullOrEmpty(unitDefense.text), "Unit defense should have a value");
        
        Debug.Log($"Unit: {unitName.text}, Attack: {unitAttack.text}, Defense: {unitDefense.text}");
    }

    [UnityTest]
    public IEnumerator UnitRow_DisplaysNone_WhenNoUnit()
    {
        var unitManager = UnitManager.Instance;
        var emptyPos = new Vector2Int(100, 100);
        
        while (unitManager.HasUnitAt(emptyPos))
            emptyPos += Vector2Int.one;
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(emptyPos);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        var container = root.Q("SelectedTileContainer");
        var unitRow = container.Q("UnitRow");
        
        Assert.AreEqual(DisplayStyle.None, unitRow.style.display.value, "UnitRow should be hidden when no unit");
    }

    [UnityTest]
    public IEnumerator AttackColumn_ShowsWhenUnitExists()
    {
        var unitManager = UnitManager.Instance;
        Vector2Int? unitPos = null;
        
        foreach (var kvp in unitManager.units)
        {
            unitPos = kvp.Key;
            break;
        }
        
        if (!unitPos.HasValue)
        {
            Assert.Inconclusive("No units in scene");
            yield break;
        }
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(unitPos.Value);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        
        var attackIcon = root.Q<Label>("AttackIcon");
        var unitAttack = root.Q<Label>("UnitAttack");
        var tileAttack = root.Q<Label>("TileAttack");
        
        Assert.IsNotNull(attackIcon, "AttackIcon should exist");
        Assert.IsNotNull(unitAttack, "UnitAttack should exist");
        Assert.IsNotNull(tileAttack, "TileAttack should exist");
        
        Assert.AreEqual(DisplayStyle.Flex, attackIcon.style.display.value, "AttackIcon should be visible with unit");
        Assert.AreEqual(DisplayStyle.Flex, unitAttack.style.display.value, "UnitAttack should be visible with unit");
        Assert.AreEqual(DisplayStyle.Flex, tileAttack.style.display.value, "TileAttack should be visible with unit");
    }

    [UnityTest]
    public IEnumerator AttackColumn_HidesWhenNoUnit()
    {
        var unitManager = UnitManager.Instance;
        var emptyPos = new Vector2Int(100, 100);
        
        while (unitManager.HasUnitAt(emptyPos))
            emptyPos += Vector2Int.one;
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(emptyPos);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        
        var attackIcon = root.Q<Label>("AttackIcon");
        var unitAttack = root.Q<Label>("UnitAttack");
        var tileAttack = root.Q<Label>("TileAttack");
        
        Assert.AreEqual(DisplayStyle.None, attackIcon.style.display.value, "AttackIcon should be hidden without unit");
        Assert.AreEqual(DisplayStyle.None, unitAttack.style.display.value, "UnitAttack should be hidden without unit");
        Assert.AreEqual(DisplayStyle.None, tileAttack.style.display.value, "TileAttack should be hidden without unit");
    }

    [UnityTest]
    public IEnumerator SelectedTile_ShowsCorrectAttackValue_ForMeleeUnit()
    {
        var unitManager = UnitManager.Instance;
        Vector2Int? meleePos = null;
        UnitInstance meleeUnit = null;
        
        foreach (var kvp in unitManager.units)
        {
            if (kvp.Value.unit.type == UnitType.Melee)
            {
                meleePos = kvp.Key;
                meleeUnit = kvp.Value;
                break;
            }
        }
        
        if (!meleePos.HasValue)
        {
            Assert.Inconclusive("No melee units in scene");
            yield break;
        }
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(meleePos.Value);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        var unitAttack = root.Q<Label>("UnitAttack");
        
        Assert.AreEqual(meleeUnit.unit.melee.ToString(), unitAttack.text, "Melee unit should show melee as attack");
    }

    [UnityTest]
    public IEnumerator SelectedTile_ShowsCorrectAttackValue_ForRangedUnit()
    {
        var unitManager = UnitManager.Instance;
        Vector2Int? rangedPos = null;
        UnitInstance rangedUnit = null;
        
        foreach (var kvp in unitManager.units)
        {
            if (kvp.Value.unit.type == UnitType.Ranged)
            {
                rangedPos = kvp.Key;
                rangedUnit = kvp.Value;
                break;
            }
        }
        
        if (!rangedPos.HasValue)
        {
            Assert.Inconclusive("No ranged units in scene");
            yield break;
        }
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(rangedPos.Value);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        var unitAttack = root.Q<Label>("UnitAttack");
        
        Assert.AreEqual(rangedUnit.unit.ranged.ToString(), unitAttack.text, "Ranged unit should show ranged as attack");
    }

    [UnityTest]
    public IEnumerator TargetTile_ShowsUnitRow_WhenUnitExists()
    {
        var unitManager = UnitManager.Instance;
        Vector2Int? unitPos = null;
        
        foreach (var kvp in unitManager.units)
        {
            unitPos = kvp.Key;
            break;
        }
        
        if (!unitPos.HasValue)
        {
            Assert.Inconclusive("No units in scene");
            yield break;
        }
        
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileHovered(unitPos.Value);
        yield return null;
        
        var targetTileUI = Object.FindObjectOfType<TargetTileUI>();
        var uiDoc = targetTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        var container = root.Q("TargetTileContainer");
        var unitRow = container.Q("UnitRow");
        
        Assert.AreEqual(DisplayStyle.Flex, unitRow.style.display.value, "TargetTile UnitRow should show for unit");
    }

    [UnityTest]
    public IEnumerator TileInfo_AlwaysShows_Regardless()
    {
        var unitManager = UnitManager.Instance;
        var anyPos = new Vector2Int(0, 0);
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(anyPos);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        
        var tileName = root.Q<Label>("TileName");
        var tileAttack = root.Q<Label>("TileAttack");
        var tileDefense = root.Q<Label>("TileDefense");
        
        Assert.IsNotNull(tileName, "Tile name should exist");
        Assert.IsNotNull(tileAttack, "Tile attack should exist");
        Assert.IsNotNull(tileDefense, "Tile defense should exist");
        Assert.IsFalse(string.IsNullOrEmpty(tileName.text), "Tile name should have value");
    }

    [UnityTest]
    public IEnumerator UnitRow_ShowsBothAttackAndDefense_WhenUnitExists()
    {
        var unitManager = UnitManager.Instance;
        Vector2Int? unitPos = null;
        UnitInstance unit = null;
        
        foreach (var kvp in unitManager.units)
        {
            unitPos = kvp.Key;
            unit = kvp.Value;
            break;
        }
        
        if (!unitPos.HasValue)
        {
            Assert.Inconclusive("No units in scene");
            yield break;
        }
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>();
        gameStateEvents[0].EmitTileSelected(unitPos.Value);
        yield return null;
        
        var selectedTileUI = Object.FindObjectOfType<SelectedTileUI>();
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        
        var unitAttack = root.Q<Label>("UnitAttack");
        var unitDefense = root.Q<Label>("UnitDefense");
        
        Assert.IsNotNull(unitAttack, "UnitAttack must exist");
        Assert.IsNotNull(unitDefense, "UnitDefense must exist");
        
        var expectedAttack = unit.unit.type == UnitType.Ranged ? unit.unit.ranged : unit.unit.melee;
        var expectedDefense = unit.unit.melee;
        
        Assert.AreEqual(expectedAttack.ToString(), unitAttack.text, "Attack value must match unit stats");
        Assert.AreEqual(expectedDefense.ToString(), unitDefense.text, "Defense value must match unit stats");
        
        Assert.AreNotEqual("0", unitAttack.text, "Attack should not be 0 for real units");
        Assert.AreNotEqual("", unitAttack.text, "Attack should not be empty");
    }
}

