# UM Monopoly — Setup Guide

This folder contains a complete C# script library targeting **Unity (2D, 2022 LTS or newer)**. Unity needs to generate its own project files (Library/, ProjectSettings/, scenes, prefabs, .meta files) when you open it the first time. Follow these steps to bootstrap.

---

## 1. Install Unity

1. Install **Unity Hub** from <https://unity.com/download>
2. From Unity Hub → Installs, install **Unity 2022.3 LTS** (any newer LTS works too)
3. Recommended modules: WebGL Build Support (optional), Documentation

---

## 2. Create the Unity Project

1. Unity Hub → **New project**
2. Template: **2D (Built-In Render Pipeline)**
3. Project name: `UMMonopoly`
4. Location: **this directory** (`c:\Users\kianx\OneDrive\Documents\Game Deve`)
5. Click **Create project**

Unity will generate `Library/`, `ProjectSettings/`, `Packages/` and an empty `Assets/`.

The `Assets/Scripts/` folder we shipped will merge with Unity's `Assets/`. Unity will then generate `.meta` files for every script — commit those too.

---

## 3. Install Required Packages

In Unity, open **Window → Package Manager** and install:

- **TextMeshPro** (Window → TextMeshPro → Import TMP Essential Resources after install)
- **Input System** (optional but recommended)
- **2D Sprite** (already in 2D template)

---

## 4. Create Scenes

### Fast path: generated starter setup

After Unity imports the project and TextMeshPro essentials, use the editor helper:

1. In the Unity top menu, click **UM Monopoly -> Build Starter Project**
2. Confirm the dialog
3. Unity will create starter scenes, placeholder prefabs, 40 board anchors, basic UI wiring, and Build Settings entries
4. Open `Assets/Scenes/MainMenu.unity`
5. Press Play

You can also run **UM Monopoly -> Validate Data Assets** to check that `MainGameConfig` has 40 ordered tiles and both card decks.

### Manual fallback

In `Assets/Scenes/`, create three scenes:

| Scene | Purpose |
|-------|---------|
| `MainMenu.unity` | New game button, player name inputs |
| `GameBoard.unity` | The board, HUD, popups |
| `EndGame.unity` | (Optional — `EndGameController` panel can also live in GameBoard) |

Add all three to **File → Build Settings → Scenes In Build**.

---

## 5. Author Tile Data (40 tiles)

For every tile in `REQUIREMENTS.md §6.1`, do:

1. Right-click in `Assets/Data/Tiles/` → **Create → UMMonopoly → Tile Data**
2. Name it after the tile (e.g. `Tile00_MainGate`, `Tile01_KK1`, …)
3. Fill in the Inspector fields:
   - `position`: 0–39
   - `tileName`: display name
   - `type`: from the dropdown
   - `purchasePrice`, `rentTable[]`, `colorGroup`, `upgradeCost` for properties/stations
   - `taxAmount` for tax tiles
   - `deckType` for card tiles

**Suggested starting prices** (mirror Monopoly defaults, in RM):
- Brown set: 60, 60 (rent 2/10/30/90/160/250)
- Light Blue: 100, 100, 120
- Pink: 140, 140, 160
- Orange: 180, 180, 200
- Red: 220, 220, 240
- Yellow: 260, 260, 280
- Green: 300, 300, 320
- Blue: 350, 400
- Stations: 200 each (rent 25/50/100/200)
- Utilities: 150 each
- Upgrade cost = same as base purchase / 2 (e.g. brown: 50)

Tweak during balance pass (Week 11).

---

## 6. Author Card Data (≥ 8 each deck)

In `Assets/Data/Cards/Akademik/` and `Assets/Data/Cards/Kampus/`:

1. Right-click → **Create → UMMonopoly → Card**
2. Pick `deck`, write `description`, choose `effect`, enter `amount`

Sample starter cards (already listed in REQUIREMENTS §6.2). Examples:

| Description | Effect | Amount |
|-------------|--------|--------|
| Scored Dean's List — collect RM 200 | CollectMoney | 200 |
| Failed final exam — pay RM 150 | PayMoney | 150 |
| Plagiarism case — go to DTC | GoToJail | 0 |
| Free makan at KKR — collect RM 50 | CollectMoney | 50 |
| Get-Out-Of-DTC card | GetOutOfJailFree | 0 |
| Move to Main Gate | MoveToTile | 0 |
| Move to LRT Universiti | MoveToTile | 5 |

