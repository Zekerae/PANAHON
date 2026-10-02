#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Panahon.Data;
using Panahon.Board;
using Panahon.Core;
using Panahon.Player;
using Panahon.Cards;
using Panahon.UI;
using Panahon.UI.MainMenu;
using Panahon.Networking;

namespace Panahon.Editor
{
    public static class PanahonSceneBuilder
    {
        [MenuItem("Tools/P.A.N.A.H.O.N./Generate Complete Multiplayer 2.5D Scene", false, 0)]
        public static void GenerateCompleteScene()
        {
            Debug.Log("[PANAHON] Generating Complete Multiplayer 2.5D Tabletop Scene...");

            // 1. Setup 2.5D Tabletop Camera and Lighting
            SetupCameraAndLighting();

            // 2. Setup Central Board Manager & Tracks
            BoardManager boardManager = SetupBoardHierarchy();

            // 3. Setup Card Database and Decks
            CardDatabase cardDb = SetupCardDatabase();
            DeckManager deckMgr = SetupDeckManager();

            // 4. Setup Core Managers
            TimelineManager timelineMgr = SetupTimelineManager();
            ParadoxManager paradoxMgr = SetupParadoxManager();
            ScoreManager scoreMgr = SetupScoreManager();
            TurnManager turnMgr = SetupTurnManager();

            // 5. Setup Networking (Photon Fusion API & Turn Sync)
            NetworkManager networkMgr = SetupNetworking(turnMgr, timelineMgr);

            // 6. Setup Canvas UI (Gameplay HUD + Main Menu + Lobby + Settings + Codex)
            UIManager uiMgr = SetupCanvasUI(timelineMgr, turnMgr, networkMgr);

            // 7. Setup Game Manager
            SetupGameManager(boardManager, turnMgr, timelineMgr, paradoxMgr, scoreMgr, deckMgr, cardDb, uiMgr);

            // 8. Setup Board Background (2.5D tabletop surface)
            SetupTabletopBackground();

            // 9. Save Scene to disk
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[PANAHON] Complete 2.5D Multiplayer Scene Generation SUCCESSFUL! Scene saved to disk. Press PLAY to test!");
            EditorUtility.DisplayDialog("P.A.N.A.H.O.N. Ready", 
                "Complete 2.5D Multiplayer Tabletop Scene generated!\n\n" +
                "• Main Menu & Online Lobby included\n" +
                "• Photon Fusion Server API integrated\n" +
                "• Settings (Audio/Graphics/Photon AppId) active\n" +
                "• How-To-Play Historical Codex ready\n\n" +
                "Press PLAY in Unity to start from the Main Menu!", "Let's Play!");
        }

        private static void SetupCameraAndLighting()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }

