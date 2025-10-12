using System;
using UnityEngine;

public enum UnitType {
    Melee,
    Ranged,
    Siege,
}

[Serializable]
public struct Unit 
{
    public string name;
    public UnitType type;
    public int health;
    public int movement;
    public int melee;
    public int ranged;
    public int range;
    public int cost;
    public bool canTravelLand;
    public bool canTravelCoast;
    public bool canTravelOcean;
}
