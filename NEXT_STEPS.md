# Next Steps to Fix Button Icon Display

## What I've Done

1. ✓ Created diagnostic tools:
   - `Assets/Tests/Editor/FontAwesomeGlyphTest.cs` - Editor window to test glyphs
   - `Assets/Tests/PlayMode/FontGlyphVerificationTest.cs` - Automated test
   
2. ✓ Added debug logging to `GameButtonsUI.cs`:
   - Logs button visibility states
   - Logs icon text values
   - Logs game state (isPlayerTurn, hasReadyUnits)

3. ✓ Created helper tools:
   - `Assets/Tests/Editor/SwitchToSolidFont.cs` - Automates font switching
   - `Assets/_UI/_Fonts/FONT_ICON_FIX.md` - Comprehensive fix guide

## What You Need to Do

### Option 1: Quick Test First (Recommended)

1. **Open Unity** with the CivBlitz project
2. **Play the game** and check the Console for `[GameButtonsUI]` log messages
3. **Look for** which icons are displaying vs. showing as blank/unknown characters
4. **Note** which scenarios work/don't work:
   - Initial game start (should show NextUnit button with play icon)
   - After moving all units (should show NextTurn button with sync icon)
   - During AI turn (should show NextTurn button with sync icon)
   - After new turn (should show NextUnit button again)

### Option 2: Generate Solid Font (Most Likely Fix)

1. **In Unity**, go to `Window > TextMesh Pro > Font Asset Creator`
2. **Configure**:
   - Source Font File: `fa-solid-900.ttf`
   - Atlas Resolution: 512 x 512
   - Character Set: Unicode Range (Hex)
   - Character Sequence: `f001-f8ff`
   - Font Render Mode: Distance Field 16
   - Padding: 5
3. **Generate Font Atlas**
4. **Save** as `fa-solid-900 SDF.asset` in `Assets/_UI/_Fonts/Font Awesome 651/`
5. **Run** `Tools > Font Awesome > Switch to Solid Font`
6. **Test** the game again

### Option 3: Run Diagnostic Tools

1. **In Unity**, go to `Window > Font Awesome Glyph Test`
2. **Click** "Load fa-regular-400 SDF"
3. **Click** "Test Icon Codes" - this will show which icons exist
4. **OR** run the PlayMode test `FontGlyphVerificationTest` from Test Runner

## Why This Is Happening

Font Awesome 6 has different variants:
- **Regular (fa-regular-400)**: Subset of icons, mostly outlines - what we're currently using
- **Solid (fa-solid-900)**: Full icon set, filled versions - what we should use

The icons we're trying to use (`\uf04b` play, `\uf021` sync, `\uf554` walking, `\uf6bb` bed) likely don't exist in the Regular variant, which is why they're not displaying.

## Expected Outcome

After switching to the solid font:
- ✓ All button icons should display correctly
- ✓ NextUnit button shows play icon
- ✓ NextTurn button shows sync/refresh icon  
- ✓ Move button shows walking icon
- ✓ Camp/Rest button shows bed icon

## If You Have Questions

Check `Assets/_UI/_Fonts/FONT_ICON_FIX.md` for detailed documentation on:
- Step-by-step font generation
- Alternative icon codes if you want to stay with Regular font
- Testing procedures
- Troubleshooting

The debug logging I added will help diagnose any remaining issues after the font fix.

