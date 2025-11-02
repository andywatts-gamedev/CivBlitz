# Tests

## Running Tests

### In Unity Editor
1. Open Window > General > Test Runner
2. Select PlayMode tab
3. Click "Run All"

### From Command Line
```bash
./run_tests.sh
```

This runs all PlayMode tests in batch mode and outputs results.

## Test Files

### Unit Tests (Fast)
**UnitManagerTests.cs** - Core UnitManager logic
- `TryGetUnit_ReturnsTrue_WhenUnitExists`
- `TryGetUnit_ReturnsFalse_WhenPositionEmpty`
- `HasUnitAt_ReturnsTrue_WhenUnitExists`
- `RegisterUnit_AddsUnitToDictionary`
- `RemoveUnit_RemovesFromDictionary`
- `UpdateUnit_ModifiesExistingUnit`
- `MultipleUnits_SameCiv_AllTracked`

### Integration Tests (Requires Game Scene)
**SelectedTileUIIntegrationTests.cs** - Full UI stack tests
- `UnitRow_DisplaysFlex_WhenUnitExists` ⭐ Main bug test
- `UnitRow_DisplaysNone_WhenNoUnit` ⭐ Main bug test
- `AttackColumn_ShowsWhenUnitExists` ⭐ Attack column conditional display
- `AttackColumn_HidesWhenNoUnit` ⭐ Attack column conditional display
- `SelectedTile_ShowsCorrectAttackValue_ForMeleeUnit`
- `SelectedTile_ShowsCorrectAttackValue_ForRangedUnit`
- `TargetTile_ShowsUnitRow_WhenUnitExists`
- `TileInfo_AlwaysShows_Regardless`
- `UnitRow_ShowsBothAttackAndDefense_WhenUnitExists`

Tests marked ⭐ directly verify the unit row and attack column visibility.

## Test Coverage

These automated tests verify:
1. **UnitManager lookup** - Units are found at correct positions
2. **UI element visibility** - UnitRow shows `DisplayStyle.Flex` when unit exists, `DisplayStyle.None` when empty
3. **Correct values** - Unit name, attack, defense match actual unit stats
4. **Melee vs Ranged** - Attack value shows correct stat based on unit type
5. **Tile info** - Terrain stats always show regardless of unit presence

## CI/CD Integration

The `run_tests.sh` script can be used in CI pipelines:
```bash
./run_tests.sh
```

Exit code 0 = all tests passed
Exit code non-zero = tests failed

Results written to `TestResults.xml` (NUnit format)

## Debug Logging

Both SelectedTileUI and TargetTileUI now log when:
- A unit is found at a position
- No unit is found at a position

Check Console during gameplay to diagnose UI display issues.

## Runtime Debugging

`TileUIDebugger.cs` can be attached to a GameObject in your scene for manual testing:

**Keyboard Shortcuts:**
- `T` - Test tile selection at (0,0)
- `Y` - Test a tile with a unit
- `U` - Test an empty tile
- `I` - Inspect UI structure and element visibility

This helps verify:
1. Unit lookup is working
2. UI elements exist and are properly named
3. Display styles are being set correctly

