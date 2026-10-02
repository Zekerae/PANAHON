using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Data;
using Panahon.Networking;

namespace Panahon.UI.MainMenu
{
    public static class MainMenuUIBuilder
    {
        public static void EnsureWorkingEventSystem()
        {
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            var standalone = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            System.Type newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newModuleType != null)
            {
                if (es.GetComponent(newModuleType) == null)
                {
                    if (standalone != null) Object.DestroyImmediate(standalone);
                    es.gameObject.AddComponent(newModuleType);
                }
            }
            else if (standalone == null)
            {
                es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        public static MainMenuManager BuildMenuHierarchy(Canvas canvas, GameObject gameplayHUD = null)
        {
            EnsureWorkingEventSystem();
            Transform parent = canvas.transform;

            // Check if already exists
            MainMenuManager existing = parent.GetComponentInChildren<MainMenuManager>(true);
            if (existing != null)
            {
                existing.ShowMainMenuScreen();
                return existing;
            }

            GameObject menuMgrObj = new GameObject("MainMenuManager");
            menuMgrObj.transform.SetParent(parent, false);
            MainMenuManager menuMgr = menuMgrObj.AddComponent<MainMenuManager>();

            // 1. Fullscreen Main Menu Root Panel
            GameObject mainPanel = CreateUIObject("MainMenuPanel", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(mainPanel, new Color(0.06f, 0.08f, 0.12f, 0.98f));

            // Central Card Box
            GameObject centerBox = CreateUIObject("CenterBox", mainPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            centerBox.GetComponent<RectTransform>().sizeDelta = new Vector2(740, 650);
            AddUIImage(centerBox, new Color(0.09f, 0.11f, 0.16f, 0.98f));

            // Title & Lore Subtitle
            GameObject titleObj = CreateUIObject("GameTitle", centerBox.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 115);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            AddUIText(titleObj, "<b><size=38><color=#ffd166>P . A . N . A . H . O . N .</color></size></b>\n<size=13><color=#8be9fd>Philippine Archaeological National Archaeological Heritage of Nations</color></size>\n<size=12><color=#b0b0b0>2.5D Multiplayer Tabletop Time-Travel Strategy</color></size>", 16, Color.white, TextAnchor.MiddleCenter);

            // Era Badges
            GameObject badges = CreateUIObject("EraBadges", centerBox.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            badges.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 30);
            badges.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -145);
            AddUIText(badges, "<color=#d97706>Spanish (1521)</color>  •  <color=#3b82f6>American (1898)</color>  •  <color=#ef4444>Japanese (1942)</color>  •  <color=#a855f7>Myth & Legend</color>", 13, Color.gray, TextAnchor.MiddleCenter);

            // Menu Buttons
            Button btnSolo = CreateModalButton("⚡ QUICK MATCH / SOLO PLAY", centerBox.transform, new Vector2(0, 80), new Color(0.18f, 0.65f, 0.38f));
            Button btnHost = CreateModalButton("🌐 HOST MULTIPLAYER ROOM (PHOTON)", centerBox.transform, new Vector2(0, 30), new Color(0.22f, 0.5f, 0.85f));
            Button btnJoin = CreateModalButton("🔗 JOIN WITH ROOM CODE", centerBox.transform, new Vector2(0, -20), new Color(0.35f, 0.38f, 0.65f));
            Button btnSettings = CreateModalButton("⚙ GAME & PHOTON SETTINGS", centerBox.transform, new Vector2(0, -70), new Color(0.28f, 0.32f, 0.42f));
            Button btnCodex = CreateModalButton("📖 HOW TO PLAY / HISTORICAL CODEX", centerBox.transform, new Vector2(0, -120), new Color(0.28f, 0.32f, 0.42f));
            Button btnQuit = CreateModalButton("✕ QUIT GAME", centerBox.transform, new Vector2(0, -175), new Color(0.55f, 0.22f, 0.22f));

            // Status notification bar
            GameObject statusObj = CreateUIObject("StatusText", centerBox.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 40);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
            Text statusTxt = AddUIText(statusObj, "Photon Fusion Ready. Connect online or play in solo tabletop mode.", 12, new Color(0.6f, 0.9f, 0.8f), TextAnchor.MiddleCenter);

            // 2. Modals: Join Dialog, Lobby, Settings, Codex
            GameObject joinDialog = CreateJoinRoomDialog(parent);
            LobbyRoomUI lobbyUI = SetupLobbyModal(parent);
            SettingsManager settingsMgr = SetupSettingsModal(parent);
            HowToPlayUI codexUI = SetupHowToPlayModal(parent);

            // 3. Pause Menu
            SetupPauseMenu(parent, settingsMgr, codexUI);

            // 4. Wire everything to MainMenuManager
            SetPrivateField(menuMgr, "mainMenuPanel", mainPanel);
            SetPrivateField(menuMgr, "joinRoomDialogPanel", joinDialog);
            SetPrivateField(menuMgr, "lobbyRoomUI", lobbyUI);
            SetPrivateField(menuMgr, "settingsManager", settingsMgr);
            SetPrivateField(menuMgr, "howToPlayUI", codexUI);
            SetPrivateField(menuMgr, "gameBoardCanvas", gameplayHUD);

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

            // Ensure NetworkManager exists
            if (NetworkManager.Instance == null)
            {
                GameObject netObj = new GameObject("NetworkManager");
                netObj.AddComponent<NetworkManager>();
                netObj.AddComponent<NetworkTurnSync>();
            }

            // Hide gameplay HUD if provided
            if (gameplayHUD != null)
            {
                gameplayHUD.SetActive(false);
            }

            mainPanel.SetActive(true);
            return menuMgr;
        }

        private static GameObject CreateJoinRoomDialog(Transform parent)
        {
            GameObject dialog = CreateUIObject("JoinRoomDialog", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(dialog, new Color(0, 0, 0, 0.75f));

            GameObject box = CreateUIObject("Box", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 240);
            AddUIImage(box, new Color(0.12f, 0.15f, 0.22f));

            GameObject title = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 45);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);
            AddUIText(title, "<b>JOIN MULTIPLAYER ROOM</b>\n<size=12>Enter the 5-character Room Code</size>", 16, Color.white, TextAnchor.MiddleCenter);

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

            Button btnJoin = CreateModalButton("CONFIRM & JOIN", box.transform, new Vector2(-90, -60), new Color(0.2f, 0.6f, 0.35f));
            btnJoin.gameObject.name = "Btn_ConfirmJoin";
            btnJoin.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 36);

            Button btnCancel = CreateModalButton("CANCEL", box.transform, new Vector2(90, -60), new Color(0.45f, 0.45f, 0.5f));
            btnCancel.gameObject.name = "Btn_CancelJoin";
            btnCancel.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 36);

