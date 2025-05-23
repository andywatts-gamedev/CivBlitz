# Input System Documentation

## Event Types

1. Raw Input Events
   - `TileClicked`: Tile was clicked/touched
   - `Cancel`: Input was cancelled
   - `TileHovered`: Tile was hovered (mouse) or long pressed (touch)

2. Game State Events
   - `TileSelected`: Valid selection made
   - `TileDeselected`: Selection cleared

## Input Managers

1. `MouseInputManager`
   - Converts mouse clicks to tile coordinates
   - Emits `TileClicked` and `Cancel` events
   - Emits `TileHovered` after 1s hover
   - Uses Unity's new Input System

2. `TouchInputManager`
   - Converts touch input to tile coordinates
   - Emits `TileClicked` and `Cancel` events
   - Emits `TileHovered` after 1s long press
   - Handles multi-touch if needed

## Event Sequences

1. Initial Selection
   ```
   User Click -> TileClicked -> CanSelectTile -> SelectTile -> TileSelected
   ```

2. Deselection (Same Tile)
   ```
   User Click -> TileClicked -> ClearSelection -> TileDeselected
   ```

3. Move
   ```
   User Click -> TileClicked -> IsValidMove -> MoveTo -> ClearSelection -> TileDeselected
   ```

4. Combat
   ```
   User Click -> TileClicked -> IsValidMove -> TryCombat -> ClearSelection -> TileDeselected
   ```

5. Turn End
   ```
   All Units Moved -> EndTurn -> ClearSelection -> TileDeselected
   ```

6. Cancel
   ```
   User Cancel -> Cancel -> ClearSelection -> TileDeselected
   ```

7. Hover/Long Press
   ```
   Mouse Hover 1s -> TileHovered -> ShowTileDetails
   Touch Long Press 1s -> TileHovered -> ShowTileDetails
   ```

## Best Practices

1. Input Handling
   - Keep input managers focused on converting input to events
   - Use Unity's new Input System for consistency
   - Handle edge cases (out of bounds, invalid input)
   - Test both mouse and touch input paths

2. Event Management
   - Clear event subscriptions in OnDisable
   - Separate raw input events from game state events
   - Keep event handling simple and focused
   - Use logging to debug input issues
