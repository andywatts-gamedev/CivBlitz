# Test Quick Reference

## The Bug Being Tested

**Problem:** When clicking a tile with a unit, the UI shows tile terrain stats but NOT the unit stats.

**Solution:** Attack column now conditionally displays:
- **With unit:** Shows attack icon, unit attack, and tile attack bonus
- **Without unit:** Hides attack column (only shows name + defense)

**Previous Issues:**
1. `TargetTile-Common.uss` had `.attack { display: none; }` hiding ALL attack columns globally
2. Attack column wasn't conditionally shown based on unit presence

## What Each Test Verifies

### ✓ UnitManagerTests (7 tests)
**Purpose:** Verify unit lookup logic works
- If these fail: Bug is in UnitManager
- If these pass: Bug is in UI layer

### ✓ SelectedTileUIIntegrationTests (6 tests)

**Critical Tests:**
1. `UnitRow_DisplaysFlex_WhenUnitExists`
   - Loads Game scene
   - Finds actual unit
   - Clicks tile
   - **Asserts:** `unitRow.style.display.value == DisplayStyle.Flex`
   - **If this fails:** Unit row isn't showing (YOUR BUG)

2. `UnitRow_DisplaysNone_WhenNoUnit`
   - Clicks empty tile
   - **Asserts:** `unitRow.style.display.value == DisplayStyle.None`
   - **If this fails:** Unit row showing when it shouldn't

**Data Tests:**
3. `SelectedTile_ShowsCorrectAttackValue_ForMeleeUnit`
   - **Asserts:** Attack label shows `unit.melee` for melee units

4. `SelectedTile_ShowsCorrectAttackValue_ForRangedUnit`
   - **Asserts:** Attack label shows `unit.ranged` for ranged units

5. `TargetTile_ShowsUnitRow_WhenUnitExists`
   - Same as #1 but for hover/target UI

6. `TileInfo_AlwaysShows_Regardless`
   - **Asserts:** Tile terrain stats always display

## Running Tests

```bash
# In Unity
Window > General > Test Runner > PlayMode > Run All

# Command line
./run_tests.sh

# Will fail on first run (bug exists)
# Fix the bug, tests should pass
```

## Expected Test Results

**Current State (Bug Exists):**
- ❌ `UnitRow_DisplaysFlex_WhenUnitExists` - FAILS
- ✓ All other tests pass

**After Fix:**
- ✓ All tests pass

## Debug Flow

1. Run tests in Unity Test Runner
2. Check which tests fail
3. If UnitManagerTests fail → Fix UnitManager
4. If Integration tests fail → Check debug logs
5. Press `I` with TileUIDebugger to inspect UI structure
6. Check if UnitRow element exists
7. Check if display style is being set correctly

