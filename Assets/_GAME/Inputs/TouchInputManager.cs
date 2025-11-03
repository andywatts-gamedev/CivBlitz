using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TouchInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
    [SerializeField] protected Grid grid;
    [SerializeField] private UIDocument uiDocument;
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
        // Find UIDocument if not assigned
        if (uiDocument == null)
        {
            uiDocument = FindFirstObjectByType<UIDocument>();
            if (uiDocument != null)
            {
                Debug.Log($"TouchInputManager: Found UIDocument on {uiDocument.gameObject.name}, panel={uiDocument.rootVisualElement.panel}");
            }
            else
            {
                Debug.LogWarning("TouchInputManager: No UIDocument found in scene");
            }
        }
        
        inputs = new MyInputActions();
        inputs.Touch.PrimaryContact.started += _ => {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var tile = GetTileXY(touchPos);
            if (tile.HasValue)
            {
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
                    events.EmitTileHovered(lastTouchedTile.Value);
                }
                else
                {
                    events.EmitTileClicked(lastTouchedTile.Value);
                }
            }
            lastTouchedTile = null;
        };
        
        events.OnTileSelected += pos => {
            isSelected = true;
            selectedTile = pos;
        };
        events.OnTileDeselected += _ => {
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
                events.EmitDragUpdated(dragStartTile.Value, currentTile.Value);
                lastDragTile = currentTile;
            }
        }
    }

    protected Vector2Int? GetTileXY(Vector2 screenPos)
    {
        // Check if click hit UI first
        if (IsPointerOverUI(screenPos))
        {
            return null;
        }
            
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
        worldPosition.z = 0;
        var cell = grid.WorldToCell(worldPosition);
        return (Vector2Int)cell;
    }

    protected bool IsPointerOverUI(Vector2 screenPos)
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return false;
        }
        
        var root = uiDocument.rootVisualElement;
        var panel = root.panel;
        if (panel == null)
        {
            return false;
        }
        
        // Unity Input System uses bottom-left origin, but UI Toolkit uses top-left origin
        // We need to flip the Y coordinate before converting to panel space
        Vector2 flippedScreenPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
        var panelPos = RuntimePanelUtils.ScreenToPanel(panel, flippedScreenPos);
        
        // Use panel.Pick() - this respects picking-mode in USS and UXML
        var pickedElement = panel.Pick(panelPos);
        
        return pickedElement != null && pickedElement != root;
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
