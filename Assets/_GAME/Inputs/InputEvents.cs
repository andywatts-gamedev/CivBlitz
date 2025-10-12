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

    public event System.Action<Vector2Int?> OnPointerMovedToTile;
    public void EmitPointerMovedToTile(Vector2Int? tile) => OnPointerMovedToTile?.Invoke(tile);
    
    public event System.Action OnHoverCleared;
    public void EmitHoverCleared() => OnHoverCleared?.Invoke();

    // Drag events for drag-to-move
    public event System.Action<Vector2Int, Vector2Int> OnDragStarted;
    public void EmitDragStarted(Vector2Int fromTile, Vector2Int toTile) => OnDragStarted?.Invoke(fromTile, toTile);

    public event System.Action<Vector2Int, Vector2Int> OnDragUpdated;
    public void EmitDragUpdated(Vector2Int fromTile, Vector2Int toTile) => OnDragUpdated?.Invoke(fromTile, toTile);

    public event System.Action<Vector2Int, Vector2Int> OnDragEnded;
    public void EmitDragEnded(Vector2Int fromTile, Vector2Int toTile) => OnDragEnded?.Invoke(fromTile, toTile);

    // Game state events
    public event System.Action<Vector2Int> OnTileSelected;
    public void EmitTileSelected(Vector2Int tile) => OnTileSelected?.Invoke(tile);

    public event System.Action<Vector2Int> OnTileDeselected;
    public void EmitTileDeselected(Vector2Int tile) => OnTileDeselected?.Invoke(tile);
} 