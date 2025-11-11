using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private UIDocument healthBarDocument;
    [SerializeField] private VisualTreeAsset healthBarTemplate;
    [SerializeField] private GameEvent onUnitStateChanged;
    [SerializeField] private GameEvent onUnitMoved;
    [SerializeField] private GameEvent onMapLoaded;

    private VisualElement root;
    private Dictionary<Vector2Int, VisualElement> healthBars = new();

    void Start()
    {
        if (healthBarDocument == null)
        {
            Debug.LogError("[HealthBarManager] UIDocument not assigned!");
            return;
        }

        root = healthBarDocument.rootVisualElement;

        // Subscribe to unit events
        if (onUnitStateChanged != null) onUnitStateChanged.Handler += UpdateHealthBars;
        if (onUnitMoved != null) onUnitMoved.Handler += UpdateHealthBars;
        if (onMapLoaded != null) onMapLoaded.Handler += UpdateHealthBars;

        // Initial update
        UpdateHealthBars();
    }

    void OnDestroy()
    {
        if (onUnitStateChanged != null) onUnitStateChanged.Handler -= UpdateHealthBars;
        if (onUnitMoved != null) onUnitMoved.Handler -= UpdateHealthBars;
        if (onMapLoaded != null) onMapLoaded.Handler -= UpdateHealthBars;
    }

    void Update()
    {
        // Update positions every frame for smooth camera movement
        UpdateHealthBarPositions();
    }

    private void UpdateHealthBars()
    {
        Debug.Log("[HealthBarManager] UpdateHealthBars called");
        
        // Clear existing health bars
        foreach (var bar in healthBars.Values)
        {
            root.Remove(bar);
        }
        healthBars.Clear();

        if (UnitManager.Instance == null) return;

        // Create health bars for damaged units
        int created = 0;
        foreach (var kvp in UnitManager.Instance.units)
        {
            var pos = kvp.Key;
            var unit = kvp.Value;
            var maxHealth = unit.unit.health;
            var currentHealth = unit.health;

            if (currentHealth < maxHealth)
            {
                CreateHealthBar(pos, currentHealth, maxHealth);
                created++;
            }
        }
        
        Debug.Log($"[HealthBarManager] Created {created} health bars from {UnitManager.Instance.units.Count} units");
    }

    private void CreateHealthBar(Vector2Int pos, int currentHealth, int maxHealth)
    {
        if (healthBarTemplate == null)
        {
            Debug.LogWarning("[HealthBarManager] Health bar template not assigned!");
            return;
        }

        var healthBar = healthBarTemplate.CloneTree();
        var container = healthBar.Q("HealthBarContainer");
        var fill = healthBar.Q("HealthBarFill");

        if (container == null || fill == null)
        {
            Debug.LogError("[HealthBarManager] Health bar template missing required elements!");
            return;
        }

        // Set fill percentage
        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);
        fill.style.width = Length.Percent(healthPercent * 100f);

        // Color based on health
        fill.RemoveFromClassList("low-health");
        fill.RemoveFromClassList("medium-health");
        if (healthPercent <= 0.33f)
        {
            fill.AddToClassList("low-health");
        }
        else if (healthPercent <= 0.66f)
        {
            fill.AddToClassList("medium-health");
        }

    root.Add(healthBar);
    healthBars[pos] = healthBar;

    // Set initial position
    UpdateHealthBarPosition(pos, healthBar);
    }

    private void UpdateHealthBarPositions()
    {
        foreach (var kvp in healthBars)
        {
            UpdateHealthBarPosition(kvp.Key, kvp.Value);
        }
    }

    private void UpdateHealthBarPosition(Vector2Int unitPos, VisualElement healthBar)
    {
        if (UnitManager.Instance == null || Game.Instance == null) return;
        if (!UnitManager.Instance.flags.ContainsKey(Game.Instance.player.civilization)) return;
        
        var flagsTilemap = UnitManager.Instance.flags[Game.Instance.player.civilization];
        if (flagsTilemap == null) return;

        Vector3 worldPos;
        if (CombatManager.Instance != null && CombatManager.Instance.TryGetCombatPosition(unitPos, out var combatPos))
        {
            worldPos = combatPos;
        }
        else
        {
            worldPos = flagsTilemap.CellToWorld((Vector3Int)unitPos);
        }
        worldPos.y += 1.2f;

        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        // Check if behind camera
        if (screenPos.z < 0)
        {
            healthBar.style.display = DisplayStyle.None;
            return;
        }
        
        healthBar.style.display = DisplayStyle.Flex;
        healthBar.style.position = Position.Absolute;
        
        // Get panel dimensions (UI Toolkit may differ from Screen dimensions)
        float panelWidth = root.resolvedStyle.width;
        float panelHeight = root.resolvedStyle.height;
        
        // Scale from screen space to panel space
        float uiX = (screenPos.x / Screen.width) * panelWidth;
        float uiY = ((Screen.height - screenPos.y) / Screen.height) * panelHeight;

        healthBar.style.left = uiX - 20;
        healthBar.style.top = uiY;
        
        // Debug.Log($"[HealthBar] Unit {unitPos}: screen={screenPos}, Panel={panelWidth}x{panelHeight}, UI=({uiX - 20}, {uiY})");
    }
}

