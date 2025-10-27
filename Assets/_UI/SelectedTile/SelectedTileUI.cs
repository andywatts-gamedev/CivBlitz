using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class SelectedTileUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container, unitRow;
    private Label unitAttack, unitDefense, tileAttack, tileDefense, unitName, tileName;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("SelectedTileContainer");
        unitRow = container.Q("UnitRow");
        unitName = container.Q<Label>("UnitName");
        unitAttack = container.Q<Label>("UnitAttack");
        unitDefense = container.Q<Label>("UnitDefense");
        tileName = container.Q<Label>("TileName");
        tileAttack = container.Q<Label>("TileAttack");
        tileDefense = container.Q<Label>("TileDefense");

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;
    }

    void OnDisable() 
    {
        events.OnTileSelected -= HandleTileSelected;
        events.OnTileDeselected -= HandleTileDeselected;
        events.OnCancel -= HandleCancel;
    }

    private void ShowTile(Vector2Int pos)
    {
        selectedTile = pos;
        container.style.display = DisplayStyle.Flex;

        unitRow.style.display = DisplayStyle.None;
        if (UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            unitName.text = unit.unit.name;
            unitAttack.text = (unit.unit.type == UnitType.Ranged ? unit.unit.ranged : unit.unit.melee).ToString();
            unitDefense.text = unit.unit.melee.ToString();
            unitRow.style.display = DisplayStyle.Flex;
        }

        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        if (tile == null) return;
        
        var terrain = tile.terrainScob.terrain;
        tileName.text = terrain.name;
        tileAttack.text = terrain.attackBonus.ToString();
        tileDefense.text = terrain.defenseBonus.ToString();
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

