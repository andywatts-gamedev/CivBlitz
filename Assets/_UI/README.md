


# UI System Documentation

## Overview
The UI system is built using Unity's UI Toolkit with a modular template-based design. All UI files are consolidated in the `_UI` folder with orientation-specific styling.

## Quick Reference for AI

### Key UI Components
- **SelectedTileUI** - Shows selected tile/unit info (attack for units, defense for terrain)
- **TargetTileUI** - Shows hover target info during combat (same layout as SelectedTile)
- **GameUI** - Main game UI controller (turn management)

### Element Naming Convention
- **Template instances** (in Game.uxml): Append "Container" suffix (e.g., `SelectedTileContainer`, `TargetTileContainer`)
- **Root elements** (inside template UXML): Match template name (e.g., `SelectedTile`, `TargetTile`)
- **Child elements**: Use PascalCase names (e.g., `UnitRow`, `TileRow`)
- **Labels**: Append type suffix (e.g., `UnitName`, `UnitAttack`, `TileDefense`)
- No deprecated elements: removed `UnitDefense`, `TileAttack` from both SelectedTile and TargetTile

### CSS Class Pattern
- `.info-row` - Horizontal layout container
- `.name-label` - Left-aligned name display
- `.icon-label` - Font Awesome glyph
- `.value-label` - Right-aligned numeric value
- `.font-awesome` - Font Awesome font reference

### File Organization
Each UI component has 4 files:
1. `ComponentName.uxml` - Structure (root element named `ComponentName`)
2. `ComponentName-Common.uss` - Shared styles
3. `ComponentName-Portrait.uss` - Portrait orientation
4. `ComponentName-Landscape.uss` - Landscape orientation

### Template System
- Templates defined in component UXML files (e.g., `SelectedTile.uxml` has root element `SelectedTile`)
- Templates instantiated in `Game.uxml` with "Container" suffix (e.g., `<ui:Instance template="SelectedTile" name="SelectedTileContainer" />`)
- Controllers query template instance name: `root.Q("SelectedTileContainer")`
- Child elements queried from container: `container.Q("UnitRow")`

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
- **Visibility**: Controlled by `container.style.display = DisplayStyle.Flex/None`
- **Element Hierarchy**:
  - `SelectedTileContainer` - Template instance name in Game.uxml (query this from root)
  - `SelectedTile` - Root element inside SelectedTile.uxml template
- **Structure**: Two separate rows without table layout
  - **UnitRow**: Shows unit name, attack icon, attack value
    - Elements: `UnitName` (name-label), `AttackIcon` (icon-label), `UnitAttack` (value-label)
    - Only displays attack, no defense value
  - **TileRow**: Shows terrain name, defense icon, defense value
    - Elements: `TileName` (name-label), `DefenseIcon` (icon-label), `TileDefense` (value-label)
    - Only displays defense, no attack value
- **Layout**: Horizontal rows with name on left, icon and value on right
- **Icons**:
  - Attack: `\uf71c` (sword glyph)
  - Defense: `\uf3ed` (shield-halved glyph)
  - Both use Font Awesome 651 Regular font
- **Design**: Simple two-row layout, no table structure
  - Row 1: Unit info (attack focused)
  - Row 2: Terrain info (defense focused)
- **Files**:
  - `SelectedTile.uxml` - UI structure
  - `SelectedTile-Common.uss` - Shared styles (row layout, fonts)
  - `SelectedTile-Portrait.uss` - Portrait orientation styles
  - `SelectedTile-Landscape.uss` - Landscape orientation styles
  - `SelectedTileUI.cs` - Controller script

### TargetTile Panel  
- **Purpose**: Shows info about the hovered unit/tile (for targeting during combat)
- **Visibility**: Controlled by `container.style.display = DisplayStyle.Flex/None`
- **Element Hierarchy**:
  - `TargetTileContainer` - Template instance name in Game.uxml (query this from root)
  - `TargetTile` - Root element inside TargetTile.uxml template
