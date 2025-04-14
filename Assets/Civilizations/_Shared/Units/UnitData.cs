using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Unit")]
public class UnitData : ScriptableObject
{
    public int health;
    public int movement;
    public int range;
    public int attack;
    public int defense;
}
