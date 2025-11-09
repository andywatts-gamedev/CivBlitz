using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class SelectedTileUI : MonoBehaviour
{
    [SerializeField] private GameStateEvents gameStateEvents;
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container, unitRow, tileRow;
    private Label unitAttack, unitDefense, tileAttack, tileDefense, unitName, tileName, attackIcon;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("SelectedTileContainer");
        unitRow = container.Q("UnitRow");
        tileRow = container.Q("TileRow");
        unitName = container.Q<Label>("UnitName");
        unitAttack = container.Q<Label>("UnitAttack");
        unitDefense = container.Q<Label>("UnitDefense");
        tileName = container.Q<Label>("TileName");
        tileAttack = container.Q<Label>("TileAttack");
        tileDefense = container.Q<Label>("TileDefense");
        attackIcon = container.Q<Label>("AttackIcon");

        gameStateEvents.OnTileSelected += HandleTileSelected;
        gameStateEvents.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
    }

    void OnDisable() 
    {
        gameStateEvents.OnTileSelected -= HandleTileSelected;
        gameStateEvents.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
    }

    private void ShowTile(Vector2Int pos)
    {
        selectedTile = pos;
        container.style.display = DisplayStyle.Flex;
        unitRow.style.display = DisplayStyle.None;
        
        bool hasUnit = UnitManager.Instance.TryGetUnit(pos, out var unit);
        
        if (hasUnit)
        {
            Debug.Log($"SelectedTile: Found unit at {pos}: {unit.unit.name}");
            unitName.text = unit.unit.name;
            unitAttack.text = (unit.unit.type == UnitType.Ranged ? unit.unit.ranged : unit.unit.melee).ToString();
            unitDefense.text = unit.unit.melee.ToString();
            unitRow.style.display = DisplayStyle.Flex;
            
            // Show attack column when unit exists
            attackIcon.style.display = DisplayStyle.Flex;
            unitAttack.style.display = DisplayStyle.Flex;
            tileAttack.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.Log($"SelectedTile: No unit at {pos}");
            
            // Hide attack column when no unit
            attackIcon.style.display = DisplayStyle.None;
            unitAttack.style.display = DisplayStyle.None;
            tileAttack.style.display = DisplayStyle.None;
        }

        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        if (tile != null)
        {
            var terrain = tile.terrainScob.terrain;
            tileName.text = terrain.name;
            tileAttack.text = terrain.attackBonus.ToString();
            tileDefense.text = terrain.defenseBonus.ToString();
            tileRow.style.display = (terrain.attackBonus != 0 || terrain.defenseBonus != 0) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    void HandleTileSelected(Vector2Int pos) => ShowTile(pos);
    
    void HandleCancel() 
    { 
        container.style.display = DisplayStyle.None; 
        selectedTile = null; 
    }
    
    void HandleTileDeselected(Vector2Int pos) 
    { 
        selectedTile = null; 
        container.style.display = DisplayStyle.None; 
        unitRow.style.display = DisplayStyle.None;
    }
}

