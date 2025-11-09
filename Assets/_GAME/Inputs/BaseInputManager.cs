using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public abstract class BaseInputManager : MonoBehaviour
{
    protected MyInputActions inputs;
    [SerializeField] protected InputEvents events;
    [SerializeField] protected Grid grid;


    protected bool IsWithinRange(int2 src, int2 tgt)
    {
        var offsets = src.y % 2 == 0 ? new[] {
            new int2(-1,0), new int2(0,-1), new int2(1,-1),
            new int2(1,0), new int2(0,1), new int2(-1,1)
        } : new[] {
            new int2(-1,-1), new int2(0,-1), new int2(1,-1),
            new int2(1,0), new int2(0,1), new int2(-1,0)
        };
        return offsets.Any(o => (src + o).Equals(tgt));
    }

    public Vector2Int? GetTileXZ(Vector2 screenPos)
    {
        // Check if click hit UI first
        if (IsPointerOverUI(screenPos))
        {
            return null;
        }
        
        // Raycast to XZ plane at Y=0
        var ray = Camera.main.ScreenPointToRay(screenPos);
        var plane = new Plane(Vector3.up, Vector3.zero);
        
        if (plane.Raycast(ray, out float distance))
        {
            var worldPosition = ray.GetPoint(distance);
            var cell = grid.WorldToCell(worldPosition);
            // Grid has CellSwizzle=XZY, so cell.y is actually world Z
            return new Vector2Int(cell.x, cell.y);
        }
        
        return null;
    }

    protected bool IsPointerOverUI(Vector2 screenPos)
    {
        // Unity 2025+ documentation confirms IsPointerOverGameObject() works with UI Toolkit
        if (EventSystem.current == null)
        {
            Debug.LogWarning($"{GetType().Name}: No EventSystem found in scene!");
            return false;
        }
        
        bool hitUI = EventSystem.current.IsPointerOverGameObject();
        Debug.Log($"{GetType().Name}: IsPointerOverGameObject() = {hitUI} at {screenPos}");
        
        if (hitUI)
        {
            Debug.Log($"{GetType().Name}: UI hit detected at {screenPos} (IsPointerOverGameObject)");
        }
        return hitUI;
    }
} 
