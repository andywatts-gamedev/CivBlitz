# UI System Documentation

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
