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
            
        var validMoves = HexGrid.GetValidMoves(from, unit.movement, UnitManager.Instance.tilemap);
        
        if (!validMoves.Contains(to))
        {
            Debug.Log($"Invalid move: {from} -> {to} not in valid moves");
            return false;
        }

        // Check if destination has a friendly unit
        if (UnitManager.Instance.TryGetUnit(to, out var target) && target.civ == unit.civ)
        {
            Debug.Log($"Invalid move: friendly unit at {to}");
            return false;
        }
            
        return true;
    }

    public void MoveTo(Vector2Int from, Vector2Int to)
    {
        if (!IsValidMove(from, to))
            return;
            
        var unit = UnitManager.Instance.units[from];
        
        // Handle combat if target exists
        if (UnitManager.Instance.TryGetUnit(to, out var target))
        {
            if (CombatManager.Instance.TryCombat(from, to))
            {
                // Combat handled movement
                Debug.Log($"Combat at {to}");
                return;
            }
            return; // Invalid combat
        }
            
        // Normal movement
        unit.movement--;
        UnitManager.Instance.units.Remove(from);
        UnitManager.Instance.units[to] = unit;

        // Update tilemap
        var tile = UnitManager.Instance.tilemap.GetTile((Vector3Int)from);
        UnitManager.Instance.tilemap.SetTile((Vector3Int)from, null);
        UnitManager.Instance.tilemap.SetTile((Vector3Int)to, tile);

        Debug.Log($"Movement after: {unit.movement}");
    }
} 