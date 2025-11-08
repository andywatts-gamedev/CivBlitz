using UnityEngine;
using UnityEngine.UIElements;

public class CombatLogUI : MonoBehaviour
{
    [SerializeField] private GameEvent onTurnChanged;
    
    private UIDocument doc;
    private VisualElement panel, entriesContainer;
    private const int MAX_ENTRIES = 20;
    
    // Font Awesome unicode glyphs
    private const string MELEE_GLYPH = "\uf71c";    // sword
    private const string RANGED_GLYPH = "\uf6b9";   // bow-arrow
    private const string SHIELD_GLYPH = "\uf3ed";   // shield
    private const string HEART_GLYPH = "\uf004";    // heart

    void Start()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        panel = root.Q("CombatLogContainer");
        if (panel == null) {
            Debug.LogError("[CombatLogUI] CombatLogContainer not found!");
            return;
        }
        
        entriesContainer = panel.Q("CombatEntries");
        if (entriesContainer == null) {
            Debug.LogError("[CombatLogUI] CombatEntries not found!");
        }

        // Start hidden until first combat
        panel.style.display = DisplayStyle.None;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatResolved += HandleCombatResolved;
        
        if (onTurnChanged != null) {
            onTurnChanged.Handler += ClearLog;
            Debug.Log($"[CombatLogUI] Subscribed to {onTurnChanged.name}");
        } else {
            Debug.LogWarning("[CombatLogUI] onTurnChanged is NULL!");
        }
    }

    void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatResolved -= HandleCombatResolved;
        
        if (onTurnChanged != null)
            onTurnChanged.Handler -= ClearLog;
    }
    
    void ClearLog()
    {
        Debug.Log($"[CombatLogUI] ClearLog called, entries={entriesContainer?.childCount}");
        if (entriesContainer == null) return;
        entriesContainer.Clear();
        panel.style.display = DisplayStyle.None;
    }

    void HandleCombatResolved(CombatEvent e)
    {
        if (panel == null) return;
        
        // Always show the panel when combat happens
        panel.style.display = DisplayStyle.Flex;
        panel.SetEnabled(true);
        
        // Clear any placeholder entries from UXML on first real combat
        if (entriesContainer.childCount > 0)
        {
            // Check if the first child is a placeholder (has no real combat data)
            var firstChild = entriesContainer[0];
            if (firstChild.Q<Label>("AttackerName") != null)
            {
                // This is the placeholder entry from UXML, remove it
                entriesContainer.RemoveAt(0);
            }
        }
        
        var entry = new VisualElement();
        entry.AddToClassList("combat-entry");

        var attackerColor = Game.Instance.civilizations[e.attackerCiv].color;
        var defenderColor = Game.Instance.civilizations[e.defenderCiv].color;
        
        // Row 1: Unit names
        var nameRow = new VisualElement();
        nameRow.AddToClassList("combat-row");
        var attackerLabel = new Label(e.attackerName);
        attackerLabel.style.color = attackerColor;
        attackerLabel.AddToClassList("combat-name");
        var vsLabel = new Label(" vs. ");
        vsLabel.AddToClassList("combat-vs");
        var defenderLabel = new Label(e.defenderName);
        defenderLabel.style.color = defenderColor;
        defenderLabel.AddToClassList("combat-name");
        nameRow.Add(attackerLabel);
        nameRow.Add(vsLabel);
        nameRow.Add(defenderLabel);
        entry.Add(nameRow);

        // Row 2: Attack/Defense stats
        var statsRow = new VisualElement();
        statsRow.AddToClassList("combat-row");
        
        var attackStr = new Label(e.attackerStrength.ToString());
        attackStr.AddToClassList("combat-stat");
        
        var attackGlyph = new Label(e.isRanged ? RANGED_GLYPH : MELEE_GLYPH);
        attackGlyph.AddToClassList("combat-glyph");
        attackGlyph.AddToClassList("font-awesome");
        
        var shieldGlyph = new Label(SHIELD_GLYPH);
        shieldGlyph.AddToClassList("combat-glyph");
        shieldGlyph.AddToClassList("font-awesome");
        
        var defenseStr = new Label(e.defenderStrength.ToString());
        defenseStr.AddToClassList("combat-stat");
        
        statsRow.Add(attackStr);
        statsRow.Add(attackGlyph);
        statsRow.Add(shieldGlyph);
        statsRow.Add(defenseStr);
        entry.Add(statsRow);

        // Row 3: Health before/after
        var healthRow = new VisualElement();
        healthRow.AddToClassList("combat-row");
        healthRow.AddToClassList("combat-health-row");
        
        // Left column - attacker health
        var attackerHealthCol = new VisualElement();
        attackerHealthCol.AddToClassList("combat-health-col");
        attackerHealthCol.AddToClassList("combat-health-col-left");
        
        var attackerHealthText = e.attackerHealthBefore == e.attackerHealthAfter 
            ? e.attackerHealthBefore.ToString() 
            : $"{e.attackerHealthBefore} → {e.attackerHealthAfter}";
        var attackerHealth = new Label(attackerHealthText);
        attackerHealth.AddToClassList("combat-health-text");
        
        var heartGlyph1 = new Label(HEART_GLYPH);
        heartGlyph1.AddToClassList("combat-glyph");
        heartGlyph1.AddToClassList("font-awesome");
        heartGlyph1.AddToClassList("combat-heart");
        
        attackerHealthCol.Add(attackerHealth);
        attackerHealthCol.Add(heartGlyph1);
        
        // Right column - defender health
        var defenderHealthCol = new VisualElement();
        defenderHealthCol.AddToClassList("combat-health-col");
        defenderHealthCol.AddToClassList("combat-health-col-right");
        
        var heartGlyph2 = new Label(HEART_GLYPH);
        heartGlyph2.AddToClassList("combat-glyph");
        heartGlyph2.AddToClassList("font-awesome");
        heartGlyph2.AddToClassList("combat-heart");
        
        var defenderHealthText = e.defenderHealthBefore == e.defenderHealthAfter 
            ? e.defenderHealthBefore.ToString() 
            : $"{e.defenderHealthBefore} → {e.defenderHealthAfter}";
        var defenderHealth = new Label(defenderHealthText);
        defenderHealth.AddToClassList("combat-health-text");
        
        defenderHealthCol.Add(heartGlyph2);
        defenderHealthCol.Add(defenderHealth);
        
        healthRow.Add(attackerHealthCol);
        healthRow.Add(defenderHealthCol);
        entry.Add(healthRow);

        entriesContainer.Add(entry);

        // Limit entries
        while (entriesContainer.childCount > MAX_ENTRIES)
            entriesContainer.RemoveAt(0);
    }

}

