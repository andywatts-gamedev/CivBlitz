using UnityEngine;
using TMPro;

public class FloatingCombatText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float duration = 2f;
    private float speed = 0.5f;
    private float fadeSpeed = 1f;
    private Vector3 direction = Vector3.up;

    public static FloatingCombatText Create(Vector3 position, int damage)
    {
        var go = new GameObject("FloatingText");
        var fct = go.AddComponent<FloatingCombatText>();
        fct.Init(position, damage);
        return fct;
    }

    private void Init(Vector3 position, int damage)
    {
        transform.position = position;
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.text = $"-{damage}";
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.sortingOrder = 100;
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        textMesh.alpha -= fadeSpeed * Time.deltaTime;
    }
} 