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

namespace Panahon.Editor
{
    public static class PanahonSceneBuilder
    {
        [MenuItem("Tools/P.A.N.A.H.O.N./Generate 2.5D Tabletop Scene (Universal 2D)", false, 0)]
        [MenuItem("Tools/P.A.N.A.H.O.N./Generate Complete Playable Scene", false, 1)]
        public static void GenerateCompleteScene()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Generate PANAHON 2.5D Scene");
            int group = Undo.GetCurrentGroup();

            Debug.Log("[PANAHON] Generating 2.5D Tabletop Game Scene...");

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

            // 5. Setup Canvas UI
            UIManager uiMgr = SetupCanvasUI(timelineMgr);

            // 6. Setup Game Manager
            SetupGameManager(boardManager, turnMgr, timelineMgr, paradoxMgr, scoreMgr, deckMgr, cardDb, uiMgr);

            Undo.CollapseUndoGroup(group);
            Debug.Log("[PANAHON] 2.5D Tabletop Scene Generation COMPLETE! Press PLAY to test the full game loop!");
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

            // 2.5D Tabletop Orthographic View (distortion-free board game perspective)
            mainCam.orthographic = true;
            mainCam.orthographicSize = 13.5f;
            mainCam.transform.position = new Vector3(0, 22f, -14f);
            mainCam.transform.rotation = Quaternion.Euler(58f, 0, 0);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.06f, 0.08f, 0.12f); // Deep temporal void

            Light dirLight = Object.FindFirstObjectByType<Light>();
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

        private static BoardManager SetupBoardHierarchy()
        {
            GameObject boardRoot = new GameObject("=== GAME BOARD ===");
            BoardManager boardManager = boardRoot.AddComponent<BoardManager>();

            // Center Chrono-Nexus table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            table.name = "ChronoNexus_CenterTable";
            table.transform.SetParent(boardRoot.transform);
            table.transform.position = new Vector3(0, -0.4f, 0);
            table.transform.localScale = new Vector3(36f, 0.5f, 36f);
            SetObjectColor(table, new Color(0.12f, 0.14f, 0.18f));

            // Generate 4 Era Tracks in 4 Quadrants
            BoardPath spanishPath = CreateEraTrack(boardRoot.transform, EraType.SpanishEra, new Vector3(8f, 0, 8f));
            BoardPath americanPath = CreateEraTrack(boardRoot.transform, EraType.AmericanEra, new Vector3(8f, 0, -8f));
            BoardPath japanesePath = CreateEraTrack(boardRoot.transform, EraType.JapaneseEra, new Vector3(-8f, 0, -8f));
            BoardPath mythPath = CreateEraTrack(boardRoot.transform, EraType.MythLegendEra, new Vector3(-8f, 0, 8f));

            boardManager.SetEraPaths(spanishPath, americanPath, japanesePath, mythPath);

            return boardManager;
        }

