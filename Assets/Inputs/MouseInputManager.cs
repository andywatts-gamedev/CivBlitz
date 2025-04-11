using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MouseInputManager : BaseInputManager
{
    private MyInputActions inputs;

    private void Awake()
    {
        inputs = new MyInputActions();
        inputs.Mouse.Click.performed += _ => {
            var tile = GetTileXY(Mouse.current.position.ReadValue());
            Debug.Log($"Click {tile.Value.x} {tile.Value.y}");
            if (tile.HasValue) events.EmitTileSelected(tile.Value);
        };
        inputs.Mouse.Cancel.performed += _ => events.EmitCancel();
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
