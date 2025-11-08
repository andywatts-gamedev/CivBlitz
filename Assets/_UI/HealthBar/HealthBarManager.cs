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

        // Initial update
        UpdateHealthBars();
    }

    void OnDestroy()
    {
        if (onUnitStateChanged != null) onUnitStateChanged.Handler -= UpdateHealthBars;
        if (onUnitMoved != null) onUnitMoved.Handler -= UpdateHealthBars;
    }

    void Update()
    {
        // Update positions every frame for smooth camera movement
        UpdateHealthBarPositions();
    }

    private void UpdateHealthBars()
    {
        // Clear existing health bars
        foreach (var bar in healthBars.Values)
        {
            root.Remove(bar);
        }
        healthBars.Clear();

        // Create health bars for damaged units
        foreach (var kvp in UnitManager.Instance.units)
        {
            var pos = kvp.Key;
            var unit = kvp.Value;
            var maxHealth = unit.unit.health;
            var currentHealth = unit.health;

            // Only show if damaged
            if (currentHealth < maxHealth)
            {
                CreateHealthBar(pos, currentHealth, maxHealth);
            }
        }
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
        healthBars[pos] = container;

        // Set initial position
        UpdateHealthBarPosition(pos, container);
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
        // Get unit's world position
        var flagsTilemap = UnitManager.Instance.flags[Game.Instance.player.civilization];
        if (flagsTilemap == null) return;

        var worldPos = flagsTilemap.CellToWorld((Vector3Int)unitPos);
        worldPos.y += 1.2f; // Position above unit

        // Convert to screen position
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        // Flip Y for UI Toolkit coordinate system
        screenPos.y = Screen.height - screenPos.y;

        // Center the health bar
        healthBar.style.left = screenPos.x - 20; // Half of width (40px)
        healthBar.style.top = screenPos.y;
    }
}

