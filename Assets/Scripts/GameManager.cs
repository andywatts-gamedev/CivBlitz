using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputEvents events;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap units;
    [SerializeField] private Tilemap terrain;
    [SerializeField] private GameObject highlightGO;

    private Vector2Int? selectedTile;
    private List<Civilization> civs = new();
    private int currentCivIndex = 0;

    public Civilization playerCiv;

    void Start()
    {
        Debug.Log("Start");
        civs.Add(playerCiv);
        currentCivIndex = 0;

        // Find all unique civs in units tilemap
        var bounds = units.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
            for (int y = bounds.min.y; y < bounds.max.y; y++)
                if (units.GetTile(new Vector3Int(x, y, 0)) is UnitTile unit && unit.civ != null)
                    if (!civs.Contains(unit.civ)) civs.Add(unit.civ);

        // Log all units in the units tilemap
        // for (int x = bounds.min.x; x < bounds.max.x; x++)
            // for (int y = bounds.min.y; y < bounds.max.y; y++)
                // if (units.GetTile(new Vector3Int(x, y, 0)) is UnitTile unit)
                    // Debug.Log($"Unit at ({x},{y}): {unit.civ?.name}");
    }

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
        if (units.GetTile((Vector3Int)tile) != null)
        {
            var unit = units.GetTile((Vector3Int)tile);
            if (!(unit is UnitTile unitTile) || unitTile.civ != playerCiv) return;
            selectedTile = tile;
            highlightGO.transform.position = grid.GetCellCenterWorld((Vector3Int)tile);
            highlightGO.SetActive(true);
        }
    }

    private void MoveTo(Vector2Int target)
    {
        if (!selectedTile.HasValue) return;
        
        var targetUnit = units.GetTile((Vector3Int)target);
        if (targetUnit != null)
        {
            Debug.Log("Attack");
        }
        else
        {
            var unit = units.GetTile((Vector3Int)selectedTile.Value) as UnitTile;
            Debug.Log($"unit civ: {unit.civ.name}");
            units.SetTile((Vector3Int)selectedTile.Value, null);
            units.SetTile((Vector3Int)target, unit);
            Debug.Log($"unit civ: {unit.civ.name}");

            var dd = units.GetTile((Vector3Int)target) as UnitTile;
            Debug.Log($"dd civ: {dd.civ.name}");
        }
        
        EndTurn();
    }

    private void HandleCancel()
    {
        selectedTile = null;
        highlightGO.SetActive(false);
    }

    private void EndTurn()
    {
        currentCivIndex = (currentCivIndex + 1) % civs.Count;
        playerCiv = civs[currentCivIndex];
        HandleCancel();
    }
}
