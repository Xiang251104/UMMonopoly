# UM Monopoly Project Status

**Last Updated:** 2026-05-14
**Current Week:** Pre-Week 1 / Unity import readiness
**Overall Status:** Core data and gameplay scaffolding are ready. Unity UI/TMP package references have been restored and the generated starter scenes now include cameras, visible board tiles, readable labels, and cleaner HUD placement. The next milestone is Play Mode smoke-testing.

---

## 1. Quick Snapshot

| Item | Status |
|------|--------|
| Concept locked | Done - UM Monopoly digital board game |
| Engine chosen | Done - Unity 2022.3 LTS target |
| Required maintenance docs | Done - requirements, architecture/design, status |
| Latest checklist | Done - see `PROJECT_CHECKLIST.md` |
| `.gitignore` | Done - Unity preset |
| Code skeleton and gameplay scripts | Done - 40 C# files including editor builder |
| Unity data assets | Done - 40 tiles, 16 cards, main config |
| Unity starter builder | Done - menu command in `Assets/Editor/` |
| Save/load hooks | Done - HUD buttons and runtime restore logic |
| Tile click popups | Done - tile info button support added |
| Balance pass | Done - first pass only |
| Unity project import | In progress - package compile and starter scene generation fixed locally |
| Generated scenes and prefabs | Done - regenerated through the headless Unity builder |
| Real board and character art | Not started |
| Audio | Not started |
| Full Unity smoke test | Pending |
| Final report | Not started |

---

## 2. Current Checklist

The active checklist is now maintained in `PROJECT_CHECKLIST.md`.

Immediate next steps:

- [ ] Open the folder in Unity 2022.3 LTS.
- [ ] Wait for Unity import to finish.
- [x] Fix current Unity Console compile errors for missing UI/TMP types.
- [ ] Run `UM Monopoly > Validate Data Assets`.
- [ ] Run `UM Monopoly > Build Starter Project`.
- [ ] Open `Assets/Scenes/MainMenu.unity`.
- [ ] Press Play and run the smoke test in `PROJECT_CHECKLIST.md`.

---

## 3. Completed Development Work

| Date | Item |
|------|------|
| 2026-05-04 | Drafted original requirements, architecture, setup, and status docs. |
| 2026-05-04 | Added Unity-oriented C# code skeleton for data, entities, systems, core flow, and UI. |
| 2026-05-11 | Created the required project-maintenance docs. |
| 2026-05-11 | Created 40 `TileDataSO` assets, 16 `CardDataSO` assets, and `MainGameConfig.asset`. |
| 2026-05-11 | Added Unity editor bootstrapper for starter scenes, placeholder prefabs, UI wiring, and Build Settings. |
| 2026-05-11 | Applied first playable balance pass to rent tables, Yuran Pengajian, and card amounts. |
| 2026-05-12 | Added save/load HUD hooks, tile info buttons, safer buy/upgrade popup behavior, station/utility ownership tracking, and turn-flow fixes. |
| 2026-05-13 | Added the latest project checklist and refreshed this status file. |
| 2026-05-14 | Added the `com.unity.ugui` package dependency so `UnityEngine.UI` and `TMPro` scripts compile in Unity. |
| 2026-05-14 | Fixed and regenerated the starter scenes so GameBoard has an active camera, visible labelled board tiles, a board backdrop, and a cleaner HUD/status layout. |

---

## 4. Verification Status

| Check | Status | Notes |
|------|--------|-------|
| Data asset count | Passed locally | 40 tile assets, 8 Akademik cards, 8 Kampus cards. |
| Main config references | Passed locally | Tiles and decks are referenced in `MainGameConfig.asset`. |
| Tile ordering | Passed locally | Tile positions match board list order. |
| Rent table shape | Passed locally | Properties use 5 rent values; stations use 4. |
| Compile-style check | Passed locally | Runtime and editor assemblies compile with no compiler output after the camera/UI builder fix. |
| Logic test harness | Passed locally | Covered movement wrapping, ownership, save/load, debt, and card effects. |
| Unity Editor import | Passed locally | Headless Unity rebuilt the starter scenes through `BuildStarterProjectHeadless`. |
| GameBoard visual preview | Passed locally | Rendered `Logs/GameBoardPreview.png`; it shows a camera-rendered colored board with visible tile labels. |
| Play Mode smoke test | Pending | Requires generated scenes and Unity Play Mode. |

---

## 5. Backlog

### High Priority

- [ ] Open and import the project in Unity.
- [x] Fix current Unity Console compile errors for missing UI/TMP package references.
- [x] Regenerate starter scenes with an active camera and visible board UI.
- [ ] Run data validation from the Unity menu.
- [x] Run the starter project builder.
- [ ] Smoke-test the full gameplay loop.
- [x] Adjust first-pass generated GameBoard UI layout after seeing the broken scene.

### Medium Priority

- [ ] Replace placeholder board visuals with final board art.
- [ ] Add character avatars.
- [ ] Add tile icons or clearer visual labels.
- [ ] Add sound effects.
- [ ] Improve dice animation and movement feel.
- [ ] Continue gameplay balance after real playtests.

### Low Priority / Stretch

- [ ] AI opponents.
- [ ] Trading.
- [ ] Mortgage system.
- [ ] Auctions.
- [ ] Online multiplayer.
- [ ] Opening cinematic.

---

## 6. Risks And Blockers

| Item | Severity | Notes |
|------|----------|-------|
| Play Mode not fully smoke-tested | Medium | Compile and scene generation now pass; a human-visible Play Mode loop still needs verification. |
| Generated scene layout may need final art polish | Medium | The builder now creates a presentable starter, but final board art and icons are still future work. |
| Team roles not filled in docs | Medium | Needed for assignment management and report clarity. |
| Repository/project board links still missing | Medium | Add once GitHub/project management is set up. |
| Balance is not playtested | Medium | Current values are a first pass only. |

---

## 7. Decisions Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-04 | Theme: UM Monopoly digital board game. | Fits the University Malaya game brief and is achievable in one semester. |
| 2026-05-04 | Scope: 2-4 player local hot-seat. | Keeps the prototype manageable. |
| 2026-05-04 | Out of scope for MVP: trading, auctions, mortgage, AI, online multiplayer. | Reduces implementation risk. |
| 2026-05-04 | Default engine: Unity 2022.3 LTS with C#. | Matches the existing architecture and likely tutorial/support availability. |
| 2026-05-04 | Cards use enum-driven effects. | Easier to author and balance through ScriptableObjects. |
| 2026-05-11 | Use ScriptableObject assets for all MVP tile and card content. | Keeps content editable in Unity Inspector. |
| 2026-05-11 | Add editor automation for project bootstrap. | Reduces manual scene/prefab setup work. |
| 2026-05-14 | Keep `com.unity.ugui` in the Unity package manifest. | Unity UI controls, EventSystems, and TextMeshPro types are supplied by this package in the current Unity import. |

---

## 8. Useful Links

- Latest checklist: `PROJECT_CHECKLIST.md`
- Requirements: `PROJECT_REQUIREMENTS.md`
- Architecture and coding design: `ARCHITECTURE_AND_CODING_DESIGN.md`
- Setup guide: `SETUP.md`
- Repository: TBD
- Project board: TBD
- Shared drive: TBD
