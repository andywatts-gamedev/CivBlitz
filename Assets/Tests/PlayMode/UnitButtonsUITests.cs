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
    public IEnumerator RestButton_ShowsActiveState_WhenUnitResting()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var restButton = root.Q<Button>("RestButton");
        Assert.IsNotNull(restButton, "RestButton not found");
        
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
        Assert.AreEqual(DisplayStyle.Flex, restButton.parent.style.display.value, "Unit buttons should be visible");
        
        // Click rest button
        var unitButtonsUI = GameObject.Find("UI").GetComponent<UnitButtonsUI>();
        unitManager.SetUnitState(testUnit.position, UnitState.Resting);
        yield return null;
        
        // Verify button has active class
        Assert.IsTrue(restButton.ClassListContains("active"), "RestButton should have 'active' class when unit is resting");
    }

    [UnityTest]
    public IEnumerator RestButton_ShowsDefaultState_WhenUnitReady()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var restButton = root.Q<Button>("RestButton");
        Assert.IsNotNull(restButton);
        
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
        Assert.IsFalse(restButton.ClassListContains("active"), "RestButton should NOT have 'active' class when unit is ready");
    }

    [UnityTest]
    public IEnumerator RestButton_ActiveState_Persists_WhenReselectingUnit()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var restButton = root.Q<Button>("RestButton");
        Assert.IsNotNull(restButton, "RestButton not found");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit and set to resting
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        unitManager.SetUnitState(testUnit.position, UnitState.Resting);
        yield return null;
        
        Assert.IsTrue(restButton.ClassListContains("active"), "RestButton should have 'active' class after setting to Resting");
        
        // Deselect unit
        gameStateEvents.EmitTileDeselected(testUnit.position);
        yield return null;
        
        // Reselect unit
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify active state persists
        Assert.IsTrue(restButton.ClassListContains("active"), "RestButton should still have 'active' class after reselecting");
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
        
        var restButton = root.Q<Button>("RestButton");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit and set to resting
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        unitManager.SetUnitState(testUnit.position, UnitState.Resting);
        yield return null;
        
        Assert.IsTrue(restButton.ClassListContains("active"), "RestButton should be active before turn ends");
        
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
        
        // Reselect unit - should no longer be resting
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify unit is back to Ready state
        Assert.IsTrue(unitManager.TryGetUnit(testUnit.position, out var updatedUnit), "Unit should exist");
        Assert.AreEqual(UnitState.Ready, updatedUnit.state, "Unit state should be Ready after new turn");
        Assert.IsFalse(restButton.ClassListContains("active"), "RestButton should NOT be active after new turn");
    }

    #endregion

    #region Rest Button Disabled Tests

    [UnityTest]
    public IEnumerator RestButton_Disabled_WhenNoActionsLeft()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var restButton = root.Q<Button>("RestButton");
        
        // Get player unit and deplete actions
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        testUnit.actionsLeft = 0;
        unitManager.UpdateUnit(testUnit.position, testUnit);
        
        // Select unit
        var gameStateEvents = Resources.FindObjectsOfTypeAll<GameStateEvents>()[0];
        gameStateEvents.EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify rest button disabled
        Assert.IsFalse(restButton.enabledSelf, "RestButton should be disabled when no actions left");
    }

    [UnityTest]
    public IEnumerator RestButton_Enabled_WhenHasActions()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var restButton = root.Q<Button>("RestButton");
        
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
        Assert.IsTrue(restButton.enabledSelf, "RestButton should be enabled when unit has actions");
    }

    #endregion
}

