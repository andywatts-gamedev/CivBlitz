using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MouseInputManager : BaseInputManager
{
    private MyInputActions inputActions;

    void Awake() => inputActions = new MyInputActions();
    void Start()
    {
        inputActions.Mouse.Position.performed += OnPosition;
        inputActions.Mouse.Click.performed += OnClick;
        inputActions.Mouse.Cancel.performed += OnCancel;
        UnsetSource();
        UnsetTarget();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    private void OnPosition(InputAction.CallbackContext context)
    {
        var tile = GetTileAtPosition(Mouse.current.position.ReadValue());

        if (isSelectingSource)
        {
            if (!tile.HasValue) UnsetSource();
            else if (!source.Equals(tile.Value)) SetSource(tile.Value);
        }
        else 
        {
            if (!tile.HasValue || !IsWithinRange(source.Value, tile.Value) || tile.Value.Equals(source.Value)) UnsetTarget();
            else SetTarget(tile.Value);
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        var tile = GetTileAtPosition(Mouse.current.position.ReadValue());
        if (isSelectingSource)
        {
            SetSource(tile.Value, true);
            return;
        }

        // if (!isSelectingSource && target.HasValue)
        // {
        //     var unitMono = Map.Instance.GetUnitMono(target.Value);
        //     if (!unitMono)
        //         moveEvent.Invoke(source.Value, target.Value);
        //     else
        //         attackEvent.Invoke(source.Value, target.Value);
        // }

        UnsetSource();
        UnsetTarget();
    }   

    private void OnCancel(InputAction.CallbackContext context)
    {
        UnsetSource();
        UnsetTarget();
    }
}
