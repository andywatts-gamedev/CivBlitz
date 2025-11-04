using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
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
}

