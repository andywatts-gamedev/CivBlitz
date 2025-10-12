using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitInstance
{
    public Unit unit;
    public Civilization civ;
    public Vector2Int position;
    public int movesLeft;
    public int health;

    public UnitInstance(Unit unit, Civilization civ, Vector2Int pos)
    {
        this.unit = unit;
        this.civ = civ;
        this.position = pos;
        this.movesLeft = unit.movement;
        this.health = unit.health;
    }
}
