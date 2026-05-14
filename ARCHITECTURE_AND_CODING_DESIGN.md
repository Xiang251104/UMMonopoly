# UM Monopoly â€” Architecture & Coding Design

**Last Updated:** 2026-05-14
**Status:** Draft (pending team agreement on engine choice)

---

## 1. Technology Stack

### 1.1 Recommended Stack

| Layer | Tool | Reason |
|-------|------|--------|
| **Game Engine** | **Unity 2022 LTS (2D)** | Strong 2D tooling, large tutorial base, free Personal license, C# is taught in most CS programs |
| **Language** | C# | Default for Unity |
| **Art** | Krita / GIMP / Aseprite | Free 2D art tools |
| **Audio** | Audacity + freesound.org | Free editing + royalty-free SFX |
| **Version Control** | Git + GitHub | Standard, free for students |
| **Project Mgmt** | GitHub Issues / Trello | Lightweight task tracking |
| **Documentation** | Markdown in repo | Lives with code, easy to version |

Runtime UI scripts use Unity's GameObject-based UI stack (`UnityEngine.UI`) and TextMeshPro (`TMPro`). In the current Unity import this is provided by `com.unity.ugui` `2.0.0`, which must remain in `Packages/manifest.json` so HUD buttons, popups, labels, and tile click targets compile.

**Alternative:** Godot 4 (GDScript) â€” lighter, fully open-source. Pick if team has more Godot experience.

### 1.2 Decision Pending

> **TODO:** Team must agree on Unity vs Godot by **Week 2**. Document final choice here and update this file.

---

## 2. High-Level Architecture

```
+-----------------------------------------------------------+
|                    PRESENTATION LAYER                      |
|  (Scene rendering, UI, animations, input handling)        |
|                                                            |
|  - MainMenuScene    - GameBoardScene    - EndGameScene    |
|  - HUD (money, turn) - Card popups      - Settings panel  |
+-----------------------------------------------------------+
                            ^
                            | (events, state queries)
                            v
+-----------------------------------------------------------+
|                    GAME LOGIC LAYER                        |
|                                                            |
|  GameManager  -  TurnController  -  DiceRoller            |
|  Player       -  Bank            -  CardDeck              |
|  Board        -  Tile (abstract) -  EventResolver         |
+-----------------------------------------------------------+
                            ^
                            | (read/write)
                            v
+-----------------------------------------------------------+
|                    DATA LAYER                              |
|                                                            |
|  ScriptableObjects: TileData, CardData, PlayerProfile     |
|  Save system: JSON file in Application.persistentDataPath |
+-----------------------------------------------------------+
```

---

## 3. Core Class Design

### 3.1 GameManager (Singleton)

Top-level coordinator. Owns the list of players, the board, and the current game state.

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Player> Players;
    public Board Board;
    public GameState CurrentState;
    public int CurrentPlayerIndex;

    public void StartGame(int playerCount);
    public void NextTurn();
    public void EndGame(Player winner);
    public void CheckBankruptcy(Player p);
}
```

### 3.2 GameState (Enum)

```
MainMenu, Setup, RollPhase, MovePhase, ResolveTilePhase,
DecisionPhase, EndTurnPhase, GameOver
```

A finite state machine drives the turn lifecycle. Only one state active at a time.

### 3.3 Player

```csharp
public class Player
{
    public string Name;
    public int PlayerId;
    public int Money;
    public int BoardPosition;
    public List<Property> OwnedProperties;
    public bool InJail;
    public int JailTurnsRemaining;
    public bool IsBankrupt;

    public void Move(int steps);
    public bool TryPay(int amount);
    public void Receive(int amount);
}
```

### 3.4 Board

```csharp
public class Board
{
    public List<Tile> Tiles;     // 40 tiles in order
    public Tile GetTile(int position);
}
```

### 3.5 Tile (Abstract Base)

```csharp
public abstract class Tile
{
    public int Position;
    public string Name;
    public abstract void OnPlayerLanded(Player p);
}

