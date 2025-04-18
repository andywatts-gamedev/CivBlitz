using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    [SerializeField] private GameObject highlight;
    private Vector2Int? selectedTile;

    void Start()
    {
        events.OnTileClicked += HandleTileClicked;
        events.OnCancel += HandleCancel;
        highlight.SetActive(false);
    }

    void OnDisable()
    {
        events.OnTileClicked -= HandleTileClicked;
        events.OnCancel -= HandleCancel;
    }

    private void HandleTileClicked(Vector2Int pos)
    {
        if (selectedTile.HasValue)
        {
            if (selectedTile.Value == pos)
            {
                events.EmitTileDeselected(pos);
                selectedTile = null;
                highlight.SetActive(false);
                return;
            }

            if (IsValidMove(selectedTile.Value, pos))
            {
                MoveTo(selectedTile.Value, pos);
                events.EmitTileDeselected(selectedTile.Value);
                selectedTile = null;
                highlight.SetActive(false);
                return;
            }
        }

        if (UnitManager.Instance.TryGetUnit(pos, out var unit) && 
            unit.civ == TurnManager.Instance.playerCiv)
        {
            if (selectedTile.HasValue)
                events.EmitTileDeselected(selectedTile.Value);
            
            selectedTile = pos;
            events.EmitTileSelected(pos);
            highlight.SetActive(true);
            highlight.transform.position = UnitManager.Instance.tilemap.CellToWorld((Vector3Int)pos);
        }
    }

    private void HandleCancel()
    {
        if (selectedTile.HasValue)
        {
            events.EmitTileDeselected(selectedTile.Value);
            selectedTile = null;
            highlight.SetActive(false);
        }
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        if (!UnitManager.Instance.TryGetUnit(from, out var unit))
            return false;
            
        if (unit.movement <= 0)
            return false;
            
        if (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving)
            return false;
            
        var validMoves = HexGrid.GetValidMoves(from, unit.movement, UnitManager.Instance.tilemap);
        
        if (!validMoves.Contains(to))
            return false;

        if (UnitManager.Instance.TryGetUnit(to, out var target) && target.civ == unit.civ)
            return false;
            
        return true;
    }

    public void MoveTo(Vector2Int from, Vector2Int to)
    {
        if (!IsValidMove(from, to))
            return;
            
        var unit = UnitManager.Instance.units[from];
        
        if (UnitManager.Instance.TryGetUnit(to, out var target))
        {
            if (CombatManager.Instance.TryCombat(from, to))
                return;
            return;
        }
            
        unit.movement--;
        UnitManager.Instance.MoveUnit(unit, from, to);
    }
} 