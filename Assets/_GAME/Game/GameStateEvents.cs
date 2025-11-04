using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GameStateEvents", menuName = "ScriptableObject/GameStateEvents")]
public class GameStateEvents : ScriptableObject
{
    public event Action<Vector2Int> OnTileSelected;
    public void EmitTileSelected(Vector2Int tile) => OnTileSelected?.Invoke(tile);

    public event Action<Vector2Int> OnTileDeselected;
    public void EmitTileDeselected(Vector2Int tile) => OnTileDeselected?.Invoke(tile);

    public event Action<Vector2Int> OnMoveModeStarted;
    public void EmitMoveModeStarted(Vector2Int unitTile) => OnMoveModeStarted?.Invoke(unitTile);
}

