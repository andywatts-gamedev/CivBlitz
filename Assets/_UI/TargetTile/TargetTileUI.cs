using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class TargetTileUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container, unitRow, tileRow;
    private Label unitAttack, tileDefense, unitName, tileName;
    private Vector2Int? targetTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("TargetTileContainer");
        unitRow = container.Q("UnitRow");
        tileRow = container.Q("TileRow");
        unitName = container.Q<Label>("UnitName");
        unitAttack = container.Q<Label>("UnitAttack");
        tileName = container.Q<Label>("TileName");
        tileDefense = container.Q<Label>("TileDefense");

        events.OnTileHovered += HandleTileHovered;
        events.OnPointerMovedToTile += HandlePointerMovedToTile;
        events.OnHoverCleared += HandleHoverCleared;
    }

    void OnDisable() 
    {
        events.OnTileHovered -= HandleTileHovered;
        events.OnPointerMovedToTile -= HandlePointerMovedToTile;
        events.OnHoverCleared -= HandleHoverCleared;
    }

    private void ShowTile(Vector2Int pos)
    {
        targetTile = pos;
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

    void HandleTileHovered(Vector2Int pos) => ShowTile(pos);
    
    void HandlePointerMovedToTile(Vector2Int? tile) 
    { 
        if (targetTile.HasValue && (!tile.HasValue || tile.Value != targetTile.Value))
        {
            container.style.display = DisplayStyle.None;
            targetTile = null;
        }
    }
    
    void HandleHoverCleared() 
    { 
        container.style.display = DisplayStyle.None; 
        targetTile = null; 
    }
}

