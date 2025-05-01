using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label selectedUnitLabel, selectedHealth, selectedMovement;
    private Label selectedHealthIcon, selectedMovementIcon;
    private Label selectedAttack, selectedDefence;
    private Label selectedAttackIcon, selectedDefenceIcon;
    private Label selectedTerrainLabel, selectedMovementCost;
    private Label selectedTerrainIcon, selectedTerrainAttack, selectedTerrainDefence;
    private Label hoverUnitLabel, hoverHealth, hoverMovement;
    private Label hoverHealthIcon, hoverMovementIcon;
    private Label hoverAttack, hoverDefence;
    private Label hoverAttackIcon, hoverDefenceIcon;
    private Label hoverTerrainLabel, hoverMovementCost;
    private Label hoverTerrainIcon, hoverTerrainAttack, hoverTerrainDefence;
    private Image playerCivIcon, aiCivIcon;
    private VisualElement selectedPanel, hoverPanel;
    private Button endTurnButton;
    private Vector2Int? selectedTile, hoveredTile;

    private const char HEALTH = '\uf21e';    // heartbeat
    private const char SWORD = '\uf71c';     // sword
    private const char WALK = '\uf554';      // person-walking
    private const char BOW = '\uf71c';       // removing bow, using sword for now
    private const char HEX = '\uf312';       // hex
    private const char SHIELD = '\uf3ed';    // shield

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        selectedPanel = root.Q("SelectedPanel");
        hoverPanel = root.Q("HoverPanel");
        
        // Get all UI elements
        selectedUnitLabel = selectedPanel.Q<Label>("SelectedUnitLabel"); 
        selectedHealth = selectedPanel.Q<Label>("SelectedHealth");
        selectedMovement = selectedPanel.Q<Label>("SelectedMovement");
        selectedHealthIcon = selectedPanel.Q<Label>("SelectedHealthIcon");
        selectedMovementIcon = selectedPanel.Q<Label>("SelectedMovementIcon");
        selectedAttack = selectedPanel.Q<Label>("SelectedAttack");
        selectedDefence = selectedPanel.Q<Label>("SelectedDefence");
        selectedAttackIcon = selectedPanel.Q<Label>("SelectedAttackIcon");
        selectedDefenceIcon = selectedPanel.Q<Label>("SelectedDefenceIcon");
        selectedTerrainLabel = selectedPanel.Q<Label>("SelectedTerrainLabel");
        selectedMovementCost = selectedPanel.Q<Label>("SelectedMovementCost");
        selectedTerrainIcon = selectedPanel.Q<Label>("SelectedTerrainIcon");
        selectedTerrainAttack = selectedPanel.Q<Label>("SelectedTerrainAttack");
        selectedTerrainDefence = selectedPanel.Q<Label>("SelectedTerrainDefence");

        hoverUnitLabel = hoverPanel.Q<Label>("HoverUnitLabel"); 
        hoverHealth = hoverPanel.Q<Label>("HoverHealth");
        hoverMovement = hoverPanel.Q<Label>("HoverMovement");
        hoverHealthIcon = hoverPanel.Q<Label>("HoverHealthIcon");
        hoverMovementIcon = hoverPanel.Q<Label>("HoverMovementIcon");
        hoverAttack = hoverPanel.Q<Label>("HoverAttack");
        hoverDefence = hoverPanel.Q<Label>("HoverDefence");
        hoverAttackIcon = hoverPanel.Q<Label>("HoverAttackIcon");
        hoverDefenceIcon = hoverPanel.Q<Label>("HoverDefenceIcon");
        hoverTerrainLabel = hoverPanel.Q<Label>("HoverTerrainLabel");
        hoverMovementCost = hoverPanel.Q<Label>("HoverMovementCost");
        hoverTerrainIcon = hoverPanel.Q<Label>("HoverTerrainIcon");
        hoverTerrainAttack = hoverPanel.Q<Label>("HoverTerrainAttack");
        hoverTerrainDefence = hoverPanel.Q<Label>("HoverTerrainDefence");

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
        selectedTerrainLabel.text = terrain.name;
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            SetSelectedLabelsVisible(false);
            return;
        }

        UpdateSelectedPanel(unit);
        SetSelectedLabelsVisible(true);
    }

    private void ShowHoveredTile(Vector2Int pos)
    {
        hoveredTile = pos;
        hoverPanel.style.display = DisplayStyle.Flex;
        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        var terrain = tile.terrainScob.terrain;
        hoverTerrainLabel.text = terrain.name;
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            SetHoverLabelsVisible(false);
            return;
        }

        UpdateHoverPanel(unit);
        SetHoverLabelsVisible(true);
    }

    private void SetSelectedLabelsVisible(bool visible)
    {
        var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        selectedUnitLabel.style.display = display;
        selectedHealth.style.display = display;
        selectedHealthIcon.style.display = display;
        selectedMovement.style.display = display;
        selectedMovementIcon.style.display = display;
        selectedAttack.style.display = display;
        selectedDefence.style.display = display;
    }

    private void SetHoverLabelsVisible(bool visible)
    {
        var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        hoverUnitLabel.style.display = display;
        hoverHealth.style.display = display;
        hoverHealthIcon.style.display = display;
        hoverMovement.style.display = display;
        hoverMovementIcon.style.display = display;
        hoverAttack.style.display = display;
        hoverDefence.style.display = display;
    }

    private void UpdateSelectedPanel(UnitInstance unit)
    {
        selectedUnitLabel.text = unit.unit.name;
        selectedHealthIcon.text = HEALTH.ToString();
        selectedHealth.text = unit.health.ToString();
        selectedMovementIcon.text = WALK.ToString();
        selectedMovement.text = $"{unit.movesLeft}/{unit.unit.movement}";
        selectedAttackIcon.text = SWORD.ToString();
        selectedAttack.text = unit.unit.melee.ToString();
        selectedDefenceIcon.text = SHIELD.ToString();
        selectedDefence.text = unit.unit.ranged.ToString();
    }

    private void UpdateHoverPanel(UnitInstance unit)
    {
        hoverUnitLabel.text = unit.unit.name;
        hoverHealthIcon.text = HEALTH.ToString();
        hoverHealth.text = unit.health.ToString();
        hoverMovementIcon.text = WALK.ToString();
        hoverMovement.text = $"{unit.movesLeft}/{unit.unit.movement}";
        hoverAttackIcon.text = SWORD.ToString();
        hoverAttack.text = unit.unit.melee.ToString();
        hoverDefenceIcon.text = SHIELD.ToString();
        hoverDefence.text = unit.unit.ranged.ToString();
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