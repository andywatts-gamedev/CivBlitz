using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class Game : Singleton<Game>
{
    [SerializeField] private InputEvents events;
    public GameObject highlightGO;
    private Vector2Int? selectedTile;

    private void OnEnable()
    {
        events.OnTileSelected += HandleTileSelected;
        events.OnCancel += HandleCancel;
    }

    private void OnDisable()
    {
        events.OnTileSelected -= HandleTileSelected;
        events.OnCancel -= HandleCancel;
    }

    private void HandleTileSelected(Vector2Int tile)
    {
        if (!selectedTile.HasValue)
            SelectTile(tile);
        else
            MoveTo(tile);
    }

    private void SelectTile(Vector2Int tile)
    {
        if (!UnitManager.Instance.TryGetUnit(tile, out var unit)) return;
        
        var unitTile = UnitManager.Instance.tilemap.GetTile((Vector3Int)tile) as UnitTile;
        if (!TurnManager.Instance.isPlayerTurn || unitTile.civ != TurnManager.Instance.playerCiv) return;
        
        selectedTile = tile;
        highlightGO.transform.position = UnitManager.Instance.tilemap.GetCellCenterWorld((Vector3Int)tile);
        highlightGO.SetActive(true);
    }

    private void MoveTo(Vector2Int target)
    {
        if (!selectedTile.HasValue || !IsValidMove(target)) return;
        
        if (!UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit)) return;

        var distance = GetHexDistance(selectedTile.Value, target);
        if (unit.movement < distance) return;

        if (UnitManager.Instance.HasUnitAt(target)) {
            if (CombatManager.Instance.TryCombat(selectedTile.Value, target))
                HandleCancel();
        } else {
            unit.movement -= distance;
            UnitManager.Instance.MoveUnit(unit, selectedTile.Value, target);
            if (unit.movement <= 0) HandleCancel();
        }
        
        if (TurnManager.Instance.isPlayerTurn && TurnManager.Instance.AlltilemapMovedForCiv(TurnManager.Instance.playerCiv))
            TurnManager.Instance.EndTurn();
    }

    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        // Convert to cube coordinates for pointy-top hex grid
        var ax = a.x - (a.y - (a.y & 1)) / 2;
        var az = a.y;
        var ay = -ax - az;
        
        var bx = b.x - (b.y - (b.y & 1)) / 2;
        var bz = b.y;
        var by = -bx - bz;
        
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    private bool IsValidMove(Vector2Int target)
    {
        if (!UnitManager.Instance.tilemap.cellBounds.Contains((Vector3Int)target)) return false;
        
        var dx = Mathf.Abs(target.x - selectedTile.Value.x);
        var dy = Mathf.Abs(target.y - selectedTile.Value.y);
        
        return (dx == 0 && dy == 1) || 
               (dx == 1 && dy == 1) || 
               (dx == 1 && dy == 0);
    }

    private void HandleCancel()
    {
        selectedTile = null;
        highlightGO.SetActive(false);
    }
}