        private static BoardPath CreateEraTrack(Transform parent, EraType era, Vector3 centerOffset)
        {
            GameObject trackObj = new GameObject($"{era}_Track");
            trackObj.transform.SetParent(parent);
            trackObj.transform.position = centerOffset;

            BoardPath path = trackObj.AddComponent<BoardPath>();
            List<BoardTile> tiles = new List<BoardTile>();

            // 10 tiles arranged in a loop around the centerOffset
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
            for (int i = 0; i < tilePattern.Length; i++)
            {
                float angle = (i / (float)tilePattern.Length) * Mathf.PI * 2f;
                Vector3 pos = centerOffset + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObj.name = $"Tile_{era}_{i}_{tilePattern[i]}";
                tileObj.transform.SetParent(trackObj.transform);
                tileObj.transform.position = pos;
                tileObj.transform.localScale = new Vector3(1.6f, 0.3f, 1.6f);

                BoardTile tile = tileObj.AddComponent<BoardTile>();
                tile.Setup(era, tilePattern[i], i);
                SetObjectColor(tileObj, tilePattern[i].GetTileColor());

                tiles.Add(tile);
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

        private static UIManager SetupCanvasUI(TimelineManager timelineMgr)
        {
            // Canvas
            GameObject canvasObj = new GameObject("PANAHON_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            UIManager uiMgr = canvasObj.AddComponent<UIManager>();

            // Top Header: Timeline Meter & Game Timer
            SetupTopHUD(canvasObj.transform, uiMgr);

            // Left/Right: 4 Player HUD cards
            SetupPlayersHUD(canvasObj.transform, uiMgr);

            // Bottom Center: Dice Roller
            SetupDiceUI(canvasObj.transform, uiMgr);

            // Time Gate Dialog Modal
            SetupTimeGateModal(canvasObj.transform, uiMgr);

            // Card Popup Modal
            SetupCardModal(canvasObj.transform, uiMgr);

            // Game Log
            SetupLogUI(canvasObj.transform, uiMgr);

            // Game Over Modal
            SetupGameOverModal(canvasObj.transform, uiMgr);

            return uiMgr;
        }

        private static void SetupTopHUD(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject topBar = CreateUIObject("TopBar", canvasTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f));
            var rt = topBar.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 90);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(topBar, new Color(0.08f, 0.1f, 0.14f, 0.95f));

            // Title
            GameObject title = CreateUIObject("TitleText", topBar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
            var titleTxt = AddUIText(title, "<b>P.A.N.A.H.O.N.</b>\n<size=12>Temporal Castaways Board Game</size>", 22, Color.white);

            // Shared Timeline Meter Bar
            GameObject timelineRoot = CreateUIObject("TimelineMeterBar", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            timelineRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 55);
            timelineRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            TimelineMeterUI timelineUI = timelineRoot.AddComponent<TimelineMeterUI>();

            // Timeline Background
            AddUIImage(timelineRoot, new Color(0.15f, 0.18f, 0.22f));

            // Slider
            GameObject sliderObj = CreateUIObject("Slider", timelineRoot.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.maxValue = 1f;

            GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            GameObject fill = CreateUIObject("Fill", fillArea.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Image fillImg = AddUIImage(fill, new Color(0.15f, 0.75f, 0.5f));
            slider.fillRect = fill.GetComponent<RectTransform>();

            // Status label
            GameObject labelObj = CreateUIObject("Label", timelineRoot.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text statusTxt = AddUIText(labelObj, "TIMELINE STABILITY: 0/10 (STABLE)", 14, Color.white, TextAnchor.MiddleCenter);

            SetPrivateField(timelineUI, "timelineSlider", slider);
            SetPrivateField(timelineUI, "fillImage", fillImg);
            SetPrivateField(timelineUI, "meterValueText", statusTxt);
            SetPrivateField(timelineUI, "statusText", statusTxt);

            SetPrivateField(uiMgr, "timelineMeterUI", timelineUI);

            // Timer
            GameObject timerObj = CreateUIObject("TimerText", topBar.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            timerObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, 0);
            timerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);
            Text timerTxt = AddUIText(timerObj, "Time: 40:00\n<size=13>Round 1</size>", 18, Color.yellow, TextAnchor.MiddleRight);
            SetPrivateField(uiMgr, "gameTimerText", timerTxt);
            SetPrivateField(uiMgr, "roundText", timerTxt);
        }

        private static void SetupPlayersHUD(Transform canvasTransform, UIManager uiMgr)
        {
            List<PlayerHUDCard> cards = new List<PlayerHUDCard>();

            for (int i = 0; i < 4; i++)
            {
                bool isLeft = (i < 2);
                int row = (i % 2);

                Vector2 anchor = isLeft ? new Vector2(0, 1) : new Vector2(1, 1);
                Vector2 pivot = isLeft ? new Vector2(0, 1) : new Vector2(1, 1);

                GameObject cardObj = CreateUIObject($"PlayerCard_{i+1}", canvasTransform, anchor, anchor, pivot);
                var rt = cardObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(260, 160);
                float posX = isLeft ? 15 : -15;
                float posY = -105 - (row * 175);
                rt.anchoredPosition = new Vector2(posX, posY);

                AddUIImage(cardObj, new Color(0.09f, 0.11f, 0.15f, 0.92f));

                PlayerHUDCard hudCard = cardObj.AddComponent<PlayerHUDCard>();

                // Header Banner
                GameObject banner = CreateUIObject("Banner", cardObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
                banner.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 32);
                banner.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                Image bannerImg = AddUIImage(banner, new Color(0.2f, 0.4f, 0.8f));

                // Name
                GameObject nameObj = CreateUIObject("Name", banner.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                nameObj.GetComponent<RectTransform>().offsetMin = new Vector2(8, 0);
                Text nameTxt = AddUIText(nameObj, $"P{i+1}: Temporal Castaway", 14, Color.white, TextAnchor.MiddleLeft);

                // Stats text
                GameObject statsObj = CreateUIObject("StatsText", cardObj.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
                statsObj.GetComponent<RectTransform>().offsetMin = new Vector2(10, 38);
                statsObj.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -36);
                Text statsTxt = AddUIText(statsObj, "Energy: 10/15 | Paradox: 0/5\nScore: 0 pts | Era: Spanish\nMission: Initializing...", 12, Color.white, TextAnchor.UpperLeft);

                // Ability Button
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

        private static void SetupDiceUI(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject diceRoot = CreateUIObject("DicePanel", canvasTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            var rt = diceRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 110);
            rt.anchoredPosition = new Vector2(0, 20);
            AddUIImage(diceRoot, new Color(0.08f, 0.1f, 0.14f, 0.95f));

            DiceRollerUI diceUI = diceRoot.AddComponent<DiceRollerUI>();

            // Roll Button
            GameObject btnObj = CreateUIObject("RollButton", diceRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 48);
            btnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-35, 0);
            AddUIImage(btnObj, new Color(0.95f, 0.55f, 0.15f));
            Button rollBtn = btnObj.AddComponent<Button>();

            GameObject btnTxtObj = CreateUIObject("Text", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text rollBtnTxt = AddUIText(btnTxtObj, "<b>ROLL DICE</b>", 16, Color.white, TextAnchor.MiddleCenter);

            // Result Display
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

        private static void SetupTimeGateModal(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject gateModal = CreateUIObject("TimeGateModal", canvasTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var rt = gateModal.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 310);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(gateModal, new Color(0.09f, 0.08f, 0.16f, 0.98f));

            TimeGateDialogUI gateUI = gateModal.AddComponent<TimeGateDialogUI>();

            // Title
            GameObject title = CreateUIObject("Title", gateModal.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            AddUIText(title, "<b>TIME GATE PORTAL ACTIVATED</b>\n<size=13>Spend 2 Energy to warp to another timeline</size>", 17, new Color(0.8f, 0.6f, 1f), TextAnchor.MiddleCenter);

            // Destination Buttons
            Button btnSpanish = CreateModalButton("Spanish Era (1521-1898)", gateModal.transform, new Vector2(0, 45));
            Button btnAmerican = CreateModalButton("American Era (1898-1946)", gateModal.transform, new Vector2(0, 0));
            Button btnJapanese = CreateModalButton("Japanese Era (1942-1945)", gateModal.transform, new Vector2(0, -45));
            Button btnMyth = CreateModalButton("Myth & Legend Era (Ancient)", gateModal.transform, new Vector2(0, -90));
            Button btnPass = CreateModalButton("Pass (Stay in Current Era)", gateModal.transform, new Vector2(0, -135), new Color(0.35f, 0.35f, 0.4f));

            SetPrivateField(gateUI, "dialogPanel", gateModal);
            SetPrivateField(gateUI, "spanishEraButton", btnSpanish);
            SetPrivateField(gateUI, "americanEraButton", btnAmerican);
            SetPrivateField(gateUI, "japaneseEraButton", btnJapanese);
            SetPrivateField(gateUI, "mythEraButton", btnMyth);
            SetPrivateField(gateUI, "passButton", btnPass);

            SetPrivateField(uiMgr, "timeGateDialogUI", gateUI);
            gateModal.SetActive(false);
        }

        private static void SetupCardModal(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject cardModal = CreateUIObject("CardModal", canvasTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var rt = cardModal.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(460, 360);
            rt.anchoredPosition = Vector2.zero;
            AddUIImage(cardModal, new Color(0.1f, 0.12f, 0.16f, 0.98f));

            CardModalUI modalUI = cardModal.AddComponent<CardModalUI>();

            // Header banner
            GameObject header = CreateUIObject("Header", cardModal.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            header.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Image headerImg = AddUIImage(header, new Color(0.2f, 0.5f, 0.85f));

            GameObject titleTxtObj = CreateUIObject("Title", header.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Text titleTxt = AddUIText(titleTxtObj, "CARD TITLE", 18, Color.white, TextAnchor.MiddleCenter);

            // Body text
            GameObject bodyObj = CreateUIObject("Body", cardModal.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(25, 55);
            bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-25, -75);
            Text bodyTxt = AddUIText(bodyObj, "Card details and historical lore will appear here.", 14, Color.white, TextAnchor.UpperLeft);

            // Close button
            Button closeBtn = CreateModalButton("CONFIRM / CLOSE", cardModal.transform, new Vector2(0, -145), new Color(0.2f, 0.6f, 0.4f));

            SetPrivateField(modalUI, "modalRoot", cardModal);
            SetPrivateField(modalUI, "cardHeaderBackground", headerImg);
            SetPrivateField(modalUI, "cardTitleText", titleTxt);
            SetPrivateField(modalUI, "cardDescriptionText", bodyTxt);
            SetPrivateField(modalUI, "closeButton", closeBtn);

            SetPrivateField(uiMgr, "cardModalUI", modalUI);
            cardModal.SetActive(false);
        }

        private static void SetupLogUI(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject logObj = CreateUIObject("ActionLog", canvasTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
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

        private static void SetupGameOverModal(Transform canvasTransform, UIManager uiMgr)
        {
            GameObject goModal = CreateUIObject("GameOverModal", canvasTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(goModal, new Color(0, 0, 0, 0.85f));

            GameObject box = CreateUIObject("Box", goModal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(560, 420);
            AddUIImage(box, new Color(0.11f, 0.13f, 0.17f));

            GameOverModalUI goUI = goModal.AddComponent<GameOverModalUI>();

            // Title
            GameObject titleObj = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 60);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            Text titleTxt = AddUIText(titleObj, "TIMELINE STABILIZED!", 24, Color.cyan, TextAnchor.MiddleCenter);

            // Scoreboard
            GameObject scoreObj = CreateUIObject("Scoreboard", box.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            scoreObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 60);
            scoreObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -90);
            Text scoreTxt = AddUIText(scoreObj, "Calculating final tallies...", 14, Color.white, TextAnchor.UpperLeft);

            // Restart Button
            Button restartBtn = CreateModalButton("PLAY AGAIN", box.transform, new Vector2(0, -170), new Color(0.2f, 0.6f, 0.4f));

            SetPrivateField(goUI, "modalPanel", goModal);
            SetPrivateField(goUI, "titleText", titleTxt);
            SetPrivateField(goUI, "scoreboardText", scoreTxt);
            SetPrivateField(goUI, "restartButton", restartBtn);

            SetPrivateField(uiMgr, "gameOverModalUI", goUI);
            goModal.SetActive(false);
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
            GameObject btnObj = CreateUIObject($"Btn_{label.Replace(" ", "_")}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 36);
            btnObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            AddUIImage(btnObj, color ?? new Color(0.2f, 0.45f, 0.75f));
            Button btn = btnObj.AddComponent<Button>();

            GameObject txtObj = CreateUIObject("Text", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIText(txtObj, label, 13, Color.white, TextAnchor.MiddleCenter);
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
        }
    }
}
#endif
