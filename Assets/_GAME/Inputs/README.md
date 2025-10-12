# Input System Documentation

## Event Types

1. Raw Input Events
   - `TileClicked`: Tile was clicked/touched
   - `Cancel`: Input was cancelled
   - `TileHovered`: Tile was hovered (mouse) or long pressed (touch)
   - `DragStarted`: Drag gesture began from a tile
   - `DragUpdated`: Drag gesture moved to new tile
   - `DragEnded`: Drag gesture ended on a tile

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
   - Emits drag events for drag-to-move gestures
   - Handles multi-touch if needed

## Event Sequences

1. Unit Selection
   ```
   User Click/Tap -> TileClicked -> CanSelectTile -> SelectTile -> TileSelected
   ```

2. Deselection (Same Tile)
   ```
   User Click/Tap -> TileClicked -> ClearSelection -> TileDeselected
   ```

3. Drag-to-Move (Primary Movement Method)
   ```
   Touch Start on Unit -> DragStarted -> ShowPathPreview
   Touch Move -> DragUpdated -> UpdatePathPreview
   Touch End on Valid Tile -> DragEnded -> ExecuteMove -> TileDeselected
   Touch End on Invalid Tile -> DragEnded -> CancelMove
   ```

4. Combat (via Drag-to-Move)
   ```
   Drag to Enemy -> DragEnded -> TryCombat -> ClearSelection -> TileDeselected
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
