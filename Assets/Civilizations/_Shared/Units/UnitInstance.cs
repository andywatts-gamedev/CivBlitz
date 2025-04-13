using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitInstance : MonoBehaviour
{

    public UnitData unitData;

    public int Health;
    public int Movement;
    public Civilization civ;

    public void Start()
    {
        Health = unitData.Health;
        Movement = unitData.Movement;



        var cell = Game.Instance.units.WorldToCell(transform.position);
        UnitTile unitTile = Game.Instance.units.GetTile(cell) as UnitTile;
        Debug.Log(unitTile);
        civ = unitTile.civ;
    }

}
