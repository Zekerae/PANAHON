# P.A.N.A.H.O.N. - Unity 2.5D Tabletop Base Game

A cooperative and competitive 2.5D tabletop time-travel board game for 4 players, built for Unity (2021 LTS / 2022+).

## Quick Start

1. Open your Unity project (URP or Built-in RP)
2. Drag the `Assets/PANAHON` folder into your project's `Assets/`
3. Go to **Tools > P.A.N.A.H.O.N. > Generate 2.5D Tabletop Scene**
4. Press **Play** to test the full game loop!

## Project Structure

```
Assets/PANAHON/
├── Scripts/
│   ├── Board/         # BoardTile, BoardPath, BoardManager, TimeGatePortal
│   ├── Cards/         # CardDatabase, DeckManager
│   ├── Core/          # GameManager, TurnManager, TimelineManager, ParadoxManager, ScoreManager
│   ├── Data/          # ScriptableObjects: EraType, TileType, GameConfig, CardData hierarchy
│   ├── Player/        # PlayerController, PlayerInventory
│   └── UI/            # UIManager, PlayerHUDCard, DiceRollerUI, TimeGateDialogUI, etc.
├── Editor/            # PanahonSceneBuilder, PanahonSetupWindow
├── Resources/         # PanahonConfig.json
└── Documentation/     # Game design doc & import guide
```

## Version History

- **v2.0.0** - 2.5D Tabletop refinement (Quads, Sprites, proper depth sorting)
- **v1.0.0** - Initial base game framework