- **Structure**: Two separate rows without table layout, reverse order from SelectedTile
  - **UnitRow**: Shows attack value, attack icon, unit name (right to left)
    - Elements: `UnitAttack` (value-label), `AttackIcon` (icon-label), `UnitName` (name-label)
    - Only displays attack, no defense value
  - **TileRow**: Shows defense value, defense icon, terrain name (right to left)
    - Elements: `TileDefense` (value-label), `DefenseIcon` (icon-label), `TileName` (name-label)
    - Only displays defense, no attack value
- **Layout**: Horizontal rows with value on left, icon in middle, name on right
- **Icons**:
  - Attack: `\uf71c` (sword glyph)
  - Defense: `\uf3ed` (shield-halved glyph)
  - Both use Font Awesome 651 Regular font
- **Design**: Simple two-row layout, no table structure
  - Row 1: Unit info (attack focused)
  - Row 2: Terrain info (defense focused)
- **Files**:
  - `TargetTile.uxml` - UI structure
  - `TargetTile-Common.uss` - Shared styles (row layout, fonts)
  - `TargetTile-Portrait.uss` - Portrait orientation styles
  - `TargetTile-Landscape.uss` - Landscape orientation styles
  - `TargetTileUI.cs` - Controller script

### Simultaneous Display
Both panels can be visible simultaneously during combat scenarios:
- SelectedTile shows your attacking unit's stats
- TargetTile shows the target unit's stats
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

## SelectedTileUI Controller

### Element References
```csharp
private VisualElement container, unitRow, tileRow;
private Label unitAttack, tileDefense, unitName, tileName;
```

### Initialization
```csharp
// Query the template instance from Game.uxml root
container = root.Q("SelectedTileContainer");
// Query child elements from within the template
unitRow = container.Q("UnitRow");
tileRow = container.Q("TileRow");
unitName = container.Q<Label>("UnitName");
unitAttack = container.Q<Label>("UnitAttack");
tileName = container.Q<Label>("TileName");
tileDefense = container.Q<Label>("TileDefense");
```

### Event Handling
- `HandleTileSelected(Vector2Int pos)` - Shows SelectedTile panel with tile/unit info
- `HandleCancel()` - Hides SelectedTile panel
- `HandleTileDeselected(Vector2Int pos)` - Hides SelectedTile panel

### Display Logic
- **UnitRow**: Shows when tile has a unit, hides otherwise
  - `unitName.text` = unit.unit.name
  - `unitAttack.text` = ranged attack for ranged units, melee for melee units
- **TileRow**: Shows when terrain has defense bonus > 0, hides otherwise
  - `tileName.text` = terrain.name
  - `tileDefense.text` = terrain.defenseBonus

### Styling Classes
- `.info-row` - Horizontal flex row with center alignment
- `.name-label` - Unit/terrain name (flex-grow: 1, font-size: 32px)
- `.icon-label` - Font Awesome icon (font-size: 32px, margins for spacing)
- `.value-label` - Numeric value (font-size: 32px, width: 64px)
- `.font-awesome` - Font Awesome font definition

## TargetTileUI Controller

### Element References
```csharp
private VisualElement container, unitRow, tileRow;
private Label unitAttack, tileDefense, unitName, tileName;
```

### Initialization
```csharp
// Query the template instance from Game.uxml root
container = root.Q("TargetTileContainer");
// Query child elements from within the template
unitRow = container.Q("UnitRow");
tileRow = container.Q("TileRow");
unitName = container.Q<Label>("UnitName");
unitAttack = container.Q<Label>("UnitAttack");
tileName = container.Q<Label>("TileName");
tileDefense = container.Q<Label>("TileDefense");
```

### Event Handling
- `HandleTileHovered(Vector2Int pos)` - Shows TargetTile panel with tile/unit info
- `HandlePointerMovedToTile(Vector2Int? tile)` - Hides panel if pointer moved away from target
- `HandleHoverCleared()` - Hides TargetTile panel

### Display Logic
- **UnitRow**: Shows when tile has a unit, hides otherwise
  - `unitName.text` = unit.unit.name
  - `unitAttack.text` = ranged attack for ranged units, melee for melee units
- **TileRow**: Shows when terrain has defense bonus > 0, hides otherwise
  - `tileName.text` = terrain.name
  - `tileDefense.text` = terrain.defenseBonus

### Styling Classes
Same as SelectedTileUI

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

## Font Awesome Integration

