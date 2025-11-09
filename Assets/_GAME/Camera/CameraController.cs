using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomSpeed = 0.1f;

    private Transform vcamTransform;

    private void Awake()
    {
        vcamTransform = vcam.transform;
    }

    public void Pan(Vector2 delta)
    {
        // For XZ plane with camera looking down: keep Y constant, move XZ
        Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(delta.x, delta.y, Camera.main.nearClipPlane)) - 
                             Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        vcamTransform.position += new Vector3(worldDelta.x, 0, worldDelta.z);
    }

    public void Zoom(float delta)
    {
        var lens = vcam.Lens;
        lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize - delta * zoomSpeed, minZoom, maxZoom);
        vcam.Lens = lens;
    }

    public void ZoomAtPoint(float delta, Vector2 screenPoint)
    {
        Vector3 worldBefore = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane));
        
        Zoom(delta);
        
        Vector3 worldAfter = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane));
        Vector3 offset = worldBefore - worldAfter;
        vcamTransform.position += new Vector3(offset.x, 0, offset.z);
    }
}

