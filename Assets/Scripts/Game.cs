using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [SerializeField] private InputEvents events;
    public Tilemap units;
    public GameObject highlightGO;

    private Vector2Int? selectedTile;
    private bool isPlayerTurn = true;
    private Dictionary<Vector2Int, UnitInstance> unitInstances = new();

    public Civilization playerCiv;
    public Civilization aiCiv;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Start");
        foreach (var unit in FindObjectsOfType<UnitInstance>())
            unitInstances[Vector2Int.RoundToInt(unit.transform.position)] = unit;
        
        var bounds = units.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
            for (int y = bounds.min.y; y < bounds.max.y; y++)
                if (units.GetTile(new Vector3Int(x, y, 0)) is UnitTile ut)
                    Debug.Log($"Unit at {x},{y}");

        ResetUnitMoves();
    }

    private void ResetUnitMoves()
    {
        foreach (var unit in unitInstances.Values)
            unit.Movement = unit.unitData.Movement;
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
        var unitInstance = unitInstances.GetValueOrDefault(tile);
        if (unitInstance != null)
        {
            Debug.Log("SelectTile");
            var unitTile = units.GetTile((Vector3Int)tile) as UnitTile;
            if (!isPlayerTurn || unitTile.civ != playerCiv) return;
            
            Debug.Log("SelectTile2");
            selectedTile = tile;
            highlightGO.transform.position = units.GetCellCenterWorld((Vector3Int)tile);
            highlightGO.SetActive(true);
        }
    }

    private void MoveTo(Vector2Int target)
    {
        if (!selectedTile.HasValue) return;
        
        var unit = unitInstances[selectedTile.Value];
        var targetUnit = unitInstances.GetValueOrDefault(target);
        var distance = Mathf.Abs(target.x - selectedTile.Value.x) + Mathf.Abs(target.y - selectedTile.Value.y);
        
        if (unit.Movement < distance) return;

        if (targetUnit != null)
        {
            var targetTile = units.GetTile((Vector3Int)target) as UnitTile;
            var sourceTile = units.GetTile((Vector3Int)selectedTile.Value) as UnitTile;
            if (targetTile.civ != sourceTile.civ)
            {
                Debug.Log("Attack");
                unit.Movement = 0;
            }
        }
        else 
        {
            unitInstances.Remove(selectedTile.Value);
            unitInstances[target] = unit;
            unit.transform.position = units.GetCellCenterWorld((Vector3Int)target);
            unit.Movement -= distance;
        }
        
        HandleCancel();
        
        if (isPlayerTurn && AllUnitsMovedForCiv(playerCiv))
            EndTurn();
    }

    private bool AllUnitsMovedForCiv(Civilization civ)
    {
        var bounds = units.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = units.GetTile(pos) as UnitTile;
                if (tile != null && tile.civ == civ)
                {
                    var unit = unitInstances[new Vector2Int(x, y)];
                    if (unit.Movement > 0) return false;
                }
            }
        return true;
    }

    private void HandleCancel()
    {
        selectedTile = null;
        highlightGO.SetActive(false);
    }

    private void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        
        if (!isPlayerTurn)
            StartCoroutine(DoAITurn());
        else
            ResetUnitMoves();
    }

    private IEnumerator DoAITurn()
    {
        yield return new WaitForSeconds(1.5f);
        EndTurn();
    }

}
