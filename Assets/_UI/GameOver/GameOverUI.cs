using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private UIDocument doc;
    private VisualElement container;
    private Label messageLabel;
    private Button restartButton;

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
        var playerWon = winner == Game.Instance.player.civilization;
        var message = playerWon ? "You Won!" : "You Lost!";
        Debug.Log($"[GameOverUI] Game Over! Winner: {winner}, Player won: {playerWon}, Message: {message}");
        if (messageLabel != null) messageLabel.text = message;
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
        SceneManager.LoadScene("Game");
    }
}

