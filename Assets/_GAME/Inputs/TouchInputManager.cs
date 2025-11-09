using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TouchInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
    [SerializeField] protected GameStateEvents gameStateEvents;
    [SerializeField] protected Grid grid;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private CameraController cameraController;
    private MyInputActions inputs;
    private Vector2Int? lastTouchedTile;
    private Vector2Int? dragStartTile;
    private Vector2Int? lastDragTile;
    private float touchStartTime;
    private const float LONG_PRESS_DELAY = 1f;
    private const float DRAG_THRESHOLD = 20f; // pixels
    private bool hasEmittedHover;
    private bool isDragging;
    private bool isPanning;
    private bool isSelected;
    private Vector2Int? selectedTile;
    private Vector2 lastTouchPosition;
    private bool isSecondTouchActive;
    private Vector2 lastPinchCenter;
    private float lastPinchDistance;

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
            Debug.LogWarning($"[Touch] Contact started at {touchPos}, CameraController={(cameraController != null ? "OK" : "NULL")}");
            var tile = GetTileXZ(touchPos);
            
            lastTouchPosition = touchPos;
            touchStartTime = Time.time;
            hasEmittedHover = false;
            isDragging = false;
            isPanning = false;
            
            // Only set dragStartTile if it's a valid tile with terrain
            if (tile.HasValue && Map.Instance.HasTerrainAt(tile.Value))
            {
                lastTouchedTile = tile;
                dragStartTile = tile;
                Debug.LogWarning($"[Touch] Started on valid tile {tile.Value}");
            }
            else
            {
                lastTouchedTile = null;
                dragStartTile = null;
                Debug.LogWarning($"[Touch] Started off-map or on UI at tile {tile}");
            }
        };
        
        inputs.Touch.SecondaryContact.started += _ => {
            isSecondTouchActive = true;
            isDragging = false;
            isPanning = false;
            UpdatePinch();
        };
        
        inputs.Touch.SecondaryContact.canceled += _ => {
            isSecondTouchActive = false;
            lastPinchDistance = 0;
        };
        inputs.Touch.PrimaryContact.canceled += _ => {
            if (isPanning)
            {
                isPanning = false;
            }
            else if (isDragging && dragStartTile.HasValue)
            {
                var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
                var endTile = GetTileXZ(touchPos);
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
                if (Map.Instance.HasTerrainAt(lastTouchedTile.Value))
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
                else
                {
                    Debug.Log($"{GetType().Name}: Touched empty area at {lastTouchedTile.Value}, ignoring");
                }
            }
            lastTouchedTile = null;
        };
        
        gameStateEvents.OnTileSelected += pos => {
            isSelected = true;
            selectedTile = pos;
        };
        gameStateEvents.OnTileDeselected += _ => {
            isSelected = false;
            selectedTile = null;
        };
    }

    private void Update()
    {
        if (isSecondTouchActive)
        {
            UpdatePinch();
            return;
        }
        
        if (!isDragging && !isPanning)
        {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var distance = Vector2.Distance(touchPos, lastTouchPosition);
            
            if (distance > DRAG_THRESHOLD)
            {
                bool startedOnSelectedTile = dragStartTile.HasValue && isSelected && dragStartTile == selectedTile;
                bool startedOnUI = IsPointerOverUI(lastTouchPosition);
                
                if (startedOnSelectedTile && !startedOnUI)
                {
                    isDragging = true;
                    var currentTile = GetTileXZ(touchPos);
                    if (currentTile.HasValue)
                    {
                        events.EmitDragStarted(dragStartTile.Value, currentTile.Value);
                        lastDragTile = currentTile;
                        Debug.Log($"[Touch] Unit drag started from {dragStartTile.Value}");
                    }
                }
                else if (!startedOnUI && !dragStartTile.HasValue)
                {
                    // Only pan if we didn't start on a valid tile
                    isPanning = true;
                    lastTouchPosition = touchPos; // Reset to current position to prevent jump
                    Debug.LogWarning($"[Touch] Pan started, distance={distance}, dragStartTile={dragStartTile}");
                }
                else
                {
                    Debug.Log($"[Touch] No action: startedOnUI={startedOnUI}, dragStartTile={dragStartTile}, isSelected={isSelected}");
                }
            }
        }
        else if (isPanning)
        {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var delta = touchPos - lastTouchPosition;
            if (cameraController != null)
            {
                cameraController.Pan(-delta);
            }
            else
            {
                Debug.LogError("[Touch] CameraController is NULL during pan!");
            }
            lastTouchPosition = touchPos;
        }
        else if (isDragging && dragStartTile.HasValue)
        {
            var touchPos = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
            var currentTile = GetTileXZ(touchPos);
            if (currentTile.HasValue && currentTile != lastDragTile)
            {
                events.EmitDragUpdated(dragStartTile.Value, currentTile.Value);
                lastDragTile = currentTile;
            }
        }
    }
    
    private void UpdatePinch()
    {
        var pos1 = inputs.Touch.PrimaryPosition.ReadValue<Vector2>();
        var pos2 = inputs.Touch.SecondaryPosition.ReadValue<Vector2>();
        
        var pinchCenter = (pos1 + pos2) / 2f;
        var pinchDistance = Vector2.Distance(pos1, pos2);
        
        if (lastPinchDistance > 0)
        {
            var delta = pinchDistance - lastPinchDistance;
            cameraController?.ZoomAtPoint(delta * 0.01f, pinchCenter);
        }
        
        lastPinchCenter = pinchCenter;
        lastPinchDistance = pinchDistance;
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
