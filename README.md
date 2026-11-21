# Tower Grid

**Unity Mobile Puzzle Game · C# · VContainer · URP**

An offline portfolio edition of a professionally developed mobile puzzle game focused on building a tower on a grid, combining spatial decisions, score bonuses, timed sessions, and meta-progression.

This repository presents the game's architecture, gameplay systems, and UI implementation. It is a team project that I contributed to professionally, not a solo-developed title.

## Gameplay

- **Grid-based building:** Drag and place buildings of different sizes and types while respecting placement and support rules.
- **Scoring system:** Earn points for placement, structural support, adjacency, completed floors, zoning, and combinations.
- **Active skills:** Demolition, refresh, and rezoning mechanics affect available building choices and the current layout.
- **Timed game loop:** Countdown, playing, tutorial, and game-over states handle match progression.
- **Meta-progression:** Daily quests, shop items, customization, and battle pass screens.
- **Local scores:** Best, average, and daily results are stored on the device in the offline edition.

## Engineering highlights

| Area | Implementation |
| --- | --- |
| Architecture | Modular services and dependency injection with **VContainer** and scene-specific lifetime scopes |
| Gameplay flow | Explicit state machine for tutorial, countdown, playing, and game-over phases |
| Building mechanics | Placement validation, grid occupancy, support checks, building spawning, and scoring |
| Game content | ScriptableObject catalogs and configuration assets for building types and visual variations |
| UI systems | Scene UI binders, popup service, overlays, shops, quests, and progression interfaces |
| Persistence | Local player settings, game progress, and scores |
| Portfolio adaptation | Offline gameplay without corporate backend, tournament services, or attribution integration |

## Tech stack

**Unity, C#, VContainer, UniTask, Universal Render Pipeline (URP), Unity UI, ScriptableObjects, PlayerPrefs.**

The project also contains third-party visual effects and haptics packages. These are separate dependencies with their own licensing requirements.

## Project structure

```text
Assets/Game/Scripts/
├── GameRoot/                 # Bootstrap, global services, popups, scene management
├── Gameplay/
│   ├── Building/             # Grid placement, building models, scoring
│   ├── Core/                 # Game loop, state machine, timer, local scores
│   ├── Root/                 # Gameplay scene entry point and lifetime scope
│   └── Skills/               # Demolition, refresh, rezoning
├── MainMenu/
│   ├── BattlePass/           # Progression and rewards
│   ├── DailyTasks/           # Quest tracking
│   ├── Shop/                 # Cosmetics, progression, wallet
│   └── Root/                 # Menu scene configuration
└── Tutorial/                 # Tutorial scene systems
```

See [Architecture Overview](docs/ARCHITECTURE.md) for a more detailed explanation of the game flow and service boundaries.

## Getting started

1. Clone the repository and add its root folder in **Unity Hub**.
2. Open the project with the Unity Editor version verified for your environment, and allow package resolution to finish.
3. Open `Assets/Game/Scenes/Boot.unity` and enter Play Mode.
4. On a fresh profile, the game starts with a tutorial session. Subsequent launches proceed to the main menu.

**Editor compatibility:** `ProjectSettings/ProjectVersion.txt` currently records Unity `2022.3.62f3`, while the provided package manifest references URP `17.0.4`. These versions may not match on a clean installation. Use a verified editor/package combination and recheck the rendering configuration if importing from scratch.

## Offline portfolio edition

The original project depended on external tournament, attribution, and company infrastructure. This edition uses a local match flow and local score storage; analytics events are handled by a no-op adapter. There is no requirement for a corporate backend or tournament account to play the local game.

## Project attribution and usage

This codebase originated from a commercial team project and is shared as an authorized portfolio example. The repository should not be interpreted as a claim that every asset, script, or gameplay feature was created by one developer.

Third-party assets and plugins are governed by their own licenses. Public repository access does **not** grant permission to redistribute or reuse those assets. No general-purpose open-source license is declared for this project.