// Subclasses:
public class PropertyTile : Tile { ... }   // Faculties
public class CardTile : Tile { ... }       // Akademik / Kampus
public class TaxTile : Tile { ... }        // Yuran / Saman
public class StationTile : Tile { ... }    // LRT / KTM
public class UtilityTile : Tile { ... }    // KKR / Wi-Fi
public class JailTile : Tile { ... }
public class GoTile : Tile { ... }
public class FreeParkingTile : Tile { ... }
public class GoToJailTile : Tile { ... }
```

### 3.6 PropertyTile

```csharp
public class PropertyTile : Tile
{
    public int PurchasePrice;
    public int[] RentTable;       // [base, 1 house, 2 houses, ..., postgrad]
    public ColorGroup Group;      // for monopoly bonus
    public Player Owner;
    public int UpgradeLevel;      // 0 = base, 4 = postgrad centre

    public override void OnPlayerLanded(Player p) { ... }
    public bool CanUpgrade();
    public void Upgrade();
}
```

### 3.7 CardDeck

```csharp
public class CardDeck
{
    public DeckType Type;          // Akademik or Kampus
    public Queue<Card> Cards;
    public Card Draw();
    public void Reshuffle();
}

public class Card
{
    public string Description;
    public CardEffect Effect;      // strategy pattern
}
```

### 3.8 Bank

Holds ungranted money pool, handles property transactions, validates affordability.

```csharp
public class Bank
{
    public bool BuyProperty(Player buyer, PropertyTile prop);
    public void PayRent(Player from, Player to, int amount);
    public void CollectTax(Player from, int amount);
    public void Award(Player to, int amount);
}
```

---

## 4. Turn Flow (State Machine)

```
[Start of Turn]
      |
      v
  RollPhase  --(roll dice)-->  MovePhase
                                    |
                                    v
                          ResolveTilePhase
                          (run Tile.OnPlayerLanded)
                                    |
                                    v
                          DecisionPhase
                          (buy? upgrade? pay?)
                                    |
                                    v
                          EndTurnPhase
                          (check bankruptcy, advance index)
                                    |
                                    v
                          [Next player or GameOver]
