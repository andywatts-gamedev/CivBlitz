using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Unit")]
public class Unit : ScriptableObject
{
    public int Health;
    public int Movement;
    public int AttackRange;
    public int AttackDamage;
    public int Defense;
}