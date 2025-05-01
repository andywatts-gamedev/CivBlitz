using UnityEngine;

[CreateAssetMenu(fileName = "InputEvents", menuName = "ScriptableObject/InputEvents")]
public class InputEvents : ScriptableObject
{
    // Raw input events
    public event System.Action<Vector2Int> OnTileClicked;
    public void EmitTileClicked(Vector2Int tile) => OnTileClicked?.Invoke(tile);

    public event System.Action OnCancel;
    public void EmitCancel() => OnCancel?.Invoke();
    
    public event System.Action<Vector2Int> OnTileHovered;
    public void EmitTileHovered(Vector2Int tile) => OnTileHovered?.Invoke(tile);

    public event System.Action OnMouseMoved;
    public void EmitMouseMoved() => OnMouseMoved?.Invoke();

    public event System.Action<Vector2Int?> OnMouseMovedToTile;
    public void EmitMouseMovedToTile(Vector2Int? tile) => OnMouseMovedToTile?.Invoke(tile);

    // Game state events
    public event System.Action<Vector2Int> OnTileSelected;
    public void EmitTileSelected(Vector2Int tile) => OnTileSelected?.Invoke(tile);

    public event System.Action<Vector2Int> OnTileDeselected;
    public void EmitTileDeselected(Vector2Int tile) => OnTileDeselected?.Invoke(tile);
} 