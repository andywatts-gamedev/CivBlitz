using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TouchInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
    [SerializeField] protected Grid grid;
    private MyInputActions inputs;
    private Vector2Int? lastTouchedTile;
    private Vector2Int? dragStartTile;
    private Vector2Int? lastDragTile;
    private float touchStartTime;
    private const float LONG_PRESS_DELAY = 1f;
    private const float DRAG_THRESHOLD = 20f; // pixels
    private bool hasEmittedHover;
    private bool isDragging;
    private bool isSelected;
    private Vector2Int? selectedTile;
    private Vector2 lastTouchPosition;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Touch.PrimaryContact.started += _ => {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var tile = GetTileXY(touchPos);
            if (tile.HasValue)
            {
                Debug.Log($"{GetType().Name}: Touch started on tile {tile.Value}");
                lastTouchedTile = tile;
                dragStartTile = tile;
                lastTouchPosition = touchPos;
                touchStartTime = Time.time;
                hasEmittedHover = false;
                isDragging = false;
            }
        };
        inputs.Touch.PrimaryContact.canceled += _ => {
            if (isDragging && dragStartTile.HasValue)
            {
                var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
                var endTile = GetTileXY(touchPos);
                if (endTile.HasValue)
                {
                    Debug.Log($"{GetType().Name}: Drag ended from {dragStartTile.Value} to {endTile.Value}");
                    events.EmitDragEnded(dragStartTile.Value, endTile.Value);
                }
                isDragging = false;
                dragStartTile = null;
                lastDragTile = null;
            }
            else if (lastTouchedTile.HasValue)
            {
                var pressTime = Time.time - touchStartTime;
                if (pressTime >= LONG_PRESS_DELAY)
                {
                    Debug.Log($"{GetType().Name}: Long press detected on tile {lastTouchedTile.Value} after {pressTime:F1}s");
                    events.EmitTileHovered(lastTouchedTile.Value);
                }
                else
                {
                    Debug.Log($"{GetType().Name}: Short press detected on tile {lastTouchedTile.Value} after {pressTime:F1}s");
                    events.EmitTileClicked(lastTouchedTile.Value);
                }
            }
            lastTouchedTile = null;
        };
        
        events.OnTileSelected += pos => {
            Debug.Log($"{GetType().Name}: Tile {pos} selected");
            isSelected = true;
            selectedTile = pos;
        };
        events.OnTileDeselected += _ => {
            Debug.Log($"{GetType().Name}: Tile deselected");
            isSelected = false;
            selectedTile = null;
        };
    }

    private void Update()
    {
        if (lastTouchedTile.HasValue && !isDragging)
        {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var distance = Vector2.Distance(touchPos, lastTouchPosition);
            
            if (distance > DRAG_THRESHOLD && dragStartTile.HasValue)
            {
                isDragging = true;
                var currentTile = GetTileXY(touchPos);
                if (currentTile.HasValue)
                {
                    Debug.Log($"{GetType().Name}: Drag started from {dragStartTile.Value} to {currentTile.Value}");
                    events.EmitDragStarted(dragStartTile.Value, currentTile.Value);
                    lastDragTile = currentTile;
                }
            }
        }
        else if (isDragging && dragStartTile.HasValue)
        {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var currentTile = GetTileXY(touchPos);
            if (currentTile.HasValue && currentTile != lastDragTile)
            {
                Debug.Log($"{GetType().Name}: Drag updated from {dragStartTile.Value} to {currentTile.Value}");
                events.EmitDragUpdated(dragStartTile.Value, currentTile.Value);
                lastDragTile = currentTile;
            }
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
