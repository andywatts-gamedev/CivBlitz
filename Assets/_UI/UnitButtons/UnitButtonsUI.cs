using UnityEngine;
using UnityEngine.UIElements;

public class UnitButtonsUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    [SerializeField] private GameEvent onUnitMoved;
    [SerializeField] private GameEvent onMovesConsumed;
    [SerializeField] private GameEvent onUnitStateChanged;
    [SerializeField] private GameEvent onTurnChanged;
    
    private UIDocument doc;
    private VisualElement container;
    private Button moveButton, restButton;
    private Vector2Int? selectedTile;
    

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("UnitButtonsContainer");
        moveButton = root.Q<Button>("MoveButton");
        restButton = root.Q<Button>("RestButton");
        
        if (moveButton != null)
        {
            moveButton.clicked += HandleMoveClicked;
        }
        
        if (restButton != null)
        {
            restButton.clicked += HandleRestClicked;
        }

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
        
        // Subscribe to game events for button state updates
        if (onTurnChanged != null) onTurnChanged.Handler += UpdateButtonStates;
        if (onUnitMoved != null) onUnitMoved.Handler += UpdateButtonStates;
        if (onMovesConsumed != null) onMovesConsumed.Handler += UpdateButtonStates;
        if (onUnitStateChanged != null) onUnitStateChanged.Handler += UpdateButtonStates;
        
        // Start hidden
        container.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        if (moveButton != null)
        {
            moveButton.clicked -= HandleMoveClicked;
        }
        
        if (restButton != null)
        {
            restButton.clicked -= HandleRestClicked;
        }
        
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
        
        if (onTurnChanged != null) onTurnChanged.Handler -= UpdateButtonStates;
        if (onUnitMoved != null) onUnitMoved.Handler -= UpdateButtonStates;
        if (onMovesConsumed != null) onMovesConsumed.Handler -= UpdateButtonStates;
        if (onUnitStateChanged != null) onUnitStateChanged.Handler -= UpdateButtonStates;
    }

    private void UpdateButtonVisibility()
    {
        if (container == null) return;
        
        bool hasSelectedUnit = selectedTile.HasValue && 
                              UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit) &&
                              unit.civ == Game.Instance.player.civilization;
        
        container.style.display = hasSelectedUnit ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateButtonStates()
    {
        UpdateButtonVisibility();
        
        if (!selectedTile.HasValue) return;
        
        if (!UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit) ||
            unit.civ != Game.Instance.player.civilization)
        {
            return;
        }

        bool canMove = unit.state == UnitState.Ready && unit.actionsLeft > 0;
        bool canRest = unit.state == UnitState.Ready && unit.actionsLeft > 0;
        bool isResting = unit.state == UnitState.Resting;
        
        if (moveButton != null)
        {
            moveButton.SetEnabled(canMove);
            // Update action point label if it exists
            var actionLabel = moveButton.Q<Label>("ActionPointsLabel");
            if (actionLabel != null)
            {
                actionLabel.text = unit.actionsLeft.ToString();
            }
        }
        
        if (restButton != null)
        {
            restButton.SetEnabled(canRest);
            
            // Toggle active class based on unit state
            if (isResting)
            {
                restButton.AddToClassList("active");
            }
            else
            {
                restButton.RemoveFromClassList("active");
            }
        }
    }

    private void HandleMoveClicked()
    {
        if (!selectedTile.HasValue) return;
        
        if (UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit) &&
            unit.civ == Game.Instance.player.civilization &&
            unit.state == UnitState.Ready)
        {
            // Enter move mode - this will be handled by input managers
            events.EmitMoveModeStarted(selectedTile.Value);
        }
    }

    private void HandleRestClicked()
    {
        if (!selectedTile.HasValue) return;
        
        if (UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit) &&
            unit.civ == Game.Instance.player.civilization &&
            unit.state == UnitState.Ready)
        {
            UnitManager.Instance.SetUnitState(selectedTile.Value, UnitState.Resting);
        }
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        selectedTile = pos;
        UpdateButtonStates();
    }

    private void HandleCancel()
    {
        selectedTile = null;
        UpdateButtonStates();
    }

    private void HandleTileDeselected(Vector2Int pos)
    {
        selectedTile = null;
        UpdateButtonStates();
    }
}
