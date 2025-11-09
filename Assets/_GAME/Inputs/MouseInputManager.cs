using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;

public class MouseInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
    [SerializeField] protected GameStateEvents gameStateEvents;
    [SerializeField] protected Grid grid;
    private MyInputActions inputs;
    private Vector2 lastMousePos;
    private bool isSelected;
    private Vector2Int? lastTile;
    private float hoverStartTime;
    private const float HOVER_DELAY = 1f;
    private bool hasEmittedHover;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Mouse.Click.performed += _ => {
            var mousePos = Mouse.current.position.ReadValue();
            var tile = GetTileXZ(mousePos);
            if (tile.HasValue)
            {
                if (Map.Instance.HasTerrainAt(tile.Value))
                {
                    Debug.Log($"{GetType().Name}: Clicked on tile {tile.Value}");
                    events.EmitTileClicked(tile.Value);
                }
                else
                {
                    Debug.Log($"{GetType().Name}: Clicked on empty area at {tile.Value}, ignoring");
                }
            }
            else
            {
                Debug.Log($"{GetType().Name}: Clicked on UI element, ignoring");
            }
        };
        inputs.Mouse.Cancel.performed += _ => {
            Debug.Log($"{GetType().Name}: Right click detected");
            isSelected = false;
            events.EmitCancel();
        };
        
        gameStateEvents.OnTileSelected += pos => {
            Debug.Log($"{GetType().Name}: Tile {pos} selected");
            isSelected = true;
        };
        gameStateEvents.OnTileDeselected += _ => {
            Debug.Log($"{GetType().Name}: Tile deselected");
            isSelected = false;
        };
    }

    private void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();
        
        if (Vector2.Distance(mousePos, lastMousePos) > 0.1f)
        {
            var tile = GetTileXZ(mousePos);
            UpdateTilePosition(mousePos);
            // Debug.Log($"{GetType().Name}: Mouse moved");
            events.EmitMouseMoved();
            events.EmitPointerMovedToTile(tile);
        }
        else if (lastTile.HasValue)
        {
            CheckHoverTime();
        }
        
        lastMousePos = mousePos;
    }

    private void UpdateTilePosition(Vector2 screenPos)
    {
        var tile = GetTileXZ(screenPos);
        
        if (tile.HasValue)
        {
            if (lastTile.HasValue && lastTile.Value != tile.Value)
            {
                Debug.Log($"{GetType().Name}: Moved from tile {lastTile.Value} to {tile.Value}");
                lastTile = tile;
                hoverStartTime = Time.time;
                hasEmittedHover = false;
            }
            else if (!lastTile.HasValue)
            {
                Debug.Log($"{GetType().Name}: Started hovering tile {tile.Value}");
                lastTile = tile;
                hoverStartTime = Time.time;
                hasEmittedHover = false;
            }
        }
        else if (lastTile.HasValue)
        {
            Debug.Log($"{GetType().Name}: Stopped hovering tile {lastTile.Value}");
            lastTile = null;
            hasEmittedHover = false;
        }
    }

    private void CheckHoverTime()
    {
        var hoverTime = Time.time - hoverStartTime;
        if (!hasEmittedHover && hoverTime >= HOVER_DELAY)
        {
            hasEmittedHover = true;
            Debug.Log($"{GetType().Name}: Emitting TileHovered for tile {lastTile.Value} after {hoverTime:F1}s");
            events.EmitTileHovered(lastTile.Value);
        }
    }

    protected Vector2Int? GetTileXZ(Vector2 screenPos)
    {
        // Check if click hit UI first
        if (IsPointerOverUI(screenPos))
        {
            return null;
        }
        
        // Raycast to XZ plane at Y=0
        var ray = Camera.main.ScreenPointToRay(screenPos);
        var plane = new Plane(Vector3.up, Vector3.zero);
        
        if (plane.Raycast(ray, out float distance))
        {
            var worldPosition = ray.GetPoint(distance);
            var cell = grid.WorldToCell(worldPosition);
            // Grid has CellSwizzle=XZY, so cell.y is actually world Z
            return new Vector2Int(cell.x, cell.y);
        }
        
        return null;
    }

    protected bool IsPointerOverUI(Vector2 screenPos)
    {
        // Unity 2025+ documentation confirms IsPointerOverGameObject() works with UI Toolkit
        if (EventSystem.current == null)
        {
            return false;
        }
        
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
