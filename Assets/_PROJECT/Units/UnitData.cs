using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Unit")]
public class UnitData : ScriptableObject
{
    public int health;
    public int movement;
    public int melee;
    public int ranged;
    public int range;
}
