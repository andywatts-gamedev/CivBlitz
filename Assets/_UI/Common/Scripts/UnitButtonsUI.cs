using UnityEngine;
using UnityEngine.UIElements;

public class UnitButtonsUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container;
    private Button restButton, fortifyButton;
    private Vector2Int? selectedTile;
    
    // Font Awesome glyphs
    private const string REST_GLYPH = "\uf236";      // bed
    private const string FORTIFY_GLYPH = "\uf3ed";   // shield

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("UnitButtonsContainer");
        restButton = root.Q<Button>("RestButton");
        fortifyButton = root.Q<Button>("FortifyButton");
        
        if (restButton != null)
        {
            restButton.text = REST_GLYPH;
            restButton.clicked += HandleRestClicked;
        }
        
        if (fortifyButton != null)
        {
            fortifyButton.text = FORTIFY_GLYPH;
            fortifyButton.clicked += HandleFortifyClicked;
        }

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
        
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
    }

    void Update()
    {
        UpdateButtonStates();
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
            Debug.Log($"Unit {unit.unit.name} is now resting");
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
            Debug.Log($"Unit {unit.unit.name} is now fortified");
        }
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        selectedTile = pos;
        UpdateButtonVisibility();
    }

    private void HandleCancel()
    {
        selectedTile = null;
        UpdateButtonVisibility();
    }

    private void HandleTileDeselected(Vector2Int pos)
    {
        selectedTile = null;
        UpdateButtonVisibility();
    }
}
