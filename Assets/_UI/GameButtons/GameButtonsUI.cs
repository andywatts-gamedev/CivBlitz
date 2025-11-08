using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GameButtonsUI : MonoBehaviour
{
    [SerializeField] private GameStateEvents gameStateEvents;
    [SerializeField] private GameEvent onUnitMoved;
    [SerializeField] private GameEvent onMovesConsumed;
    [SerializeField] private GameEvent onUnitStateChanged;
    [SerializeField] private GameEvent onTurnChanged;
    
    private UIDocument doc;
    private Button nextUnitButton;
    private Button nextTurnButton;
    private Button aiTurnButton;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        nextUnitButton = root.Q<Button>("NextUnitButton");
        nextTurnButton = root.Q<Button>("NextTurnButton");
        aiTurnButton = root.Q<Button>("AiTurnButton");
        
        if (nextUnitButton != null)
        {
            nextUnitButton.clicked += HandleNextUnitClicked;
        }
        else
        {
            Debug.LogError("GameButtonsUI: Next unit button not found!");
        }
        
        if (nextTurnButton != null)
        {
            nextTurnButton.clicked += HandleNextTurnClicked;
        }
        else
        {
            Debug.LogError("GameButtonsUI: Next turn button not found!");
        }
        
        if (aiTurnButton == null)
        {
            Debug.LogError("GameButtonsUI: AI turn button not found!");
        }

        gameStateEvents.OnTileSelected += HandleTileSelected;
        gameStateEvents.OnTileDeselected += HandleTileDeselected;
        
        // Subscribe to game events for button state updates
        if (onTurnChanged != null) onTurnChanged.Handler += UpdateButtonState;
        if (onUnitMoved != null) onUnitMoved.Handler += UpdateButtonState;
        if (onMovesConsumed != null) onMovesConsumed.Handler += UpdateButtonState;
        if (onUnitStateChanged != null) onUnitStateChanged.Handler += UpdateButtonState;
        
        UpdateButtonState();
    }

    void OnDisable()
    {
        if (nextUnitButton != null)
        {
            nextUnitButton.clicked -= HandleNextUnitClicked;
        }
        
        if (nextTurnButton != null)
        {
            nextTurnButton.clicked -= HandleNextTurnClicked;
        }
        
        gameStateEvents.OnTileSelected -= HandleTileSelected;
        gameStateEvents.OnTileDeselected -= HandleTileDeselected;
        
        if (onTurnChanged != null) onTurnChanged.Handler -= UpdateButtonState;
        if (onUnitMoved != null) onUnitMoved.Handler -= UpdateButtonState;
        if (onMovesConsumed != null) onMovesConsumed.Handler -= UpdateButtonState;
        if (onUnitStateChanged != null) onUnitStateChanged.Handler -= UpdateButtonState;
    }


    public void UpdateButtonState()
    {
        if (nextUnitButton == null || nextTurnButton == null || aiTurnButton == null) return;
        
        var turnManager = TurnManager.Instance;
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        bool hasReadyUnits = nextReadyUnit != null;
        bool isAITurn = !turnManager.isPlayerTurn;
        
        Debug.Log($"[GameButtonsUI] UpdateButtonState called - isAITurn: {isAITurn}, hasReadyUnits: {hasReadyUnits}");
        
        if (isAITurn)
        {
            nextUnitButton.style.display = DisplayStyle.None;
            nextTurnButton.style.display = DisplayStyle.None;
            aiTurnButton.style.display = DisplayStyle.Flex;
            Debug.Log($"[GameButtonsUI] AI Turn - showing spinner");
        }
        else if (hasReadyUnits)
        {
            nextUnitButton.style.display = DisplayStyle.Flex;
            nextTurnButton.style.display = DisplayStyle.None;
            aiTurnButton.style.display = DisplayStyle.None;
            Debug.Log($"[GameButtonsUI] Player Turn - showing next unit button");
        }
        else
        {
            nextUnitButton.style.display = DisplayStyle.None;
            nextTurnButton.style.display = DisplayStyle.Flex;
            aiTurnButton.style.display = DisplayStyle.None;
            Debug.Log($"[GameButtonsUI] Player Turn - showing next turn button");
        }
    }

    private void HandleNextUnitClicked()
    {
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        
        if (nextReadyUnit != null)
        {
            // Select next ready unit
            selectedTile = nextReadyUnit.position;
            gameStateEvents.EmitTileSelected(nextReadyUnit.position);
        }
    }

    private void HandleNextTurnClicked()
    {
        if (selectedTile.HasValue)
        {
            gameStateEvents.EmitTileDeselected(selectedTile.Value);
        }
        TurnManager.Instance.EndTurn();
        TurnManager.Instance.StartAITurn();
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        selectedTile = pos;
    }

    private void HandleTileDeselected(Vector2Int pos)
    {
        selectedTile = null;
    }
}
