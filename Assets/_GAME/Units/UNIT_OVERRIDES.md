# Unit Override System

## Overview

The Override Tilemap allows you to modify unit properties during map authoring without creating duplicate unit types.

## How It Works

### In Scene:
1. Create a Tilemap named "Overrides" (sibling to Terrain)
2. Paint override tiles at positions where you want modified units
3. Export normally - overrides are captured in MapData

### Override Tiles:

Create override tiles in Unity:
1. Right-click in Project → Create → 2D → Tiles → Tile
2. Change script reference to `UnitOverrideTile`
3. Set properties:
   - **Health Override**: Custom health value (e.g., 50 for half health)
   - Leave 0 for no override
4. Save as `Health_50.asset`, `Health_25.asset`, etc.

### Visual Feedback:

Use tile sprites/colors to indicate overrides:
- Red tint = damaged unit
- Gold tint = veteran unit
- Or use actual sprite graphics

## Example Scene Setup

```
Map
├── Terrain (Tilemap)
│   └── Grass, Ocean, etc.
├── Overrides (Tilemap)  ← NEW!
│   └── Paint Health_50 at positions with wounded units
├── Japan (CivilizationTilemap)
│   ├── Flags (Tilemap)
│   └── Units (Tilemap)
└── Rome (CivilizationTilemap)
    ├── Flags (Tilemap)
    └── Units (Tilemap)
```

## Example Usage

### Combat Test Scenario:
```
Position (0,0):
- Terrain: Grass
- Unit: Japan Warrior
- Override: Health_50  ← Warrior starts with 50 HP instead of 100

Position (1,0):
- Terrain: Grass  
- Unit: Rome Warrior
- Override: (none)  ← Warrior has full health
```

### In MapData Inspector:

After export, you'll see:
```
Unit Placements
├── [0] Position: (0,0), Civ: Japan, Unit: Warrior, Health Override: 50
└── [1] Position: (1,0), Civ: Rome, Unit: Warrior, Health Override: 0
```

## Creating Override Tiles

### Quick Setup:

1. **Create Assets:**
   - `Assets/_GAME/Units/Tiles/Overrides/Health_25.asset`
   - `Assets/_GAME/Units/Tiles/Overrides/Health_50.asset`
   - `Assets/_GAME/Units/Tiles/Overrides/Health_75.asset`

2. **Configure Each:**
   - Script: UnitOverrideTile
   - Health Override: 25, 50, or 75
   - Sprite: Use colored square or icon (optional)
   - Color: Red tint for visual indication

3. **Use in Editor:**
   - Select Overrides tilemap
   - Select override tile from palette
   - Paint at unit positions that need it

## Notes

- Override tiles are **optional** - only paint where needed
- Multiple units can have same override tile
- Overrides are embedded in MapData on export
- If Overrides tilemap doesn't exist, export works normally (no overrides)
- Health Override of 0 = use unit's default health

## Future Extensions

Add more override properties:
- Experience/promotions
- Custom movement range
- Status effects
- Equipment/items