### Available Font Files
Located in `Assets/_UI/Common/Fonts/src/`:
- **fa-solid-900.ttf** - Solid icons (primary)
- **fa-regular-400.ttf** - Regular/outline icons
- **fa-light-300.ttf** - Light weight icons
- **fa-thin-100.ttf** - Thin weight icons
- **fa-brands-400.ttf** - Brand logos
- **fa-sharp-solid-900.ttf** - Sharp solid icons
- **fa-sharp-regular-400.ttf** - Sharp regular icons
- **fa-sharp-light-300.ttf** - Sharp light icons
- **fa-sharp-thin-100.ttf** - Sharp thin icons
- **fa-duotone-900.ttf** - Duotone icons
- **fa-v4compatibility.ttf** - Legacy compatibility

### Font Classes
```css
/* Base Font Awesome Classes */
.fa-solid { /* Uses fa-solid-900.ttf */ }
.fa-regular { /* Uses fa-regular-400.ttf */ }
.font-awesome { /* Uses fa-solid-900.ttf (fallback) */ }

/* Specific Icon Classes */
.fa-play { /* Play icon - uses fa-regular-400.ttf */ }
.fa-forward { /* Forward icon - uses fa-solid-900.ttf */ }
.fa-shield { /* Shield icon - uses fa-solid-900.ttf */ }
.fa-bed { /* Bed icon - uses fa-solid-900.ttf */ }
```

### Available Icons by Style

#### Solid Style (`fa-solid`) - Primary
- **Shield**: `&#xf3ed;` (fortify/defense) ✅
- **Sword**: `&#xf71c;` (combat/attack) ✅
- **Play**: `&#xf04b;` (next unit action) ✅
- **Fast Forward**: `&#xf04e;` (double play) ✅
- **Fast Forward Step**: `&#xf050;` (next turn) ✅
- **Bed**: `&#xf236;` (rest) ✅
- **Circle Play**: `&#xf144;` (alternative play) ✅

#### Regular Style (`fa-regular`) - Limited
- **Shield**: `&#xf3ed;` (fortify/defense) ❌ Not available
- **Play**: `&#xf04b;` (next unit action) ✅ Available
- **Fast Forward**: `&#xf04e;` (double play) ❌ Not available
- **Fast Forward Step**: `&#xf050;` (next turn) ❌ Not available
- **Bed**: `&#xf236;` (rest) ❌ Not available

**Note**: Most action icons (forward, shield, bed) are only available in Solid style. Play icon is available in Regular style.

### Icon Usage Guidelines
1. **Specific classes**: Use `.fa-play`, `.fa-forward`, `.fa-shield`, `.fa-bed` for specific icons
2. **Base classes**: Use `.fa-solid` or `.fa-regular` for general Font Awesome styling
3. **Icon codes**: Use HTML entity format `&#xfXXX;` in UXML
4. **Testing**: Test icons in UI Builder viewport (Inspector may show squares)
5. **Fallback**: If specific icon doesn't work, check Font Awesome documentation for style availability

### Button Panel Icons
- **Game Buttons**: 
  - Play (`&#xf04b;`) with class `fa-play` (next unit)
  - Fast Forward Step (`&#xf050;`) with class `fa-forward` (next turn)
- **Unit Buttons**: 
  - Rest (`&#xf236;`) with class `fa-bed`
  - Fortify (`&#xf3ed;`) with class `fa-shield`

## Migration Notes
This system was migrated from `_GAME/UI/` to `_UI/` with the following changes:
- Renamed `UI.cs` → `GameUI.cs`
- Split `SelectedPanel` → `SelectedTile` + `HoverTile` 
- Created orientation-specific USS files
- Updated all font paths and GUIDs
- Consolidated theme management
- Added Font Awesome icon system






Font Awesome v6.5.2
F132 Shield
f3ed shield halved
F312 Hex
F554 Walk
F6B9 Archer
F71C Sword
F71D Crossed Swords
f015 Home


f04b Play
f051 Forward-step
f04e Forward
f050 Forward Fast
f2f9 Rotate right

e57d Camp
f6bb Tent
e1b4 House Turret
e486 fort
e586 observation tower




Actions: Regular
Table: Solid





