using UnityEngine;
using UnityEngine.UIElements;

public class CombatLogUI : MonoBehaviour
{
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
        if (panel == null) return;
        
        entriesContainer = panel.Q("CombatEntries");

        // Start hidden until first combat
        panel.style.display = DisplayStyle.None;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatResolved += HandleCombatResolved;
    }

    void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatResolved -= HandleCombatResolved;
    }

    void HandleCombatResolved(CombatEvent e)
    {
        if (panel == null) return;
        
        panel.style.display = DisplayStyle.Flex;
        
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
        
        // Left column - defender health
        var defenderHealthCol = new VisualElement();
        defenderHealthCol.AddToClassList("combat-health-col");
        defenderHealthCol.AddToClassList("combat-health-col-left");
        
        var defenderHealth = new Label($"{e.defenderHealthBefore} → {e.defenderHealthAfter}");
        defenderHealth.AddToClassList("combat-health-text");
        
        var heartGlyph1 = new Label(HEART_GLYPH);
        heartGlyph1.AddToClassList("combat-glyph");
        heartGlyph1.AddToClassList("font-awesome");
        heartGlyph1.AddToClassList("combat-heart");
        
        defenderHealthCol.Add(defenderHealth);
        defenderHealthCol.Add(heartGlyph1);
        
        // Right column - attacker health
        var attackerHealthCol = new VisualElement();
        attackerHealthCol.AddToClassList("combat-health-col");
        attackerHealthCol.AddToClassList("combat-health-col-right");
        
        var heartGlyph2 = new Label(HEART_GLYPH);
        heartGlyph2.AddToClassList("combat-glyph");
        heartGlyph2.AddToClassList("font-awesome");
        heartGlyph2.AddToClassList("combat-heart");
        
        var attackerHealth = new Label($"{e.attackerHealthBefore} → {e.attackerHealthAfter}");
        attackerHealth.AddToClassList("combat-health-text");
        
        attackerHealthCol.Add(heartGlyph2);
        attackerHealthCol.Add(attackerHealth);
        
        healthRow.Add(defenderHealthCol);
        healthRow.Add(attackerHealthCol);
        entry.Add(healthRow);

        entriesContainer.Add(entry);

        // Limit entries
        while (entriesContainer.childCount > MAX_ENTRIES)
            entriesContainer.RemoveAt(0);
    }

}

