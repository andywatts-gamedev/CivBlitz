using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class SelectedTileUI : MonoBehaviour
{
    [SerializeField] private GameStateEvents gameStateEvents;
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container, unitRow, tileRow;
    private Label unitAttack, tileDefense, unitName, tileName;
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
        tileName = container.Q<Label>("TileName");
        tileDefense = container.Q<Label>("TileDefense");

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
        unitRow.style.display = DisplayStyle.None;
        tileRow.style.display = DisplayStyle.None;
        
        bool hasUnit = UnitManager.Instance.TryGetUnit(pos, out var unit);
        
        if (hasUnit)
        {
            unitName.text = unit.unit.name;
            unitAttack.text = (unit.unit.type == UnitType.Ranged ? unit.unit.ranged : unit.unit.melee).ToString();
            unitRow.style.display = DisplayStyle.Flex;
        }

        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        if (tile != null)
        {
            var terrain = tile.terrainScob.terrain;
            tileName.text = terrain.name;
            tileDefense.text = terrain.defenseBonus.ToString();
            if (terrain.defenseBonus != 0) tileRow.style.display = DisplayStyle.Flex;
        }

        container.style.display = (hasUnit || (tile != null && tile.terrainScob.terrain.defenseBonus != 0)) ? DisplayStyle.Flex : DisplayStyle.None;
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