---

## 7. Create the GameConfig

1. Right-click in `Assets/Data/` → **Create → UMMonopoly → Game Config**
2. Name it `MainGameConfig`
3. Drag all 40 `TileDataSO` assets into the `tiles` list (must be in order, position 0..39)
4. Drag Akademik cards into `akademikDeck`, Kampus cards into `kampusDeck`
5. Adjust `startingMoney`, `salaryOnGo`, etc. as desired

---

## 8. Wire Up GameBoard Scene

In `GameBoard.unity`:

1. Create empty GameObject **GameManager** → add `GameManager` script → drag `MainGameConfig` into the `config` slot
2. Create empty GameObject **TurnController** → add `TurnController` script
3. Create empty GameObject **GameBootstrap** → add `GameBootstrap` script (auto-wires the rest)
4. Build the **Canvas** (UI Canvas, Render Mode: Screen Space - Overlay):
   - Add `HUDController` to a child of Canvas → wire turn label, dice label, Roll/EndTurn/Buy buttons
   - Roll button OnClick → `TurnController.OnRollButtonPressed`
   - End Turn button OnClick → `TurnController.OnEndTurnPressed`
   - Buy button OnClick → `HUDController.OnBuyPressed`
   - Add `CardPopup` panel
   - Add `PropertyPopup` panel
   - Add `EndGameController` panel
   - Add `DiceUI` with two TMP texts for dice faces
5. **PlayerCard prefab**: card with name, money, property count, color tag, bankrupt overlay → bind `PlayerCardUI`. Drop into HUD's `playerPanelRoot`.
6. **PlayerToken prefab**: a sprite (any placeholder circle) → wire as `playerTokenPrefab` on `BoardView`
7. **Board GameObject**: add `BoardView` script. Place 40 empty Transforms around the screen edge representing each tile position; drag them into `tileAnchors[0..39]` in order.

---

## 9. Wire Up Main Menu Scene

In `MainMenu.unity`:

1. Create Canvas with 4 TMP InputFields for player names + a Start button + a Quit button
2. Add `MainMenuController` to a manager GameObject; wire inputs and button callbacks
3. Set `gameSceneName` to `GameBoard`

---

## 10. Run It

Press Play with `MainMenu` scene open. Enter names → Start → roll dice → play.

---

## 11. Project Layout (after Unity creates its files)

```
Game Deve/
├── Assets/
│   ├── Scripts/        ← already shipped
│   ├── Scenes/         ← you create
│   ├── Data/
│   │   ├── Tiles/      ← you create (40 SOs)
│   │   ├── Cards/      ← you create (16+ SOs)
│   │   └── MainGameConfig.asset
│   ├── Prefabs/        ← you create (PlayerToken, PlayerCard, etc.)
│   ├── Art/            ← you create
│   └── Audio/          ← you create
├── Library/            ← Unity-generated, gitignored
├── ProjectSettings/    ← Unity-generated, commit this
├── Packages/           ← Unity-generated, commit this
├── REQUIREMENTS.md
├── ARCHITECTURE.md
├── STATUS.md
├── SETUP.md            ← you are here
└── .gitignore
```

---

## 12. Smoke Test Checklist (after step 10)

- [ ] Main menu loads
- [ ] Start moves you to GameBoard
- [ ] Player cards appear with starting money
- [ ] Roll button rolls dice, dice faces update
- [ ] Token moves to correct tile
- [ ] Landing on unowned property enables Buy button
- [ ] Buying deducts money + adds property to player
- [ ] Landing on owned property pays rent
- [ ] Card tiles show a popup
- [ ] Tax tiles deduct money
- [ ] Go-to-jail tile teleports to position 10
- [ ] End Turn moves to next player
- [ ] Bankrupt player gets greyed out
- [ ] Last solvent player triggers EndGame screen

If any check fails, tag the relevant script and we debug.

---

## Engine Pivot Note

If your team chooses **Godot** instead, the architecture and class structure carry over directly — port the C# files to GDScript (or use Godot's C# support). Unity-specific bits to swap:

- `MonoBehaviour` → `Node`
- `ScriptableObject` → Godot `Resource`
- `JsonUtility` → `JSON.parse` / `Marshalls`
- TMP → built-in `Label`
- `SceneManager.LoadScene` → `get_tree().change_scene_to_file()`
