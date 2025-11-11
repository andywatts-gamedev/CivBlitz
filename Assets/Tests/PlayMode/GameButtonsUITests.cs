using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class GameButtonsUITests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    [UnityTest]
    public IEnumerator NextTurnButton_ShowsWhenNoMovesLeft()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var nextUnitButton = root.Q<Button>("NextUnitButton");
        var nextTurnButton = root.Q<Button>("NextTurnButton");
        
        Assert.IsNotNull(nextUnitButton);
        Assert.IsNotNull(nextTurnButton);
        
        // Consume all player unit moves
        var playerCiv = Game.Instance.player.civilization;
        if (unitManager.civUnits.ContainsKey(playerCiv))
        {
            var units = new List<UnitInstance>(unitManager.civUnits[playerCiv]);
            foreach (var unit in units)
            {
                unit.actionsLeft = 0;
                unitManager.UpdateUnit(unit.position, unit);
            }
        }
        
        // Manually trigger UI update
        var gameButtonsUI = GameObject.Find("UI").GetComponent<GameButtonsUI>();
        gameButtonsUI.UpdateButtonState();
        yield return null;
        
        // NextTurnButton should show, NextUnitButton should hide
        Assert.AreEqual(DisplayStyle.None, nextUnitButton.style.display.value);
        Assert.AreEqual(DisplayStyle.Flex, nextTurnButton.style.display.value);
    }

    [UnityTest]
    public IEnumerator NextTurnButton_ShowsWhenAllUnitsFortified()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var nextUnitButton = root.Q<Button>("NextUnitButton");
        var nextTurnButton = root.Q<Button>("NextTurnButton");
        
        Assert.IsNotNull(nextUnitButton);
        Assert.IsNotNull(nextTurnButton);
        
        // Set all player units to Fortified state
        var playerCiv = Game.Instance.player.civilization;
        if (unitManager.civUnits.ContainsKey(playerCiv))
        {
            var units = new List<UnitInstance>(unitManager.civUnits[playerCiv]);
            foreach (var unit in units)
            {
                unitManager.SetUnitState(unit.position, UnitState.Fortified);
            }
        }
        
        yield return null;
        
        // NextTurnButton should show, NextUnitButton should hide
        Assert.AreEqual(DisplayStyle.None, nextUnitButton.style.display.value, 
            "NextUnitButton should be hidden when all units are resting");
        Assert.AreEqual(DisplayStyle.Flex, nextTurnButton.style.display.value,
            "NextTurnButton should be visible when all units are resting");
    }

    [UnityTest]
    public IEnumerator NextTurnButton_ChangesToSpinner_DuringAITurn()
    {
        yield return null;
        
        var turnManager = TurnManager.Instance;
        var unitManager = UnitManager.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        var nextUnitButton = root.Q<Button>("NextUnitButton");
        var nextTurnButton = root.Q<Button>("NextTurnButton");
        var aiTurnButton = root.Q<Button>("AiTurnButton");
        
        Assert.IsNotNull(nextUnitButton);
        Assert.IsNotNull(nextTurnButton);
        Assert.IsNotNull(aiTurnButton);
        
        // Consume all player unit moves
        var playerCiv = Game.Instance.player.civilization;
        if (unitManager.civUnits.ContainsKey(playerCiv))
        {
            var units = new List<UnitInstance>(unitManager.civUnits[playerCiv]);
            foreach (var unit in units)
            {
                unit.actionsLeft = 0;
                unitManager.UpdateUnit(unit.position, unit);
            }
        }
        
        // Manually trigger UI update
        var gameButtonsUI = GameObject.Find("UI").GetComponent<GameButtonsUI>();
        gameButtonsUI.UpdateButtonState();
        yield return null;
        
        // End player turn, then start AI turn
        turnManager.EndTurn();
        yield return null;
        
        turnManager.StartAITurn();
        yield return null;
        
        // During AI turn: should show spinner
        Assert.AreEqual(DisplayStyle.None, nextUnitButton.style.display.value);
        Assert.AreEqual(DisplayStyle.None, nextTurnButton.style.display.value);
        Assert.AreEqual(DisplayStyle.Flex, aiTurnButton.style.display.value);
        
        // Wait for AI turn to complete
        while (!turnManager.isPlayerTurn)
        {
            yield return null;
        }
        
        // After AI turn: should show next unit button
        Assert.AreEqual(DisplayStyle.Flex, nextUnitButton.style.display.value);
        Assert.AreEqual(DisplayStyle.None, nextTurnButton.style.display.value);
        Assert.AreEqual(DisplayStyle.None, aiTurnButton.style.display.value);
    }
}
