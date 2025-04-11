using UnityEngine;

[CreateAssetMenu(fileName = "InputEvents", menuName = "ScriptableObject/InputEvents")]
public class InputEvents : ScriptableObject
{
    public event System.Action<Vector2Int> OnTileSelected;
    public event System.Action OnCancel;
    
    public void EmitTileSelected(Vector2Int tile) => OnTileSelected?.Invoke(tile);
    public void EmitCancel() => OnCancel?.Invoke();
} 