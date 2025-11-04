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
    private Label buttonIcon;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        nextUnitButton = root.Q<Button>("NextUnitButton");
        nextTurnButton = root.Q<Button>("NextTurnButton");
        buttonIcon = nextTurnButton?.Q<Label>();
        
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
        if (nextUnitButton == null || nextTurnButton == null) return;
        
        var turnManager = TurnManager.Instance;
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        bool hasReadyUnits = nextReadyUnit != null;
        bool isAITurn = !turnManager.isPlayerTurn;
        
        // Show spinner during AI turn
        if (isAITurn)
        {
            nextUnitButton.style.display = DisplayStyle.None;
            nextTurnButton.style.display = DisplayStyle.Flex;
            nextTurnButton.text = "\uf110"; // Spinner icon
        }
        else
        {
            // Show/hide buttons based on game state
            nextUnitButton.style.display = hasReadyUnits ? DisplayStyle.Flex : DisplayStyle.None;
            nextTurnButton.style.display = hasReadyUnits ? DisplayStyle.None : DisplayStyle.Flex;
            nextTurnButton.text = "\uf2f9"; // Rotate icon
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
