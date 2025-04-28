using UnityEngine;
using TMPro;

public class FloatingCombatText : MonoBehaviour
{
    private const float DURATION = 1f;
    private const float SPEED = 1f;
    private const float FADE_SPEED = 1f;

    public static void Create(Vector3 position, int attackDamage, Color defenderColor, Vector3 defenderPos, 
                            int retaliationDamage, Color attackerColor, Vector3 attackerPos)
    {
        // Main attack damage - floats toward defender
        if (attackDamage > 0) {
            var attackText = new GameObject("AttackDamage").AddComponent<TextMeshPro>();
            attackText.transform.position = defenderPos;
            attackText.text = $"-{attackDamage}";
            attackText.fontSize = 8;
            attackText.alignment = TextAlignmentOptions.Center;
            attackText.sortingOrder = 100;
            attackText.color = defenderColor;

            var move = attackText.gameObject.AddComponent<DamageTextMover>();
            move.Init( ((defenderPos - position )*2) + (Vector3.up * 4f));
        }

        // Retaliation damage - floats toward attacker
        if (retaliationDamage > 0) {
            var retaliationText = new GameObject("RetaliationDamage").AddComponent<TextMeshPro>();
            retaliationText.transform.position = attackerPos;
            retaliationText.text = $"-{retaliationDamage}";
            retaliationText.fontSize = 8;
            retaliationText.alignment = TextAlignmentOptions.Center;
            retaliationText.sortingOrder = 100;
            retaliationText.color = attackerColor;

            var move = retaliationText.gameObject.AddComponent<DamageTextMover>();
            move.Init( ((attackerPos - position )*2) + (Vector3.up * 4f));
        }
    }
}

public class DamageTextMover : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Vector3 direction;
    private float elapsed;
    private const float DURATION = 2f;
    private const float SPEED = 0.5f;

    public void Init(Vector3 dir)
    {
        textMesh = GetComponent<TextMeshPro>();
        direction = dir.normalized;
        Destroy(gameObject, DURATION);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += direction * SPEED * Time.deltaTime;
        textMesh.alpha = Mathf.Lerp(1, 0, elapsed / DURATION);
    }
} 