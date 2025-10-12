using UnityEngine;
using UnityEngine.Tilemaps;

public class CivilizationTilemap : MonoBehaviour
{
    public CivilizationSCOB civ;
    public Tilemap flags;
    public Tilemap units;

    void Start()
    {
        UnitManager.Instance.flags[civ.civilization] = flags;

        // Loop over civ flags and register units
        foreach (var pos in flags.cellBounds.allPositionsWithin)
        {
            var tile = flags.GetTile(pos) as Tile;
            if (tile != null)
            {
                Debug.Log("tile: " + tile);
                var unit = units.GetTile(pos) as UnitTile;
                Debug.Log("unit: " + unit);
                if (unit != null)
                    UnitManager.Instance.RegisterUnit(civ.civilization, unit.unitSCOB.unit, (Vector2Int)pos);
            }
        }
    }

    void OnValidate()
    {
        GetComponent<Tilemap>().color = civ?.color ?? Color.white;
        gameObject.name = civ?.name ?? "Civilization";
    }
}
