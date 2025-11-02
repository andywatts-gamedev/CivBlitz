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
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
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
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
        yield return null;
        
        // Ensure unit is ready
        unitManager.SetUnitState(testUnit.position, UnitState.Ready);
        yield return null;
        
        // Verify button does NOT have active class
        Assert.IsFalse(restButton.ClassListContains("active"), "RestButton should NOT have 'active' class when unit is ready");
    }

    #endregion

    #region Action Point Display Tests

    [UnityTest]
    public IEnumerator MoveButton_DisplaysActionPoints_ForSelectedUnit()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var moveButton = root.Q<Button>("MoveButton");
        var actionLabel = moveButton?.Q<Label>("ActionPointsLabel");
        Assert.IsNotNull(actionLabel, "ActionPointsLabel not found on MoveButton");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Select unit
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify label shows correct action points
        Assert.AreEqual(testUnit.actionsLeft.ToString(), actionLabel.text, "Action points label should match unit actionsLeft");
    }

    [UnityTest]
    public IEnumerator MoveButton_ShowsZero_WhenNoActionsLeft()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var moveButton = root.Q<Button>("MoveButton");
        var actionLabel = moveButton?.Q<Label>("ActionPointsLabel");
        
        // Get player unit and deplete actions
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        testUnit.actionsLeft = 0;
        unitManager.UpdateUnit(testUnit.position, testUnit);
        
        // Select unit
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify label shows 0
        Assert.AreEqual("0", actionLabel.text);
        
        // Verify button is grayed out (disabled)
        Assert.IsFalse(moveButton.enabledSelf, "MoveButton should be disabled when no actions left");
    }

    [UnityTest]
    public IEnumerator MoveButton_UpdatesActionPoints_AfterMove()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var moveButton = root.Q<Button>("MoveButton");
        var actionLabel = moveButton?.Q<Label>("ActionPointsLabel");
        
        // Get player unit
        var playerCiv = game.player.civilization;
        var playerUnits = unitManager.civUnits[playerCiv];
        var testUnit = playerUnits[0];
        
        // Ensure unit has actions
        if (testUnit.actionsLeft == 0)
        {
            testUnit.actionsLeft = testUnit.unit.movement;
            unitManager.UpdateUnit(testUnit.position, testUnit);
        }
        
        var initialActions = testUnit.actionsLeft;
        
        // Select unit
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
        yield return null;
        
        // Move unit - find a valid move that passes all checks
        var validMoves = HexGrid.GetValidMoves(testUnit.position, testUnit.unit.movement, unitManager.unitTilemap);
        Assert.IsTrue(validMoves.Count > 0, "Unit should have moves in range");
        
        var from = testUnit.position;
        Vector2Int? to = null;
        
        // Find first move that's actually valid (terrain, etc)
        foreach (var move in validMoves)
        {
            if (game.IsValidMove(from, move))
            {
                to = move;
                break;
            }
        }
        
        Assert.IsTrue(to.HasValue, "Unit should have at least one valid move");
        var destination = to.Value;
        
        game.MoveTo(from, destination);
        yield return null;
        
        // Wait for move animation to complete
        while (unitManager.isMoving)
        {
            yield return null;
        }
        yield return null;
        
        // Verify unit moved and action points decreased in data
        Assert.IsTrue(unitManager.TryGetUnit(destination, out var movedUnit), "Unit should be at new position");
        Assert.AreEqual(initialActions - 1, movedUnit.actionsLeft, "Unit actionsLeft should decrease by 1");
        
        // Re-select the unit to update UI
        events[0].EmitTileSelected(destination);
        yield return null;
        
        // Verify UI label updated
        var expectedActions = (initialActions - 1).ToString();
        Assert.AreEqual(expectedActions, actionLabel.text, "Action points label should match unit actionsLeft");
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
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
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
        var events = Resources.FindObjectsOfTypeAll<InputEvents>();
        events[0].EmitTileSelected(testUnit.position);
        yield return null;
        
        // Verify rest button enabled
        Assert.IsTrue(restButton.enabledSelf, "RestButton should be enabled when unit has actions");
    }

    #endregion
}

