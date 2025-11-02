# Unit Buttons Feature Plan

## Current State
- Move button (walk icon)
- Rest button (camp icon)  
- Buttons enable/disable based on unit state and movesLeft
- Container shows/hides based on selected unit

## Requirements

### 1. Terminology Change
- Rename `movesLeft` → `actionsLeft` throughout codebase
- Update all references in UnitInstance, tests, UI

### 2. Rest/Camp Button Visual Indication
**When resting:**
- Button should show "active" state (different styling)
- Unit flag on tilemap should show rest indicator

**Tests:**
- `RestButton_ShowsActiveState_WhenUnitResting()`
- `RestButton_ShowsDefaultState_WhenUnitReady()`
- `UnitFlag_ShowsRestIndicator_WhenUnitResting()`

### 3. Move Button Action Point Display
**Show remaining actions on button:**
- Display number overlay on move button
- Gray out when actionsLeft == 0
- Update on moves consumed

**Tests:**
- `MoveButton_DisplaysActionPoints_ForSelectedUnit()`
- `MoveButton_ShowsZero_WhenNoActionsLeft()`
- `MoveButton_UpdatesActionPoints_AfterMove()`
- `MoveButton_GrayedOut_WhenNoActionsLeft()`

### 4. Rest Button Disabled When No Actions
**Current:** Rest enabled if Ready and movesLeft > 0
**New:** Keep same logic but with renamed actionsLeft

**Tests:**
- `RestButton_Disabled_WhenNoActionsLeft()`
- `RestButton_Enabled_WhenHasActions()`

### 5. Container Positioning (verify, don't change yet)
**Verify CSS is correct:**
- Landscape: bottom: 2%
- Portrait: bottom: 20%

**Tests:**
- `Container_PositionedCorrectly_InLandscape()`
- `Container_PositionedCorrectly_InPortrait()`

## Implementation Order

1. **Rename movesLeft → actionsLeft**
   - Update UnitInstance struct
   - Update all manager classes
   - Update UI code
   - Update existing tests

2. **Write tests for rest button active state**
   - Create tests first (TDD)
   - Implement CSS class toggling

3. **Write tests for action point display**
   - Add Label to move button UXML
   - Update button to show count

4. **Write tests for unit flag rest indicator**
   - Update tilemap rendering

5. **Add positioning verification tests**
   - Check computed styles in landscape/portrait

## Files to Modify
- `Assets/_GAME/Units/UnitInstance.cs` - rename field
- `Assets/_GAME/Units/UnitManager.cs` - update all refs
- `Assets/_GAME/Game/Game.cs` - update refs
- `Assets/_GAME/Game/TurnManager.cs` - update refs
- `Assets/_UI/UnitButtons/UnitButtonsUI.cs` - display logic
- `Assets/_UI/UnitButtons/UnitButtons.uxml` - add Label
- `Assets/_UI/UnitButtons/UnitButtons-Common.uss` - active state CSS
- `Assets/Tests/PlayMode/UnitButtonsUITests.cs` - NEW file
- All existing tests - update terminology

