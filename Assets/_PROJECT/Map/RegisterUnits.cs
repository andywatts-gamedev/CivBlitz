using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CivTilemap))]
public class RegisterUnits : MonoBehaviour
{
    // public Tilemap flags;
    public Tilemap units;

    void Start()
    {
        var civ = GetComponent<CivTilemap>().civAsset;

        foreach (var pos in units.cellBounds.allPositionsWithin)
        {
            var tile = units.GetTile(pos) as UnitTile;
            if (tile != null)
            {
                UnitManager.Instance.RegisterUnit(civ, tile.unitData, (Vector2Int)pos);
            }
        }
    }
}
