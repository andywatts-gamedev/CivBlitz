using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Unit")]
public class UnitData : ScriptableObject
{
    public int Health;
    public int Movement;
    public int AttackRange;
    public int AttackDamage;
    public int Defense;
}