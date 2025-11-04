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
        
        CombatManager.Instance.OnGameOver += HandleGameOver;
        
        container.style.display = DisplayStyle.None;
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
        messageLabel.text = playerWon ? "You Won!" : "You Lost!";
        container.style.display = DisplayStyle.Flex;
    }

    private void HandleRestart()
    {
        SceneManager.LoadScene("Game");
    }
}

