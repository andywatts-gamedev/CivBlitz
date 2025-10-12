using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TouchInputManager : MonoBehaviour
{
    [SerializeField] protected InputEvents events;
    [SerializeField] protected Grid grid;
    private MyInputActions inputs;
    private Vector2Int? lastTouchedTile;
    private float touchStartTime;
    private const float LONG_PRESS_DELAY = 1f;
    private bool hasEmittedHover;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Touch.PrimaryContact.started += _ => {
            var tile = GetTileXY(inputs.Touch.PrimaryPosition.ReadValue<Vector2>());
            if (tile.HasValue)
            {
                Debug.Log($"{GetType().Name}: Touch started on tile {tile.Value}");
                lastTouchedTile = tile;
                touchStartTime = Time.time;
                hasEmittedHover = false;
            }
        };
        inputs.Touch.PrimaryContact.canceled += _ => {
            if (lastTouchedTile.HasValue)
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
                lastTouchedTile = null;
            }
            Debug.Log($"{GetType().Name}: Touch ended");
            events.EmitCancel();
        };
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
