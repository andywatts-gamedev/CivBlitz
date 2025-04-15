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
            var mousePos = Mouse.current.position.ReadValue();
            var tile = GetTileXY(mousePos);
            if (tile.HasValue) events.EmitTileClicked(tile.Value);
        };
        inputs.Mouse.Cancel.performed += _ => events.EmitCancel();
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}
