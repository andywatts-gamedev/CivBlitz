using UnityEngine;

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
} 