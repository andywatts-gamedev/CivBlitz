using UnityEngine;
using TMPro;

public class FloatingCombatText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float duration = 5f;
    private float speed = 0.5f;
    private float fadeSpeed = 0.5f;
    private Vector3 direction = Vector3.up;

    public static FloatingCombatText Create(Vector3 position, int attackDamage, Color defenderColor, int retaliationDamage, Color attackerColor)
    {
        var go = new GameObject("FloatingText");
        var fct = go.AddComponent<FloatingCombatText>();
        fct.Init(position, attackDamage, defenderColor, retaliationDamage, attackerColor);
        return fct;
    }

    private void Init(Vector3 position, int attackDamage, Color defenderColor, int retaliationDamage, Color attackerColor)
    {
        transform.position = position;
        textMesh = gameObject.AddComponent<TextMeshPro>();
        
        // Always show main attack damage on top, retaliation below if it exists
        textMesh.text = $"<color=#{ColorUtility.ToHtmlStringRGB(defenderColor)}>-{attackDamage}</color>";
        if (retaliationDamage > 0) {
            textMesh.text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(attackerColor)}>-{retaliationDamage}</color>";
        }
        
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