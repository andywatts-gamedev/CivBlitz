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
        events.OnTileClicked += HandleTileClicked;
        events.OnCancel += HandleCancel;
    }

    private void OnDisable()
    {
        events.OnTileClicked -= HandleTileClicked;
        events.OnCancel -= HandleCancel;
    }

    private void HandleTileClicked(Vector2Int tile)
    {
        if (!selectedTile.HasValue) {
            if (CanSelectTile(tile)) {
                SelectTile(tile);
                events.EmitTileSelected(tile);
            }
        }
        else if (tile == selectedTile.Value) {
            ClearSelection();
            // events.EmitTileDeselected(tile);
        }
        else if (IsValidMove(tile)) {
            MoveTo(tile);
        }
    }

    private bool CanSelectTile(Vector2Int tile)
    {
        if (!UnitManager.Instance.TryGetUnit(tile, out var unit)) return false;
        
        var unitTile = UnitManager.Instance.tilemap.GetTile((Vector3Int)tile) as UnitTile;
        return TurnManager.Instance.isPlayerTurn && unitTile.civ == TurnManager.Instance.playerCiv;
    }

    private void SelectTile(Vector2Int tile)
    {
        selectedTile = tile;
        highlightGO.SetActive(true);
        highlightGO.transform.position = UnitManager.Instance.tilemap.GetCellCenterWorld((Vector3Int)tile);
    }

    private void MoveTo(Vector2Int target)
    {
        if (!selectedTile.HasValue || !IsValidMove(target)) return;
        
        if (!UnitManager.Instance.TryGetUnit(selectedTile.Value, out var unit)) return;

        var distance = GetHexDistance(selectedTile.Value, target);
        if (unit.movement < distance) return;

        if (UnitManager.Instance.HasUnitAt(target)) {
            if (CombatManager.Instance.TryCombat(selectedTile.Value, target)) {
                ClearSelection();
            }
        } else {
            unit.movement -= distance;
            UnitManager.Instance.MoveUnit(unit, selectedTile.Value, target);
            ClearSelection();
        }
        
        if (TurnManager.Instance.isPlayerTurn && TurnManager.Instance.AlltilemapMovedForCiv(TurnManager.Instance.playerCiv)) {
            TurnManager.Instance.EndTurn();
            ClearSelection();
        }
    }

    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        var ax = a.x - (a.y - (a.y & 1)) / 2;
        var bx = b.x - (b.y - (b.y & 1)) / 2;
        var az = a.y;
        var bz = b.y;
        var ay = -ax - az;
        var by = -bx - bz;
        
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    private bool IsValidMove(Vector2Int target)
    {
        if (!UnitManager.Instance.tilemap.cellBounds.Contains((Vector3Int)target)) return false;
        var dx = Mathf.Abs(target.x - selectedTile.Value.x);
        var dy = Mathf.Abs(target.y - selectedTile.Value.y);
        return (dx == 0 && dy == 1) || (dx == 1 && dy == 1) || (dx == 1 && dy == 0);
    }

    private void HandleCancel()
    {
        ClearSelection();
        events.EmitCancel();
    }

    private void ClearSelection()
    {
        if (selectedTile.HasValue) {
            events.EmitTileDeselected(selectedTile.Value);
        }
        selectedTile = null;
        Debug.Log($"Game#ClearSelection: Setting highlight inactive: {highlightGO != null}");
        highlightGO.SetActive(false);
    }
}
