using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitTile : Tile
{

    public Civilization civ;

    private void OnValidate() => base.color = civ?.color ?? Color.white;

}