            dialog.SetActive(false);
            return dialog;
        }

        private static LobbyRoomUI SetupLobbyModal(Transform parent)
        {
            GameObject lobbyObj = CreateUIObject("LobbyRoomPanel", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(lobbyObj, new Color(0.06f, 0.08f, 0.12f, 0.98f));

            LobbyRoomUI lobbyUI = lobbyObj.AddComponent<LobbyRoomUI>();

            GameObject box = CreateUIObject("CenterBox", lobbyObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(860, 660);
            AddUIImage(box, new Color(0.1f, 0.12f, 0.17f));

            GameObject codeObj = CreateUIObject("RoomCodeText", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            codeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 45);
            codeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, -20);
            Text codeTxt = AddUIText(codeObj, "ROOM CODE: <color=#50fa7b><b>PANAH</b></color>", 22, Color.white, TextAnchor.MiddleCenter);

            Button btnCopy = CreateModalButton("COPY", box.transform, new Vector2(250, 305), new Color(0.2f, 0.4f, 0.6f));
            btnCopy.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 32);

            GameObject statusObj = CreateUIObject("Status", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 30);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -65);
            Text statusTxt = AddUIText(statusObj, "Waiting for players to connect and select Castaways...", 13, Color.cyan, TextAnchor.MiddleCenter);

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

