using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class TouchInputManager : BaseInputManager
{
    private MyInputActions inputActions;

    void Awake() => inputActions = new MyInputActions();
    void Start()
    {
        Debug.Log("Start");
        inputActions.Enable();
        inputActions.Touch.Enable();

        UnsetSource();
        UnsetTarget();

        inputActions.Touch.PrimaryPosition.performed += OnPrimaryPosition;
        inputActions.Touch.PrimaryContact.started += OnPrimaryTouchStarted;
        inputActions.Touch.PrimaryContact.canceled += OnPrimaryTouchCanceled;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    private void OnPrimaryTouchStarted(InputAction.CallbackContext context)
    {
        Debug.Log("OnPrimaryTouchStarted");
        var screenPos = inputActions.Touch.PrimaryPosition.ReadValue<Vector2>();
        var tile = GetTileAtPosition(screenPos);
        if (tile.HasValue && isSelectingSource)
        {
            SetSource(tile.Value, true);
            return;
        }
    }

    private void OnPrimaryTouchCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("OnPrimaryTouchCanceled");
        // if (target.HasValue)
        // {
        //     var unitMono = Map.Instance.GetUnitMono(target.Value);
        //     if (!unitMono)
        //         moveEvent.Invoke(source.Value, target.Value);
        //     else
        //         attackEvent.Invoke(source.Value, target.Value);
        // }
        // UnsetSource();
        // UnsetTarget();
    }

    private void OnPrimaryPosition(InputAction.CallbackContext context)
    {
        // Debug.Log("OnPrimaryPosition");
        var tile = GetTileAtPosition(context.ReadValue<Vector2>());
        if (!isSelectingSource)
        {
            if (!tile.HasValue || !IsWithinRange(source.Value, tile.Value) || tile.Value.Equals(source.Value)) 
                UnsetTarget();
            else if (tile.HasValue)
                SetTarget(tile.Value);
        }
    }

}
