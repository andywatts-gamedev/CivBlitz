using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class UnitButtonsUITests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    #region Rest Button Active State Tests

    [UnityTest]
    public IEnumerator FortifyButton_ShowsActiveState_WhenUnitFortified()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        Assert.IsNotNull(fortifyButton, "FortifyButton not found");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        Assert.IsTrue(playerUnits.Count > 0, "No player units found");
        
        var testUnit = playerUnits[0];
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify button visible
        Assert.AreEqual(DisplayStyle.Flex, fortifyButton.parent.style.display.value, "Unit buttons should be visible");
        
        // Click rest button
        var unitButtonsUI = GameObject.Find("UI").GetComponent<UnitButtonsUI>();
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        // Verify button has active class
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should have 'active' class when unit is fortified");
    }

    [UnityTest]
    public IEnumerator FortifyButton_ShowsDefaultState_WhenUnitReady()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        Assert.IsNotNull(fortifyButton);
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Ensure unit is ready
        unitManager.SetUnitState(testUnit.position, UnitState.Ready);
        yield return null;
        
        // Verify button does NOT have active class
        Assert.IsFalse(fortifyButton.ClassListContains("active"), "FortifyButton should NOT have 'active' class when unit is ready");
    }

    [UnityTest]
    public IEnumerator FortifyButton_ActiveState_Persists_WhenReselectingUnit()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        Assert.IsNotNull(fortifyButton, "FortifyButton not found");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit and set to fortified
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should have 'active' class after setting to Fortified");
        
        // Deselect unit
        gameStateEvents.EmitTileDeselected(testUnit.position);
        yield return null;
        
        // Reselect unit
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify active state persists
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should still have 'active' class after reselecting");
    }

    [UnityTest]
    public IEnumerator RestState_ClearsAfterNewTurn()
    {
        yield return null;
        
        var turnManager = TurnManager.Instance;
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit and set to fortified
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should be active before turn ends");
        
        // End turn and complete AI turn
        turnManager.EndTurn();
        yield return null;
        
        turnManager.StartAITurn();
        yield return null;
        
        // Wait for AI turn to complete
        while (!turnManager.isPlayerTurn)
        {
            yield return null;
        }
        yield return null;
        
        // Reselect unit - should no longer be fortified
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify unit is back to Ready state
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var updatedUnit), "Unit should exist");
        Assert.AreEqual(UnitState.Ready, updatedUnit.state, "Unit state should be Ready after new turn");
        Assert.IsFalse(fortifyButton.ClassListContains("active"), "FortifyButton should NOT be active after new turn");
    }

    [UnityTest]
    public IEnumerator FortifyButton_ClearsActive_WhenToggledFromFortifiedToReady()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit and set to fortified
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should be active when fortified");
        Assert.AreEqual(UnitState.Fortified, testUnit.state, "Unit should be Fortified");
        
        // Toggle back to Ready
        unitManager.SetUnitState(testUnit.position, UnitState.Ready);
        yield return null;
        
        // Verify unit returned to Ready state and button updated
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var updatedUnit), "Unit should exist");
        Assert.AreEqual(UnitState.Ready, updatedUnit.state, "Unit should be Ready after toggling off rest");
        Assert.IsFalse(fortifyButton.ClassListContains("active"), "FortifyButton should NOT be active after toggling off");
    }

    [UnityTest]
    public IEnumerator FortifyButton_TogglesActiveClass_ThroughMultipleStateChanges()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Toggle to Fortified
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var unit1), "Unit should exist");
        Assert.AreEqual(UnitState.Fortified, unit1.state, "Unit should be Fortified after first toggle");
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should be active");
        
        // Toggle to Ready
        unitManager.SetUnitState(testUnit.position, UnitState.Ready);
        yield return null;
        
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var unit2), "Unit should exist");
        Assert.AreEqual(UnitState.Ready, unit2.state, "Unit should be Ready after second toggle");
        Assert.IsFalse(fortifyButton.ClassListContains("active"), "FortifyButton should NOT be active");
        
        // Toggle to Fortified again
        unitManager.SetUnitState(testUnit.position, UnitState.Fortified);
        yield return null;
        
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var unit3), "Unit should exist");
        Assert.AreEqual(UnitState.Fortified, unit3.state, "Unit should be Fortified after third toggle");
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should be active again");
    }

    #endregion

    #region Rest Button Disabled Tests

    [UnityTest]
    public IEnumerator FortifyButton_Disabled_WhenReadyAndNoActionsLeft()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit and deplete actions
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        testUnit.actionsLeft = 0;
        testUnit.state = UnitState.Ready;
        unitManager.UpdateUnit(testUnit.position, testUnit);
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify rest button disabled when Ready and no actions
        Assert.IsFalse(fortifyButton.enabledSelf, "FortifyButton should be disabled when Ready and no actions left");
    }

    [UnityTest]
    public IEnumerator FortifyButton_Enabled_WhenFortifiedEvenWithNoActions()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit, set to fortified with no actions
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        testUnit.actionsLeft = 0;
        testUnit.state = UnitState.Fortified;
        unitManager.UpdateUnit(testUnit.position, testUnit);
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify rest button enabled when Fortified (to allow toggling off)
        Assert.IsTrue(fortifyButton.enabledSelf, "FortifyButton should be enabled when Fortified even with no actions");
        Assert.IsTrue(fortifyButton.ClassListContains("active"), "FortifyButton should be active when Fortified");
    }

    [UnityTest]
    public IEnumerator FortifyButton_Enabled_WhenHasActions()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var fortifyButton = root.Q<Button>("FortifyButton");
        
        // Get player unit with actions
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Ensure has actions
        if (testUnit.actionsLeft == 0)
        {
            testUnit.actionsLeft = testUnit.unit.movement;
            unitManager.UpdateUnit(testUnit.position, testUnit);
        }
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify rest button enabled
        Assert.IsTrue(fortifyButton.enabledSelf, "FortifyButton should be enabled when unit has actions");
    }

    #endregion
}

