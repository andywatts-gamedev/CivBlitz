


# UI System Documentation

## Overview
The UI system is built using Unity's UI Toolkit with a modular template-based design. All UI files are consolidated in the `_UI` folder with orientation-specific styling.

## Architecture

### File Structure
```
_UI/
├── Game.uxml                    # Main game UI (uses templates)
├── SelectedTile.uxml           # Selected unit info panel
├── HoverTile.uxml              # Hovered unit info panel
├── SelectedTile-Landscape.uss  # Landscape styles for selected panel
├── SelectedTile-Portrait.uss   # Portrait styles for selected panel
├── HoverTile-Landscape.uss     # Landscape styles for hover panel
├── HoverTile-Portrait.uss      # Portrait styles for hover panel
├── Game-Landscape.uss          # Landscape styles for game UI
├── Game-Portrait.uss           # Portrait styles for game UI
├── Scripts/
│   └── GameUI.cs               # Main UI controller (renamed from UI.cs)
└── Themes/
    ├── Landscape.tss           # Landscape theme (imports all landscape styles)
    └── Portrait.tss            # Portrait theme (imports all portrait styles)
```

### Theme System
- **ThemeManager** automatically switches between Landscape.tss and Portrait.tss based on screen orientation
- Each TSS file imports the appropriate orientation-specific USS files
- No inline styles in UXML files - all styling handled by themes

## Panel System

### SelectedTile Panel
- **Purpose**: Shows info about the currently selected unit/tile
- **Visibility**: Controlled by `selectedPanel.style.display = DisplayStyle.Flex/None`
- **Positioning**: 
  - Landscape: `left: 1%, top: 1%` (top-left corner)
  - Portrait: `left: 1%, top: 1%` (top-left corner)
- **Content**: Unit stats, terrain info, health, movement points

### HoverTile Panel  
- **Purpose**: Shows info about the hovered unit/tile (for targeting during combat)
- **Visibility**: Controlled by `hoverPanel.style.display = DisplayStyle.Flex/None`
- **Positioning**:
  - Landscape: `right: 1%, top: 1%` (top-right corner)
  - Portrait: `right: 1%, top: 1%` (top-right corner)
- **Content**: Target unit stats, terrain info, health, movement points

### Simultaneous Display
Both panels can be visible simultaneously during combat scenarios:
- SelectedTile shows your attacking unit's stats
- HoverTile shows the target unit's stats
- Positioned on opposite sides to avoid overlap

## Responsive Design

### Sizing Strategy
- **Width**: `width: auto` with `max-width: 40%/45%` - panels size to content
- **Height**: `min-height: 15%/12%` - responsive to screen size
- **Padding**: Uses percentages (1%, 1.5%, 2%) for scaling
- **No fixed pixels** - everything scales with display resolution

### Orientation Handling
- **Landscape**: Panels at top-left and top-right
- **Portrait**: Panels at top-left and top-right (same layout)
- **Font paths**: Updated to use `_UI/Fonts/` instead of `_GAME/UI/Fonts/`

## GameUI Controller

### Key Methods
```csharp
// Panel visibility control
selectedPanel.style.display = DisplayStyle.Flex/None;
hoverPanel.style.display = DisplayStyle.Flex/None;

// Element queries (updated for new structure)
selectedPanel = root.Q("SelectedTile");
hoverPanel = root.Q("HoverTile");
```

### Event Handling
- `HandleTileSelected()` - Shows SelectedTile panel
- `HandleTileHovered()` - Shows HoverTile panel  
- `HandleHoverCleared()` - Hides HoverTile panel
- `HandleCancel()` - Hides SelectedTile panel
- `HandleTileDeselected()` - Hides SelectedTile panel

## TemplateContainer Fix
**Critical**: TemplateContainers need explicit height to contain absolutely positioned panels:
```css
TemplateContainer {
    flex-grow: 1;
    flex-shrink: 0;
    min-height: 15%; /* Prevents 0-height containers */
}
```

## Scene Configuration
- **UIDocument**: References `/Assets/_UI/Game.uxml`
- **GameUI Component**: Attached to UI GameObject (renamed from UI.cs)
- **ThemeManager**: Configured with Landscape/Portrait TSS files

## CSS Best Practices Used
1. **Percentages over pixels** for responsive scaling
2. **Auto-sizing width** with max-width constraints
3. **Flex properties** for proper container behavior
4. **!important declarations** to override conflicting styles
5. **Orientation-specific files** for clean separation

## Common Issues & Solutions

### Panels not visible
- Check TemplateContainer has `min-height`
- Verify CSS doesn't have `display: none`
- Ensure GameUI script queries correct element names

### Positioning problems  
- Use `!important` on positioning properties
- Set `bottom: auto !important` to override conflicting bottom positioning
- Check for CSS conflicts between orientation files

### Theme not loading
- Verify TSS files import correct USS files
- Check ThemeManager has correct TSS references in scene
- Ensure no inline styles in UXML files

## Migration Notes
This system was migrated from `_GAME/UI/` to `_UI/` with the following changes:
- Renamed `UI.cs` → `GameUI.cs`
- Split `SelectedPanel` → `SelectedTile` + `HoverTile` 
- Created orientation-specific USS files
- Updated all font paths and GUIDs
- Consolidated theme management