```

Doubles â†’ re-roll (max 3 in a row â†’ jail).

---

## 5. Data Storage

### 5.1 ScriptableObjects (static config)

- `TileDataSO`: position, name, type, prices, color group
- `CardDataSO`: text, effect type, parameters
- `GameConfigSO`: starting money, salary on GO, jail fine

These are designer-editable in Unity Inspector â€” no code changes needed to balance.
Current content baseline:
- 40 tile assets live in `Assets/Data/Tiles/` and are ordered by board position 0-39.
- 8 Akademik card assets live in `Assets/Data/Cards/Akademik/`.
- 8 Kampus card assets live in `Assets/Data/Cards/Kampus/`.
- `Assets/Data/MainGameConfig.asset` references all tile and card assets.
- Property rent tables use five values: base rent plus upgrade levels 1-4.
- First playable tuning is data-only: purchase prices remain stable, top-end rent/card swings are softened, and no runtime rule code changes are required.
- Runtime save/load is handled by `GameManager` using `SaveSystem` snapshots, including property, station, and utility ownership.
- Tile detail popups are triggered through generated `TileInfoButton` UI targets and `PropertyPopup.Show(Tile)`.
- Bankruptcy is triggered when rent/tax cannot be fully paid, because there is no mortgage/liquidation flow in the MVP.

Editor bootstrap support:
- `Assets/Editor/UMMonopolyProjectBuilder.cs` provides **UM Monopoly -> Build Starter Project**.
- The builder creates starter scenes, scene cameras, placeholder prefabs, 40 visible board tiles with labels, player/token anchors, UI wiring, and Build Settings entries.
- Runtime scripts remain unchanged; the editor script only scaffolds Unity assets and scenes inside the Editor.

### 5.2 Runtime Save Format (JSON)

```json
{
  "version": "1.0",
  "currentPlayerIndex": 0,
  "turnNumber": 12,
  "players": [
    { "id": 0, "name": "Ali", "money": 1200, "position": 14, "properties": [6, 8, 9], "inJail": false }
  ],
  "propertyOwnership": { "6": 0, "8": 0, "9": 0 }
}
```

Saved to `Application.persistentDataPath/savegame.json`.

---

## 6. Folder Structure (Unity)

```
Assets/
â”œâ”€â”€ Art/
â”‚   â”œâ”€â”€ Board/
â”‚   â”œâ”€â”€ Cards/
â”‚   â”œâ”€â”€ Characters/
â”‚   â””â”€â”€ UI/
â”œâ”€â”€ Audio/
â”‚   â”œâ”€â”€ BGM/
â”‚   â””â”€â”€ SFX/
â”œâ”€â”€ Prefabs/
â”‚   â”œâ”€â”€ PlayerToken.prefab
â”‚   â”œâ”€â”€ DicePrefab.prefab
â”‚   â””â”€â”€ TilePrefab.prefab
â”œâ”€â”€ Scenes/
â”‚   â”œâ”€â”€ MainMenu.unity
â”‚   â”œâ”€â”€ GameBoard.unity
â”‚   â””â”€â”€ EndGame.unity
â”œâ”€â”€ Scripts/
â”‚   â”œâ”€â”€ Core/          (GameManager, TurnController, GameState)
â”‚   â”œâ”€â”€ Entities/      (Player, Tile + subclasses, Card)
â”‚   â”œâ”€â”€ Systems/       (Bank, CardDeck, DiceRoller, SaveSystem)
â”‚   â”œâ”€â”€ UI/            (HUD, popups, menus)
â”‚   â””â”€â”€ Data/          (ScriptableObjects)
â”œâ”€â”€ Data/
â”‚   â”œâ”€â”€ Tiles/         (40 TileData assets)
â”‚   â””â”€â”€ Cards/         (Card data assets)
â””â”€â”€ Resources/
```

---

## 7. Coding Conventions

| Topic | Rule |
|-------|------|
| **Naming** | PascalCase for classes/methods/public fields; camelCase for locals/privates |
| **File names** | One public class per file, filename = class name |
| **Comments** | Only when WHY is non-obvious. Use XML doc comments on public APIs |
| **Magic numbers** | Move to ScriptableObjects or `const` |
| **Singletons** | Only `GameManager`. Avoid otherwise |
| **MonoBehaviour vs plain class** | Use plain C# classes for logic-only entities (Player, Card). MonoBehaviour only when Unity lifecycle is needed |
| **Events** | Use C# `event Action<T>` for cross-system signals (e.g. `OnTurnEnded`, `OnPlayerBankrupt`) |
| **Async** | Use coroutines for sequential animations; avoid `async/await` unless necessary |

---

## 8. Git Workflow

- **Main branch:** `main` (always playable)
- **Working branch:** `dev`
- **Feature branches:** `feature/<name>`, e.g. `feature/dice-roller`
- **Commit style:** `<area>: <short summary>` e.g. `bank: handle rent on mortgaged property`
- **PRs:** Self-review before merge. At least 1 teammate review for changes to `Core/`.
- **Unity-specific:** include `.gitignore` for Unity (Library/, Temp/, Logs/), use Unity YAML merge tool for .unity/.prefab merge conflicts.

---

## 9. Testing Strategy

| Type | Scope | Owner |
|------|-------|-------|
| **Unit tests** | Bank, Player money math, dice fairness | Programmer (Lead) |
| **Integration tests** | Full turn flow via state machine | Programmer (Lead) |
| **Playtesting** | Full games, balance, fun factor | Whole team weekly from Week 8 |

Unity Test Framework for unit tests in `Assets/Tests/`.

---

## 10. Risk & Mitigation

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Engine choice debate stalls start | Med | High | Hard deadline Week 2 to decide |
| Scope creep (auctions, online MP) | High | High | Stick to MVP list in REQUIREMENTS.md |
| Art bottleneck | Med | Med | Use placeholder art until Week 6; finalize art Week 10â€“11 |
| Git merge conflicts in scenes | High | Med | Single-owner per scene; use prefabs heavily |
| Team member dropout / illness | Low | High | Pair-programming on critical systems; cross-train |
| Balance issues (game too long/short) | High | Low | Weekly playtests from Week 8, easy via ScriptableObjects |

---

## 11. Open Architectural Questions

1. **AI opponent?** â€” defer decision until Week 8 based on velocity
2. **Animation framework?** â€” Unity Animator vs DOTween (recommend DOTween for code-driven tweens)
3. **Localization?** â€” out of scope unless trivial; English with BM flavor words inline

