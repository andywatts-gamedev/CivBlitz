# Font Awesome Icon Fix Guide

## Problem
Button icons are not displaying because the glyphs don't exist in `fa-regular-400 SDF.asset`.

Font Awesome 6 has different icon sets:
- **Regular (fa-regular-400)**: Subset of icons, mostly outline/hollow versions
- **Solid (fa-solid-900)**: Full set of icons, filled versions (RECOMMENDED)

## Current Icons and Status
- NextUnitButton: `\uf04b` (play) - likely NOT in regular
- NextTurnButton: `\uf021` (sync/arrows-rotate) - MAY be in regular
- MoveButton: `\uf554` (walking) - likely NOT in regular
- RestButton: `\uf6bb` (bed) - likely NOT in regular

## Solution A: Use Solid Font (RECOMMENDED)

### Step 1: Generate fa-solid-900 SDF Asset in Unity

1. **Open Unity** and the CivBlitz project
2. **Open TextMesh Pro Font Asset Creator**:
   - Go to `Window > TextMesh Pro > Font Asset Creator`
3. **Configure the Font Asset Creator**:
   - **Source Font File**: Select `Assets/_UI/_Fonts/Font Awesome 651/fa-solid-900.ttf`
   - **Atlas Resolution**: 512 x 512 (or 1024 x 1024 for better quality)
   - **Character Set**: Unicode Range (Hex)
   - **Character Sequence (Hex)**: 
     ```
     f001-f8ff
     ```
   - **Font Render Mode**: Distance Field 16
   - **Padding**: 5
   - **Packing Method**: Optimum
4. **Generate Font Atlas**: Click "Generate Font Atlas"
5. **Save**: Click "Save" or "Save as..." and save it as `fa-solid-900 SDF.asset` in the same directory

### Step 2: Update UXML Files

The helper script `SwitchToSolidFont.cs` in the Editor folder will do this automatically.

Or manually update these files to reference the solid font:
- `Assets/_UI/GameButtons/GameButtons.uxml`
- `Assets/_UI/UnitButtons/UnitButtons.uxml`

Change:
```xml
style="-unity-font-definition: url(&quot;project://database/Assets/_UI/_Fonts/Font%20Awesome%20651/fa-regular-400%20SDF.asset?...
```

To:
```xml
style="-unity-font-definition: url(&quot;project://database/Assets/_UI/_Fonts/Font%20Awesome%20651/fa-solid-900%20SDF.asset?...
```

## Solution B: Use Regular-Compatible Icons

If you want to stay with the regular font, replace the icon codes with regular-compatible alternatives:

### Icon Replacements
- NextUnitButton play (`\uf04b`) → circle-play (`\uf144`) *if available*
- NextTurnButton sync (`\uf021`) → arrow-right (`\uf061`) or caret-right (`\uf0da`)
- MoveButton walking (`\uf554`) → shoe-prints (`\uf54b`) *if available*
- RestButton bed (`\uf6bb`) → moon (`\uf186`)

Update in:
- `Assets/_UI/GameButtons/GameButtons.uxml`
- `Assets/_UI/GameButtons/GameButtonsUI.cs`
- `Assets/_UI/UnitButtons/UnitButtons.uxml`

## Testing

1. **Run the Font Glyph Verification Test**:
   ```
   Window > General > Test Runner > PlayMode tab > Run FontGlyphVerificationTest
   ```
   This will log which glyphs exist in the current font.

2. **Run the Editor Glyph Test**:
   ```
   Window > Font Awesome Glyph Test
   ```
   Load the font asset and click "Test Icon Codes" to see which are available.

3. **Test in Game**:
   - Play the game
   - Check console logs for `[GameButtonsUI]` messages showing button states and icons
   - Verify buttons display correctly in all scenarios:
     - Initial state (NextUnit button with play icon)
     - After moving all units (NextTurn button with sync icon)  
     - During AI turn (NextTurn button with sync icon)
     - After turn ends (back to NextUnit button)

## Recommended Approach

1. Generate `fa-solid-900 SDF.asset` (Solution A)
2. Run the glyph verification test to confirm all icons exist
3. Update UXML files to use solid font
4. Test the game

This is the most reliable solution since solid fonts contain the full icon set.

