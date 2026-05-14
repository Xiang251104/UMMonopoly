# UM Monopoly â€” Project Requirements

**Course:** WIG3005 Game Development, Semester 2 2025/2026
**Team Size:** 5 members
**Submission:** Week 14 (presentation + report + working prototype)
**Last Updated:** 2026-05-04

---

## 1. Course Deliverables (from assignment brief)

The final submission must include:

1. **Working prototype** of the game (presented Week 14)
2. **Report** with two parts:
   - **Part A â€” Conceptual Design:** sketches, storyline, storyboard, character + world design
   - **Part B â€” Technical:** explanation of each tool used in prototype development
3. **Presentation** of the final game

The game concept must cover:

- [ ] High concept statement (paragraph)
- [ ] Player role(s) / avatar description
- [ ] Primary gameplay mode (camera, interaction, challenges)
- [ ] Genre (or hybrid justification)
- [ ] Target audience + expected rating
- [ ] Target platform / special equipment
- [ ] Any licensed IP used
- [ ] Competition modes (single/dual/multi, competitive/cooperative)
- [ ] Game progression summary (levels/missions/storyline)
- [ ] Game world description

---

## 2. High Concept

**Universiti Malaya: The Board** â€” a digital Monopoly-style board game where 2â€“4 players take on the role of UM students competing to "graduate" successfully. Players roll dice, traverse a board themed around UM faculties and landmarks, buy and upgrade properties (faculties), draw event cards reflecting UM campus life (assignment deadlines, makan at KKR, parking saman, scholarship wins), and aim to be the last financially-solvent student or the first to reach a target score. The game blends classic Monopoly mechanics with UM cultural references for entertainment and nostalgia value.

---

## 3. Functional Requirements

### 3.1 Core Gameplay (Must-Have / MVP)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-01 | Support 2â€“4 local players (hot-seat turn-based) | Must |
| FR-02 | Dice roll mechanic (2 six-sided dice) with animation | Must |
| FR-03 | Player avatar moves around board based on roll | Must |
| FR-04 | Buy unowned faculty when landed on | Must |
| FR-05 | Pay rent to faculty owner when landing on owned tile | Must |
| FR-06 | Draw "Akademik" / "Kampus" cards on chance/community tiles | Must |
| FR-07 | Jail mechanic (DTC tile) â€” skip turns or pay fine | Must |
| FR-08 | Tax tiles (Yuran Pengajian / Saman) | Must |
| FR-09 | Win condition: last solvent player OR target score reached | Must |
| FR-10 | Money tracking, balance display per player | Must |
| FR-11 | Property ownership tracking with color-coded sets | Must |
| FR-12 | Upgrade properties (build "Postgrad Centre") | Must |

### 3.2 Should-Have

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-13 | Save / load game state | Should |
| FR-14 | Background music + SFX (dice, money, card draw) | Should |
| FR-15 | Animated player tokens | Should |
| FR-16 | Tile information popup (rent table, owner) | Should |
| FR-17 | Settings menu (volume, player names) | Should |

### 3.3 Could-Have (Stretch Goals)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-18 | Single-player AI opponent | Could |
| FR-19 | Mortgage properties | Could |
| FR-20 | Property trading between players | Could |
| FR-21 | Auctions for unbought properties | Could |
| FR-22 | Online multiplayer | Won't (out of scope for v1) |

---

## 4. Non-Functional Requirements

| ID | Requirement |
|----|-------------|
| NFR-01 | Runs on Windows 10/11 PC, 1920x1080 minimum resolution |
| NFR-02 | Game session length: 30â€“60 minutes typical |
| NFR-03 | Frame rate: 60 FPS on mid-range hardware |
| NFR-04 | Mouse + keyboard controls (no special equipment) |
| NFR-05 | All UM IP usage must be educational/parody (no commercial use) |
| NFR-06 | Code committed to Git daily; no single point of failure |

---

## 5. Game Design Specifications

### 5.1 Player & Avatar

- **Role:** UM student trying to graduate / become wealthiest alumnus
- **Avatars:** 4 selectable student characters (e.g. nerd, athlete, artsy, foodie) â€” purely cosmetic
- **Starting money:** RM 1500 (Monopoly-equivalent)
- **Starting position:** Main Gate (GO equivalent), receive RM 200 each pass

### 5.2 Camera & Interaction

- **Camera:** Fixed top-down 2D view of full board
- **Interaction:** Mouse-driven â€” click dice to roll, click tiles for info, click buttons for actions
- **Challenges:** Resource management (money), risk (rent payments), luck (dice + cards), tile-acquisition strategy

### 5.3 Genre

**Digital board game / strategy hybrid** with light RPG elements (event cards). Inherits from:
- **Board game / Monopoly clone:** core mechanic
- **Strategy:** property acquisition, upgrade decisions
- **Casual / party:** short sessions, social play

### 5.4 Target Audience

- **Primary:** UM students, alumni, prospective students (ages 17â€“30)
- **Secondary:** Malaysian gamers familiar with UM culture
- **Rating equivalent:** PG / E (suitable for everyone)

### 5.5 Platform

- Windows PC (standalone executable)
- No special equipment required

### 5.6 IP / Licensing

