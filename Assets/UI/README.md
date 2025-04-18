# UI System Documentation

## Overview
The UI system is built using Unity's UI Toolkit and follows a modular design pattern. It consists of several key components:

1. **UI.uxml**: The main UI document that defines the visual structure
2. **UI.cs**: The controller that manages UI state and interactions
3. **UI.uss**: The stylesheet that defines the visual appearance

## Info Panel Behavior

The info panel displays tile and unit information with the following behavior:

### Tile Selection
- Clicking a tile selects it and shows the info panel
- The panel remains visible while the tile is selected
- Pressing Escape or clicking elsewhere deselects the tile and hides the panel

### Hover Behavior
- Hovering over a tile for 1 second shows the info panel
- Moving the mouse hides the panel immediately
- If a tile is selected, moving the mouse does not hide the panel
- The panel shows both tile and unit data if a unit is present

### Panel Content
- Shows terrain type and movement cost
- If a unit is present, shows unit stats (health, movement, combat)
- Updates in real-time as the mouse moves over different tiles
- Only visible when a tile is selected or being hovered

## Components

1. `UI.cs`
   - Manages unit detail panel
   - Handles event subscriptions
   - Updates unit stats display

2. `UI.uxml`
   - Defines panel layout
   - Contains labels for unit stats
   - Uses UIElements for modern UI

## Event Handling
****
1. `TileSelected`
   - Show unit panel
   - Update unit stats
   - Display: Flex

2. `TileDeselected`
   - Hide unit panel
   - Display: None

3. `Cancel`
   - Hide unit panel
   - Display: None

## UI Elements

1. Unit Panel
   - Name label
   - Health label
   - Movement label
   - Range label
   - Attack label
   - Defense label

2. Icons
   - Sword (attack)
   - Shield (defense)
   - Walk (movement)
   - Bow (range)

## Suggested Tests

```csharp
[UnityTest]
public IEnumerator SelectTile_ShouldShowPanel()
{
    // Setup
    var ui = CreateGameObject<UI>();
    var tile = new Vector2Int(0, 0);
    
    // Act
    ui.HandleTileSelected(tile);
    yield return null;
    
    // Assert
    Assert.IsTrue(ui.unitPanel.style.display == DisplayStyle.Flex);
}

[UnityTest]
public IEnumerator DeselectTile_ShouldHidePanel()
{
    // Setup
    var ui = CreateGameObject<UI>();
    var tile = new Vector2Int(0, 0);
    
    // Act
    ui.HandleTileSelected(tile);
    yield return null;
    ui.HandleTileDeselected(tile);
    yield return null;
    
    // Assert
    Assert.IsTrue(ui.unitPanel.style.display == DisplayStyle.None);
}

[UnityTest]
public IEnumerator Cancel_ShouldHidePanel()
{
    // Setup
    var ui = CreateGameObject<UI>();
    var tile = new Vector2Int(0, 0);
    
    // Act
    ui.HandleTileSelected(tile);
    yield return null;
    ui.HandleCancel();
    yield return null;
    
    // Assert
    Assert.IsTrue(ui.unitPanel.style.display == DisplayStyle.None);
}
```

## Best Practices

1. UI Management
   - Keep UI elements organized in UXML
   - Use UIElements for modern UI
   - Handle all UI updates in UI class

2. Event Handling
   - Clear event subscriptions in OnDisable
   - Keep UI updates simple and focused
   - Use consistent display states

3. Performance
   - Minimize UI updates
   - Cache UI elements
   - Use efficient layout
