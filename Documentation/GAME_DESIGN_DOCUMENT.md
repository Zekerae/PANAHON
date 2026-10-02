# P.A.N.A.H.O.N. - Game Design Document (GDD)

**Title**: P.A.N.A.H.O.N. (Temporal Castaways)  
**Genre**: Cooperative & Competitive Tabletop Time-Travel Strategy  
**Players**: 4 Players  
**Time Limit**: 40 Minutes Maximum  
**Target Platform**: Unity Engine (PC / Mac / Mobile / WebGL)

---

## 1. Overview & Core Concept
**P.A.N.A.H.O.N.** (*"Time" / "Era" in Filipino*) is a board game set across Philippine history and mythology. Players embody **Temporal Castaways** tasked with correcting anomalies across 4 timelines. While players compete individually for points, they share a cooperative **Timeline Meter**. If history suffers too much disruption and reaches maximum destabilization (10/10), the timeline collapses and **all players lose**.

---

## 2. The 4 Historical Timelines

| Timeline | Era | Aesthetic / Themes | Key Relics |
|---|---|---|---|
| **1. Spanish Era** | 1521–1898 | Terra cotta, stone fortresses, Intramuros, galleon trade | Golden Monstrance of Intramuros, Spoliarium Charcoal Sketch |
| **2. American Era** | 1898–1946 | Vintage navy, industrial machinery, ham radios, telegrams | Corregidor Ham Radio Transceiver, MacArthur Telegrams |
| **3. Japanese Era** | 1942–1945 | Bamboo crimson, wartime resistance, guerrilla intelligence | Guerrilla Intelligence Cipher Book, Bamboo Courier Tubes |
| **4. Myth & Legend Era** | Pre-Colonial | Mystical violet, Bathala, Bakunawa, spirits, sacred gold | Golden Tara of Agusan, Bakunawa Lunar Pearl |

---

## 3. The 4 Temporal Castaways & Unique Abilities

| Player | Name | Role | Unique Ability | Cooldown |
|---|---|---|---|---|
| **P1 (Blue)** | **Dr. Mateo Cruz** | *Chrono-Archivist* | **Archival Mastery**: Restores +3 Energy and -1 Paradox. | 2 Turns |
| **P2 (Pink)** | **Tala Dimaculangan** | *Rift-Walker* | **Warp Harmonic**: Reduces Time Gate energy cost from 2 to 1. Restores +4 Energy. | 2 Turns |
| **P3 (Red)** | **Capt. Andres Valiente** | *Paradox Dampener* | **Timeline Anchor**: Absorbs anomaly fallout; cleanses -2 Personal Paradox. | 2 Turns |
| **P4 (Green)** | **Maya Santos** | *Quantum Scavenger* | **Temporal Salvage**: Scavenges chronal energy; earns +20 Points and +2 Energy. | 2 Turns |

---

## 4. Game Board & Tile Types

Each of the 4 timelines features an era track consisting of 10 sequential tiles:
1. **Start Tile (Nexus)**: Starting entry point; safe resting location.
2. **Clue Tile**: Draw a Clue Card related to target artifacts (Appearance, Purpose, Historical Connection, Location).
3. **Event Tile**: Triggers historic occurrences (e.g. Galleon Fair, Telegraph Synchrony, Bathala's Blessing) that alter Energy or Meters.
4. **Artifact Tile**: Claim an artifact if you hold the required clues, or deliver a held artifact to return it to history.
5. **Danger Tile**: Hazard (patrols, temporal feedback loops, Bakunawa's wrath) causing Energy loss, Paradox increase, or skipped turns.
6. **Time Gate Tile**: Allows the player to spend Energy to jump across eras into another timeline.

### Movement Restriction
- Movement along an era track is **forward-only** (players cannot move backward).
- If a player misses an artifact or tile, they must travel forward to a **Time Gate** to jump across eras or loop back.

---

## 5. Meters & Resources

### A. Energy Marker (Per Player)
- **Scale**: 0 to 15 (Starts at 10).
- **Consumption**: Jumping through Time Gates costs **2 Energy** (1 Energy for Tala).
- **Depletion**: At 0 Energy, players cannot travel between eras until restored through resting, abilities, or events.

### B. Paradox Marker (Per Player)
- **Scale**: 0 to 5 (Starts at 0).
- **Increase**: Making timeline mistakes (e.g., returning an artifact to the wrong era) or drawing Danger cards.
- **Consequences**:
  - Reaching 5 Paradox causes temporal feedback: player loses their next turn, loses 3 Energy, and adds +1 Destabilization to the shared Timeline Meter.
  - End of game: **-10 points per Paradox level**.

### C. Timeline Meter (Cooperative / Shared)
- **Scale**: 0 to 10 (Starts at 0).
- **Destabilization**: Occurs when events disrupt history, players trigger hazards, or paradoxes overflow.
- **CRITICAL CONDITION**: If this meter reaches **10/10 (100%)**, **the timeline collapses and ALL PLAYERS LOSE INSTANTLY**, regardless of individual scores.

---

## 6. Scoring System (When Timeline is Preserved)

| Action / Resource | Score Adjustment |
|---|---|
| Assigned Mission Completed | **+50 Points** |
| Artifact Returned to Correct Timeline | **+35 Points** each |
| Clues Collected | **+5 Points** each |
| Remaining Energy | **+2 Points** per energy |
| Individual Paradox Penalty | **-10 Points** per paradox level |
| Shared Timeline Stability Bonus | **+20 Points** (if meter ≤ 3) / **+10 Points** (if meter ≤ 6) |

---

## 7. Victory Conditions
1. **Cooperative Loss**: Timeline Meter reaches 10 -> Game ends in defeat.
2. **Cooperative & Competitive Victory**:
   - All 4 players complete their assigned mission, OR the 40-minute timer expires while the timeline remains intact.
   - The player with the highest final score is named the **Master Chrono-Keeper**!
