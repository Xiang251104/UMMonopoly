using System.Collections.Generic;
using System.IO;
using TMPro;
using UMMonopoly.Core;
using UMMonopoly.Data;
using UMMonopoly.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UMMonopoly.EditorTools
{
    /// <summary>
    /// Creates a usable starter Unity setup from the script/data skeleton.
    /// Run from Unity via UM Monopoly > Build Starter Project.
    /// </summary>
    public static class UMMonopolyProjectBuilder
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameBoardScenePath = "Assets/Scenes/GameBoard.unity";
        private const string EndGameScenePath = "Assets/Scenes/EndGame.unity";
        private const string MainGameConfigPath = "Assets/Data/MainGameConfig.asset";
        private const string TokenSpritePath = "Assets/Art/Generated/TokenCircle.png";
        private const string PlayerTokenPrefabPath = "Assets/Prefabs/PlayerToken.prefab";
        private const string PlayerCardPrefabPath = "Assets/Prefabs/PlayerCard.prefab";

        [MenuItem("UM Monopoly/Build Starter Project")]
        public static void BuildStarterProject()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build UM Monopoly Starter Project",
                    "This will create or overwrite starter scenes and placeholder prefabs. Existing scripts and data assets are preserved.",
                    "Build Starter",
                    "Cancel"))
            {
                return;
            }

            BuildStarterProjectInternal();

            EditorUtility.DisplayDialog(
                "UM Monopoly Starter Project Built",
                "Created starter scenes, camera setup, visible board tiles, placeholder prefabs, UI wiring, and Build Settings entries.",
                "OK");
        }

        public static void BuildStarterProjectHeadless()
        {
            BuildStarterProjectInternal();
        }

        public static void CaptureGameBoardPreview()
        {
            EditorSceneManager.OpenScene(GameBoardScenePath, OpenSceneMode.Single);
            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new MissingReferenceException("GameBoard preview failed because the scene has no Camera.");
            }

            const int width = 1280;
            const int height = 720;
            var renderTexture = new RenderTexture(width, height, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;

            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();

            var preview = new Texture2D(width, height, TextureFormat.RGB24, false);
            preview.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            preview.Apply();

            File.WriteAllBytes("Logs/GameBoardPreview.png", preview.EncodeToPNG());

            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            Object.DestroyImmediate(preview);
            Object.DestroyImmediate(renderTexture);
        }

        private static void BuildStarterProjectInternal()
        {
            EnsureFolders();
            var tokenPrefab = CreatePlayerTokenPrefab();
            var playerCardPrefab = CreatePlayerCardPrefab();

            CreateMainMenuScene();
            CreateGameBoardScene(tokenPrefab, playerCardPrefab);
            CreateEndGameScene();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("UM Monopoly/Validate Data Assets")]
        public static void ValidateDataAssets()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(MainGameConfigPath);
            if (config == null)
            {
                EditorUtility.DisplayDialog("Validation Failed", $"Missing {MainGameConfigPath}.", "OK");
                return;
            }

            var errors = new List<string>();
            if (config.tiles == null || config.tiles.Count != 40)
            {
                errors.Add("MainGameConfig.tiles must contain exactly 40 entries.");
            }
            else
            {
                for (int i = 0; i < config.tiles.Count; i++)
                {
                    if (config.tiles[i] == null)
                    {
                        errors.Add($"Tile slot {i} is empty.");
                        continue;
                    }

                    if (config.tiles[i].position != i)
                    {
                        errors.Add($"Tile slot {i} contains position {config.tiles[i].position}.");
                    }
                }
            }

            if (config.akademikDeck == null || config.akademikDeck.Count < 8)
            {
                errors.Add("Akademik deck must contain at least 8 cards.");
            }

            if (config.kampusDeck == null || config.kampusDeck.Count < 8)
            {
                errors.Add("Kampus deck must contain at least 8 cards.");
            }

            EditorUtility.DisplayDialog(
                errors.Count == 0 ? "Validation Passed" : "Validation Failed",
                errors.Count == 0 ? "MainGameConfig has 40 ordered tiles and both 8-card decks." : string.Join("\n", errors),
                "OK");
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[]
            {
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Art",
                "Assets/Art/Generated",
                "Assets/Audio",
                "Assets/Audio/BGM",
                "Assets/Audio/SFX",
                "Assets/Resources"
            })
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreatePlayerTokenPrefab()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "PlayerToken";
            root.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            Object.DestroyImmediate(root.GetComponent<Collider>());
            var renderer = root.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(Color.white);

            var prefab = SavePrefab(root, PlayerTokenPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Sprite CreateTokenSprite()
        {
            if (!File.Exists(TokenSpritePath))
            {
                var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                var clear = new Color(0f, 0f, 0f, 0f);
                var fill = Color.white;
                var center = new Vector2(31.5f, 31.5f);
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        texture.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), center) <= 28f ? fill : clear);
                    }
                }
                texture.Apply();
                File.WriteAllBytes(TokenSpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(TokenSpritePath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(TokenSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64f;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(TokenSpritePath);
        }

        private static PlayerCardUI CreatePlayerCardPrefab()
        {
            var root = CreateUIObject("PlayerCard", null, new Vector2(260f, 92f));
            var background = root.AddComponent<Image>();
            background.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

            var card = root.AddComponent<PlayerCardUI>();
            var colorTag = CreateUIObject("ColorTag", root.transform, new Vector2(10f, 76f));
            Anchor(colorTag.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f));
            card.colorTag = colorTag.AddComponent<Image>();
            card.colorTag.color = Color.red;

            card.nameLabel = CreateText("NameLabel", root.transform, "Player", 18, TextAlignmentOptions.Left, new Vector2(190f, 24f));
            Anchor(card.nameLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -18f));

            card.moneyLabel = CreateText("MoneyLabel", root.transform, "RM 1500", 16, TextAlignmentOptions.Left, new Vector2(190f, 22f));
            Anchor(card.moneyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -44f));

            card.propertyCountLabel = CreateText("PropertyCountLabel", root.transform, "0 props", 14, TextAlignmentOptions.Left, new Vector2(190f, 20f));
            Anchor(card.propertyCountLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -68f));

            var overlay = CreateUIObject("BankruptOverlay", root.transform, new Vector2(260f, 92f));
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.62f);
            var overlayText = CreateText("BankruptLabel", overlay.transform, "BANKRUPT", 18, TextAlignmentOptions.Center, new Vector2(240f, 40f));
            overlayText.color = Color.white;
            card.bankruptOverlay = overlay;
            overlay.SetActive(false);

            var prefabRoot = SavePrefab(root, PlayerCardPrefabPath);
            Object.DestroyImmediate(root);
            return prefabRoot.GetComponent<PlayerCardUI>();
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            CreateSceneCamera("MainMenuCamera", new Color(0.03f, 0.05f, 0.08f, 1f), 5.4f);
            CreateEventSystem();
            var canvas = CreateCanvas("MainMenuCanvas");
            var panel = CreatePanel("MenuPanel", canvas.transform, new Vector2(620f, 520f), new Color(0.08f, 0.1f, 0.14f, 0.9f));

            var title = CreateText("Title", panel.transform, "UM Monopoly", 46, TextAlignmentOptions.Center, new Vector2(560f, 70f));
            title.color = Color.white;
            Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f));

            var controllerObject = new GameObject("MainMenuManager");
            var controller = controllerObject.AddComponent<MainMenuController>();
            controller.gameSceneName = "GameBoard";
            controller.playerNameInputs = new TMP_InputField[4];

            for (int i = 0; i < 4; i++)
            {
                var input = CreateInputField($"Player{i + 1}Input", panel.transform, $"Player {i + 1}", new Vector2(390f, 46f));
                Anchor(input.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -138f - i * 58f));
                controller.playerNameInputs[i] = input;
            }

            var startButton = CreateButton("StartButton", panel.transform, "Start Game", new Vector2(190f, 52f));
            Anchor(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-110f, 58f));
            UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartNewGame);

            var quitButton = CreateButton("QuitButton", panel.transform, "Quit", new Vector2(150f, 52f));
            Anchor(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(105f, 58f));
            UnityEventTools.AddPersistentListener(quitButton.onClick, controller.QuitGame);

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateGameBoardScene(GameObject tokenPrefab, PlayerCardUI playerCardPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GameBoard";

            CreateSceneCamera("GameCamera", new Color(0.025f, 0.035f, 0.045f, 1f), 5.85f);
            CreateEventSystem();

            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(MainGameConfigPath);
            var gameManagerObject = new GameObject("GameManager");
            var gameManager = gameManagerObject.AddComponent<GameManager>();
            gameManager.config = config;

            var turnControllerObject = new GameObject("TurnController");
            var turnController = turnControllerObject.AddComponent<TurnController>();

            var boardObject = new GameObject("Board");
            var boardView = boardObject.AddComponent<BoardView>();
            boardView.playerTokenPrefab = tokenPrefab;
            boardView.tileAnchors = CreateBoardAnchors(boardObject.transform, config);

            var canvas = CreateCanvas("GameBoardCanvas");
            var hudObject = new GameObject("HUD", typeof(RectTransform));
            hudObject.transform.SetParent(canvas.transform, false);
            Stretch(hudObject.GetComponent<RectTransform>());
            var hud = hudObject.AddComponent<HUDController>();

            var statusPanel = CreatePanel("StatusPanel", hudObject.transform, new Vector2(310f, 154f), new Color(0.04f, 0.06f, 0.09f, 0.88f));
            Anchor(statusPanel.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(175f, -94f));

            hud.turnLabel = CreateText("TurnLabel", statusPanel.transform, "Player 1's Turn", 22, TextAlignmentOptions.Left, new Vector2(260f, 34f));
            Anchor(hud.turnLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(152f, -28f));

            hud.diceLabel = CreateText("DiceLabel", statusPanel.transform, "Rolled: -", 20, TextAlignmentOptions.Left, new Vector2(260f, 30f));
            Anchor(hud.diceLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(152f, -62f));

            hud.rollButton = CreateButton("RollButton", hudObject.transform, "Roll", new Vector2(120f, 44f));
            Anchor(hud.rollButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-135f, 34f));
            UnityEventTools.AddPersistentListener(hud.rollButton.onClick, turnController.OnRollButtonPressed);

            hud.buyButton = CreateButton("BuyButton", hudObject.transform, "Buy", new Vector2(120f, 44f));
            Anchor(hud.buyButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f));
            UnityEventTools.AddPersistentListener(hud.buyButton.onClick, hud.OnBuyPressed);

            hud.endTurnButton = CreateButton("EndTurnButton", hudObject.transform, "End Turn", new Vector2(140f, 44f));
            Anchor(hud.endTurnButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 34f));
            UnityEventTools.AddPersistentListener(hud.endTurnButton.onClick, turnController.OnEndTurnPressed);

            hud.saveButton = CreateButton("SaveButton", statusPanel.transform, "Save", new Vector2(112f, 36f));
            Anchor(hud.saveButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(82f, 28f));
            UnityEventTools.AddPersistentListener(hud.saveButton.onClick, hud.OnSavePressed);

            hud.loadButton = CreateButton("LoadButton", statusPanel.transform, "Load", new Vector2(112f, 36f));
            Anchor(hud.loadButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(204f, 28f));
            UnityEventTools.AddPersistentListener(hud.loadButton.onClick, hud.OnLoadPressed);

            var playerPanel = CreateUIObject("PlayerPanelRoot", hudObject.transform, new Vector2(292f, 430f));
            Anchor(playerPanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -244f));
            var layout = playerPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            hud.playerPanelRoot = playerPanel.transform;
            hud.playerCardPrefab = playerCardPrefab;

            CreateDiceUI(statusPanel.transform);
            CreateCardPopup(hudObject.transform);
            var propertyPopup = CreatePropertyPopup(hudObject.transform);
            CreateTileInfoButtons(hudObject.transform, propertyPopup);
            CreateEndGamePanel(hudObject.transform);

            var bootstrapObject = new GameObject("GameBootstrap");
            var bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
            bootstrap.gameManager = gameManager;
            bootstrap.hud = hud;
            bootstrap.boardView = boardView;

            EditorSceneManager.SaveScene(scene, GameBoardScenePath);
        }

        private static void CreateEndGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "EndGame";
            CreateSceneCamera("EndGameCamera", new Color(0.03f, 0.05f, 0.08f, 1f), 5.4f);
            CreateEventSystem();
            var canvas = CreateCanvas("EndGameCanvas");
            var title = CreateText("EndGameTitle", canvas.transform, "Game Over", 54, TextAlignmentOptions.Center, new Vector2(640f, 90f));
            title.color = Color.white;
            Anchor(title.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero);
            EditorSceneManager.SaveScene(scene, EndGameScenePath);
        }

        private static Transform[] CreateBoardAnchors(Transform boardRoot, GameConfigSO config)
        {
            var anchors = new Transform[40];
            CreateBoardBackground(boardRoot);

            const float half = 4.5f;
            for (int i = 0; i < 40; i++)
            {
                var anchor = new GameObject($"TileAnchor_{i:00}").transform;
                anchor.SetParent(boardRoot, false);
                anchor.localPosition = BoardPosition(i, half);
                CreateTileVisual(anchor, GetTileData(config, i), i);
                anchors[i] = anchor;
            }
            return anchors;
        }

        private static TileDataSO GetTileData(GameConfigSO config, int index)
        {
            return config != null && config.tiles != null && index >= 0 && index < config.tiles.Count
                ? config.tiles[index]
                : null;
        }

        private static void CreateBoardBackground(Transform boardRoot)
        {
            var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "BoardBackdrop";
            background.transform.SetParent(boardRoot, false);
            background.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            background.transform.localScale = new Vector3(7.25f, 7.25f, 1f);
            Object.DestroyImmediate(background.GetComponent<Collider>());
            background.GetComponent<MeshRenderer>().sharedMaterial = CreateUnlitMaterial(new Color(0.075f, 0.095f, 0.11f, 1f));

            var title = new GameObject("BoardTitle");
            title.transform.SetParent(boardRoot, false);
            title.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var label = title.AddComponent<TextMeshPro>();
            label.text = "UM\nMONOPOLY";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 1.45f;
            label.color = new Color(0.88f, 0.93f, 0.95f, 0.92f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(4.5f, 2f);
            title.GetComponent<MeshRenderer>().sortingOrder = -1;
        }

        private static void CreateTileVisual(Transform anchor, TileDataSO tileData, int index)
        {
            var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tile.name = "TileVisual";
            tile.transform.SetParent(anchor, false);
            tile.transform.localPosition = Vector3.zero;
            tile.transform.localScale = IsCornerTile(index) ? new Vector3(0.96f, 0.96f, 1f) : new Vector3(0.92f, 0.8f, 1f);
            Object.DestroyImmediate(tile.GetComponent<Collider>());
            var tileColor = ResolveTileColor(tileData);
            tile.GetComponent<MeshRenderer>().sharedMaterial = CreateUnlitMaterial(tileColor);

            var labelObject = new GameObject("TileLabel");
            labelObject.transform.SetParent(anchor, false);
            labelObject.transform.localPosition = new Vector3(0f, -0.02f, -0.04f);
            var label = labelObject.AddComponent<TextMeshPro>();
            label.text = BuildTileLabel(tileData, index);
            label.fontSize = IsCornerTile(index) ? 0.56f : 0.44f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = ColorForText(tileColor);
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.rectTransform.sizeDelta = IsCornerTile(index) ? new Vector2(0.82f, 0.66f) : new Vector2(0.78f, 0.52f);
            labelObject.GetComponent<MeshRenderer>().sortingOrder = 2;
        }

        private static Material CreateUnlitMaterial(Color color)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            material.color = color;
            return material;
        }

        private static bool IsCornerTile(int index)
        {
            return index == 0 || index == 10 || index == 20 || index == 30;
        }

        private static string BuildTileLabel(TileDataSO tileData, int index)
        {
            if (tileData == null || string.IsNullOrWhiteSpace(tileData.tileName))
            {
                return index.ToString();
            }

            string name = tileData.tileName
                .Replace("Faculty of ", string.Empty)
                .Replace("Faculty ", string.Empty)
                .Replace("Academy of ", string.Empty)
                .Replace("Universiti", "Uni.")
                .Replace("Centre", "Ctr.");

            return name.Length <= 16 ? name : name.Substring(0, 16);
        }

        private static Color ResolveTileColor(TileDataSO tileData)
        {
            if (tileData == null)
            {
                return new Color(0.86f, 0.88f, 0.9f, 1f);
            }

            if (tileData.tileColor != Color.white)
            {
                return tileData.tileColor;
            }

            switch (tileData.type)
            {
                case TileType.Go:
                    return new Color(0.25f, 0.71f, 0.45f, 1f);
                case TileType.Property:
                    return ColorForGroup(tileData.colorGroup);
                case TileType.Station:
                    return new Color(0.34f, 0.42f, 0.52f, 1f);
                case TileType.Utility:
                    return new Color(0.21f, 0.62f, 0.68f, 1f);
                case TileType.Tax:
                    return new Color(0.85f, 0.64f, 0.24f, 1f);
                case TileType.Card:
                    return new Color(0.48f, 0.38f, 0.78f, 1f);
                case TileType.Jail:
                case TileType.GoToJail:
                    return new Color(0.74f, 0.24f, 0.22f, 1f);
                case TileType.FreeParking:
                    return new Color(0.26f, 0.58f, 0.8f, 1f);
                default:
                    return new Color(0.86f, 0.88f, 0.9f, 1f);
            }
        }

        private static Color ColorForGroup(ColorGroup group)
        {
            switch (group)
            {
                case ColorGroup.Brown: return new Color(0.55f, 0.32f, 0.18f, 1f);
                case ColorGroup.LightBlue: return new Color(0.5f, 0.78f, 0.95f, 1f);
                case ColorGroup.Pink: return new Color(0.86f, 0.32f, 0.62f, 1f);
                case ColorGroup.Orange: return new Color(0.9f, 0.5f, 0.2f, 1f);
                case ColorGroup.Red: return new Color(0.82f, 0.22f, 0.2f, 1f);
                case ColorGroup.Yellow: return new Color(0.92f, 0.78f, 0.22f, 1f);
                case ColorGroup.Green: return new Color(0.23f, 0.62f, 0.34f, 1f);
                case ColorGroup.Blue: return new Color(0.18f, 0.32f, 0.78f, 1f);
                default: return new Color(0.86f, 0.88f, 0.9f, 1f);
            }
        }

        private static Color ColorForText(Color background)
        {
            float luminance = 0.2126f * background.r + 0.7152f * background.g + 0.0722f * background.b;
            return luminance > 0.52f ? new Color(0.06f, 0.08f, 0.1f, 1f) : Color.white;
        }

        private static Vector3 BoardPosition(int index, float half)
        {
            int side = index / 10;
            int offset = index % 10;
            float t = offset / 9f;
            switch (side)
            {
                case 0: return new Vector3(Mathf.Lerp(half, -half, t), -half, 0f);
                case 1: return new Vector3(-half, Mathf.Lerp(-half, half, t), 0f);
                case 2: return new Vector3(Mathf.Lerp(-half, half, t), half, 0f);
                default: return new Vector3(half, Mathf.Lerp(half, -half, t), 0f);
            }
        }

        private static Vector2 TileButtonAnchorMin(int index) => TileButtonAnchor(index);

        private static Vector2 TileButtonAnchorMax(int index) => TileButtonAnchor(index);

        private static Vector2 TileButtonOffset(int index)
        {
            return Vector2.zero;
        }

        private static Vector2 TileButtonAnchor(int index)
        {
            int side = index / 10;
            int offset = index % 10;
            float t = offset / 9f;
            switch (side)
            {
                case 0: return new Vector2(Mathf.Lerp(0.72f, 0.28f, t), 0.16f);
                case 1: return new Vector2(0.24f, Mathf.Lerp(0.18f, 0.82f, t));
                case 2: return new Vector2(Mathf.Lerp(0.28f, 0.72f, t), 0.84f);
                default: return new Vector2(0.76f, Mathf.Lerp(0.82f, 0.18f, t));
            }
        }

        private static void CreateDiceUI(Transform parent)
        {
            var root = CreatePanel("DiceUI", parent, new Vector2(120f, 50f), new Color(0.11f, 0.14f, 0.2f, 0.86f));
            Anchor(root.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(82f, 76f));
            var diceUI = root.AddComponent<DiceUI>();
            diceUI.die1Label = CreateText("Die1Label", root.transform, "-", 25, TextAlignmentOptions.Center, new Vector2(44f, 38f));
            Anchor(diceUI.die1Label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 0f));
            diceUI.die2Label = CreateText("Die2Label", root.transform, "-", 25, TextAlignmentOptions.Center, new Vector2(44f, 38f));
            Anchor(diceUI.die2Label.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-32f, 0f));
        }

        private static void CreateCardPopup(Transform parent)
        {
            var host = CreateUIObject("CardPopup", parent, Vector2.zero);
            Stretch(host.GetComponent<RectTransform>());
            var root = CreatePanel("Panel", host.transform, new Vector2(520f, 260f), new Color(0.05f, 0.07f, 0.1f, 0.95f));
            Anchor(root.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var popup = host.AddComponent<CardPopup>();
            popup.panelRoot = root;
            popup.deckLabel = CreateText("DeckLabel", root.transform, "Card", 28, TextAlignmentOptions.Center, new Vector2(460f, 44f));
            Anchor(popup.deckLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f));
            popup.descriptionLabel = CreateText("DescriptionLabel", root.transform, "Card description", 21, TextAlignmentOptions.Center, new Vector2(440f, 96f));
            Anchor(popup.descriptionLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f));
            popup.closeButton = CreateButton("CloseButton", root.transform, "Close", new Vector2(120f, 42f));
            Anchor(popup.closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f));
            root.SetActive(false);
        }

        private static PropertyPopup CreatePropertyPopup(Transform parent)
        {
            var host = CreateUIObject("PropertyPopup", parent, Vector2.zero);
            Stretch(host.GetComponent<RectTransform>());
            var root = CreatePanel("Panel", host.transform, new Vector2(560f, 360f), new Color(0.05f, 0.07f, 0.1f, 0.96f));
            Anchor(root.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var popup = host.AddComponent<PropertyPopup>();
            popup.panelRoot = root;
            popup.nameLabel = CreateText("NameLabel", root.transform, "Property", 26, TextAlignmentOptions.Center, new Vector2(500f, 54f));
            Anchor(popup.nameLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f));
            popup.priceLabel = CreateText("PriceLabel", root.transform, "Price", 19, TextAlignmentOptions.Center, new Vector2(450f, 34f));
            Anchor(popup.priceLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f));
            popup.rentLabel = CreateText("RentLabel", root.transform, "Rent", 19, TextAlignmentOptions.Center, new Vector2(450f, 34f));
            Anchor(popup.rentLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -154f));
            popup.ownerLabel = CreateText("OwnerLabel", root.transform, "Owner", 19, TextAlignmentOptions.Center, new Vector2(450f, 34f));
            Anchor(popup.ownerLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -196f));
            popup.buyButton = CreateButton("BuyButton", root.transform, "Buy", new Vector2(118f, 42f));
            Anchor(popup.buyButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-128f, 44f));
            popup.upgradeButton = CreateButton("UpgradeButton", root.transform, "Upgrade", new Vector2(130f, 42f));
            Anchor(popup.upgradeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(4f, 44f));
            popup.closeButton = CreateButton("CloseButton", root.transform, "Close", new Vector2(118f, 42f));
            Anchor(popup.closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(136f, 44f));
            root.SetActive(false);
            return popup;
        }

        private static void CreateEndGamePanel(Transform parent)
        {
            var host = CreateUIObject("EndGame", parent, Vector2.zero);
            Stretch(host.GetComponent<RectTransform>());
            var root = CreatePanel("Panel", host.transform, new Vector2(520f, 260f), new Color(0.05f, 0.07f, 0.1f, 0.96f));
            Anchor(root.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var controller = host.AddComponent<EndGameController>();
            controller.panelRoot = root;
            controller.winnerLabel = CreateText("WinnerLabel", root.transform, "Winner", 28, TextAlignmentOptions.Center, new Vector2(460f, 100f));
            Anchor(controller.winnerLabel.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero);
            var returnButton = CreateButton("ReturnToMenuButton", root.transform, "Main Menu", new Vector2(160f, 44f));
            Anchor(returnButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f));
            UnityEventTools.AddPersistentListener(returnButton.onClick, controller.OnReturnToMenu);
            root.SetActive(false);
        }

        private static void CreateTileInfoButtons(Transform parent, PropertyPopup popup)
        {
            var root = CreateUIObject("TileInfoButtons", parent, Vector2.zero);
            Stretch(root.GetComponent<RectTransform>());

            for (int i = 0; i < 40; i++)
            {
                var button = CreateUIObject($"TileInfoButton_{i:00}", root.transform, new Vector2(76f, 56f));
                var image = button.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                var clickTarget = button.AddComponent<TileInfoButton>();
                clickTarget.tilePosition = i;
                clickTarget.popup = popup;
                Anchor(button.GetComponent<RectTransform>(), TileButtonAnchorMin(i), TileButtonAnchorMax(i), TileButtonOffset(i));
            }
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return canvas;
        }

        private static Camera CreateSceneCamera(string name, Color backgroundColor, float orthographicSize)
        {
            var cameraObject = new GameObject(name);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.depth = -1f;
            return camera;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
        {
            var panel = CreateUIObject(name, parent, size);
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            if (parent != null) obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return obj;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment, Vector2 size)
        {
            var obj = CreateUIObject(name, parent, size);
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size)
        {
            var obj = CreateUIObject(name, parent, size);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.17f, 0.32f, 0.56f, 1f);
            var button = obj.AddComponent<Button>();
            var text = CreateText("Label", obj.transform, label, 18, TextAlignmentOptions.Center, size);
            Stretch(text.rectTransform);
            return button;
        }

        private static TMP_InputField CreateInputField(string name, Transform parent, string placeholder, Vector2 size)
        {
            var root = CreateUIObject(name, parent, size);
            var background = root.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.94f);
            var input = root.AddComponent<TMP_InputField>();

            var textArea = CreateUIObject("TextArea", root.transform, size - new Vector2(24f, 10f));
            Stretch(textArea.GetComponent<RectTransform>(), new Vector2(12f, 5f), new Vector2(-12f, -5f));

            var placeholderText = CreateText("Placeholder", textArea.transform, placeholder, 18, TextAlignmentOptions.Left, size);
            placeholderText.color = new Color(0.35f, 0.36f, 0.4f, 0.8f);
            Stretch(placeholderText.rectTransform);

            var inputText = CreateText("Text", textArea.transform, string.Empty, 18, TextAlignmentOptions.Left, size);
            inputText.color = Color.black;
            Stretch(inputText.rectTransform);

            input.textViewport = textArea.GetComponent<RectTransform>();
            input.placeholder = placeholderText;
            input.textComponent = inputText;
            return input;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path)?.Replace("\\", "/"));
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameBoardScenePath, true),
                new EditorBuildSettingsScene(EndGameScenePath, true)
            };
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchoredPosition)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
