using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label nameLabel, healthLabel, movementLabel, rangeLabel, meleeLabel, rangedLabel;
    private Label playerCivLabel, aiCivLabel;
    private Label terrainLabel, movementCostLabel;
    private VisualElement infoPanel;
    private Button endTurnButton;
    private Vector2Int? currentTile;
    private bool isSelected;

    private char health = '';
    private char sword = '';
    private char walk = '';
    private char bow = '\u26b2';

    void Start()
    {
        doc = GetComponent<UIDocument>();
        if (doc == null) {
            Debug.LogError("No UIDocument found!");
            return;
        }

        var root = doc.rootVisualElement;
        if (root == null) {
            Debug.LogError("No root element found!");
            return;
        }

        infoPanel = root.Q("InfoPanel");
        if (infoPanel == null) {
            Debug.LogError("No InfoPanel found!");
            return;
        }

        nameLabel = infoPanel.Q<Label>("Name"); 
        healthLabel = infoPanel.Q<Label>("Health");
        movementLabel = infoPanel.Q<Label>("Movement");
        rangeLabel = infoPanel.Q<Label>("Range");
        meleeLabel = infoPanel.Q<Label>("Melee");
        rangedLabel = infoPanel.Q<Label>("Ranged");

        terrainLabel = infoPanel.Q<Label>("Terrain");
        movementCostLabel = infoPanel.Q<Label>("MovementCost");

        playerCivLabel = root.Q<Label>("PlayerCiv");
        aiCivLabel = root.Q<Label>("AICiv");
        endTurnButton = root.Q<Button>("EndTurn");

        if (endTurnButton == null) {
            endTurnButton = new Button(() => TurnManager.Instance.EndTurn()) { text = "End Turn" };
            root.Add(endTurnButton);
        } else {
            endTurnButton.clicked += () => TurnManager.Instance.EndTurn();
        }

        if (nameLabel == null) Debug.LogError("Name label not found!");
        if (healthLabel == null) Debug.LogError("Health label not found!");
        if (movementLabel == null) Debug.LogError("Movement label not found!");
        if (rangeLabel == null) Debug.LogError("Range label not found!");
        if (meleeLabel == null) Debug.LogError("Melee label not found!");
        if (rangedLabel == null) Debug.LogError("Ranged label not found!");
        if (playerCivLabel == null) Debug.LogError("PlayerCiv label not found!");
        if (aiCivLabel == null) Debug.LogError("AICiv label not found!");
        if (terrainLabel == null) Debug.LogError("Terrain label not found!");
        if (movementCostLabel == null) Debug.LogError("MovementCost label not found!");
        
        infoPanel.style.display = DisplayStyle.None;
        
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

    public void ShowTile(Vector2Int pos)
    {
        currentTile = pos;
        infoPanel.style.display = DisplayStyle.Flex;
        UpdateTilePanel(pos);
        
        if (UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            UpdateUnitPanel(unit);
            nameLabel.style.display = DisplayStyle.Flex;
            healthLabel.style.display = DisplayStyle.Flex;
            movementLabel.style.display = DisplayStyle.Flex;
            meleeLabel.style.display = DisplayStyle.Flex;
            rangedLabel.style.display = unit.unitData?.ranged > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            rangeLabel.style.display = unit.unitData?.ranged > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
        else
        {
            nameLabel.style.display = DisplayStyle.None;
            healthLabel.style.display = DisplayStyle.None;
            movementLabel.style.display = DisplayStyle.None;
            meleeLabel.style.display = DisplayStyle.None;
            rangedLabel.style.display = DisplayStyle.None;
            rangeLabel.style.display = DisplayStyle.None;
        }
    }

    public void HideTile()
    {
        if (!isSelected)
        {
            infoPanel.style.display = DisplayStyle.None;
            currentTile = null;
        }
    }

    void UpdateTilePanel(Vector2Int pos)
    {
        terrainLabel.text = "Grassland";
        movementCostLabel.text = "1";
    }

    void UpdateUnitPanel(UnitInstance unit)
    {
        string unitName = unit.unitData != null ? unit.unitData.name.Replace("Data", "") : "Unknown";
        nameLabel.text = unitName;
        healthLabel.text = $"{health} {unit.health}";
        movementLabel.text = $"{walk} {unit.movement}";
        meleeLabel.text = $"{sword} {unit.unitData?.melee}";
        rangedLabel.text = $"{bow} {unit.unitData?.ranged}";
        rangeLabel.text = $"{bow} {unit.unitData?.range}";
    
    }

    void HandleTileSelected(Vector2Int pos)
    {
        isSelected = true;
        ShowTile(pos);
    }

    void HandleCancel()
    {
        isSelected = false;
        HideTile();
    }

    void HandleTileDeselected(Vector2Int pos)
    {
        isSelected = false;
        HideTile();
    }

    void UpdateTurnLabels()
    {
        if (TurnManager.Instance.playerCiv != null && TurnManager.Instance.aiCiv != null) {
            playerCivLabel.text = TurnManager.Instance.playerCiv.Name;
            aiCivLabel.text = TurnManager.Instance.aiCiv.Name;
            endTurnButton.SetEnabled(TurnManager.Instance.isPlayerTurn);

            if (TurnManager.Instance.isPlayerTurn) {
                playerCivLabel.style.borderTopColor = new Color(1, 1, 1, 1);
                playerCivLabel.style.borderRightColor = new Color(1, 1, 1, 1);
                playerCivLabel.style.borderBottomColor = new Color(1, 1, 1, 1);
                playerCivLabel.style.borderLeftColor = new Color(1, 1, 1, 1);
                playerCivLabel.style.borderTopWidth = 2;
                playerCivLabel.style.borderRightWidth = 2;
                playerCivLabel.style.borderBottomWidth = 2;
                playerCivLabel.style.borderLeftWidth = 2;
                
                aiCivLabel.style.borderTopColor = new Color(0, 0, 0, 0);
                aiCivLabel.style.borderRightColor = new Color(0, 0, 0, 0);
                aiCivLabel.style.borderBottomColor = new Color(0, 0, 0, 0);
                aiCivLabel.style.borderLeftColor = new Color(0, 0, 0, 0);
                aiCivLabel.style.borderTopWidth = 0;
                aiCivLabel.style.borderRightWidth = 0;
                aiCivLabel.style.borderBottomWidth = 0;
                aiCivLabel.style.borderLeftWidth = 0;
            }
            else
            {
                aiCivLabel.style.borderTopColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderRightColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderBottomColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderLeftColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderTopWidth = 2;
                aiCivLabel.style.borderRightWidth = 2;
                aiCivLabel.style.borderBottomWidth = 2;
                aiCivLabel.style.borderLeftWidth = 2;
                
                playerCivLabel.style.borderTopColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderRightColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderBottomColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderLeftColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderTopWidth = 0;
                playerCivLabel.style.borderRightWidth = 0;
                playerCivLabel.style.borderBottomWidth = 0;
                playerCivLabel.style.borderLeftWidth = 0;
            }
        }
    }
}