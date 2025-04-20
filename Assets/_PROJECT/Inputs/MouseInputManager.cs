using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MouseInputManager : BaseInputManager
{
    private MyInputActions inputs;
    private Vector2Int? lastHoveredTile;
    private Vector2 lastMousePos;
    private float hoverStartTime;
    private const float HOVER_DELAY = 1f;
    [SerializeField] private UI ui;
    private bool isSelected;
    private bool hasEmittedHover;
    private bool isHoverSelection;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Mouse.Click.performed += _ => {
            var mousePos = Mouse.current.position.ReadValue();
            var tile = GetTileXY(mousePos);
            if (tile.HasValue) events.EmitTileClicked(tile.Value);
        };
        inputs.Mouse.Cancel.performed += _ => {
            isSelected = false;
            hasEmittedHover = false;
            isHoverSelection = false;
            events.EmitCancel();
        };
        
        events.OnTileSelected += pos => {
            isSelected = true;
            hasEmittedHover = true;
        };
        events.OnTileDeselected += _ => {
            isSelected = false;
            hasEmittedHover = false;
            isHoverSelection = false;
        };
    }

    private void Start()
    {
        isSelected = false; // Ensure initial state
        hasEmittedHover = false;
        isHoverSelection = false;
    }

    private void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();
        var tile = GetTileXY(mousePos);
        
        if (Vector2.Distance(mousePos, lastMousePos) > 0.1f)
        {
            if (!isSelected)
            {
                ui.HideTile();
                hasEmittedHover = false;
                isHoverSelection = false;
                hoverStartTime = Time.time;
            }
        }
        lastMousePos = mousePos;
        
        if (tile.HasValue)
        {
            if (lastHoveredTile.HasValue && lastHoveredTile.Value == tile.Value)
            {
                if (!hasEmittedHover && Time.time - hoverStartTime >= HOVER_DELAY)
                {
                    isHoverSelection = true;
                    ui.ShowTile(tile.Value);
                }
            }
            else
            {
                if (isSelected && isHoverSelection && lastHoveredTile.HasValue)
                {
                    events.EmitTileDeselected(lastHoveredTile.Value);
                }
                lastHoveredTile = tile;
                hoverStartTime = Time.time;
                hasEmittedHover = false;
                isHoverSelection = false;
            }
        }
        else
        {
            if (isSelected && isHoverSelection && lastHoveredTile.HasValue)
            {
                events.EmitTileDeselected(lastHoveredTile.Value);
            }
            lastHoveredTile = null;
            hasEmittedHover = false;
            isHoverSelection = false;
        }
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
