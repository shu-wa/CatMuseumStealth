# Cat Museum Stealth

## Overview

Cat Museum Stealth is a 3D stealth game prototype developed in Unity.  
The player controls a cute two-head-tall cat burglar who sneaks through a museum, steals artworks, swaps them with dummy items, and avoids being caught by patrolling guard cats.

This project focuses on gameplay systems, AI behavior, interaction design, and extensible Unity project structure.

## Concept

The core concept is a stealth game where the player must decide whether to safely swap an artwork with a matching dummy or quickly steal it by force.

Swapping is safer but takes longer.  
Stealing is faster but increases the alert level more.  
If a guard sees the player during an interaction, the action is canceled and the alert level increases.

## Implemented Features

### Player

- Third-person player movement
- Camera-relative WASD movement
- Shift dash
- Movement lock during interaction
- Animation-ready movement parameters:
  - `IsMoving`
  - `IsSprinting`
  - `MoveAmount`
  - `CurrentSpeed`

### Camera

- Mouse-controlled third-person camera
- Pitch and yaw control
- Cursor lock/unlock
- Camera collision using SphereCast
- Camera does not pass through walls or ceilings

### Artwork Interaction

- Artwork data managed with ScriptableObject
- Artworks have:
  - Category
  - Size
  - Value
  - Alert increase when stolen
  - Alert increase when swapped
- Dummy item system
- Inventory capacity system
- Interaction prompt UI
- Timed interactions:
  - Swap
  - Steal
  - Recover dummy
  - Place dummy
- Interaction progress shown on UI
- Player cannot move during interactions

### Dummy System

- Matching dummy is required for safe swapping
- Dummy matching uses artwork category and size
- After swapping, placed dummy can be recovered
- Empty pedestals can be covered by placing a matching dummy
- This creates strategic choices around risk, capacity, and item reuse

### Alert System

- Global alert level
- Alert increases depending on action
- Different thresholds for future gameplay expansion:
  - Middle alert
  - High alert
  - Maximum alert

### Room System

- Exhibition room zones
- Current room is shown on UI
- Room effects modify alert increase
- Example room types:
  - Painting Room
  - Sculpture Room
  - Special Room

### Guard System

- Guard patrol using NavMeshAgent
- Patrol route with multiple points
- Guard vision detection
- Guards can detect the player during interaction
- If caught during interaction:
  - Interaction is canceled
  - Alert level increases
  - Warning message is shown

### UI

- Alert level display
- Inventory capacity display
- Score display
- Current room display
- Interaction prompt
- Interaction progress text
- Notice messages

## Technical Highlights

- Unity 3D / URP project
- ScriptableObject-based data design
- Component-based interaction system
- Layer-based detection:
  - Interactable
  - Player
  - Room
  - CameraBlocker
- NavMeshAgent patrol AI
- SphereCast camera collision
- OverlapSphere interaction detection
- Guard vision using distance, angle, and obstacle raycast
- Extensible class structure for future chase/search AI

## Current Gameplay Loop

1. The player explores the museum.
2. The player approaches an artwork.
3. The UI shows artwork value, size, category, and dummy availability.
4. The player chooses:
   - `E`: Swap with dummy
   - `F`: Steal by force
5. The action takes time.
6. The player cannot move while interacting.
7. If a guard sees the player during the action, the action is canceled.
8. The alert level increases.
9. The player tries to collect valuable artworks while managing risk and capacity.

## Planned Features

- Guard chase behavior
- Guard search behavior after losing sight of the player
- Guard memory system
- Alert-level-based guard behavior
- Carrying stolen items affecting suspicion
- Body check system
- Mouse toy distraction item
- Multiple museum maps
- Result screen
- Save system
- Character animations
- Final cat character models and museum assets

## Controls

| Input | Action |
|---|---|
| WASD | Move |
| Shift + WASD | Dash |
| Mouse | Rotate camera |
| E | Swap / Place dummy |
| F | Steal / Recover dummy |

## Project Structure

```text
Assets/CatMuseum
├── Materials
├── Prefabs
├── Scenes
├── ScriptableObjects
├── Scripts
│   ├── Camera
│   ├── Core
│   ├── Data
│   ├── Environment
│   ├── Guard
│   ├── Interact
│   ├── Items
│   ├── Player
│   └── UI
└── Textures
```

## Development Notes

This project is being developed with scalability in mind.  
Gameplay values are separated into ScriptableObjects where possible, reusable objects are managed as Prefabs, and map-specific objects are kept inside map scenes.

The current prototype prioritizes gameplay systems before final visual polish.
