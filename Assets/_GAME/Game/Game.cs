using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;


public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [SerializeField] private InputEvents events;
    [SerializeField] private GameObject highlight;
    private Vector2Int? selectedTile;

    public CivilizationSCOB player;
    public CivilizationSCOB ai;
    
    [ShowInInspector]
    public Dictionary<Civilization, CivilizationSCOB> civilizations;

    public Vector3 flagScale = new Vector3(2f, 2f, 2f);
    public Vector3 unitScale = new Vector3(0.8f, 0.8f, 0.8f);

    void Awake() => Instance = this;

    void Start()
    {
        events.OnTileClicked += HandleTileClicked;
        events.OnCancel += HandleCancel;
        highlight.SetActive(false);
        civilizations = Resources.LoadAll<CivilizationSCOB>("").ToDictionary(c => c.civilization, c => c);
    }

    void OnDisable()
    {
        events.OnTileClicked -= HandleTileClicked;
        events.OnCancel -= HandleCancel;
    }

    private void HandleTileClicked(Vector2Int pos)
    {
        Debug.Log("HandleTileClicked: " + pos);
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
                Debug.Log("Valid move: " + selectedTile.Value + " -> " + pos);
                MoveTo(selectedTile.Value, pos);
                events.EmitTileDeselected(selectedTile.Value);
                selectedTile = null;
                highlight.SetActive(false);
                return;
            }
        }

        if (UnitManager.Instance.TryGetUnit(pos, out var unit) && unit.civ == player.civilization)
        {
            Debug.Log("Unit clicked: " + pos);
            if (selectedTile.HasValue)
                events.EmitTileDeselected(selectedTile.Value);
            
            selectedTile = pos;
            events.EmitTileSelected(pos);
            highlight.SetActive(true);
            highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)pos);
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
            
        if (unit.movesLeft <= 0)
            return false;
            
        if (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving)
            return false;

        // Get terrain at target
        var terrainTile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)to) as TerrainTile;
        if (terrainTile == null) return false;
        
        // Check if unit can travel on terrain
        var terrain = terrainTile.terrainScob.terrain;
        var canTravel = terrain.type switch {
            TerrainType.Ocean => unit.unit.canTravelOcean,
            TerrainType.Coast => unit.unit.canTravelCoast,
            _ => unit.unit.canTravelLand
        };
        if (!canTravel) return false;
        
        // Check movement cost
        if (unit.movesLeft < terrain.movementCost)
            return false;
            
        var validMoves = HexGrid.GetValidMoves(from, unit.unit.movement, UnitManager.Instance.unitTilemap);
        if (!validMoves.Contains(to))
            return false;

        if (UnitManager.Instance.TryGetUnit(to, out var target) && target.civ == unit.civ)
            return false;
            
        return true;
    }

    public void MoveTo(Vector2Int from, Vector2Int to)
    {
        Debug.Log("MoveTo: " + from + " -> " + to);
        if (!IsValidMove(from, to))
            return;
            
        var unit = UnitManager.Instance.units[from];
        
        if (UnitManager.Instance.TryGetUnit(to, out var target))
        {
            if (CombatManager.Instance.TryCombat(from, to))
                return;
            return;
        }
            
        unit.movesLeft--;
        UnitManager.Instance.MoveUnit(from, to);
        
        // Check if any player units can still move
        if (!UnitManager.Instance.units.Any(u => u.Value.civ == Game.Instance.player.civilization && u.Value.movesLeft > 0))
            TurnManager.Instance.EndTurn();
    }
} 