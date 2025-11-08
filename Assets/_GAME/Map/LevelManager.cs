using UnityEngine;
using Sirenix.OdinInspector;

public class LevelManager : Singleton<LevelManager>
{
    [Title("Level Progression")]
    [SerializeField] private MapData[] levels;
    
    [ShowInInspector, ReadOnly]
    private int currentLevelIndex = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        // Subscribe to game over event
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(Civilization winner)
    {
        // Only progress if player won
        if (Game.Instance != null && winner == Game.Instance.player.civilization)
        {
            Debug.Log($"[LevelManager] Player won! Current level: {currentLevelIndex}");
            // Level progression will be triggered by player action (button click)
            // Don't auto-advance here
        }
    }

    public void LoadNextLevel()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("[LevelManager] No levels configured!");
            RestartCurrentLevel();
            return;
        }

        currentLevelIndex++;
        
        if (currentLevelIndex >= levels.Length)
        {
            Debug.Log("[LevelManager] Completed all levels! Restarting from beginning...");
            currentLevelIndex = 0;
        }

        LoadLevel(currentLevelIndex);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length)
        {
            Debug.LogError($"[LevelManager] Invalid level index: {index}");
            return;
        }

        currentLevelIndex = index;
        var mapData = levels[index];
        
        if (mapData == null)
        {
            Debug.LogError($"[LevelManager] Level {index} has null MapData!");
            return;
        }

        Debug.Log($"[LevelManager] Loading level {index}: {mapData.name}");

        // Clear game state
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.isPlayerTurn = true;
        }

        // Load the map
        if (MapLoader.Instance != null)
        {
            MapLoader.Instance.LoadMap(mapData);
        }
    }

    public MapData GetCurrentLevel()
    {
        if (levels == null || currentLevelIndex < 0 || currentLevelIndex >= levels.Length)
        {
            return null;
        }
        return levels[currentLevelIndex];
    }

    public int GetCurrentLevelIndex() => currentLevelIndex;
    public int GetTotalLevels() => levels?.Length ?? 0;
    public bool HasNextLevel() => currentLevelIndex < (levels?.Length ?? 0) - 1;
}

