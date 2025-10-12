# Civilization-Style Hex Game – Unified Interaction Design (Touch + Mouse)

Goal: Deliver a consistent, intuitive experience for both iPad (touch) and desktop (mouse) players.  
Principles: immediate feedback, minimal errors, natural gestures, clear visual cues.

---

## 1. Tile Inspection / Unit Selection

| Action | Touch | Mouse | Behavior |
|--------|--------|--------|-----------|
| Inspect terrain / unit | **Single tap** on any tile | **Left-click** on any tile | Shows info panel with terrain, owner, and unit stats. Highlights the tapped hex. |
| Select own unit | **Tap** on tile with player’s unit | **Left-click** on own unit | Highlights unit and shows movement range overlay. Panel updates with unit stats. |
| Deselect / close info | **Tap empty tile** or **UI background** | **Left-click** empty tile or background | Removes highlight and closes info panel. |

---

## 2. Map Navigation

| Action | Touch | Mouse | Behavior |
|--------|--------|--------|-----------|
| Pan map | **Drag with one finger** (when not selecting a unit or after panel delay) | **Click-drag with middle mouse** or **right-click-drag** | Moves camera view. |
| Zoom | **Pinch gesture** | **Mouse wheel scroll** | Smooth zoom in/out centered on current view. |

---

## 3. Unit Movement

| Action | Touch | Mouse | Behavior |
|--------|--------|--------|-----------|
| Select unit | **Tap on unit** | **Left-click on unit** | Highlights unit and shows movement range overlay. Panel updates with unit stats. |
| Move to tile | **Drag from unit to destination** | **Left-click-drag from unit to destination** | Shows path preview during gesture. Commits move on release. |
| Cancel selection | **Tap empty tile** | **Right-click empty tile** | Clears selection and highlights. |

---

## 4. Combat Interaction

| Action | Touch | Mouse | Behavior |
|--------|--------|--------|-----------|
| Preview attack | **Tap-hold (~400ms)** on adjacent enemy tile | **Hover over enemy** or **Right-click** enemy tile | Displays combat preview panel (expected damage, odds). No commitment yet. |
| Commit attack | **Release finger** while still on enemy tile after preview | **Left-click** on same enemy after preview | Executes attack and updates UI. |
| Cancel preview | **Slide finger off tile** before releasing OR **tap elsewhere** | **Move cursor away** or **Right-click empty tile** | Dismisses preview panel, no action taken. |

---

## 5. UI and Feedback Cues

| Context | Visual / Haptic Feedback |
|----------|--------------------------|
| Tap / click tile | Brief glow and sound confirm input. |
| Selected unit | Persistent blue border + subtle idle animation. |
| Enemy in range | Red hex outline when unit selected. |
| Possible moves | Blue-tinted hexes. |
| Combat preview | Floating panel near enemy hex with attack stats. |
| Successful move/attack | Distinct animation + sound cue. |
| Cancel / deselect | Fades highlights and closes panels smoothly. |
| Touch feedback (iPad) | Optional light haptic pulse on tap and preview trigger. |

---

## 6. Drag-to-Move (Primary Movement Method)

| Feature | Touch | Mouse | Behavior |
|----------|--------|--------|-----------|
| Drag start | **Touch and drag from unit** | **Left-click-drag from unit** | Shows path preview line from unit to finger/cursor. |
| Drag update | **Move finger over tiles** | **Drag cursor over tiles** | Updates path preview in real-time, highlights valid destination tiles. |
| Drag end | **Release finger on valid tile** | **Release mouse on valid tile** | Commits move, executes pathfinding, moves unit. |
| Drag cancel | **Release finger on invalid tile** | **Release mouse on invalid tile** | Cancels move, clears preview, no action taken. |

---

### Summary

- **Tap / Click = inspect or select**  
- **Drag = move unit with preview**  
- **Release on valid tile = commit move**  
- **Release on invalid tile = cancel**  
- **Tap / Right-click empty = deselect**  

Consistent feedback, clear highlights, and minimal mode confusion ensure a smooth UX across platforms.