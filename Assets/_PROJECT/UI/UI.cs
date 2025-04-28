using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label nameLabel, healthLabel, movementLabel, rangeLabel, meleeLabel, rangedLabel;
    private Image playerCivIcon, aiCivIcon;
    private Label terrainLabel, movementCostLabel;
    private VisualElement infoPanel;
    private Button endTurnButton;
    private Vector2Int? currentTile;
    private bool isSelected;

    private const char HEALTH = '';
    private const char SWORD = '';
    private const char WALK = '';
    private const char BOW = '\u26b2';

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        infoPanel = root.Q("InfoPanel");
        
        // Get all UI elements
        nameLabel = infoPanel.Q<Label>("Name"); 
        healthLabel = infoPanel.Q<Label>("Health");
        movementLabel = infoPanel.Q<Label>("Movement");
        rangeLabel = infoPanel.Q<Label>("Range");
        meleeLabel = infoPanel.Q<Label>("Melee");
        rangedLabel = infoPanel.Q<Label>("Ranged");
        terrainLabel = infoPanel.Q<Label>("Terrain");
        movementCostLabel = infoPanel.Q<Label>("MovementCost");
        playerCivIcon = root.Q<Image>("PlayerCivIcon");
        aiCivIcon = root.Q<Image>("AICivIcon");
        endTurnButton = root.Q<Button>("EndTurn") ?? CreateEndTurnButton(root);

        infoPanel.style.display = DisplayStyle.None;
        
        // Subscribe to events
        events.OnTileSelected += HandleTileSelected;
        events.OnCancel += HandleCancel;
        events.OnTileDeselected += HandleTileDeselected;
        TurnManager.Instance.OnTurnChanged += UpdateTurnLabels;
        
        UpdateTurnLabels();
    }

    void OnDisable() 
    {
        events.OnTileSelected -= HandleTileSelected;
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

    public void ShowTile(Vector2Int pos)
    {
        currentTile = pos;
        infoPanel.style.display = DisplayStyle.Flex;
        terrainLabel.text = "Grassland";
        movementCostLabel.text = "1";
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            SetUnitLabelsVisible(false);
            return;
        }

        UpdateUnitPanel(unit);
        SetUnitLabelsVisible(true, unit.unit.ranged > 0);
    }

    private void SetUnitLabelsVisible(bool visible, bool hasRanged = false)
    {
        var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        nameLabel.style.display = display;
        healthLabel.style.display = display;
        movementLabel.style.display = display;
        meleeLabel.style.display = display;
        rangedLabel.style.display = hasRanged ? display : DisplayStyle.None;
        rangeLabel.style.display = hasRanged ? display : DisplayStyle.None;
    }

    private void UpdateUnitPanel(UnitInstance unit)
    {
        nameLabel.text = unit.unit.name;
        healthLabel.text = $"{HEALTH} {unit.health}";
        movementLabel.text = $"{WALK} {unit.movesLeft}/{unit.unit.movement}";
        meleeLabel.text = $"{SWORD} {unit.unit.melee}";
        rangedLabel.text = $"{BOW} {unit.unit.ranged}";
        rangeLabel.text = $"{BOW} {unit.unit.range}";
    }

    public void HideTile() { if (!isSelected) { infoPanel.style.display = DisplayStyle.None; currentTile = null; } }
    void HandleTileSelected(Vector2Int pos) { isSelected = true; ShowTile(pos); }
    void HandleCancel() { isSelected = false; HideTile(); }
    void HandleTileDeselected(Vector2Int pos) { isSelected = false; HideTile(); }

    void UpdateTurnLabels()
    {
        var player = Game.Instance.player;
        var aiCiv = UnitManager.Instance.civUnits.Keys.First(c => c != player.civilization);
        
        playerCivIcon.image = Game.Instance.civilizations[player.civilization].icon;
        aiCivIcon.image = Game.Instance.civilizations[aiCiv].icon;
        endTurnButton.SetEnabled(TurnManager.Instance.isPlayerTurn);

        // var activeIcon = TurnManager.Instance.isPlayerTurn ? playerCivIcon : aiCivIcon;
        // var inactiveIcon = TurnManager.Instance.isPlayerTurn ? aiCivIcon : playerCivIcon;

        // SetBorderStyle(activeIcon, true);
        // SetBorderStyle(inactiveIcon, false);
    }

}