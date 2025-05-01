using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label selectedName, selectedHealth, selectedMovement, selectedRange, selectedMelee, selectedRanged;
    private Label selectedHealthIcon, selectedMovementIcon, selectedRangeIcon, selectedMeleeIcon, selectedRangedIcon;
    private Label hoverName, hoverHealth, hoverMovement, hoverRange, hoverMelee, hoverRanged;
    private Label hoverHealthIcon, hoverMovementIcon, hoverRangeIcon, hoverMeleeIcon, hoverRangedIcon;
    private Image playerCivIcon, aiCivIcon;
    private Label selectedTerrain, selectedMovementCost;
    private Label hoverTerrain, hoverMovementCost;
    private VisualElement selectedPanel, hoverPanel;
    private Button endTurnButton;
    private Vector2Int? selectedTile, hoveredTile;

    private const char HEALTH = '';
    private const char SWORD = '';
    private const char WALK = '';
    private const char BOW = '\u26b2';

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        selectedPanel = root.Q("SelectedPanel");
        hoverPanel = root.Q("HoverPanel");
        
        // Get all UI elements
        selectedName = selectedPanel.Q<Label>("SelectedName"); 
        selectedHealth = selectedPanel.Q<Label>("SelectedHealth");
        selectedMovement = selectedPanel.Q<Label>("SelectedMovement");
        selectedRange = selectedPanel.Q<Label>("SelectedRange");
        selectedMelee = selectedPanel.Q<Label>("SelectedMelee");
        selectedRanged = selectedPanel.Q<Label>("SelectedRanged");
        selectedHealthIcon = selectedPanel.Q<Label>("SelectedHealthIcon");
        selectedMovementIcon = selectedPanel.Q<Label>("SelectedMovementIcon");
        selectedRangeIcon = selectedPanel.Q<Label>("SelectedRangeIcon");
        selectedMeleeIcon = selectedPanel.Q<Label>("SelectedMeleeIcon");
        selectedRangedIcon = selectedPanel.Q<Label>("SelectedRangedIcon");
        selectedTerrain = selectedPanel.Q<Label>("SelectedTerrain");
        selectedMovementCost = selectedPanel.Q<Label>("SelectedMovementCost");

        hoverName = hoverPanel.Q<Label>("HoverName"); 
        hoverHealth = hoverPanel.Q<Label>("HoverHealth");
        hoverMovement = hoverPanel.Q<Label>("HoverMovement");
        hoverRange = hoverPanel.Q<Label>("HoverRange");
        hoverMelee = hoverPanel.Q<Label>("HoverMelee");
        hoverRanged = hoverPanel.Q<Label>("HoverRanged");
        hoverHealthIcon = hoverPanel.Q<Label>("HoverHealthIcon");
        hoverMovementIcon = hoverPanel.Q<Label>("HoverMovementIcon");
        hoverRangeIcon = hoverPanel.Q<Label>("HoverRangeIcon");
        hoverMeleeIcon = hoverPanel.Q<Label>("HoverMeleeIcon");
        hoverRangedIcon = hoverPanel.Q<Label>("HoverRangedIcon");
        hoverTerrain = hoverPanel.Q<Label>("HoverTerrain");
        hoverMovementCost = hoverPanel.Q<Label>("HoverMovementCost");

        playerCivIcon = root.Q<Image>("PlayerCivIcon");
        aiCivIcon = root.Q<Image>("AICivIcon");
        endTurnButton = root.Q<Button>("EndTurn") ?? CreateEndTurnButton(root);

        selectedPanel.style.display = DisplayStyle.None;
        hoverPanel.style.display = DisplayStyle.None;
        
        // Subscribe to events
        events.OnTileSelected += HandleTileSelected;
        events.OnTileHovered += HandleTileHovered;
        events.OnMouseMovedToTile += HandleMouseMovedToTile;
        events.OnCancel += HandleCancel;
        events.OnTileDeselected += HandleTileDeselected;
        TurnManager.Instance.OnTurnChanged += UpdateTurnLabels;
        
        UpdateTurnLabels();
    }

    void OnDisable() 
    {
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileHovered -= HandleTileHovered;
        events.OnMouseMovedToTile -= HandleMouseMovedToTile;
        events.OnCancel -= HandleCancel;
        events.OnTileDeselected -= HandleTileDeselected;
        TurnManager.Instance.OnTurnChanged -= UpdateTurnLabels;
    }

    private Button CreateEndTurnButton(VisualElement root)
    {
        var btn = new Button(() => TurnManager.Instance.EndTurn()) { text = "End Turn" };
        root.Add(btn);
        return btn;
    }

    private void ShowSelectedTile(Vector2Int pos)
    {
        selectedTile = pos;
        selectedPanel.style.display = DisplayStyle.Flex;
        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        var terrain = tile.terrainScob.terrain;
        selectedTerrain.text = terrain.name;
        selectedMovementCost.text = terrain.movementCost.ToString();
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            SetSelectedLabelsVisible(false);
            return;
        }

        UpdateSelectedPanel(unit);
        SetSelectedLabelsVisible(true, unit.unit.ranged > 0);
    }

    private void ShowHoveredTile(Vector2Int pos)
    {
        hoveredTile = pos;
        hoverPanel.style.display = DisplayStyle.Flex;
        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        var terrain = tile.terrainScob.terrain;
        hoverTerrain.text = terrain.name;
        hoverMovementCost.text = terrain.movementCost.ToString();
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            SetHoverLabelsVisible(false);
            return;
        }

        UpdateHoverPanel(unit);
        SetHoverLabelsVisible(true, unit.unit.ranged > 0);
    }

    private void SetSelectedLabelsVisible(bool visible, bool hasRanged = false)
    {
        var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        selectedName.style.display = display;
        selectedHealth.style.display = display;
        selectedHealthIcon.style.display = display;
        selectedMovement.style.display = display;
        selectedMovementIcon.style.display = display;
        selectedMelee.style.display = display;
        selectedMeleeIcon.style.display = display;
        selectedRanged.style.display = hasRanged ? display : DisplayStyle.None;
        selectedRangedIcon.style.display = hasRanged ? display : DisplayStyle.None;
        selectedRange.style.display = hasRanged ? display : DisplayStyle.None;
        selectedRangeIcon.style.display = hasRanged ? display : DisplayStyle.None;
    }

    private void SetHoverLabelsVisible(bool visible, bool hasRanged = false)
    {
        var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        hoverName.style.display = display;
        hoverHealth.style.display = display;
        hoverHealthIcon.style.display = display;
        hoverMovement.style.display = display;
        hoverMovementIcon.style.display = display;
        hoverMelee.style.display = display;
        hoverMeleeIcon.style.display = display;
        hoverRanged.style.display = hasRanged ? display : DisplayStyle.None;
        hoverRangedIcon.style.display = hasRanged ? display : DisplayStyle.None;
        hoverRange.style.display = hasRanged ? display : DisplayStyle.None;
        hoverRangeIcon.style.display = hasRanged ? display : DisplayStyle.None;
    }

    private void UpdateSelectedPanel(UnitInstance unit)
    {
        selectedName.text = unit.unit.name;
        selectedHealthIcon.text = HEALTH.ToString();
        selectedHealth.text = unit.health.ToString();
        selectedMovementIcon.text = WALK.ToString();
        selectedMovement.text = $"{unit.movesLeft}/{unit.unit.movement}";
        selectedMeleeIcon.text = SWORD.ToString();
        selectedMelee.text = unit.unit.melee.ToString();
        selectedRangedIcon.text = BOW.ToString();
        selectedRanged.text = unit.unit.ranged.ToString();
        selectedRangeIcon.text = BOW.ToString();
        selectedRange.text = unit.unit.range.ToString();
    }

    private void UpdateHoverPanel(UnitInstance unit)
    {
        hoverName.text = unit.unit.name;
        hoverHealthIcon.text = HEALTH.ToString();
        hoverHealth.text = unit.health.ToString();
        hoverMovementIcon.text = WALK.ToString();
        hoverMovement.text = $"{unit.movesLeft}/{unit.unit.movement}";
        hoverMeleeIcon.text = SWORD.ToString();
        hoverMelee.text = unit.unit.melee.ToString();
        hoverRangedIcon.text = BOW.ToString();
        hoverRanged.text = unit.unit.ranged.ToString();
        hoverRangeIcon.text = BOW.ToString();
        hoverRange.text = unit.unit.range.ToString();
    }

    void HandleTileSelected(Vector2Int pos) { ShowSelectedTile(pos); }
    void HandleTileHovered(Vector2Int pos) { ShowHoveredTile(pos); }
    void HandleMouseMovedToTile(Vector2Int? tile) 
    { 
        if (hoveredTile.HasValue && (!tile.HasValue || tile.Value != hoveredTile.Value))
        {
            hoverPanel.style.display = DisplayStyle.None;
            hoveredTile = null;
        }
    }
    void HandleCancel() { selectedPanel.style.display = DisplayStyle.None; selectedTile = null; }
    void HandleTileDeselected(Vector2Int pos) { selectedPanel.style.display = DisplayStyle.None; selectedTile = null; }

    void UpdateTurnLabels()
    {
        var player = Game.Instance.player;
        var aiCiv = UnitManager.Instance.civUnits.Keys.First(c => c != player.civilization);
        
        playerCivIcon.image = Game.Instance.civilizations[player.civilization].icon;
        aiCivIcon.image = Game.Instance.civilizations[aiCiv].icon;
        endTurnButton.SetEnabled(TurnManager.Instance.isPlayerTurn);
    }
}