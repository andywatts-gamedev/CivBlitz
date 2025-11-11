using UnityEngine;
using UnityEngine.UIElements;

public class UnitButtonsUI : MonoBehaviour
{
    [SerializeField] private GameStateEvents gameStateEvents;
    [SerializeField] private GameEvent onUnitMoved;
    [SerializeField] private GameEvent onMovesConsumed;
    [SerializeField] private GameEvent onUnitStateChanged;
    [SerializeField] private GameEvent onTurnChanged;
    
    private UIDocument doc;
    private VisualElement container;
    private Button moveButton, fortifyButton;
    private Vector2Int? selectedTile;
    

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("UnitButtonsContainer");
        moveButton = root.Q<Button>("MoveButton");
        fortifyButton = root.Q<Button>("FortifyButton");
        
        if (moveButton != null)
        {
            moveButton.clicked += HandleMoveClicked;
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.clicked += HandleFortifyClicked;
        }

        gameStateEvents.OnTileSelected += HandleTileSelected;
        gameStateEvents.OnTileDeselected += HandleTileDeselected;
        
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
        
        if (fortifyButton != null)
        {
            fortifyButton.clicked -= HandleFortifyClicked;
        }
        
        gameStateEvents.OnTileSelected -= HandleTileSelected;
        gameStateEvents.OnTileDeselected -= HandleTileDeselected;
        
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
        bool isFortified = unit.state == UnitState.Fortified;
        bool canToggleFortify = (unit.state == UnitState.Ready && unit.actionsLeft > 0) || isFortified;
        
        if (moveButton != null)
        {
            moveButton.SetEnabled(canMove);
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.SetEnabled(canToggleFortify);
            
            // Toggle active class based on unit state
            if (isFortified)
            {
                if (!fortifyButton.ClassListContains("active"))
                {
                    fortifyButton.AddToClassList("active");
                }
            }
            else
            {
                if (fortifyButton.ClassListContains("active"))
                {
                    fortifyButton.RemoveFromClassList("active");
                }
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
            gameStateEvents.EmitMoveModeStarted(selectedTile.Value);
        }
    }

    private void HandleFortifyClicked()
    {
        if (!selectedTile.HasValue) return;
        
        if (UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit))
        {
            if (unit.civ == Game.Instance.player.civilization)
            {
                // Toggle fortified state
                var newState = unit.state == UnitState.Fortified ? UnitState.Ready : UnitState.Fortified;
                UnitManager.Instance.SetUnitState(selectedTile.Value, newState);
                UpdateButtonStates();
            }
        }
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        selectedTile = pos;
        UpdateButtonStates();
    }

    private void HandleTileDeselected(Vector2Int pos)
    {
        selectedTile = null;
        UpdateButtonStates();
    }
}
