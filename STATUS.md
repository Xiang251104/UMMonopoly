# UM Monopoly — Project Status

**Last Updated:** 2026-05-04
**Current Week:** Pre-Week 1 (planning + initial scripts shipped)
**Overall Status:** 🟡 Code skeleton complete — awaiting Unity project bootstrap and team kickoff

---

## 1. Quick Snapshot

| Item | Status |
|------|--------|
| Concept locked | 🟢 UM Monopoly digital board game |
| Team roles assigned | 🔴 Not started |
| Engine chosen | 🟡 Defaulted to Unity (subject to team override) |
| Repository created | 🔴 Not started |
| `.gitignore` | 🟢 Done (Unity preset) |
| Code skeleton (C# scripts) | 🟢 Done — 27 files |
| Unity project bootstrap | 🔴 Pending (Unity Hub + project creation) |
| Tile data assets (40 SOs) | 🔴 Pending — instructions in SETUP.md |
| Card data assets (≥16 SOs) | 🔴 Pending — instructions in SETUP.md |
| Scenes (MainMenu / GameBoard) | 🔴 Pending — created in Unity Editor |
| UI prefabs (PlayerCard, Token) | 🔴 Pending |
| Art (board, characters) | 🔴 Not started |
| Audio (BGM, SFX) | 🔴 Not started |
| Prototype playable | 🔴 ~50% (logic done, no Unity scenes) |
| Report drafted | 🔴 0% |

Legend: 🟢 Done · 🟡 In progress · 🔴 Not started · ⚠️ Blocked

---

## 2. Code Skeleton Inventory (shipped 2026-05-04)

### Data layer (`Assets/Scripts/Data/`)
- `ColorGroup.cs` — color sets for properties
- `TileType.cs`, `CardDeckType` — enums
- `CardEffectType.cs` — card effect strategy enum
- `TileDataSO.cs` — ScriptableObject for tile config
- `CardDataSO.cs` — ScriptableObject for card config
- `GameConfigSO.cs` — global game tuning + tile/card lists

### Entities (`Assets/Scripts/Entities/`)
- `Player.cs` — money, position, properties, jail state, bankruptcy
- `Tile.cs` (abstract) + 9 subclasses: `PropertyTile`, `StationTile`, `UtilityTile`, `TaxTile`, `CardTile`, `JailTile`, `GoTile`, `GoToJailTile`, `FreeParkingTile`
- `Board.cs` — builds tile list, queries by group/type
- `Card.cs`, `CardEffectResolver.cs` — card execution
- `GameContext.cs` — passed to tiles for system access
- `EventBus.cs` — static C# events for UI ↔ logic decoupling

### Systems (`Assets/Scripts/Systems/`)
- `Bank.cs` — buy / rent / tax / award (handles partial payment on bankruptcy)
- `CardDeck.cs` — shuffled draw queue with auto-reshuffle
- `DiceRoller.cs` — 2d6, doubles detection
- `SaveSystem.cs` — JSON snapshot save/load to persistentDataPath

### Core (`Assets/Scripts/Core/`)
- `GameState.cs` — turn-phase enum
- `GameManager.cs` — singleton, owns everything, drives turns
- `TurnController.cs` — coroutine pacing for dice + movement animations
- `GameBootstrap.cs` — wires GameManager + UI on scene start

### UI (`Assets/Scripts/UI/`)
- `HUDController.cs` — top-level HUD, player cards, turn buttons
- `PlayerCardUI.cs` — per-player money/properties display
- `BoardView.cs` — token spawning + movement
- `DiceUI.cs` — dice face display
- `CardPopup.cs` — Akademik/Kampus card popup
- `PropertyPopup.cs` — buy/upgrade dialog
- `MainMenuController.cs` — new game flow
- `EndGameController.cs` — winner screen

**Total:** 27 C# files across 5 folders.

---

## 3. Team Roster

| Member | Role | Contact | Notes |
|--------|------|---------|-------|
| TBD | Project Lead / Game Designer | — | Owns GDD, balancing, report |
| TBD | Programmer (Lead) | — | Core game logic, state machine |
| TBD | Programmer (UI/UX) | — | Board rendering, animations, input |
| TBD | Artist / 2D Designer | — | Board art, cards, character avatars |
| TBD | Audio + QA / Storyteller | — | SFX, music, event card writing, playtest |

> **Action:** Fill in names + contacts at first team meeting.

---

## 4. 14-Week Timeline

| Week | Dates | Milestone | Status |
|------|-------|-----------|--------|
| 1 | TBD | Team kickoff, role assignment, concept lock | 🟡 Concept locked, roles pending |
| 2 | TBD | Engine decision, repo + project skeleton, GDD draft | 🟡 Skeleton done, engine defaulted, repo pending |
| 3 | TBD | Board layout finalized, art style guide, placeholder assets | 🔴 |
| 4 | TBD | Core data models (Player, Tile, Board), tile data populated | 🟡 Models done, SO assets pending |
| 5 | TBD | Dice + movement working, basic UI | 🟡 Logic done, scenes pending |
| 6 | TBD | Buy / rent mechanics, money tracking | 🟢 Logic complete |
| 7 | TBD | Card decks (Akademik / Kampus), event resolution | 🟢 Logic complete; card text pending |
| 8 | TBD | Jail, tax, stations, utilities — all tile types working | 🟢 Logic complete |
| 9 | TBD | Property upgrades, win condition, end game screen | 🟢 Logic complete |
| 10 | TBD | First full playtest. Art polish begins. Audio integration | 🔴 |
| 11 | TBD | UM-themed art finalized. Balance pass. Save/load | 🟡 Save/load logic done; art + balance pending |
| 12 | TBD | Beta build. Internal playtest x3. Bug fixes | 🔴 |
| 13 | TBD | Final bug fixes. Report writing (Part A + Part B) | 🔴 |
| 14 | TBD | **PRESENTATION + SUBMISSION** | 🔴 |

> **Action:** Fill in actual week dates once semester calendar confirmed.

---

## 5. Current Sprint Goals (Week 1)

- [ ] Hold team kickoff meeting
- [ ] Confirm 5 members + assign roles
- [ ] Review and approve REQUIREMENTS.md as a team
- [ ] Confirm or override engine choice (currently Unity)
- [ ] Create GitHub repo and add all members; push existing scripts
- [ ] Set up shared communication channel (WhatsApp / Discord)
- [ ] Schedule weekly recurring meeting
- [ ] **Programmer Lead:** install Unity 2022 LTS, create project per SETUP.md, verify scripts compile
- [ ] **Designer:** start drafting tile data sheet (40 rows: name, price, rents, group)
- [ ] **Designer + Storyteller:** write at least 8 Akademik + 8 Kampus card lines
- [ ] **Artist:** sketch 4 character avatar concepts and 1 board mockup

---

## 6. Backlog (prioritized)

### High Priority (next 2 weeks)
- [ ] Author 40 `TileDataSO` assets in Unity (per REQUIREMENTS §6.1)
- [ ] Author ≥16 `CardDataSO` assets
- [ ] Author `MainGameConfig` and wire all references
- [ ] Build MainMenu and GameBoard scenes
- [ ] Create PlayerCard and PlayerToken prefabs
- [ ] Smoke-test play flow end-to-end (per SETUP.md §12)

### Medium Priority
- [ ] Character avatar concepts (4 sketches)
- [ ] Board background art (digital final)
- [ ] UI wireframes (HUD, popups, menus)
- [ ] Sound effect list + SFX integration
- [ ] Storyboard for opening cinematic (optional)
- [ ] Save/load buttons in UI (logic already done)
- [ ] Dice animation polish (DOTween)

### Low Priority / Stretch
- [ ] AI opponent design + implementation
- [ ] Mortgage system
- [ ] Trading system
- [ ] Auction system
- [ ] BGM track

---

## 7. Completed Work

| Date | Item |
|------|------|
| 2026-05-04 | REQUIREMENTS.md drafted |
| 2026-05-04 | ARCHITECTURE.md drafted |
| 2026-05-04 | STATUS.md drafted |
| 2026-05-04 | `.gitignore` (Unity preset) |
| 2026-05-04 | Full C# code skeleton: Data, Entities, Systems, Core, UI (27 files) |
| 2026-05-04 | SETUP.md with bootstrap instructions for Unity |

---

## 8. Blockers & Risks

| Item | Severity | Owner | Notes |
|------|----------|-------|-------|
| Team not yet formed | High | All | Cannot validate engine choice or kickoff sprint |
| Unity not yet installed | High | Programmer Lead | Cannot test if scripts compile or build SOs |
| Engine choice may be revisited | Med | Team | Defaulted to Unity; if Godot is preferred, port required (see SETUP.md §12) |
| Semester calendar dates not confirmed | Low | Project Lead | Affects week-by-week deadlines |
| `JsonUtility` won't serialize Dictionary | Low | Programmer Lead | SaveSystem flattens to parallel lists; handled |
| Scenes/prefabs are binary — merge conflicts possible | Med | Programmer (UI) | One-owner-per-scene rule + heavy prefab use |

---

## 9. Decisions Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-04 | Theme: UM Monopoly digital board game | Fits "University Malaya Game" brief; achievable in 14 weeks |
| 2026-05-04 | Scope: 2–4 player local hot-seat, no online | Keeps prototype achievable |
| 2026-05-04 | Out of scope for v1: trading, auctions, mortgage, AI, online MP | Risk reduction |
| 2026-05-04 | Default engine: Unity 2022 LTS, C# | Most common, biggest tutorial pool, architecture matches |
| 2026-05-04 | Architecture: layered (Data ← Entities ← Systems ← Core ← UI) with EventBus for UI/logic decoupling | Testable game logic, swappable UI |
| 2026-05-04 | Tiles modeled as polymorphic class hierarchy with `OnPlayerLanded()` | Clean dispatch, easy to extend |
| 2026-05-04 | Cards use enum-driven effect resolver, not subclass per effect | Simpler authoring; balance via SO inspector |
| 2026-05-04 | Save format: JSON via `JsonUtility` to `Application.persistentDataPath` | Built-in, no extra dependency |

---

## 10. Meeting Notes

> Add weekly meeting notes here. Format: date, attendees, decisions, action items.

### YYYY-MM-DD — Kickoff Meeting (template)
- **Attendees:**
- **Decisions:**
- **Action items:**

---

## 11. Useful Links

- Repository: _TBD_
- Project board: _TBD_
- Shared drive (assets, design docs): _TBD_
- GDD: _TBD_

---

## 12. How to Update This File

- Update **Quick Snapshot** at end of every week
- Add new completed items to **Completed Work** as they finish (with date)
- Move tasks from **Backlog** → **Current Sprint Goals** at start of each week
- Log every major decision in **Decisions Log** with rationale
- Mark blockers immediately when discovered
- Bump **Last Updated** date at the top