            mainCam.orthographic = true;
            mainCam.orthographicSize = 13.5f;
            mainCam.transform.position = new Vector3(0, 22f, -14f);
            mainCam.transform.rotation = Quaternion.Euler(58f, 0, 0);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);

            Light dirLight = Object.FindAnyObjectByType<Light>();
            if (dirLight == null || dirLight.type != LightType.Directional)
            {
                GameObject lightObj = new GameObject("Directional Light");
                dirLight = lightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
            }
            dirLight.transform.rotation = Quaternion.Euler(60f, -25f, 0);
            dirLight.color = new Color(1f, 0.96f, 0.92f);
            dirLight.intensity = 1.15f;
        }

        private static void SetupTabletopBackground()
        {
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "TabletopSurface";
            bg.transform.position = new Vector3(0, -0.5f, 0);
            bg.transform.rotation = Quaternion.Euler(90, 0, 0);
            bg.transform.localScale = new Vector3(42f, 42f, 1f);

            var col = bg.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            SetObjectColor(bg, new Color(0.08f, 0.10f, 0.14f));
        }

        private static BoardManager SetupBoardHierarchy()
        {
            GameObject boardRoot = new GameObject("=== GAME BOARD ===");
            BoardManager boardManager = boardRoot.AddComponent<BoardManager>();

            BoardPath spanishPath = CreateEraTrack(boardRoot.transform, EraType.SpanishEra, new Vector3(8f, 0, 8f));
            BoardPath americanPath = CreateEraTrack(boardRoot.transform, EraType.AmericanEra, new Vector3(8f, 0, -8f));
            BoardPath japanesePath = CreateEraTrack(boardRoot.transform, EraType.JapaneseEra, new Vector3(-8f, 0, -8f));
            BoardPath mythPath = CreateEraTrack(boardRoot.transform, EraType.MythLegendEra, new Vector3(-8f, 0, 8f));

            boardManager.SetEraPaths(spanishPath, americanPath, japanesePath, mythPath);

            CreateEraLabel(boardRoot.transform, "SPANISH ERA\n1521-1898", new Vector3(8f, 0.1f, 8f), EraType.SpanishEra.GetEraColor());
            CreateEraLabel(boardRoot.transform, "AMERICAN ERA\n1898-1946", new Vector3(8f, 0.1f, -8f), EraType.AmericanEra.GetEraColor());
            CreateEraLabel(boardRoot.transform, "JAPANESE ERA\n1942-1945", new Vector3(-8f, 0.1f, -8f), EraType.JapaneseEra.GetEraColor());
            CreateEraLabel(boardRoot.transform, "MYTH & LEGEND\nPre-Colonial", new Vector3(-8f, 0.1f, 8f), EraType.MythLegendEra.GetEraColor());
            CreateEraLabel(boardRoot.transform, "CHRONO\nNEXUS", new Vector3(0, 0.1f, 0), new Color(0.8f, 0.8f, 0.9f));

            return boardManager;
        }

        private static void CreateEraLabel(Transform parent, string text, Vector3 position, Color color)
        {
            GameObject labelObj = new GameObject($"EraLabel_{text.Replace("\n", "_")}");
            labelObj.transform.SetParent(parent);
            labelObj.transform.position = position;
            labelObj.transform.rotation = Quaternion.Euler(90, 0, 0);

            TextMesh tm = labelObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.2f;
            tm.fontSize = 28;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
        }

        private static BoardPath CreateEraTrack(Transform parent, EraType era, Vector3 centerOffset)
        {
            GameObject trackObj = new GameObject($"{era}_Track");
            trackObj.transform.SetParent(parent);
            trackObj.transform.position = centerOffset;

            BoardPath path = trackObj.AddComponent<BoardPath>();
            List<BoardTile> tiles = new List<BoardTile>();

            TileType[] tilePattern = new TileType[]
            {
                TileType.Start,
                TileType.Clue,
                TileType.Event,
                TileType.Danger,
                TileType.Artifact,
                TileType.TimeGate,
                TileType.Clue,
                TileType.Event,
                TileType.Danger,
                TileType.TimeGate
            };

            float radius = 5.5f;
            Color eraColor = era.GetEraColor();

            for (int i = 0; i < tilePattern.Length; i++)
            {
                float angle = (i / (float)tilePattern.Length) * Mathf.PI * 2f;
                Vector3 pos = centerOffset + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tileObj.name = $"Tile_{era}_{i}_{tilePattern[i]}";
                tileObj.transform.SetParent(trackObj.transform);
                tileObj.transform.position = pos;
                tileObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                tileObj.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

                BoardTile tile = tileObj.AddComponent<BoardTile>();
                tile.Setup(era, tilePattern[i], i);

                Color tileColor = Color.Lerp(tilePattern[i].GetTileColor(), eraColor, 0.15f);
                SetObjectColor(tileObj, tileColor);

                tile.CreateTileLabel(tile.GetTileSymbol());
                tiles.Add(tile);
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                BoardTile currentTile = tiles[i];
                BoardTile nextTile = (i + 1 < tiles.Count) ? tiles[i + 1] : tiles[0];

                GameObject lineObj = new GameObject($"PathLine_{era}_{i}");
                lineObj.transform.SetParent(trackObj.transform);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, currentTile.transform.position + Vector3.up * 0.02f);
                lr.SetPosition(1, nextTile.transform.position + Vector3.up * 0.02f);
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Standard"));
                lr.startColor = eraColor * 0.6f;
                lr.endColor = eraColor * 0.6f;
                lr.sortingOrder = -1;
            }

            path.InitializePath(era, tiles);
            return path;
        }

        private static CardDatabase SetupCardDatabase()
        {
            GameObject dbObj = new GameObject("CardDatabase");
            CardDatabase db = dbObj.AddComponent<CardDatabase>();
            db.PopulateDefaultDatabase();
            return db;
        }

        private static DeckManager SetupDeckManager()
        {
            GameObject deckObj = new GameObject("DeckManager");
            return deckObj.AddComponent<DeckManager>();
        }

        private static TimelineManager SetupTimelineManager()
        {
            GameObject tmObj = new GameObject("TimelineManager");
            return tmObj.AddComponent<TimelineManager>();
        }

        private static ParadoxManager SetupParadoxManager()
        {
            GameObject pmObj = new GameObject("ParadoxManager");
            return pmObj.AddComponent<ParadoxManager>();
        }

        private static ScoreManager SetupScoreManager()
        {
            GameObject smObj = new GameObject("ScoreManager");
            return smObj.AddComponent<ScoreManager>();
        }

        private static TurnManager SetupTurnManager()
        {
            GameObject turnObj = new GameObject("TurnManager");
            return turnObj.AddComponent<TurnManager>();
        }

        private static NetworkManager SetupNetworking(TurnManager tm, TimelineManager tlm)
        {
            GameObject netObj = new GameObject("NetworkManager");
            NetworkManager netMgr = netObj.AddComponent<NetworkManager>();

            var config = Resources.Load<PhotonFusionConfig>("PhotonFusionConfig");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PhotonFusionConfig>();
                string path = "Assets/PANAHON/Resources/PhotonFusionConfig.asset";
                System.IO.Directory.CreateDirectory("Assets/PANAHON/Resources");
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
            }
            netMgr.SetConfig(config);

            netObj.AddComponent<NetworkTurnSync>();
            return netMgr;
        }

        private static UIManager SetupCanvasUI(TimelineManager timelineMgr, TurnManager turnMgr, NetworkManager netMgr)
        {
            GameObject canvasObj = new GameObject("PANAHON_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            MainMenuUIBuilder.EnsureWorkingEventSystem();

            UIManager uiMgr = canvasObj.AddComponent<UIManager>();

            // Root Gameplay HUD Container
            GameObject hudRoot = CreateUIObject("GameplayHUD", canvasObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            hudRoot.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            hudRoot.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // In-Game HUD Elements
            SetupTopHUD(hudRoot.transform, uiMgr);
            SetupPlayersHUD(hudRoot.transform, uiMgr);
            SetupDiceUI(hudRoot.transform, uiMgr);
            SetupTimeGateModal(hudRoot.transform, uiMgr);
            SetupCardModal(hudRoot.transform, uiMgr);
            SetupLogUI(hudRoot.transform, uiMgr);
            SetupGameOverModal(hudRoot.transform, uiMgr);

            // Settings & Codex Modals (Shared across Menu and In-Game Pause)
            SettingsManager settingsMgr = SetupSettingsModal(canvasObj.transform);
            HowToPlayUI codexUI = SetupHowToPlayModal(canvasObj.transform);

            // In-Game Pause Menu
            SetupPauseMenu(canvasObj.transform, settingsMgr, codexUI);

            // Main Menu & Multiplayer Lobby System
            SetupMainMenuSystem(canvasObj.transform, hudRoot, settingsMgr, codexUI);

            return uiMgr;
        }

        private static void SetupTopHUD(Transform parentTransform, UIManager uiMgr)
        {
            GameObject topBar = CreateUIObject("TopBar", parentTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f));
            var rt = topBar.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 90);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(topBar, new Color(0.08f, 0.1f, 0.14f, 0.95f));

            // Title
            GameObject title = CreateUIObject("TitleText", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
            AddUIText(title, "<b>P.A.N.A.H.O.N.</b>\n<size=12>2.5D Multiplayer Tabletop</size>", 22, Color.white);

            // Timeline Meter Bar
            GameObject timelineRoot = CreateUIObject("TimelineMeterBar", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            timelineRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 55);
            timelineRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            TimelineMeterUI timelineUI = timelineRoot.AddComponent<TimelineMeterUI>();
            AddUIImage(timelineRoot, new Color(0.15f, 0.18f, 0.22f));

            GameObject sliderObj = CreateUIObject("Slider", timelineRoot.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.maxValue = 1f;

            GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            GameObject fill = CreateUIObject("Fill", fillArea.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Image fillImg = AddUIImage(fill, new Color(0.15f, 0.75f, 0.5f));
            slider.fillRect = fill.GetComponent<RectTransform>();

            GameObject labelObj = CreateUIObject("Label", timelineRoot.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text statusTxt = AddUIText(labelObj, "TIMELINE STABILITY: 0/10 (STABLE)", 14, Color.white, TextAnchor.MiddleCenter);

            SetPrivateField(timelineUI, "timelineSlider", slider);
            SetPrivateField(timelineUI, "fillImage", fillImg);
            SetPrivateField(timelineUI, "meterValueText", statusTxt);
            SetPrivateField(timelineUI, "statusText", statusTxt);
            SetPrivateField(uiMgr, "timelineMeterUI", timelineUI);

            // Timer & Round
            GameObject timerObj = CreateUIObject("TimerText", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            timerObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-110, 0);
            timerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);
            Text timerTxt = AddUIText(timerObj, "Time: 40:00\n<size=13>Round 1</size>", 18, Color.yellow, TextAnchor.MiddleRight);
            SetPrivateField(uiMgr, "gameTimerText", timerTxt);
            SetPrivateField(uiMgr, "roundText", timerTxt);

            // In-Game Pause Button [ || ]
            GameObject pauseBtnObj = CreateUIObject("PauseBtn", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            pauseBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 0);
            pauseBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
            AddUIImage(pauseBtnObj, new Color(0.2f, 0.25f, 0.35f));
            Button pauseBtn = pauseBtnObj.AddComponent<Button>();
            GameObject pauseTxtObj = CreateUIObject("Text", pauseBtnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIText(pauseTxtObj, "<b>||</b>", 20, Color.white, TextAnchor.MiddleCenter);

            pauseBtn.onClick.AddListener(() =>
            {
                var p = Object.FindAnyObjectByType<PauseMenuUI>();
                if (p != null) p.TogglePause();
            });
        }

        private static void SetupPlayersHUD(Transform parentTransform, UIManager uiMgr)
        {
            List<PlayerHUDCard> cards = new List<PlayerHUDCard>();
            Color[] playerAccentColors = {
                new Color(0.2f, 0.5f, 0.9f),
                new Color(0.85f, 0.35f, 0.7f),
                new Color(0.85f, 0.25f, 0.2f),
                new Color(0.25f, 0.8f, 0.35f)
            };

            for (int i = 0; i < 4; i++)
            {
                bool isLeft = (i < 2);
                int row = (i % 2);

                Vector2 anchor = isLeft ? new Vector2(0, 1) : new Vector2(1, 1);
                GameObject cardObj = CreateUIObject($"PlayerCard_{i+1}", parentTransform, anchor, anchor, anchor);
                var cardRt = cardObj.GetComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(260, 160);
                float posX = isLeft ? 15 : -15;
                float posY = -105 - (row * 175);
                cardRt.anchoredPosition = new Vector2(posX, posY);
                AddUIImage(cardObj, new Color(0.09f, 0.11f, 0.15f, 0.92f));

                PlayerHUDCard hudCard = cardObj.AddComponent<PlayerHUDCard>();

                GameObject banner = CreateUIObject("Banner", cardObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
                banner.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 32);
                banner.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                Image bannerImg = AddUIImage(banner, playerAccentColors[i]);

                GameObject nameObj = CreateUIObject("Name", banner.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                nameObj.GetComponent<RectTransform>().offsetMin = new Vector2(8, 0);
                Text nameTxt = AddUIText(nameObj, $"P{i+1}: Temporal Castaway", 14, Color.white, TextAnchor.MiddleLeft);

                GameObject statsObj = CreateUIObject("StatsText", cardObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
                statsObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 38);
                statsObj.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -36);
                Text statsTxt = AddUIText(statsObj, "Energy: 10/15 | Paradox: 0/5\nScore: 0 pts | Era: Spanish\nMission: Initializing...", 12, Color.white, TextAnchor.UpperLeft);

                GameObject btnObj = CreateUIObject("AbilityButton", cardObj.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
                btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-16, 30);
                btnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 6);
                AddUIImage(btnObj, new Color(0.25f, 0.35f, 0.5f));
                Button btn = btnObj.AddComponent<Button>();

                GameObject btnTxtObj = CreateUIObject("Text", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                Text btnTxt = AddUIText(btnTxtObj, "Special Ability", 12, Color.white, TextAnchor.MiddleCenter);

                SetPrivateField(hudCard, "playerNameText", nameTxt);
                SetPrivateField(hudCard, "characterColorBanner", bannerImg);
                SetPrivateField(hudCard, "energyText", statsTxt);
                SetPrivateField(hudCard, "missionText", statsTxt);
                SetPrivateField(hudCard, "abilityButton", btn);
                SetPrivateField(hudCard, "abilityButtonText", btnTxt);

                cards.Add(hudCard);
            }

            SetPrivateField(uiMgr, "playerHUDCards", cards);
        }

        private static void SetupDiceUI(Transform parentTransform, UIManager uiMgr)
        {
            GameObject diceRoot = CreateUIObject("DicePanel", parentTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            var rt = diceRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 110);
            rt.anchoredPosition = new Vector2(0, 20);
            AddUIImage(diceRoot, new Color(0.08f, 0.1f, 0.14f, 0.95f));

            DiceRollerUI diceUI = diceRoot.AddComponent<DiceRollerUI>();

            GameObject btnObj = CreateUIObject("RollButton", diceRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 48);
            btnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-35, 0);
            AddUIImage(btnObj, new Color(0.95f, 0.55f, 0.15f));
            Button rollBtn = btnObj.AddComponent<Button>();

            GameObject btnTxtObj = CreateUIObject("Text", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text rollBtnTxt = AddUIText(btnTxtObj, "<b>ROLL DICE</b>", 16, Color.white, TextAnchor.MiddleCenter);

            GameObject resObj = CreateUIObject("ResultBox", diceRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            resObj.GetComponent<RectTransform>().sizeDelta = new Vector2(55, 55);
            resObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(85, 0);
            AddUIImage(resObj, new Color(0.18f, 0.22f, 0.3f));

            GameObject resTxtObj = CreateUIObject("Text", resObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text resTxt = AddUIText(resTxtObj, "1", 28, Color.yellow, TextAnchor.MiddleCenter);

            SetPrivateField(diceUI, "rollButton", rollBtn);
            SetPrivateField(diceUI, "rollButtonText", rollBtnTxt);
            SetPrivateField(diceUI, "diceResultText", resTxt);
            SetPrivateField(uiMgr, "diceRollerUI", diceUI);
        }

        private static void SetupTimeGateModal(Transform parentTransform, UIManager uiMgr)
        {
            GameObject gateModal = CreateUIObject("TimeGateModal", parentTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var rt = gateModal.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 310);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(gateModal, new Color(0.09f, 0.08f, 0.16f, 0.98f));

            TimeGateDialogUI gateUI = gateModal.AddComponent<TimeGateDialogUI>();

            GameObject title = CreateUIObject("Title", gateModal.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            AddUIText(title, "<b>TIME GATE PORTAL ACTIVATED</b>\n<size=13>Spend Energy to warp to another timeline</size>", 17, new Color(0.8f, 0.6f, 1f), TextAnchor.MiddleCenter);

            Button btnSpanish = CreateModalButton("◎ Spanish Era (1521-1898)", gateModal.transform, new Vector2(0, 45), EraType.SpanishEra.GetEraColor() * 0.7f);
            Button btnAmerican = CreateModalButton("◎ American Era (1898-1946)", gateModal.transform, new Vector2(0, 0), EraType.AmericanEra.GetEraColor() * 0.7f);
            Button btnJapanese = CreateModalButton("◎ Japanese Era (1942-1945)", gateModal.transform, new Vector2(0, -45), EraType.JapaneseEra.GetEraColor() * 0.7f);
            Button btnMyth = CreateModalButton("◎ Myth & Legend Era (Ancient)", gateModal.transform, new Vector2(0, -90), EraType.MythLegendEra.GetEraColor() * 0.7f);
            Button btnPass = CreateModalButton("✕ Pass (Stay in Current Era)", gateModal.transform, new Vector2(0, -135), new Color(0.35f, 0.35f, 0.4f));

            SetPrivateField(gateUI, "dialogPanel", gateModal);
            SetPrivateField(gateUI, "spanishEraButton", btnSpanish);
            SetPrivateField(gateUI, "americanEraButton", btnAmerican);
            SetPrivateField(gateUI, "japaneseEraButton", btnJapanese);
            SetPrivateField(gateUI, "mythEraButton", btnMyth);
            SetPrivateField(gateUI, "passButton", btnPass);

            SetPrivateField(uiMgr, "timeGateDialogUI", gateUI);
            gateModal.SetActive(false);
        }

        private static void SetupCardModal(Transform parentTransform, UIManager uiMgr)
        {
            GameObject cardModal = CreateUIObject("CardModal", parentTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var rt = cardModal.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(460, 360);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(cardModal, new Color(0.1f, 0.12f, 0.16f, 0.98f));

            CardModalUI modalUI = cardModal.AddComponent<CardModalUI>();

            GameObject header = CreateUIObject("Header", cardModal.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            header.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Image headerImg = AddUIImage(header, new Color(0.2f, 0.5f, 0.85f));

            GameObject titleTxtObj = CreateUIObject("Title", header.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text titleTxt = AddUIText(titleTxtObj, "CARD TITLE", 18, Color.white, TextAnchor.MiddleCenter);

            GameObject bodyObj = CreateUIObject("Body", cardModal.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(25, 55);
            bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-25, -75);
            Text bodyTxt = AddUIText(bodyObj, "Card details and historical lore will appear here.", 14, Color.white, TextAnchor.UpperLeft);

            Button closeBtn = CreateModalButton("CONFIRM / CLOSE", cardModal.transform, new Vector2(0, -145), new Color(0.2f, 0.6f, 0.4f));

            SetPrivateField(modalUI, "modalRoot", cardModal);
            SetPrivateField(modalUI, "cardHeaderBackground", headerImg);
            SetPrivateField(modalUI, "cardTitleText", titleTxt);
            SetPrivateField(modalUI, "cardDescriptionText", bodyTxt);
            SetPrivateField(modalUI, "closeButton", closeBtn);

            SetPrivateField(uiMgr, "cardModalUI", modalUI);
            cardModal.SetActive(false);
        }

        private static void SetupLogUI(Transform parentTransform, UIManager uiMgr)
        {
            GameObject logObj = CreateUIObject("ActionLog", parentTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            var rt = logObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(340, 130);
            rt.anchoredPosition = new Vector2(15, 15);
            AddUIImage(logObj, new Color(0.05f, 0.07f, 0.09f, 0.88f));

            ActionLogUI logUI = logObj.AddComponent<ActionLogUI>();

            GameObject textObj = CreateUIObject("Text", logObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(8, 8);
            textObj.GetComponent<RectTransform>().offsetMax = new Vector2(-8, -8);
            Text logTxt = AddUIText(textObj, "Game initialized. Welcome to P.A.N.A.H.O.N.!", 11, new Color(0.85f, 0.85f, 0.85f), TextAnchor.LowerLeft);

            SetPrivateField(logUI, "logContentText", logTxt);
            SetPrivateField(uiMgr, "actionLogUI", logUI);
        }

        private static void SetupGameOverModal(Transform parentTransform, UIManager uiMgr)
        {
            GameObject goModal = CreateUIObject("GameOverModal", parentTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(goModal, new Color(0, 0, 0, 0.85f));

            GameObject box = CreateUIObject("Box", goModal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(560, 420);
            AddUIImage(box, new Color(0.11f, 0.13f, 0.17f));

            GameOverModalUI goUI = goModal.AddComponent<GameOverModalUI>();

            GameObject titleObj = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 60);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            Text titleTxt = AddUIText(titleObj, "TIMELINE STABILIZED!", 24, Color.cyan, TextAnchor.MiddleCenter);

            GameObject scoreObj = CreateUIObject("Scoreboard", box.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            scoreObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 60);
            scoreObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -90);
            Text scoreTxt = AddUIText(scoreObj, "Calculating final tallies...", 14, Color.white, TextAnchor.UpperLeft);

            Button restartBtn = CreateModalButton("PLAY AGAIN", box.transform, new Vector2(0, -170), new Color(0.2f, 0.6f, 0.4f));

            SetPrivateField(goUI, "modalPanel", goModal);
            SetPrivateField(goUI, "titleText", titleTxt);
            SetPrivateField(goUI, "scoreboardText", scoreTxt);
            SetPrivateField(goUI, "restartButton", restartBtn);

            SetPrivateField(uiMgr, "gameOverModalUI", goUI);
            goModal.SetActive(false);
        }

        // ==========================================
        // Main Menu & Multiplayer System UI
        // ==========================================

        private static void SetupMainMenuSystem(Transform canvasTransform, GameObject hudRoot, SettingsManager settingsMgr, HowToPlayUI codexUI)
        {
            GameObject menuMgrObj = new GameObject("MainMenuManager");
            menuMgrObj.transform.SetParent(canvasTransform, false);
            MainMenuManager menuMgr = menuMgrObj.AddComponent<MainMenuManager>();

            // Fullscreen Main Menu Root Panel
            GameObject mainPanel = CreateUIObject("MainMenuPanel", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(mainPanel, new Color(0.06f, 0.08f, 0.12f, 0.98f));

            // Central Card Box
            GameObject centerBox = CreateUIObject("CenterBox", mainPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            centerBox.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 640);
            AddUIImage(centerBox, new Color(0.09f, 0.11f, 0.16f, 0.96f));

            // Logo & Title
            GameObject titleObj = CreateUIObject("GameTitle", centerBox.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 110);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25);
            AddUIText(titleObj, "<b><size=38><color=#ffd166>P . A . N . A . H . O . N .</color></size></b>\n<size=13><color=#8be9fd>Philippine Archaeological National Archaeological Heritage of Nations</color></size>\n<size=12><color=#b0b0b0>2.5D Multiplayer Tabletop Time-Travel Strategy</color></size>", 16, Color.white, TextAnchor.MiddleCenter);

            // Era Badge Chips
            GameObject badges = CreateUIObject("EraBadges", centerBox.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            badges.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 32);
            badges.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -145);
            AddUIText(badges, "<color=#d97706>Spanish (1521)</color>  •  <color=#3b82f6>American (1898)</color>  •  <color=#ef4444>Japanese (1942)</color>  •  <color=#a855f7>Myth & Legend</color>", 13, Color.gray, TextAnchor.MiddleCenter);

            // Vertical Menu Buttons
            Button btnSolo = CreateModalButton("⚡ QUICK MATCH / SOLO PLAY", centerBox.transform, new Vector2(0, 80), new Color(0.18f, 0.65f, 0.38f));
            Button btnHost = CreateModalButton("🌐 HOST MULTIPLAYER ROOM (PHOTON)", centerBox.transform, new Vector2(0, 30), new Color(0.22f, 0.5f, 0.85f));
            Button btnJoin = CreateModalButton("🔗 JOIN WITH ROOM CODE", centerBox.transform, new Vector2(0, -20), new Color(0.35f, 0.38f, 0.65f));
            Button btnSettings = CreateModalButton("⚙ GAME & PHOTON SETTINGS", centerBox.transform, new Vector2(0, -70), new Color(0.28f, 0.32f, 0.42f));
            Button btnCodex = CreateModalButton("📖 HOW TO PLAY / HISTORICAL CODEX", centerBox.transform, new Vector2(0, -120), new Color(0.28f, 0.32f, 0.42f));
            Button btnQuit = CreateModalButton("✕ QUIT GAME", centerBox.transform, new Vector2(0, -175), new Color(0.55f, 0.22f, 0.22f));

            // Status notification bar at bottom
            GameObject statusObj = CreateUIObject("StatusText", centerBox.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 40);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
            Text statusTxt = AddUIText(statusObj, "Photon Fusion Ready. Connect online or play in solo tabletop mode.", 12, new Color(0.6f, 0.9f, 0.8f), TextAnchor.MiddleCenter);

            // Join Room Dialog Modal
            GameObject joinDialog = CreateJoinRoomDialog(canvasTransform);

            // Multiplayer Lobby UI Modal
            LobbyRoomUI lobbyUI = SetupLobbyModal(canvasTransform);

            // Link to MainMenuManager
            SetPrivateField(menuMgr, "mainMenuPanel", mainPanel);
            SetPrivateField(menuMgr, "joinRoomDialogPanel", joinDialog);
            SetPrivateField(menuMgr, "lobbyRoomUI", lobbyUI);
            SetPrivateField(menuMgr, "settingsManager", settingsMgr);
            SetPrivateField(menuMgr, "howToPlayUI", codexUI);
            SetPrivateField(menuMgr, "gameBoardCanvas", hudRoot);

            SetPrivateField(menuMgr, "playSoloButton", btnSolo);
            SetPrivateField(menuMgr, "hostGameButton", btnHost);
            SetPrivateField(menuMgr, "joinGameButton", btnJoin);
            SetPrivateField(menuMgr, "settingsButton", btnSettings);
            SetPrivateField(menuMgr, "howToPlayButton", btnCodex);
            SetPrivateField(menuMgr, "quitGameButton", btnQuit);
            SetPrivateField(menuMgr, "notificationBannerText", statusTxt);

            InputField codeInput = joinDialog.GetComponentInChildren<InputField>();
            Button confirmJoinBtn = joinDialog.transform.Find("Box/Btn_ConfirmJoin")?.GetComponent<Button>();
            Button cancelJoinBtn = joinDialog.transform.Find("Box/Btn_CancelJoin")?.GetComponent<Button>();

            SetPrivateField(menuMgr, "roomCodeInputField", codeInput);
            SetPrivateField(menuMgr, "confirmJoinButton", confirmJoinBtn);
            SetPrivateField(menuMgr, "cancelJoinButton", cancelJoinBtn);

            // Start in Main Menu mode (HUD hidden until match launched)
            hudRoot.SetActive(false);
            mainPanel.SetActive(true);
        }

        private static GameObject CreateJoinRoomDialog(Transform canvasTransform)
        {
            GameObject dialog = CreateUIObject("JoinRoomDialog", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(dialog, new Color(0, 0, 0, 0.75f));

            GameObject box = CreateUIObject("Box", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 240);
            AddUIImage(box, new Color(0.12f, 0.15f, 0.22f));

            GameObject title = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 45);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            AddUIText(title, "<b>JOIN MULTIPLAYER ROOM</b>\n<size=12>Enter the 5-character Room Code</size>", 16, Color.white, TextAnchor.MiddleCenter);

            // InputField
            GameObject inputObj = CreateUIObject("InputCode", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 45);
            inputObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
            AddUIImage(inputObj, new Color(0.2f, 0.24f, 0.32f));

            InputField inputField = inputObj.AddComponent<InputField>();
            inputField.characterLimit = 8;

            GameObject textObj = CreateUIObject("Text", inputObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            textObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            Text inputText = AddUIText(textObj, "", 20, Color.yellow, TextAnchor.MiddleCenter);
            inputField.textComponent = inputText;

            GameObject placeholderObj = CreateUIObject("Placeholder", inputObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            placeholderObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            Text phText = AddUIText(placeholderObj, "e.g. AB123", 16, Color.gray, TextAnchor.MiddleCenter);
            inputField.placeholder = phText;

            Button btnJoin = CreateModalButton("CONFIRM & JOIN", box.transform, new Vector2(0, -50), new Color(0.2f, 0.6f, 0.35f));
            btnJoin.gameObject.name = "Btn_ConfirmJoin";
            btnJoin.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 36);
            btnJoin.GetComponent<RectTransform>().anchoredPosition = new Vector2(-90, -60);

            Button btnCancel = CreateModalButton("CANCEL", box.transform, new Vector2(0, -50), new Color(0.45f, 0.45f, 0.5f));
            btnCancel.gameObject.name = "Btn_CancelJoin";
            btnCancel.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 36);
            btnCancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(90, -60);

            dialog.SetActive(false);
            return dialog;
        }

        private static LobbyRoomUI SetupLobbyModal(Transform canvasTransform)
        {
            GameObject lobbyObj = CreateUIObject("LobbyRoomPanel", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(lobbyObj, new Color(0.06f, 0.08f, 0.12f, 0.98f));

            LobbyRoomUI lobbyUI = lobbyObj.AddComponent<LobbyRoomUI>();

            GameObject box = CreateUIObject("CenterBox", lobbyObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(860, 660);
            AddUIImage(box, new Color(0.1f, 0.12f, 0.17f));

            // Room Code Header
            GameObject codeObj = CreateUIObject("RoomCodeText", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            codeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 45);
            codeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, -20);
            Text codeTxt = AddUIText(codeObj, "ROOM CODE: <color=#50fa7b><b>PANAH</b></color>", 22, Color.white, TextAnchor.MiddleCenter);

            Button btnCopy = CreateModalButton("COPY", box.transform, new Vector2(250, 305), new Color(0.2f, 0.4f, 0.6f));
            btnCopy.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 32);

            // Status message
            GameObject statusObj = CreateUIObject("Status", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 30);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -65);
            Text statusTxt = AddUIText(statusObj, "Waiting for players to connect and select Castaways...", 13, Color.cyan, TextAnchor.MiddleCenter);

            // 4 Player Slots
            List<Text> nameTexts = new List<Text>();
            List<Text> charTexts = new List<Text>();
            List<Image> banners = new List<Image>();

            for (int i = 0; i < 4; i++)
            {
                float y = 175 - (i * 70);
                GameObject slotObj = CreateUIObject($"Slot_{i+1}", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                slotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(780, 60);
                slotObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);
                Image slotBg = AddUIImage(slotObj, new Color(0.14f, 0.17f, 0.24f));
                banners.Add(slotBg);

                GameObject nameObj = CreateUIObject("Name", slotObj.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                nameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 0);
                nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 40);
                Text nTxt = AddUIText(nameObj, $"Slot {i+1}: Waiting...", 16, Color.white);
                nameTexts.Add(nTxt);

                GameObject chObj = CreateUIObject("Character", slotObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                chObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 40);
                Text cTxt = AddUIText(chObj, "No Castaway Selected", 14, Color.yellow, TextAnchor.MiddleCenter);
                charTexts.Add(cTxt);
            }

            // Character Select Buttons
            Button btnMateo = CreateModalButton("1. Dr. Mateo (Spanish)", box.transform, new Vector2(-300, -145), EraType.SpanishEra.GetEraColor() * 0.7f);
            btnMateo.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnTala = CreateModalButton("2. Tala (American)", box.transform, new Vector2(-100, -145), EraType.AmericanEra.GetEraColor() * 0.7f);
            btnTala.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnAndres = CreateModalButton("3. Andres (Japanese)", box.transform, new Vector2(100, -145), EraType.JapaneseEra.GetEraColor() * 0.7f);
            btnAndres.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnMaya = CreateModalButton("4. Maya (Myth)", box.transform, new Vector2(300, -145), EraType.MythLegendEra.GetEraColor() * 0.7f);
            btnMaya.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            // Bottom Actions
            Button btnReady = CreateModalButton("<b>READY UP</b>", box.transform, new Vector2(-180, -235), new Color(0.2f, 0.7f, 0.35f));
            btnReady.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 50);

            Button btnStart = CreateModalButton("<b>START GAME (HOST)</b>", box.transform, new Vector2(80, -235), new Color(0.9f, 0.55f, 0.15f));
            btnStart.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 50);

            Button btnLeave = CreateModalButton("LEAVE ROOM", box.transform, new Vector2(290, -235), new Color(0.45f, 0.45f, 0.5f));
            btnLeave.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 50);

            SetPrivateField(lobbyUI, "roomCodeText", codeTxt);
            SetPrivateField(lobbyUI, "copyCodeButton", btnCopy);
            SetPrivateField(lobbyUI, "connectionStatusText", statusTxt);

            SetPrivateField(lobbyUI, "slotNameTexts", nameTexts);
            SetPrivateField(lobbyUI, "slotCharacterTexts", charTexts);
            SetPrivateField(lobbyUI, "slotReadyBanners", banners);

            SetPrivateField(lobbyUI, "selectMateoButton", btnMateo);
            SetPrivateField(lobbyUI, "selectTalaButton", btnTala);
            SetPrivateField(lobbyUI, "selectAndresButton", btnAndres);
            SetPrivateField(lobbyUI, "selectMayaButton", btnMaya);

            SetPrivateField(lobbyUI, "readyButton", btnReady);
            SetPrivateField(lobbyUI, "readyButtonText", btnReady.GetComponentInChildren<Text>());
            SetPrivateField(lobbyUI, "startGameButton", btnStart);
            SetPrivateField(lobbyUI, "leaveRoomButton", btnLeave);

            lobbyObj.SetActive(false);
            return lobbyUI;
        }

        private static SettingsManager SetupSettingsModal(Transform canvasTransform)
        {
            GameObject modal = CreateUIObject("SettingsModal", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(modal, new Color(0, 0, 0, 0.85f));

            GameObject box = CreateUIObject("Box", modal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 560);
            AddUIImage(box, new Color(0.10f, 0.13f, 0.18f));

            SettingsManager settingsMgr = modal.AddComponent<SettingsManager>();

            // Title
            GameObject title = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            AddUIText(title, "<b>SETTINGS & PHOTON CONFIGURATION</b>", 20, Color.cyan, TextAnchor.MiddleCenter);

            // Photon Server Section
            GameObject netHeader = CreateUIObject("NetHeader", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            netHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 30);
            netHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);
            AddUIText(netHeader, "<b>PHOTON FUSION SERVER CLOUD:</b>", 14, Color.yellow, TextAnchor.MiddleLeft);

            GameObject appIdLabel = CreateUIObject("AppIdLabel", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            appIdLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 25);
            appIdLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
            AddUIText(appIdLabel, "Fusion App ID (from Photon Dashboard):", 12, Color.white, TextAnchor.MiddleLeft);

            GameObject inputObj = CreateUIObject("AppIdInput", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 38);
            inputObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -135);
            AddUIImage(inputObj, new Color(0.18f, 0.22f, 0.3f));

            InputField appIdInput = inputObj.AddComponent<InputField>();
            GameObject inTxtObj = CreateUIObject("Text", inputObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            inTxtObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            Text inTxt = AddUIText(inTxtObj, "", 14, Color.yellow, TextAnchor.MiddleLeft);
            appIdInput.textComponent = inTxt;

            GameObject phObj = CreateUIObject("Placeholder", inputObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            phObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            Text phTxt = AddUIText(phObj, "Paste your Photon Fusion App ID here (e.g. 7eafcbb2-...)", 12, Color.gray, TextAnchor.MiddleLeft);
            appIdInput.placeholder = phTxt;

            // Test Connection button
            Button testBtn = CreateModalButton("TEST PHOTON CLOUD CONNECTION", box.transform, new Vector2(-150, 85), new Color(0.2f, 0.55f, 0.75f));
            testBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 36);

            GameObject statusObj = CreateUIObject("StatusText", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 36);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, 85);
            Text statusTxt = AddUIText(statusObj, "<color=#50fa7b>Ready</color>", 12, Color.white, TextAnchor.MiddleLeft);

            // Audio Section
            GameObject audioHeader = CreateUIObject("AudioHeader", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            audioHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 30);
            audioHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 35);
            AddUIText(audioHeader, "<b>AUDIO & DISPLAY:</b>", 14, Color.yellow, TextAnchor.MiddleLeft);

            GameObject volLabel = CreateUIObject("VolLabel", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            volLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 25);
            volLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            AddUIText(volLabel, "Master Volume: 80%  •  Music: 70%  •  SFX: 90%", 12, Color.white, TextAnchor.MiddleLeft);

            // Bottom Actions
            Button btnReset = CreateModalButton("RESET DEFAULTS", box.transform, new Vector2(-140, -230), new Color(0.4f, 0.4f, 0.45f));
            btnReset.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 42);

            Button btnClose = CreateModalButton("SAVE & CLOSE", box.transform, new Vector2(140, -230), new Color(0.2f, 0.65f, 0.35f));
            btnClose.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 42);

            SetPrivateField(settingsMgr, "photonAppIdInput", appIdInput);
            SetPrivateField(settingsMgr, "testPhotonButton", testBtn);
            SetPrivateField(settingsMgr, "photonStatusText", statusTxt);
            SetPrivateField(settingsMgr, "resetDefaultsButton", btnReset);
            SetPrivateField(settingsMgr, "closeButton", btnClose);

            modal.SetActive(false);
            return settingsMgr;
        }

        private static HowToPlayUI SetupHowToPlayModal(Transform canvasTransform)
        {
            GameObject modal = CreateUIObject("HowToPlayModal", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(modal, new Color(0, 0, 0, 0.88f));

            GameObject box = CreateUIObject("Box", modal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            AddUIImage(box, new Color(0.10f, 0.12f, 0.17f));

            HowToPlayUI codexUI = modal.AddComponent<HowToPlayUI>();

            // Tabs across top
            Button btnTab1 = CreateModalButton("Overview", box.transform, new Vector2(-300, 260), new Color(0.25f, 0.35f, 0.5f));
            btnTab1.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 34);

            Button btnTab2 = CreateModalButton("The 4 Eras", box.transform, new Vector2(-150, 260), new Color(0.25f, 0.35f, 0.5f));
            btnTab2.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 34);

            Button btnTab3 = CreateModalButton("Characters", box.transform, new Vector2(0, 260), new Color(0.25f, 0.35f, 0.5f));
            btnTab3.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 34);

            Button btnTab4 = CreateModalButton("Board & Tiles", box.transform, new Vector2(150, 260), new Color(0.25f, 0.35f, 0.5f));
            btnTab4.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 34);

            Button btnTab5 = CreateModalButton("Timeline Rules", box.transform, new Vector2(300, 260), new Color(0.25f, 0.35f, 0.5f));
            btnTab5.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 34);

            // Title
            GameObject titleObj = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(740, 40);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -65);
            Text titleTxt = AddUIText(titleObj, "<b>P.A.N.A.H.O.N. HISTORICAL CODEX</b>", 18, Color.yellow, TextAnchor.MiddleCenter);

            // Content Body
            GameObject bodyObj = CreateUIObject("Body", box.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(35, 75);
            bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-35, -115);
            Text bodyTxt = AddUIText(bodyObj, "Loading codex lore...", 14, Color.white, TextAnchor.UpperLeft);

            // Close button
            Button closeBtn = CreateModalButton("CLOSE CODEX", box.transform, new Vector2(0, -255), new Color(0.2f, 0.6f, 0.4f));
            closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 42);

            SetPrivateField(codexUI, "tabOverviewButton", btnTab1);
            SetPrivateField(codexUI, "tabErasButton", btnTab2);
            SetPrivateField(codexUI, "tabCharactersButton", btnTab3);
            SetPrivateField(codexUI, "tabTilesButton", btnTab4);
            SetPrivateField(codexUI, "tabTimelineButton", btnTab5);

            SetPrivateField(codexUI, "contentTitleText", titleTxt);
            SetPrivateField(codexUI, "contentBodyText", bodyTxt);
            SetPrivateField(codexUI, "closeButton", closeBtn);

            modal.SetActive(false);
            return codexUI;
        }

        private static void SetupPauseMenu(Transform canvasTransform, SettingsManager settingsMgr, HowToPlayUI codexUI)
        {
            GameObject pauseObj = CreateUIObject("PauseMenu", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(pauseObj, new Color(0, 0, 0, 0.85f));

            PauseMenuUI pauseUI = pauseObj.AddComponent<PauseMenuUI>();

            GameObject box = CreateUIObject("Box", pauseObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 400);
            AddUIImage(box, new Color(0.10f, 0.12f, 0.17f));

            GameObject title = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            AddUIText(title, "<b>GAME PAUSED</b>\n<size=12>P.A.N.A.H.O.N. Tabletop Match</size>", 20, Color.cyan, TextAnchor.MiddleCenter);

            Button btnResume = CreateModalButton("RESUME GAME", box.transform, new Vector2(0, 50), new Color(0.2f, 0.65f, 0.35f));
            Button btnSettings = CreateModalButton("SETTINGS & PHOTON", box.transform, new Vector2(0, 0), new Color(0.25f, 0.35f, 0.5f));
            Button btnCodex = CreateModalButton("HOW TO PLAY / CODEX", box.transform, new Vector2(0, -50), new Color(0.25f, 0.35f, 0.5f));
            Button btnQuit = CreateModalButton("QUIT TO MAIN MENU", box.transform, new Vector2(0, -115), new Color(0.55f, 0.22f, 0.22f));

            SetPrivateField(pauseUI, "pausePanel", pauseObj);
            SetPrivateField(pauseUI, "settingsManager", settingsMgr);
            SetPrivateField(pauseUI, "howToPlayUI", codexUI);

            SetPrivateField(pauseUI, "resumeButton", btnResume);
            SetPrivateField(pauseUI, "settingsButton", btnSettings);
            SetPrivateField(pauseUI, "howToPlayButton", btnCodex);
            SetPrivateField(pauseUI, "quitToMenuButton", btnQuit);

            pauseObj.SetActive(false);
        }

        private static void SetupGameManager(BoardManager bm, TurnManager tm, TimelineManager tlm, ParadoxManager pm, ScoreManager sm, DeckManager dm, CardDatabase cdb, UIManager ui)
        {
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();

            SetPrivateField(gm, "boardManager", bm);
            SetPrivateField(gm, "turnManager", tm);
            SetPrivateField(gm, "timelineManager", tlm);
            SetPrivateField(gm, "paradoxManager", pm);
            SetPrivateField(gm, "scoreManager", sm);
            SetPrivateField(gm, "deckManager", dm);
            SetPrivateField(gm, "cardDatabase", cdb);
        }

        // --- Helper Methods ---

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            return obj;
        }

        private static Image AddUIImage(GameObject obj, Color color)
        {
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static Text AddUIText(GameObject obj, string text, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }

        private static Button CreateModalButton(string label, Transform parent, Vector2 anchoredPos, Color? color = null)
        {
            string safeName = label.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("&", "and").Replace("<b>", "").Replace("</b>", "").Replace("/", "_");
            GameObject btnObj = CreateUIObject($"Btn_{safeName}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 42);
            btnObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            AddUIImage(btnObj, color ?? new Color(0.2f, 0.45f, 0.75f));
            Button btn = btnObj.AddComponent<Button>();

            GameObject txtObj = CreateUIObject("Text", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIText(txtObj, label, 14, Color.white, TextAnchor.MiddleCenter);
            return btn;
        }

        private static void SetObjectColor(GameObject obj, Color c)
        {
            Renderer r = obj.GetComponent<Renderer>();
            if (r != null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                if (s == null) s = Shader.Find("Hidden/InternalErrorShader");
                Material m = new Material(s);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                r.sharedMaterial = m;
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[PanahonSceneBuilder] Could not find field '{fieldName}' on {target.GetType().Name}");
            }
        }
    }
}
#endif