            Button btnMateo = CreateModalButton("1. Dr. Mateo (Spanish)", box.transform, new Vector2(-300, -145), EraType.SpanishEra.GetEraColor() * 0.7f);
            btnMateo.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnTala = CreateModalButton("2. Tala (American)", box.transform, new Vector2(-100, -145), EraType.AmericanEra.GetEraColor() * 0.7f);
            btnTala.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnAndres = CreateModalButton("3. Andres (Japanese)", box.transform, new Vector2(100, -145), EraType.JapaneseEra.GetEraColor() * 0.7f);
            btnAndres.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

            Button btnMaya = CreateModalButton("4. Maya (Myth)", box.transform, new Vector2(300, -145), EraType.MythLegendEra.GetEraColor() * 0.7f);
            btnMaya.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 42);

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

        private static SettingsManager SetupSettingsModal(Transform parent)
        {
            GameObject modal = CreateUIObject("SettingsModal", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(modal, new Color(0, 0, 0, 0.85f));

            GameObject box = CreateUIObject("Box", modal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 560);
            AddUIImage(box, new Color(0.10f, 0.13f, 0.18f));

            SettingsManager settingsMgr = modal.AddComponent<SettingsManager>();

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

            Button testBtn = CreateModalButton("TEST PHOTON CLOUD CONNECTION", box.transform, new Vector2(-150, 85), new Color(0.2f, 0.55f, 0.75f));
            testBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 36);

            GameObject statusObj = CreateUIObject("StatusText", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 36);
            statusObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, 85);
            Text statusTxt = AddUIText(statusObj, "<color=#50fa7b>Ready</color>", 12, Color.white, TextAnchor.MiddleLeft);

            // Audio
            GameObject audioHeader = CreateUIObject("AudioHeader", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            audioHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 30);
            audioHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 35);
            AddUIText(audioHeader, "<b>AUDIO & DISPLAY:</b>", 14, Color.yellow, TextAnchor.MiddleLeft);

            GameObject volLabel = CreateUIObject("VolLabel", box.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            volLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 25);
            volLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            AddUIText(volLabel, "Master Volume: 80%  •  Music: 70%  •  SFX: 90%", 12, Color.white, TextAnchor.MiddleLeft);

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

        private static HowToPlayUI SetupHowToPlayModal(Transform parent)
        {
            GameObject modal = CreateUIObject("HowToPlayModal", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            AddUIImage(modal, new Color(0, 0, 0, 0.88f));

            GameObject box = CreateUIObject("Box", modal.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            AddUIImage(box, new Color(0.10f, 0.12f, 0.17f));

            HowToPlayUI codexUI = modal.AddComponent<HowToPlayUI>();

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

            GameObject titleObj = CreateUIObject("Title", box.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(740, 40);
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -65);
            Text titleTxt = AddUIText(titleObj, "<b>P.A.N.A.H.O.N. HISTORICAL CODEX</b>", 18, Color.yellow, TextAnchor.MiddleCenter);

            GameObject bodyObj = CreateUIObject("Body", box.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(35, 75);
            bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-35, -115);
            Text bodyTxt = AddUIText(bodyObj, "Loading codex lore...", 14, Color.white, TextAnchor.UpperLeft);

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

        private static void SetupPauseMenu(Transform parent, SettingsManager settingsMgr, HowToPlayUI codexUI)
        {
            GameObject pauseObj = CreateUIObject("PauseMenu", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
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

        // --- Helpers ---
        public static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            return obj;
        }

        public static Image AddUIImage(GameObject obj, Color color)
        {
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text AddUIText(GameObject obj, string text, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }

        public static Button CreateModalButton(string label, Transform parent, Vector2 anchoredPos, Color? color = null)
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

        public static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
