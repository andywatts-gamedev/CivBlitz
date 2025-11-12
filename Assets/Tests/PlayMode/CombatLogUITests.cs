using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class CombatLogUITests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    [UnityTest]
    public IEnumerator CombatLog_ShowsTurnIndicatorAfterTurnChange()
    {
        yield return null;
        
        var combatManager = CombatManager.Instance;
        var turnManager = TurnManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var container = root.Q("CombatLogContainer");
        var entriesContainer = container.Q("CombatEntries");
        
        Assert.IsNotNull(container, "CombatLogContainer should exist");
        Assert.IsNotNull(entriesContainer, "CombatEntries should exist");
        
        // Simulate combat by manually invoking the event
        var playerCiv = game.player.civilization;
        var aiCiv = game.ai.civilization;
        var combatEvent = new CombatEvent {
            attackerName = "Warrior",
            attackerPos = new Vector2Int(0, 0),
            attackerCiv = playerCiv,
            attackerStrength = 20,
            attackerHealthBefore = 100,
            attackerHealthAfter = 80,
            isRanged = false,
            defenderName = "Archer",
            defenderPos = new Vector2Int(1, 0),
            defenderCiv = aiCiv,
            defenderStrength = 15,
            defenderHealthBefore = 100,
            defenderHealthAfter = 70,
            strengthDiff = 5,
            attackDamage = 30,
            retaliationDamage = 20,
            defenderKilled = false,
            attackerKilled = false
        };
        
        combatManager.TriggerCombatResolvedForTest(combatEvent);
        yield return null;
        
        var entryCountBefore = entriesContainer.childCount;
        Assert.Greater(entryCountBefore, 0, "Combat log should have entries after combat");
        
        // End turn
        turnManager.EndTurn();
        yield return null;
        
        // Log should persist
        Assert.AreEqual(entryCountBefore, entriesContainer.childCount, "Combat log should persist after turn change");
        Assert.AreEqual(DisplayStyle.Flex, container.style.display.value, "Combat log should remain visible");
        
        // Trigger another combat
        combatManager.TriggerCombatResolvedForTest(combatEvent);
        yield return null;
        
        // Check that new entry has new-turn-entry class
        var lastEntry = entriesContainer[entriesContainer.childCount - 1];
        Assert.IsTrue(lastEntry.ClassListContains("new-turn-entry"), "New combat entry after turn change should have new-turn-entry class");
    }

    [UnityTest]
    public IEnumerator CombatLog_ShowsWhenCombatOccurs()
    {
        yield return null;
        
        var combatManager = CombatManager.Instance;
        var game = Game.Instance;
        var doc = GameObject.Find("UI").GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var container = root.Q("CombatLogContainer");
        
        Assert.IsNotNull(container, "CombatLogContainer should exist");
        
        // Combat log should start hidden
        Assert.AreEqual(DisplayStyle.None, container.style.display.value, "Combat log should start hidden");
        
        // Simulate combat
        var playerCiv = game.player.civilization;
        var aiCiv = game.ai.civilization;
        var combatEvent = new CombatEvent {
            attackerName = "Warrior",
            attackerPos = new Vector2Int(0, 0),
            attackerCiv = playerCiv,
            attackerStrength = 20,
            attackerHealthBefore = 100,
            attackerHealthAfter = 100,
            isRanged = false,
            defenderName = "Archer",
            defenderPos = new Vector2Int(1, 0),
            defenderCiv = aiCiv,
            defenderStrength = 15,
            defenderHealthBefore = 100,
            defenderHealthAfter = 70,
            strengthDiff = 5,
            attackDamage = 30,
            retaliationDamage = 0,
            defenderKilled = false,
            attackerKilled = false
        };
        
        combatManager.TriggerCombatResolvedForTest(combatEvent);
        yield return null;
        
        Assert.AreEqual(DisplayStyle.Flex, container.style.display.value, "Combat log should be visible after combat");
    }
}


