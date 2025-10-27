using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class TargetTileUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;
    private VisualElement container, unitRow;
    private Label unitAttack, unitDefense, tileAttack, tileDefense, unitName, tileName;
    private Vector2Int? targetTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        container = root.Q("TargetTileContainer");
        unitRow = container.Q("UnitRow");
        unitName = container.Q<Label>("UnitName");
        unitAttack = container.Q<Label>("UnitAttack");
        unitDefense = container.Q<Label>("UnitDefense");
        tileName = container.Q<Label>("TileName");
        tileAttack = container.Q<Label>("TileAttack");
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
        container.style.display = DisplayStyle.Flex;

        unitRow.style.display = DisplayStyle.None;
        if (UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            unitName.text = unit.unit.name;
            unitAttack.text = unit.unit.melee.ToString();
            unitDefense.text = unit.unit.ranged.ToString();
            unitRow.style.display = DisplayStyle.Flex;
        }

        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        if (tile == null) return;
        
        var terrain = tile.terrainScob.terrain;
        tileName.text = terrain.name;
        tileAttack.text = terrain.attackBonus.ToString();
        tileDefense.text = terrain.defenseBonus.ToString();
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

