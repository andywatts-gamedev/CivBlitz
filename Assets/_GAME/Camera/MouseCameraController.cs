using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraController : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    private MyInputActions inputs;

    private void Awake()
    {
        inputs = new MyInputActions();
    }

    private void Update()
    {
        var scroll = inputs.Mouse.Scroll.ReadValue<Vector2>();
        if (scroll.y != 0)
        {
            cameraController?.Zoom(scroll.y * 0.1f);
        }
    }

    private void OnEnable() => inputs.Enable();
    private void OnDisable() => inputs.Disable();
}

