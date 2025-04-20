using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TouchInputManager : BaseInputManager
{
    private Vector2Int? lastTouchedTile;
    private float touchStartTime;
    private const float LONG_PRESS_DELAY = 1f;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Touch.PrimaryContact.started += _ => {
            var tile = GetTileXY(inputs.Touch.PrimaryPosition.ReadValue<Vector2>());
            if (tile.HasValue)
            {
                lastTouchedTile = tile;
                touchStartTime = Time.time;
            }
        };
        inputs.Touch.PrimaryContact.canceled += _ => {
            if (lastTouchedTile.HasValue)
            {
                if (Time.time - touchStartTime >= LONG_PRESS_DELAY)
                {
                    events.EmitTileSelected(lastTouchedTile.Value);
                }
                else
                {
                    events.EmitTileClicked(lastTouchedTile.Value);
                }
                lastTouchedTile = null;
            }
            events.EmitCancel();
        };
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
