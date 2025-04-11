using UnityEngine;

public class UnitInstance : MonoBehaviour
{

    public UnitData unitData;

    public int Health;
    public int Movement;

    public void Awake()
    {
        Health = unitData.Health;
        Movement = unitData.Movement;
    }

}


