using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class CivTilemap : MonoBehaviour
{
    public Civilization civAsset;
    public Tilemap flags;
    public Tilemap units;

    public void OnValidate()
    {
        if (!flags || !units || !civAsset) return;
        flags.color = civAsset.color;
    }
} 