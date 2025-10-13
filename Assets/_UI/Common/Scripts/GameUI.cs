using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.Tilemaps;

public class GameUI : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    private UIDocument doc;

    private VisualElement selectedTileContainer, unitRow, tileRow;
    private Label unitAttack, unitDefense, tileAttack, tileDefense, unitName, tileName;
    private Vector2Int? selectedTile;

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        selectedTileContainer = root.Q("SelectedTileContainer");
        unitRow = selectedTileContainer.Q("UnitRow");
        unitName = selectedTileContainer.Q<Label>("UnitName");
        unitAttack = selectedTileContainer.Q<Label>("UnitAttack");
        unitDefense = selectedTileContainer.Q<Label>("UnitDefense");
        tileName = selectedTileContainer.Q<Label>("TileName");
        tileAttack = selectedTileContainer.Q<Label>("TileAttack");
        tileDefense = selectedTileContainer.Q<Label>("TileDefense");

        events.OnTileSelected += HandleTileSelected;
        events.OnTileDeselected += HandleTileDeselected;
        events.OnCancel += HandleCancel;

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


    private void ShowSelectedTile(Vector2Int pos)
    {
        selectedTile = pos;
        selectedTileContainer.style.display = DisplayStyle.Flex;

        // Display Unit Row
        unitRow.style.display = DisplayStyle.None;
        if (UnitManager.Instance.TryGetUnit(pos, out var unit))
        {
            unitName.text = unit.unit.name;
            unitAttack.text = unit.unit.melee.ToString();
            unitDefense.text = unit.unit.ranged.ToString();
            unitRow.style.display = DisplayStyle.Flex;
        }

        // Display Tile Row
        var tile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)pos) as TerrainTile;
        var terrain = tile.terrainScob.terrain;
        tileName.text = terrain.name;
        tileAttack.text = terrain.attackBonus.ToString();
        tileDefense.text = terrain.defenseBonus.ToString();
    }

    void HandleTileSelected(Vector2Int pos) => ShowSelectedTile(pos);
    void HandleCancel() { selectedTileContainer.style.display = DisplayStyle.None; selectedTile = null; }
    void HandleTileDeselected(Vector2Int pos) 
    { 
        selectedTile = null; 
        selectedTileContainer.style.display = DisplayStyle.None; 
        unitRow.style.display = DisplayStyle.None;
    }

    void UpdateTurnLabels()
    {
        var player = Game.Instance.player;
        if (player == null || UnitManager.Instance.civUnits.Count < 2) return;
            
        var aiCiv = UnitManager.Instance.civUnits.Keys.First(c => c != player.civilization);
    }
}

