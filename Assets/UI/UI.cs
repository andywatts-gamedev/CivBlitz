using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private Label nameLabel, healthLabel, movementLabel, rangeLabel, attackLabel, defenseLabel;
    private VisualElement unitPanel;

    private char sword = '';
    private char shield = '';
    private char walk = '';
    private char vs = '';
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
        attackLabel = unitPanel.Q<Label>("Attack");
        defenseLabel = unitPanel.Q<Label>("Defense");

        if (nameLabel == null) Debug.LogError("Name label not found!");
        if (healthLabel == null) Debug.LogError("Health label not found!");
        if (movementLabel == null) Debug.LogError("Movement label not found!");
        if (rangeLabel == null) Debug.LogError("Range label not found!");
        if (attackLabel == null) Debug.LogError("Attack label not found!");
        if (defenseLabel == null) Debug.LogError("Defense label not found!");

        Debug.Log($"UI#Start: UI Labels found: Name={nameLabel != null}, Health={healthLabel != null}, " +
                 $"Movement={movementLabel != null}, Range={rangeLabel != null}, " +
                 $"Attack={attackLabel != null}, Defense={defenseLabel != null}");
        
        // Hide panel initially
        unitPanel.style.display = DisplayStyle.None;
        
        events.OnTileSelected += HandleTileSelected;
        events.OnCancel += HandleCancel;
        events.OnTileDeselected += HandleTileDeselected;
    }

    void OnDisable() 
    {
        events.OnTileSelected -= HandleTileSelected;
        events.OnCancel -= HandleCancel;
        events.OnTileDeselected -= HandleTileDeselected;
    }

    void HandleTileSelected(Vector2Int pos)
    {
        Debug.Log($"UI#HandleTileSelected: called for position {pos}");
        
        if (!UnitManager.Instance.TryGetUnit(pos, out var unit)) {
            Debug.Log("UI#HandleTileSelected: No unit found at position");
            unitPanel.style.display = DisplayStyle.None;
            return;
        }

        Debug.Log($"UI#HandleTileSelected: Found unit: name={unit.unitData?.name}, health={unit.health}");

        if (nameLabel == null || healthLabel == null) {
            Debug.LogError("UI#HandleTileSelected: UI labels are null!");
            return;
        }

        // Use the asset name instead of a non-existent name field
        string unitName = unit.unitData != null ? unit.unitData.name.Replace("Data", "") : "Unknown";
        nameLabel.text = unitName;
        // healthLabel.text = $"{heart} {unit.health}";
        movementLabel.text = $"{walk} {unit.movement}";
        rangeLabel.text = $"{bow} {unit.unitData?.range}";
        attackLabel.text = $"{sword} {unit.unitData?.attack}";
        defenseLabel.text = $"{shield} {unit.unitData?.defense}";
        
        Debug.Log($"UI#HandleTileSelected: Updated labels: name={nameLabel.text}, health={healthLabel.text}");
        
        unitPanel.style.display = DisplayStyle.Flex;
        Debug.Log("UI#HandleTileSelected: Set panel to Flex display");
    }

    void HandleCancel()
    {
        Debug.Log("UI#HandleCancel: called ***********************");
        unitPanel.style.display = DisplayStyle.None;
    }

    void HandleTileDeselected(Vector2Int pos)
    {
        unitPanel.style.display = DisplayStyle.None;
    }
}