using UnityEngine;
using UnityEngine.UIElements;

public class UnitButtonsUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container;
    private Button restButton, fortifyButton;
    private Vector2Int? selectedTile;
    

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("UnitButtonsContainer");
        restButton = root.Q<Button>("RestButton");
        fortifyButton = root.Q<Button>("FortifyButton");
        
        if (restButton != null)
        {
            restButton.clicked += HandleRestClicked;
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.clicked += HandleFortifyClicked;
        }

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
        
        // Subscribe to game events for button state updates
        TurnManager.Instance.OnTurnChanged += UpdateButtonStates;
        UnitManager.Instance.OnUnitMoved += UpdateButtonStates;
        UnitManager.Instance.OnMovesConsumed += UpdateButtonStates;
        UnitManager.Instance.OnUnitStateChanged += UpdateButtonStates;
        
        // Start hidden
        container.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        if (restButton != null)
        {
            restButton.clicked -= HandleRestClicked;
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.clicked -= HandleFortifyClicked;
        }
        
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
        
        // Unsubscribe from game events
        TurnManager.Instance.OnTurnChanged -= UpdateButtonStates;
        UnitManager.Instance.OnUnitMoved -= UpdateButtonStates;
        UnitManager.Instance.OnMovesConsumed -= UpdateButtonStates;
        UnitManager.Instance.OnUnitStateChanged -= UpdateButtonStates;
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

        bool canRest = unit.state == UnitState.Ready && unit.movesLeft > 0;
        bool canFortify = unit.state == UnitState.Ready && unit.movesLeft > 0;
        
        if (restButton != null)
        {
            restButton.SetEnabled(canRest);
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.SetEnabled(canFortify);
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

    private void HandleFortifyClicked()
    {
        if (!selectedTile.HasValue) return;
        
        if (UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit) &&
            unit.civ == Game.Instance.player.civilization &&
            unit.state == UnitState.Ready)
        {
            UnitManager.Instance.SetUnitState(selectedTile.Value, UnitState.Fortified);
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