- UM landmarks, faculty names, campus references used in **parody / educational** context
- All character art original
- No copyrighted music â€” use royalty-free or original SFX
- Disclaimer slide in credits: "Fan project, not affiliated with University Malaya"

### 5.7 Competition Mode

- **2â€“4 player local hot-seat**, fully **competitive** (last player standing wins)
- No cooperative mode in v1

---

## 6. Game World

### 6.1 Board Tiles (40 total â€” standard Monopoly layout)

| Position | Tile Type | Name |
|----------|-----------|------|
| 0 | Corner | **Main Gate (GO)** â€” collect RM 200 |
| 1 | Property | Kolej Kediaman 1 (KK1) |
| 2 | Card | Akademik Card |
| 3 | Property | Kolej Kediaman 2 (KK2) |
| 4 | Tax | Yuran Pengajian â€” pay RM 200 |
| 5 | Station | LRT Universiti |
| 6 | Property | Faculty of Arts (FAS) |
| 7 | Card | Kampus Card |
| 8 | Property | Faculty of Languages (FBL) |
| 9 | Property | Faculty of Education (FOE) |
| 10 | Corner | **DTC (Just Visiting / Jail)** |
| 11 | Property | Faculty of Business (FBE) |
| 12 | Utility | KKR Water |
| 13 | Property | Faculty of Economics |
| 14 | Property | Faculty of Law |
| 15 | Station | KTM Universiti |
| 16 | Property | Faculty of Medicine |
| 17 | Card | Akademik Card |
| 18 | Property | Faculty of Dentistry |
| 19 | Property | Faculty of Pharmacy |
| 20 | Corner | **Sunken Garden (Free Parking)** |
| 21 | Property | Faculty of Science |
| 22 | Card | Kampus Card |
| 23 | Property | Faculty of Engineering |
| 24 | Property | FCSIT |
| 25 | Station | LRT Kerinchi |
| 26 | Property | Faculty of Built Environment |
| 27 | Property | Academy of Islamic Studies |
| 28 | Utility | Library Wi-Fi |
| 29 | Property | Academy of Malay Studies |
| 30 | Corner | **Go to DTC (Jail)** |
| 31 | Property | Sports Centre |
| 32 | Property | Cultural Centre |
| 33 | Card | Akademik Card |
| 34 | Property | Rimba Ilmu |
| 35 | Station | KTM KL Sentral |
| 36 | Card | Kampus Card |
| 37 | Property | Chancellery |
| 38 | Tax | Saman Parking â€” pay RM 100 |
| 39 | Property | Vice-Chancellor's Office |

### 6.2 Card Examples

**Akademik Cards (academic events):**
- "Scored Dean's List â€” collect RM 200"
- "Failed final exam â€” pay RM 150"
- "Scholarship awarded â€” collect RM 300"
- "Plagiarism case â€” go to DTC"

**Kampus Cards (campus life events):**
- "Free makan at KKR â€” collect RM 50"
- "Kena saman parking â€” pay RM 100"
- "Won UM Idol â€” collect RM 250"
- "Kantin food poisoning â€” pay RM 75 medical"

### 6.3 Implemented Data Baseline

The MVP data baseline has been authored as Unity assets:

- 40 board tile assets in `Assets/Data/Tiles/`
- 8 Akademik card assets in `Assets/Data/Cards/Akademik/`
- 8 Kampus card assets in `Assets/Data/Cards/Kampus/`
- `Assets/Data/MainGameConfig.asset` with all 40 tiles and both decks assigned

Property rent tables use five values: base rent plus upgrade levels 1-4.

The first playable balance pass keeps purchase prices stable, reduces late-upgrade rent spikes, lowers `Yuran Pengajian` from RM 200 to RM 150, and softens several large card rewards/penalties so the prototype is less likely to end from one unlucky event.

Additional first playable dev behavior:

- Save and load controls are available from the generated HUD.
- Clicking generated board tile info buttons opens tile details.
- Buy actions are limited to the tile where the current player is standing.
- Property upgrades can be triggered from the property popup when the current player owns the full color set and can afford the upgrade.
- Station and utility ownership is tracked for UI display and save/load.

---
## 7. Constraints

- **Time:** 14 weeks total, ~10 weeks for development
- **Team:** 5 members, varied skill levels (assumed)
- **Budget:** Free tools only (Unity Personal / Godot, Aseprite trial / Krita / GIMP, Audacity)
- **Hardware:** Personal laptops, no specialized equipment

---

## 8. Acceptance Criteria (for Week 14 demo)

The prototype is considered complete if:

1. 2â€“4 players can play a full game from start to finish
2. All 40 tiles function correctly
3. Buy/rent/upgrade mechanics work
4. Both card decks function with at least 8 cards each
5. Win condition correctly detects bankruptcy
6. No game-breaking crashes during a 30-min playtest
7. UI clearly shows whose turn it is, money, and properties
8. Game has UM-themed visual identity (board art, faculty names, landmarks)

---

## 9. Out of Scope (v1)

- Online multiplayer
- Mobile port
- Procedurally generated boards
- Voice acting
- Localization beyond English/Bahasa Malaysia mix (kept as flavor)

