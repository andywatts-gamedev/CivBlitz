using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GameButtonsUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Button nextUnitButton;
    private Button nextTurnButton;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        nextUnitButton = root.Q<Button>("NextUnitButton");
        nextTurnButton = root.Q<Button>("NextTurnButton");
        
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

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
        
        // Subscribe to game events for button state updates
        TurnManager.Instance.OnTurnChanged += UpdateButtonState;
        UnitManager.Instance.OnUnitMoved += UpdateButtonState;
        UnitManager.Instance.OnMovesConsumed += UpdateButtonState;
        
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
        
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
        
        // Unsubscribe from game events
        TurnManager.Instance.OnTurnChanged -= UpdateButtonState;
        UnitManager.Instance.OnUnitMoved -= UpdateButtonState;
        UnitManager.Instance.OnMovesConsumed -= UpdateButtonState;
    }


    private void UpdateButtonState()
    {
        if (nextUnitButton == null || nextTurnButton == null) return;
        
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        bool hasReadyUnits = nextReadyUnit != null;
        
        // Show/hide buttons based on game state
        nextUnitButton.style.display = hasReadyUnits ? DisplayStyle.Flex : DisplayStyle.None;
        nextTurnButton.style.display = hasReadyUnits ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void HandleNextUnitClicked()
    {
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        
        if (nextReadyUnit != null)
        {
            // Select next ready unit
            selectedTile = nextReadyUnit.position;
            events.EmitTileSelected(nextReadyUnit.position);
        }
    }

    private void HandleNextTurnClicked()
    {
        TurnManager.Instance.StartAITurn();
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        selectedTile = pos;
    }

    private void HandleCancel()
    {
        selectedTile = null;
    }

    private void HandleTileDeselected(Vector2Int pos)
    {
        selectedTile = null;
    }
}
