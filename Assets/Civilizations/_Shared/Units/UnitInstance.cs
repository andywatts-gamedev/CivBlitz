using UnityEngine;
using UnityEngine.Tilemaps;

public struct UnitInstance
{
    public UnitData unitData;
    public int health;
    public int movement;
    public Civilization civ;

    public void Start()
    {
        health = unitData.health;
        movement = unitData.movement;
    }
}
