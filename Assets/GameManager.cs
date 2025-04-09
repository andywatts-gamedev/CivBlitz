using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public Grid grid;
    public Tilemap units;
    public Tilemap terrain;

    public Vector3Int highlight;
    public GameObject highlightGO;

    public bool sourceSelected;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;
        Vector3Int tile = grid.WorldToCell(worldPosition);

        if (!sourceSelected)
            Select(tile);
        else
            Move(tile);

    }

    void Select(Vector3Int tile)
    {
        var unit = units.GetTile(tile);
        Debug.Log($"Tile: {tile}");
        if (unit != null) 
        {
            highlight = tile;
            highlightGO.transform.position = grid.GetCellCenterWorld(tile);
            highlightGO.SetActive(true);
            sourceSelected = true;
            Debug.Log("Source selected");
        }
    }
    

    void Move(Vector3Int tile)
    {
        if (tile == highlight)
        {
            Deselect(tile);
            return;
        }

        var tileUnit = units.GetTile(tile);
        if (tileUnit != null) 
        {
            Debug.Log("Attack");
        }
        else
        {
            Debug.Log("Move");
            var unit = units.GetTile(highlight);
            Debug.Log($"Unit: {unit}");
            units.SetTile(highlight, null);
            units.SetTile(tile, unit);
            Deselect(tile);
        }
    }


    void Deselect(Vector3Int tile)
    {
        highlightGO.SetActive(false);
        highlight = Vector3Int.zero;
        sourceSelected = false;
        Debug.Log("Source deselected");
        return;
    }


        
}
