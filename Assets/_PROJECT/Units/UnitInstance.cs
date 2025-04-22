using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitInstance
{
    public UnitData unitData;
    public Civilization civ;
    public Vector2Int position;
    public int movesLeft;
    public int health;

    public UnitInstance(UnitData unitData, Civilization civ, Vector2Int pos)
    {
        this.unitData = unitData;
        this.civ = civ;
        this.position = pos;
        this.movesLeft = unitData.movement;
        this.health = unitData.health;
    }
}
