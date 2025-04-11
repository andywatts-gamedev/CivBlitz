using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TouchInputManager : BaseInputManager
{

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Touch.PrimaryContact.started += _ => {
            var tile = GetTileXY(inputs.Touch.PrimaryPosition.ReadValue<Vector2>());
            if (tile.HasValue) events.EmitTileSelected(tile.Value);
        };
        inputs.Touch.PrimaryContact.canceled += _ => events.EmitCancel();
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
