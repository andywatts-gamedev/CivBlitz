﻿using System;
using UnityEngine;

[Serializable]
public struct Unit 
{
    public string name;
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
