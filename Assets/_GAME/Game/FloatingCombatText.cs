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
        if (attackDamage > 0) {
            var startPos = defenderPos + Vector3.up * 2f;
            var attackText = new GameObject("AttackDamage").AddComponent<TextMeshPro>();
            attackText.transform.position = startPos;
            attackText.text = $"-{attackDamage}";
            attackText.fontSize = 8;
            attackText.alignment = TextAlignmentOptions.Center;
            attackText.sortingOrder = 100;
            attackText.color = defenderColor;

            var move = attackText.gameObject.AddComponent<DamageTextMover>();
            move.Init(Vector3.up);
        }

        if (retaliationDamage > 0) {
            var startPos = attackerPos + Vector3.up * 2f;
            var retaliationText = new GameObject("RetaliationDamage").AddComponent<TextMeshPro>();
            retaliationText.transform.position = startPos;
            retaliationText.text = $"-{retaliationDamage}";
            retaliationText.fontSize = 8;
            retaliationText.alignment = TextAlignmentOptions.Center;
            retaliationText.sortingOrder = 100;
            retaliationText.color = attackerColor;

            var move = retaliationText.gameObject.AddComponent<DamageTextMover>();
            move.Init(Vector3.up);
        }
    }
}

public class DamageTextMover : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Vector3 direction;
    private float elapsed;
    private const float DURATION = 1.5f;
    private const float SPEED = 2f;

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