using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private UIDocument doc;
    private VisualElement container;
    private Label messageLabel;
    private Button restartButton;
    private bool playerWon = false;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        
        container = root.Q("GameOverContainer");
        messageLabel = root.Q<Label>("GameOverMessage");
        restartButton = root.Q<Button>("RestartButton");
        
        if (restartButton != null)
        {
            restartButton.clicked += HandleRestart;
        }
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnGameOver += HandleGameOver;
            Debug.Log("[GameOverUI] Subscribed to OnGameOver event");
        }
        else
        {
            Debug.LogError("[GameOverUI] CombatManager.Instance is null!");
        }
        
        if (container != null)
        {
            container.style.display = DisplayStyle.None;
        }
    }

    void OnDisable()
    {
        if (restartButton != null)
        {
            restartButton.clicked -= HandleRestart;
        }
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(Civilization winner)
    {
        playerWon = winner == Game.Instance.player.civilization;
        var message = playerWon ? "You Won!" : "You Lost!";
        Debug.Log($"[GameOverUI] Game Over! Winner: {winner}, Player won: {playerWon}, Message: {message}");
        if (messageLabel != null) messageLabel.text = message;
        
        // Update button text based on outcome
        if (restartButton != null)
        {
            if (playerWon && LevelManager.Instance != null && LevelManager.Instance.HasNextLevel())
            {
                restartButton.text = "Next Level";
            }
            else if (playerWon)
            {
                restartButton.text = "Play Again";
            }
            else
            {
                restartButton.text = "Retry";
            }
        }
        
        if (container != null)
        {
            container.style.display = DisplayStyle.Flex;
            Debug.Log($"[GameOverUI] Container display set to Flex");
        }
        else
        {
            Debug.LogError("[GameOverUI] Container is null!");
        }
    }

    private void HandleRestart()
    {
        // Hide UI
        if (container != null)
        {
            container.style.display = DisplayStyle.None;
        }

        // If LevelManager exists, use it for progression
        if (LevelManager.Instance != null)
        {
            if (playerWon)
            {
                LevelManager.Instance.LoadNextLevel();
            }
            else
            {
                LevelManager.Instance.RestartCurrentLevel();
            }
        }
        else
        {
            // Fallback to scene reload if no LevelManager
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

