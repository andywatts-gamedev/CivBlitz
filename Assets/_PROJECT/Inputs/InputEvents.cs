using UnityEngine;

[CreateAssetMenu(fileName = "InputEvents", menuName = "ScriptableObject/InputEvents")]
public class InputEvents : ScriptableObject
{
    // Raw input events
    public event System.Action<Vector2Int> OnTileClicked;
    public event System.Action OnCancel;
    
    // Game state events
    public event System.Action<Vector2Int> OnTileSelected;
    public event System.Action<Vector2Int> OnTileDeselected;
    
    // Input event emitters
    public void EmitTileClicked(Vector2Int tile) => OnTileClicked?.Invoke(tile);
    public void EmitCancel() => OnCancel?.Invoke();
    
    // Game state emitters
    public void EmitTileSelected(Vector2Int tile) => OnTileSelected?.Invoke(tile);
    public void EmitTileDeselected(Vector2Int tile) => OnTileDeselected?.Invoke(tile);
} 