# P.A.N.A.H.O.N. - Game Design Document (GDD)

## Game Overview
**P.A.N.A.H.O.N.** (Philippine Acronym for National Archaeological Heritage of Nations) is a cooperative and competitive 2.5D tabletop time-travel board game for 4 players. Players travel across different historical eras of the Philippines to investigate clues, locate displaced artifacts, complete assigned missions, and return artifacts to their correct timelines.

**Genre:** 2.5D Cooperative/Competitive Tabletop Strategy  
**Players:** 4 (exactly)  
**Target Platform:** Unity 2021 LTS / 2022+  
**Perspective:** 2.5D Orthographic Top-Down  

---

## Game Story
Players take the role of **Temporal Castaways** — individuals responsible for correcting disturbances across different timelines. Mysterious artifacts have been displaced from their original eras. Each player receives a mission involving a specific artifact. To complete their mission, players must:
1. Travel through different eras
2. Gather clues
3. Identify the correct artifact
4. Return the artifact to its proper timeline

---

## Four Historical Eras

| Era | Period | Theme Color | Key Elements |
|-----|--------|-------------|--------------|
| **Spanish Era** | 1521-1898 | Terra Cotta | Galleons, Intramuros, Colonial relics |
| **American Era** | 1898-1946 | Navy Blue | Commonwealth, Telegraphs, Modern industry |
| **Japanese Era** | 1942-1945 | Crimson | Guerrilla resistance, Wartime intelligence |
| **Myth & Legend** | Pre-Colonial | Amethyst | Bathala, Bakunawa, Ancient spirits |

---

## Player Characters (Temporal Castaways)

| Character | Role | Starting Era | Unique Ability | Effect |
|-----------|------|-------------|----------------|--------|
| Dr. Mateo Cruz | Chrono-Archivist | Spanish | Archival Mastery | +3 Energy, -1 Paradox |
| Tala Dimaculangan | Rift-Walker | American | Warp Harmonic | +4 Energy, reduced Time Gate cost |
| Capt. Andres Valiente | Paradox Dampener | Japanese | Timeline Anchor | -2 Individual Paradox |
| Maya Santos | Quantum Scavenger | Myth & Legend | Temporal Salvage | +20 Points, +2 Energy |

---

## Core Mechanics

### Movement
- Forward-only along the circular era track
- Roll 1d6 to determine spaces moved
- No backward movement

### Tile Types

| Tile | Symbol | Effect |
|------|--------|--------|
| **Start (Timeline Nexus)** | ★ | Recovery point, +1 Energy on landing |
| **Clue** | 🔎 | Draw a Clue Card, gain clue + 5 pts |
| **Event** | ⚡ | Draw an Event Card (variable effects) |
| **Artifact** | ◆ | Acquire or return artifacts |
| **Danger** | ⚠ | Hazard penalties (energy, paradox, timeline) |
| **Time Gate** | ◎ | Portal to jump between eras (costs Energy) |

### Resource Management
- **Energy** (0-15): Spent on Time Gate jumps, lost to Dangers. Restored by Events, abilities, and Start tiles.
- **Individual Paradox** (0-5): Gained by wrong actions. At 5/5: lose 3 Energy, skip a turn, +1 to shared Timeline Meter.
- **Score**: Accumulated through missions, clues, artifacts, and events.

### Shared Timeline Meter (0-10)
- **Cooperative mechanic**: Increased by Paradox overflows, Danger cards, wrong artifact placements
- **Decreased by**: Correct artifact returns, certain Events, cooperative actions
- **At 10/10**: TIMELINE COLLAPSE — ALL PLAYERS LOSE (cooperative defeat)

### Time Gate Travel
- Costs 2 Energy (1 for Tala the Rift-Walker)
- Warps player to the Start tile of the target era
- Free warps can be granted by special Events (e.g., Bathala's Blessing)

### Unique Abilities
- Each character has a special ability with a cooldown (default: 2 turns)
- Can be used once per cooldown cycle during the player's turn

---

## Card Types

### Mission Cards (4 total, 1 per player)
Assigned at game start. Specifies:
- Target artifact to find
- Required clue count to acquire it
- Era to deliver it to
- Completion points (50 pts)

### Clue Cards (12 total)
Found on Clue tiles. Categories:
- Appearance, Purpose, Historical Connection, Location
- Each clue links to a specific artifact

### Artifact Cards (6 total)
Found on Artifact tiles in displacement eras:
- Golden Monstrance of Intramuros (Spanish → found in American)
- Corregidor Ham Radio Transceiver (American → found in Japanese)
- Guerrilla Intelligence Cipher Book (Japanese → found in Spanish)
- Golden Tara of Agusan (Myth → found in American)
- Spoliarium Charcoal Sketch (Spanish → found in Myth)
- Bakunawa Lunar Pearl (Myth → found in Japanese)

### Event Cards (8 total)
Variable positive/mixed effects:
- Energy restoration, Timeline stabilization
- Bonus movement, Free era warps, Point bonuses

### Danger Cards (6 total)
Negative temporal hazards:
- Energy drain, Paradox increase, Timeline destabilization
- Skip turns, Forced relocation to Start

---

## Win/Lose Conditions

### Victory (Competitive within Cooperative)
- All 4 players complete their missions before time expires (40 minutes)
- Individual scores tallied for competitive ranking
- Highest scorer wins the individual competition

### Defeat (Cooperative)
- Shared Timeline Meter reaches 10/10 → **Timeline Collapse**
- ALL players lose regardless of individual scores

### Scoring Breakdown

| Category | Points |
|----------|--------|
| Mission Complete | +50 |
| Correct Artifact Return | +30 each |
| Clue Collected | +5 each |
| Remaining Energy | +2 per point |
| Timeline Stability Bonus | +20 (0-3), +10 (4-6), +0 (7+) |
| Paradox Penalty | -10 per point |

---

## 2.5D Visual Design

### Camera Setup
- Orthographic projection
- Size: 13.5
- Position: (0, 22, -14)
- Rotation: (58°, 0°, 0°)

### Board Layout
- 4 era tracks arranged in quadrants around a central Chrono-Nexus
- Each track: 10 tiles in a circular loop
- Tiles: Flat Quads facing the camera
- Path lines: LineRenderers connecting sequential tiles
- Era labels: TextMesh markers at each quadrant center

### Player Tokens
- SpriteRenderer with procedural circular textures
- Color-coded per character
- Hop animation for movement (arc via Y and Z offsets)
- Sorting order for proper depth layering

---

## Technical Architecture

### Namespace Structure
```
Panahon.Data    → Enums, ScriptableObjects, CardData
Panahon.Board   → BoardTile, BoardPath, BoardManager, TimeGatePortal
Panahon.Core    → GameManager, TurnManager, TimelineManager, ParadoxManager, ScoreManager
Panahon.Player  → PlayerController, PlayerInventory
Panahon.Cards   → CardDatabase, DeckManager
Panahon.UI      → UIManager, PlayerHUDCard, DiceRollerUI, etc.
Panahon.Editor  → PanahonSceneBuilder, PanahonSetupWindow
```

### Design Patterns
- **Singleton**: All managers use static Instance patterns
- **Event-Driven**: Extensive use of C# events for decoupled communication
- **ScriptableObject Data**: Card types and character data are ScriptableObject-based
- **Coroutine Sequencing**: Turn flow, movement, and animations use coroutines

### Assembly Definitions
- `Panahon.Scripts.asmdef` → Runtime assembly (all gameplay scripts)
- `Panahon.Editor.asmdef` → Editor-only assembly (scene builder, setup window)
