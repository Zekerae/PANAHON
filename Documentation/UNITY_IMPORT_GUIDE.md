# P.A.N.A.H.O.N. - Unity Import & Quickstart Guide

This base game package contains the complete, ready-to-run architecture for **P.A.N.A.H.O.N.** in Unity.

---

## 🚀 Quickstart: How to Import into Unity

You can use this project in **two easy ways**:

### Option A: Create a New Project in Unity Hub (Recommended)
1. In Unity Hub, click **New project**.
2. Select the **`Universal 2D`** template (or `Universal 3D`).
   > **Note**: For P.A.N.A.H.O.N.'s **2.5D Tabletop** aesthetic, **`Universal 2D`** provides the optimal 2D/2.5D renderer, sprite sorting, orthographic camera, and crisp UI.
3. Name your project (e.g. `PANAHON_2.5D`) and click **Create project**.
4. Once Unity opens, copy the `Assets/PANAHON` folder into your project's `Assets/` directory.

### Option B: Import into an Existing Unity Project
1. Open your existing Unity project in Unity Editor.
2. Copy the `Assets/PANAHON` folder into your project's `Assets/` directory:
   ```
   YourProject/Assets/PANAHON/
   ```
3. Unity will automatically compile the C# scripts.

---

## ⚡ 1-Click Scene Generation

Once the scripts are compiled in Unity:

1. Open a new empty scene (`File > New Scene`).
2. Go to the top Unity menu bar and click:  
   👉 **`Tools > P.A.N.A.H.O.N. > Generate Complete Playable Scene`**
   *(or open `Window > P.A.N.A.H.O.N. > Setup & Control Panel`)*
3. **Done!** The script automatically creates:
   - The central Chrono-Nexus board table.
   - 4 Quadrant Timeline tracks (Spanish Era, American Era, Japanese Era, Myth/Legend Era).
   - All 40 board tiles color-coded by tile type (Start, Clue, Event, Artifact, Danger, Time Gate).
   - 4 Player pieces (Dr. Mateo Cruz, Tala Dimaculangan, Capt. Andres Valiente, Maya Santos).
   - Complete Card Database with authentic Philippine history lore.
   - All 5 card decks (Missions, Clues, Artifacts, Events, Dangers).
   - Complete UI Canvas (Cooperative Timeline Meter, 4 Player HUD cards, Animated 1d6 Dice Roller, Time Gate warping modal, Card modal, and Game Log).
   - Configures camera and lighting.
4. Press **Play (▶)** in the Unity toolbar to immediately play the game!

---

## 📁 Project Architecture Overview

```
Assets/PANAHON/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs       # Master state coordinator (Setup, Roll-off, Playing, Loss, Victory)
│   │   ├── TurnManager.cs       # 4-player turn sequencing, movement, tile resolution
│   │   ├── TimelineManager.cs   # Shared cooperative Timeline Meter (0-10) & collapse trigger
│   │   ├── ParadoxManager.cs    # Individual player paradox (0-5) & penalties
│   │   └── ScoreManager.cs      # 40-minute round timer & end-of-game score calculator
│   ├── Data/
│   │   ├── EraType.cs           # SpanishEra, AmericanEra, JapaneseEra, MythLegendEra
│   │   ├── TileType.cs          # Start, Clue, Event, Artifact, Danger, TimeGate
│   │   ├── CardData.cs          # Base ScriptableObject for all cards
│   │   ├── MissionCardData.cs   # Individual player missions & return era requirements
│   │   ├── ClueCardData.cs      # Clue hints (Appearance, Purpose, Connection, Location)
│   │   ├── ArtifactCardData.cs  # Relic data (Origin era, displaced era, score value)
│   │   ├── EventCardData.cs     # Historical events & meter adjustments
│   │   ├── DangerCardData.cs    # Temporal hazards & penalties
│   │   └── PlayerCharacterData.cs # Character stats, colors, and unique abilities
│   ├── Board/
│   │   ├── BoardTile.cs         # Node on board, path connection, visual color
│   │   ├── BoardPath.cs         # Era track node manager
│   │   ├── BoardManager.cs      # Inter-era routing & Time Gate portal registry
│   │   └── TimeGatePortal.cs    # Time Gate mechanics & energy cost
│   ├── Player/
│   │   ├── PlayerController.cs  # Pawn movement animation, stats, abilities
│   │   └── PlayerInventory.cs   # Active mission, clues held, artifacts claimed/returned
│   ├── Cards/
│   │   ├── CardDatabase.cs      # Complete database of Philippine historical cards
│   │   └── DeckManager.cs       # Deck shuffling, drawing, and discard piles
│   └── UI/
│       ├── UIManager.cs         # Canvas coordinator
│       ├── TimelineMeterUI.cs   # Shared cooperative timeline stability gauge
│       ├── PlayerHUDCard.cs     # Individual player status panel & ability button
│       ├── DiceRollerUI.cs      # Animated 1d6 dice roller
│       ├── TimeGateDialogUI.cs  # Destination era selector modal
│       ├── CardModalUI.cs       # Card inspection popup
│       ├── ActionLogUI.cs       # Real-time game history feed
│       └── GameOverModalUI.cs   # Leaderboard or Timeline Collapse game-over screen
├── Editor/
│   ├── PanahonSceneBuilder.cs   # 1-Click scene generator
│   └── PanahonSetupWindow.cs   # Interactive Editor hub window
├── Resources/
│   └── PanahonConfig.json       # Balance configurations & constants
└── Documentation/
    ├── UNITY_IMPORT_GUIDE.md    # This guide
    └── GAME_DESIGN_DOCUMENT.md  # Complete rulebook and design spec
```

---

## 🎮 How Gameplay Runs in Unity

1. **Roll for First Turn**: At start, all 4 players roll a d6; the highest roll goes first.
2. **Player Turn**:
   - The active player's HUD highlights.
   - The player may trigger their **Unique Ability** if available.
   - The player clicks **Roll Movement** (rolls 1-6).
   - The player's token hops forward node-by-node along the track.
   - **Forward Movement Only**: Players cannot go backward on an era track, but can use Time Gates to jump to other eras or loop back!
3. **Tile Resolution**:
   - **Clue Tile**: Reveals a clue card for an artifact.
   - **Artifact Tile**: If clues are fulfilled, player acquires the artifact. If returning an artifact in the correct era, earns +35 pts and stabilizes the timeline! (Returning to wrong era adds +2 Paradox).
   - **Event Tile**: Triggers historic events (energy changes, timeline shifts).
   - **Danger Tile**: Temporal hazard (-Energy, +Paradox, timeline damage, or skipped turns).
   - **Time Gate Tile**: Modal opens allowing player to spend Energy to jump to another era.
4. **Endgame**:
   - **Timeline Collapse (Cooperative Defeat)**: If the shared Timeline Meter reaches 10, the timeline collapses and **all players lose instantly**!
   - **Victory / Timer Expiry**: When all missions are completed or the 40-minute timer runs out, the game calculates final scores and crowns the Master Chrono-Keeper!
