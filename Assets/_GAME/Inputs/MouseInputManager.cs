using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MouseInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
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
            var tile = GetTileXY(mousePos);
            if (tile.HasValue)
            {
                Debug.Log($"{GetType().Name}: Clicked on tile {tile.Value}");
                events.EmitTileClicked(tile.Value);
            }
        };
        inputs.Mouse.Cancel.performed += _ => {
            Debug.Log($"{GetType().Name}: Right click detected");
            isSelected = false;
            events.EmitCancel();
        };
        
        events.OnTileSelected += pos => {
            Debug.Log($"{GetType().Name}: Tile {pos} selected");
            isSelected = true;
        };
        events.OnTileDeselected += _ => {
            Debug.Log($"{GetType().Name}: Tile deselected");
            isSelected = false;
        };
    }

    private void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();
        
        if (Vector2.Distance(mousePos, lastMousePos) > 0.1f)
        {
            var tile = GetTileXY(mousePos);
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
        var tile = GetTileXY(screenPos);
        
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

    protected Vector2Int? GetTileXY(Vector2 screenPos)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
        worldPosition.z = 0;
        var cell = grid.WorldToCell(worldPosition);
        return (Vector2Int)cell;
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
