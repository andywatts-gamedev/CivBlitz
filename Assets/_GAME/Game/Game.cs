using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;


public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [SerializeField] private InputEvents events;
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject pathPreview;
    private Vector2Int? selectedTile;
    private bool isDragging;

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
        events.OnDragStarted += HandleDragStarted;
        events.OnDragUpdated += HandleDragUpdated;
        events.OnDragEnded += HandleDragEnded;
        highlight.SetActive(false);
        if (pathPreview != null) pathPreview.SetActive(false);
        civilizations = Resources.LoadAll<CivilizationSCOB>("").ToDictionary(c => c.civilization, c => c);
    }

    void OnDisable()
    {
        events.OnTileClicked -= HandleTileClicked;
        events.OnCancel -= HandleCancel;
        events.OnDragStarted -= HandleDragStarted;
        events.OnDragUpdated -= HandleDragUpdated;
        events.OnDragEnded -= HandleDragEnded;
    }

    private void HandleTileClicked(Vector2Int pos)
    {
        Debug.Log("HandleTileClicked: " + pos);
        
        // If clicking the same selected tile, deselect it
        if (selectedTile.HasValue && selectedTile.Value == pos)
        {
            events.EmitTileDeselected(pos);
            selectedTile = null;
            highlight.SetActive(false);
            return;
        }

        // If clicking a different tile, deselect current selection first
        if (selectedTile.HasValue)
        {
            events.EmitTileDeselected(selectedTile.Value);
            selectedTile = null;
            highlight.SetActive(false);
        }

        // Always select any clicked tile and show its data
        selectedTile = pos;
        events.EmitTileSelected(pos);
        
        // Show outline for any selected tile
        highlight.SetActive(true);
        highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)pos);
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

    private void HandleDragStarted(Vector2Int fromTile, Vector2Int toTile)
    {
        Debug.Log($"Drag started: {fromTile} -> {toTile}");
        if (!UnitManager.Instance.TryGetUnit(fromTile, out var unit) || unit.civ != player.civilization)
            return;
            
        isDragging = true;
        selectedTile = fromTile;
        events.EmitTileSelected(fromTile);
        highlight.SetActive(true);
        highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)fromTile);
        
        UpdatePathPreview(fromTile, toTile);
    }

    private void HandleDragUpdated(Vector2Int fromTile, Vector2Int toTile)
    {
        if (!isDragging) return;
        Debug.Log($"Drag updated: {fromTile} -> {toTile}");
        UpdatePathPreview(fromTile, toTile);
    }

    private void HandleDragEnded(Vector2Int fromTile, Vector2Int toTile)
    {
        Debug.Log($"Drag ended: {fromTile} -> {toTile}");
        isDragging = false;
        
        if (pathPreview != null) pathPreview.SetActive(false);
        
        if (IsValidMove(fromTile, toTile))
        {
            MoveTo(fromTile, toTile);
            events.EmitTileDeselected(fromTile);
            selectedTile = null;
            highlight.SetActive(false);
        }
        else
        {
            // Invalid move - keep selection but clear preview
            Debug.Log("Invalid drag move - cancelling");
        }
    }

    private void UpdatePathPreview(Vector2Int fromTile, Vector2Int toTile)
    {
        if (pathPreview == null) return;
        
        var isValid = IsValidMove(fromTile, toTile);
        pathPreview.SetActive(true);
        
        // Simple line preview from start to end
        var startPos = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)fromTile);
        var endPos = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)toTile);
        
        // Create a simple line renderer or use existing path preview system
        var lineRenderer = pathPreview.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = pathPreview.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 2;
        }
        
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.startColor = isValid ? Color.green : Color.red;
        lineRenderer.endColor = isValid ? Color.green : Color.red;
    }
} 