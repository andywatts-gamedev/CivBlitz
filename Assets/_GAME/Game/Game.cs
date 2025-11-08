using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;


public class Game : Singleton<Game>
{
    [SerializeField] private InputEvents events;
    [SerializeField] private GameStateEvents gameStateEvents;
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

    protected override void Awake()
    {
        base.Awake();
        civilizations = Resources.LoadAll<CivilizationSCOB>("Civilizations").ToDictionary(c => c.civilization, c => c);
        Debug.Log("Civilizations: " + civilizations.Count);
    }

    void Start()
    {
        events.OnTileClicked += HandleTileClicked;
        gameStateEvents.OnTileSelected += HandleTileSelected;
        events.OnCancel += HandleCancel;
        events.OnDragStarted += HandleDragStarted;
        events.OnDragUpdated += HandleDragUpdated;
        events.OnDragEnded += HandleDragEnded;
        highlight.SetActive(false);
        if (pathPreview != null) pathPreview.SetActive(false);
    }

    void OnDisable()
    {
        events.OnTileClicked -= HandleTileClicked;
        gameStateEvents.OnTileSelected -= HandleTileSelected;
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
            gameStateEvents.EmitTileDeselected(pos);
            selectedTile = null;
            highlight.SetActive(false);
            return;
        }

        // If clicking a different tile, deselect current selection first
        if (selectedTile.HasValue)
        {
            gameStateEvents.EmitTileDeselected(selectedTile.Value);
            selectedTile = null;
            highlight.SetActive(false);
        }

        // Always select any clicked tile and show its data
        selectedTile = pos;
        gameStateEvents.EmitTileSelected(pos);
        
        // Show outline for any selected tile
        highlight.SetActive(true);
        highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)pos);
    }

    private void HandleTileSelected(Vector2Int pos)
    {
        Debug.Log("HandleTileSelected: " + pos);
        
        // If selecting a different tile, deselect current selection first
        if (selectedTile.HasValue && selectedTile.Value != pos)
        {
            gameStateEvents.EmitTileDeselected(selectedTile.Value);
            selectedTile = null;
            highlight.SetActive(false);
        }

        // Select the new tile and show highlight
        selectedTile = pos;
        highlight.SetActive(true);
        highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)pos);
    }

    private void HandleCancel()
    {
        if (selectedTile.HasValue)
        {
            gameStateEvents.EmitTileDeselected(selectedTile.Value);
            selectedTile = null;
            highlight.SetActive(false);
        }
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        if (!UnitManager.Instance.TryGetUnit(from, out var unit))
            return false;
            
        if (unit.actionsLeft <= 0)
            return false;
            
        if (UnitManager.Instance.isMoving || CombatManager.Instance.isCombatMoving)
            return false;

        // Get terrain at target
        var terrainTile = UnitManager.Instance.terrainTilemap.GetTile((Vector3Int)to) as TerrainTile;
        if (terrainTile == null)
            return false;
        
        // Check if unit can travel on terrain
        var terrain = terrainTile.terrainScob.terrain;
        var canTravel = terrain.type switch {
            TerrainType.Ocean => unit.unit.canTravelOcean,
            TerrainType.Coast => unit.unit.canTravelCoast,
            _ => unit.unit.canTravelLand
        };
        if (!canTravel)
            return false;
        
        // Check movement cost
        if (unit.actionsLeft < terrain.movementCost)
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
        
        if (UnitManager.Instance.TryGetUnit(to, out var target))
        {
            if (CombatManager.Instance.TryCombat(from, to))
                return;
            return;
        }
            
        UnitManager.Instance.MoveUnit(from, to);
        UnitManager.Instance.EmitMovesConsumed();
    }

    private void HandleDragStarted(Vector2Int fromTile, Vector2Int toTile)
    {
        Debug.Log($"Drag started: {fromTile} -> {toTile}");
        if (!UnitManager.Instance.TryGetUnit(fromTile, out var unit) || 
            unit.civ != player.civilization || 
            unit.state != UnitState.Ready)
            return;
            
        isDragging = true;
        selectedTile = fromTile;
        gameStateEvents.EmitTileSelected(fromTile);
        highlight.SetActive(true);
        highlight.transform.position = UnitManager.Instance.flags[player.civilization].CellToWorld((Vector3Int)fromTile);
        
        UpdatePathPreview(fromTile, toTile);
    }

    private void HandleDragUpdated(Vector2Int fromTile, Vector2Int toTile)
    {
        if (!isDragging) return;
        Debug.Log($"Drag updated: {fromTile} -> {toTile}");
        UpdatePathPreview(fromTile, toTile);
        
        // Show hover panel for enemy units during drag
        if (UnitManager.Instance.TryGetUnit(toTile, out var targetUnit) && targetUnit.civ != player.civilization)
        {
            Debug.Log($"Dragging over enemy unit: {targetUnit.unit.name}");
            events.EmitTileHovered(toTile);
        }
        else
        {
            // Clear hover panel when not dragging over enemy units
            events.EmitHoverCleared();
        }
    }

    private void HandleDragEnded(Vector2Int fromTile, Vector2Int toTile)
    {
        Debug.Log($"Drag ended: {fromTile} -> {toTile}");
        isDragging = false;
        
        if (pathPreview != null) pathPreview.SetActive(false);
        
        // Clear hover panel when drag ends
        events.EmitHoverCleared();
        
        if (IsValidMove(fromTile, toTile))
        {
            MoveTo(fromTile, toTile);
            gameStateEvents.EmitTileDeselected(fromTile);
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
        Debug.Log($"UpdatePathPreview: {fromTile} -> {toTile}");
        
        // Create pathPreview GameObject if it doesn't exist
        if (pathPreview == null)
        {
            pathPreview = new GameObject("PathPreview");
            pathPreview.transform.SetParent(transform);
            Debug.Log("Created pathPreview GameObject");
        }
        
        var isValid = IsValidMove(fromTile, toTile);
        Debug.Log($"Move valid: {isValid}");
        pathPreview.SetActive(true);
        
        // Use the terrain tilemap for consistent world positioning
        var startPos = UnitManager.Instance.terrainTilemap.CellToWorld((Vector3Int)fromTile);
        var endPos = UnitManager.Instance.terrainTilemap.CellToWorld((Vector3Int)toTile);
        
        // Create or get line renderer
        var lineRenderer = pathPreview.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = pathPreview.AddComponent<LineRenderer>();
            
            // Use URP-compatible material
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            lineRenderer.material = new Material(shader);
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 10; // Make sure it renders on top
            
            // Mobile compatibility settings for proper width taper
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.generateLightingData = false;
            lineRenderer.numCapVertices = 4; // Smooth caps
            lineRenderer.numCornerVertices = 4; // Smooth corners
            
            // Ensure proper rendering on mobile
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            
            // Force width curve for mobile - some mobile GPUs ignore startWidth/endWidth
            var widthCurve = new AnimationCurve();
            widthCurve.AddKey(0f, 0.3f); // Start width
            widthCurve.AddKey(1f, 0.1f); // End width
            lineRenderer.widthCurve = widthCurve;
        }
        
        // Set positions with slight offset to avoid z-fighting
        startPos.z = -0.1f;
        endPos.z = -0.1f;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        
        // Set colors based on validity with alpha for better visibility
        var color = isValid ? new Color(0, 1, 0, 0.8f) : new Color(1, 0, 0, 0.8f);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
} 