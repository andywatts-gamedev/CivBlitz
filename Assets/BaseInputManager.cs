using Unity.Mathematics;
using UnityEngine;
using System.Linq;
public abstract class BaseInputManager : MonoBehaviour
{
    [SerializeField] protected LayerMask colliderMask;
    public GameObject sourceVisual, targetVisual;
    public Grid grid;
    public bool isSelectingSource;
    public int2? source, target = null;
    public GameEventSourceTarget moveEvent;
    public GameEventSourceTarget attackEvent;

    [SerializeField] private GameEventInt2 sourceHighlight;
    [SerializeField] private GameEventInt2 targetHighlight;

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

    protected int2? GetTileAtPosition(Vector2 screenPos)
    {
        var worldPos = GetWorldPosition(screenPos);
        if (worldPos == Vector3.zero) return null;
        var cell = grid.WorldToCell(worldPos);
        return Map.Instance.IsWithinBounds(new int2(cell.x, cell.y)) ? new int2(cell.x, cell.y) : null;
    }

    protected void SetSource(int2 tile, bool isSelection = false)
    {
        source = tile;
        sourceVisual.SetActive(true);
        sourceVisual.transform.position = grid.CellToWorld(new Vector3Int(tile.x, tile.y, 0)) + new Vector3(0, 0.05f, 0);
        sourceHighlight.Invoke(tile);

        // var unit = Map.Instance.GetUnitGO(tile);
        // if (unit && unit.GetComponent<UnitMono>().team == GameManager.Instance.team)
        //     isSelectingSource = !isSelection;
    }

    protected void UnsetSource()
    {
        isSelectingSource = true;
        sourceVisual.SetActive(false);
        source = null;
        sourceHighlight.Invoke(null);
    }

    protected void SetTarget(int2 tile)
    {
        target = tile;
        targetVisual.SetActive(true);
        targetVisual.transform.position = grid.CellToWorld(new Vector3Int(tile.x, tile.y, 0)) + new Vector3(0, 0.05f, 0);
        targetHighlight.Invoke(tile);
    }

    protected void UnsetTarget()
    {
        targetVisual.SetActive(false);
        target = null;
        targetHighlight.Invoke(null);
    }

    protected Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit, 999f, colliderMask) ? hit.point : Vector3.zero;
    }
} 