using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label nameLabel, healthLabel, movementLabel, rangeLabel, meleeLabel, rangedLabel;
    private Label playerCivLabel, aiCivLabel;
    private VisualElement unitPanel;
    private Button endTurnButton;

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

        unitPanel = root.Q("Unit");
        if (unitPanel == null) {
            Debug.LogError("No Unit panel found!");
            return;
        }

        nameLabel = unitPanel.Q<Label>("Name"); 
        healthLabel = unitPanel.Q<Label>("Health");
        movementLabel = unitPanel.Q<Label>("Movement");
        rangeLabel = unitPanel.Q<Label>("Range");
        meleeLabel = unitPanel.Q<Label>("Melee");
        rangedLabel = unitPanel.Q<Label>("Ranged");

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
        
        unitPanel.style.display = DisplayStyle.None;
        
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
            } else {
                playerCivLabel.style.borderTopColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderRightColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderBottomColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderLeftColor = new Color(0, 0, 0, 0);
                playerCivLabel.style.borderTopWidth = 0;
                playerCivLabel.style.borderRightWidth = 0;
                playerCivLabel.style.borderBottomWidth = 0;
                playerCivLabel.style.borderLeftWidth = 0;
                
                aiCivLabel.style.borderTopColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderRightColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderBottomColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderLeftColor = new Color(1, 1, 1, 1);
                aiCivLabel.style.borderTopWidth = 2;
                aiCivLabel.style.borderRightWidth = 2;
                aiCivLabel.style.borderBottomWidth = 2;
                aiCivLabel.style.borderLeftWidth = 2;
            }
        }
    }

    void HandleTileSelected(Vector2Int pos)
    {
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit)) {
            unitPanel.style.display = DisplayStyle.None;
            return;
        }

        string unitName = unit.unitData != null ? unit.unitData.name.Replace("Data", "") : "Unknown";
        nameLabel.text = unitName;
        healthLabel.text = $"{health} {unit.health}";
        movementLabel.text = $"{walk} {unit.movement}";
        
        // Show melee attack
        meleeLabel.text = $"{sword} {unit.unitData?.melee}";
        
        // Show ranged attack and range if it exists
        if (unit.unitData?.ranged > 0) {
            rangedLabel.text = $"{bow} {unit.unitData.ranged}";
            rangedLabel.style.display = DisplayStyle.Flex;
            if (unit.unitData.range > 0) {
                rangeLabel.text = $"{bow} {unit.unitData.range}";
                rangeLabel.style.display = DisplayStyle.Flex;
            } else {
                rangeLabel.style.display = DisplayStyle.None;
            }
        } else {
            rangedLabel.style.display = DisplayStyle.None;
            rangeLabel.style.display = DisplayStyle.None;
        }
        
        unitPanel.style.display = DisplayStyle.Flex;
    }

    void HandleCancel()
    {
        unitPanel.style.display = DisplayStyle.None;
    }

    void HandleTileDeselected(Vector2Int pos)
    {
        unitPanel.style.display = DisplayStyle.None;
    }
}