using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[TestFixture]
public class GameOverTests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.LoadScene("Game");
    }

    [UnityTest]
    public IEnumerator GameOver_TriggersWhenPlayerHasNoUnits()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        
        // Remove all player units
        var playerCiv = game.player.civilization;
        var playerUnits = new List<UnitInstance>(unitManager.civUnits[playerCiv]);
        foreach (var unit in playerUnits)
        {
            unitManager.RemoveUnit(unit.position);
        }
        
        var winner = unitManager.CheckForGameOver();
        Assert.IsNotNull(winner, "Winner should be determined");
        Assert.AreEqual(game.ai.civilization, winner.Value, "AI should win when player has no units");
    }

    [UnityTest]
    public IEnumerator GameOver_TriggersWhenAIHasNoUnits()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        
        // Remove all AI units
        var aiCiv = game.ai.civilization;
        var aiUnits = new List<UnitInstance>(unitManager.civUnits[aiCiv]);
        foreach (var unit in aiUnits)
        {
            unitManager.RemoveUnit(unit.position);
        }
        
        var winner = unitManager.CheckForGameOver();
        Assert.IsNotNull(winner, "Winner should be determined");
        Assert.AreEqual(game.player.civilization, winner.Value, "Player should win when AI has no units");
    }

    [UnityTest]
    public IEnumerator GameOver_NoWinnerWhenBothHaveUnits()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var winner = unitManager.CheckForGameOver();
        
        Assert.IsNull(winner, "No winner when both sides have units");
    }

    [UnityTest]
    public IEnumerator GameOver_UIDisplaysWhenWinnerDetermined()
    {
        yield return null;
        
        var unitManager = UnitManager.Instance;
        var game = Game.Instance;
        var combatManager = CombatManager.Instance;
        var gameOverUI = GameObject.Find("UI").GetComponent<GameOverUI>();
        var doc = gameOverUI.GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var container = root.Q("GameOverContainer");
        
        Assert.IsNotNull(gameOverUI, "GameOverUI should exist");
        Assert.IsNotNull(container, "GameOverContainer should exist");
        Assert.AreEqual(DisplayStyle.None, container.style.display.value, "GameOver UI should start hidden");
        
        // Remove all AI units to trigger win
        var aiCiv = game.ai.civilization;
        var aiUnits = new List<UnitInstance>(unitManager.civUnits[aiCiv]);
        foreach (var unit in aiUnits)
        {
            unitManager.RemoveUnit(unit.position);
        }
        
        // Manually invoke game over event
        var winner = unitManager.CheckForGameOver();
        Assert.IsNotNull(winner, "Winner should be determined");
        combatManager.TriggerGameOverForTest(winner.Value);
        
        yield return null;
        
        Assert.AreEqual(DisplayStyle.Flex, container.style.display.value, "GameOver UI should be visible after win");
    }
}

