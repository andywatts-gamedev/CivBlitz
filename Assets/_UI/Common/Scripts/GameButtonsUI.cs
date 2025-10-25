using UnityEngine;
using UnityEngine.UIElements;

public class GameButtonsUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Button turnButton;
    private Vector2Int? selectedTile;
    
    // Font Awesome glyphs
    private const string NEXT_UNIT_GLYPH = "\uf04e";    // play
    private const string NEXT_TURN_GLYPH = "\uf050";    // forward

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        turnButton = root.Q<Button>("TurnButton");
        if (turnButton != null)
        {
            turnButton.clicked += HandleTurnButtonClicked;
        }

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
        
        UpdateButtonState();
    }

    void OnDisable()
    {
        if (turnButton != null)
        {
            turnButton.clicked -= HandleTurnButtonClicked;
        }
        
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
    }

    void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (turnButton == null) return;
        
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        bool hasReadyUnits = nextReadyUnit != null;
        
        if (hasReadyUnits)
        {
            turnButton.text = NEXT_UNIT_GLYPH;
            turnButton.SetEnabled(true);
        }
        else
        {
            turnButton.text = NEXT_TURN_GLYPH;
            turnButton.SetEnabled(true);
        }
    }

    private void HandleTurnButtonClicked()
    {
        var nextReadyUnit = UnitManager.Instance.GetNextReadyUnit();
        
        if (nextReadyUnit != null)
        {
            // Select next ready unit
            selectedTile = nextReadyUnit.position;
            events.EmitTileSelected(nextReadyUnit.position);
        }
        else
        {
            // End turn
            TurnManager.Instance.EndTurn();
        }
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
