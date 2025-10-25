using UnityEngine;
using UnityEngine.Tilemaps;

public enum UnitState
{
    Ready,
    Resting,
    Fortified
}

public class UnitInstance
{
    public Unit unit;
    public Civilization civ;
    public Vector2Int position;
    public int movesLeft;
    public int health;
    public UnitState state;

    public UnitInstance(Unit unit, Civilization civ, Vector2Int pos)
    {
        this.unit = unit;
        this.civ = civ;
        this.position = pos;
        this.movesLeft = unit.movement;
        this.health = unit.health;
        this.state = UnitState.Ready;
    }
}
