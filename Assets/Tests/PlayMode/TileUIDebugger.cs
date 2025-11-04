using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Debug utility to verify tile UI behavior at runtime
/// Attach to a GameObject in your scene and use keyboard shortcuts
/// </summary>
public class TileUIDebugger : MonoBehaviour
{
    [SerializeField] private GameStateEvents gameStateEvents;
    
    void Update()
    {
        // Press T to test tile selection at (0,0)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestTileSelection(new Vector2Int(0, 0));
        }
        
        // Press Y to test tile with unit
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestTileWithUnit();
        }
        
        // Press U to test empty tile
        if (Input.GetKeyDown(KeyCode.U))
        {
            TestEmptyTile();
        }
        
        // Press I to inspect UI structure
        if (Input.GetKeyDown(KeyCode.I))
        {
            InspectUIStructure();
        }
    }
    
    void TestTileSelection(Vector2Int pos)
    {
        Debug.Log($"=== Testing tile selection at {pos} ===");
        
        if (UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            Debug.Log($"Unit exists: {unit.unit.name}");
            Debug.Log($"  Melee: {unit.unit.melee}");
            Debug.Log($"  Ranged: {unit.unit.ranged}");
            Debug.Log($"  Type: {unit.unit.type}");
        }
        else
        {
            Debug.Log($"No unit at {pos}");
        }
        
        gameStateEvents.EmitTileSelected(pos);
    }
    
    void TestTileWithUnit()
    {
        Debug.Log("=== Testing tile with unit ===");
        
        foreach (var kvp in UnitManager.Instance.units)
        {
            TestTileSelection(kvp.Key);
            return;
        }
        
        Debug.LogWarning("No units found in UnitManager");
    }
    
    void TestEmptyTile()
    {
        Debug.Log("=== Testing empty tile ===");
        var emptyPos = new Vector2Int(50, 50);
        
        while (UnitManager.Instance.HasUnitAt(emptyPos))
        {
            emptyPos += Vector2Int.one;
        }
        
        TestTileSelection(emptyPos);
    }
    
    void InspectUIStructure()
    {
        Debug.Log("=== Inspecting UI Structure ===");
        
        var selectedTileUI = FindObjectOfType<SelectedTileUI>();
        if (selectedTileUI == null)
        {
            Debug.LogError("SelectedTileUI not found!");
            return;
        }
        
        var uiDoc = selectedTileUI.GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument not found!");
            return;
        }
        
        var root = uiDoc.rootVisualElement;
        var container = root.Q("SelectedTileContainer");
        
        if (container == null)
        {
            Debug.LogError("SelectedTileContainer not found in UI!");
            LogAllChildren(root, 0);
            return;
        }
        
        Debug.Log($"Container display: {container.style.display.value}");
        
        var unitRow = container.Q("UnitRow");
        if (unitRow != null)
        {
            Debug.Log($"UnitRow display: {unitRow.style.display.value}");
            Debug.Log($"UnitRow visible: {unitRow.visible}");
            
            var unitName = container.Q<Label>("UnitName");
            var unitAttack = container.Q<Label>("UnitAttack");
            var unitDefense = container.Q<Label>("UnitDefense");
            
            Debug.Log($"  UnitName: '{unitName?.text ?? "NULL"}' (exists: {unitName != null})");
            Debug.Log($"  UnitAttack: '{unitAttack?.text ?? "NULL"}' (exists: {unitAttack != null}, display: {unitAttack?.style.display.value})");
            Debug.Log($"  UnitDefense: '{unitDefense?.text ?? "NULL"}' (exists: {unitDefense != null}, display: {unitDefense?.style.display.value})");
            
            if (unitAttack != null)
            {
                Debug.Log($"  UnitAttack computed style - display: {unitAttack.resolvedStyle.display}, width: {unitAttack.resolvedStyle.width}, visibility: {unitAttack.resolvedStyle.visibility}");
            }
        }
        else
        {
            Debug.LogError("UnitRow not found!");
            LogAllChildren(container, 0);
        }
        
        var tileRow = container.Q("TileRow");
        if (tileRow != null)
        {
            var tileName = container.Q<Label>("TileName");
            var tileAttack = container.Q<Label>("TileAttack");
            var tileDefense = container.Q<Label>("TileDefense");
            
            Debug.Log($"Tile - Name: '{tileName?.text}', Attack: '{tileAttack?.text}', Defense: '{tileDefense?.text}'");
        }
    }
    
    void LogAllChildren(VisualElement element, int depth)
    {
        var indent = new string(' ', depth * 2);
        Debug.Log($"{indent}- {element.name} ({element.GetType().Name})");
        
        foreach (var child in element.Children())
        {
            LogAllChildren(child, depth + 1);
        }
    }
}

