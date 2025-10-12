using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

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

    public Vector2Int? GetTileXY(Vector2 screenPos)
    {
        // TODO check if UI.  (over IsPointerOverGameObject)
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
        worldPosition.z = 0;
        var cell = grid.WorldToCell(worldPosition);
        return (Vector2Int)cell;
    }
} 
