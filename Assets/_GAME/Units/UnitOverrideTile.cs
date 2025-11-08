using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitOverrideTile : Tile
{
    [Tooltip("Override unit health percentage (1-100). E.g., 75 = 75% health. 0 = no override.")]
    [Range(0, 100)]
    public int healthOverride;
    
    [Tooltip("Future: experience, promotions, etc.")]
    public int experienceOverride;
}

