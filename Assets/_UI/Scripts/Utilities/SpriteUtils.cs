using UnityEngine;
using UnityEngine.Tilemaps;

public static class SpriteUtils 
{
    public static GameObject CreateMovingSprite(string name, Sprite sprite, Color color, int sortingOrder, Vector3 position, Vector3 scale) 
    {
        var go = new GameObject(name);
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        go.transform.position = position;
        go.transform.localScale = scale;
        return go;
    }

    public static GameObject CreateMovingUnitSprite(Tile tile, Vector2Int pos, Civilization civ, bool isFlag)
    {
        var tilemap = isFlag ? UnitManager.Instance.flags[civ] : UnitManager.Instance.unitTilemap;
        var scale = isFlag ? Game.Instance.flagScale : Game.Instance.unitScale;
        var sortingOrder = isFlag ? 10 : 20;
        var color = isFlag ? tilemap.color : tile.color;
        
        return CreateMovingSprite(
            "UnitMove",
            tile.sprite,
            color,
            sortingOrder,
            tilemap.CellToWorld((Vector3Int)pos),
            scale
        );
    }
} 