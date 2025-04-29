using System;
using UnityEngine;

[Serializable]
public struct Terrain
{
    public string name; 
    public int movementCost;
    public int defenseBonus;

    public TerrainType type;
    public TerrainFeature feature;
    public TerrainHeight height;
}

public enum TerrainType
{
    None,
    Plains,
    Grassland,
    Desert,
    Tundra,
    Snow,
    Coast,
    Ocean
}

public enum TerrainHeight
{
    None,
    Hill,
    Mountain
}

public enum TerrainFeature
{
    None,
    Rainforest,
    Woods,
    Marsh,
    Oasis
}
