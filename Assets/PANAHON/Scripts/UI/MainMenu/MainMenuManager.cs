using System;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Networking;
using Panahon.Core;

namespace Panahon.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Main Menu Root Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject joinRoomDialogPanel;
        [SerializeField] private LobbyRoomUI lobbyRoomUI;
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private HowToPlayUI howToPlayUI;
        [SerializeField] private GameObject gameBoardCanvas;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playSoloButton;
        [SerializeField] private Button hostGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitGameButton;

        [Header("Join Room Dialog")]
        [SerializeField] private InputField roomCodeInputField;
        [SerializeField] private Button confirmJoinButton;
        [SerializeField] private Button cancelJoinButton;

        [Header("Status & Feedback")]
        [SerializeField] private Text notificationBannerText;

        private void Start()
        {
            BindButtons();

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnGameStartRequested += HandleGameStartRequested;
                NetworkManager.Instance.OnStatusMessage += ShowNotification;
            }

            ShowMainMenuScreen();
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnGameStartRequested -= HandleGameStartRequested;
                NetworkManager.Instance.OnStatusMessage -= ShowNotification;
            }
        }

        private void BindButtons()
        {
            if (playSoloButton != null) playSoloButton.onClick.AddListener(OnPlaySoloClicked);
            if (hostGameButton != null) hostGameButton.onClick.AddListener(OnHostGameClicked);
            if (joinGameButton != null) joinGameButton.onClick.AddListener(OnJoinGameClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            if (quitGameButton != null) quitGameButton.onClick.AddListener(OnQuitClicked);

            if (confirmJoinButton != null) confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
            if (cancelJoinButton != null) cancelJoinButton.onClick.AddListener(() => joinRoomDialogPanel.SetActive(false));
        }

        public void ShowMainMenuScreen()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (joinRoomDialogPanel != null) joinRoomDialogPanel.SetActive(false);
            if (lobbyRoomUI != null) lobbyRoomUI.gameObject.SetActive(false);
            if (settingsManager != null) settingsManager.gameObject.SetActive(false);
            if (howToPlayUI != null) howToPlayUI.gameObject.SetActive(false);

            ShowNotification("Welcome to P.A.N.A.H.O.N. - Choose a game mode to begin.");
        }

        private void OnPlaySoloClicked()
        {
            ShowNotification("Starting Solo / Hotseat Board Match with 4 Castaways...");
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.HostRoom("SOLO");
                NetworkManager.Instance.AutoFillRemainingSlotsWithAI();
                NetworkManager.Instance.StartGame();
            }
            else
            {
                LaunchGameBoard();
            }
        }

        private void OnHostGameClicked()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.HostRoom();
            }

            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (lobbyRoomUI != null) lobbyRoomUI.gameObject.SetActive(true);
        }

        private void OnJoinGameClicked()
        {
            if (joinRoomDialogPanel != null)
            {
                joinRoomDialogPanel.SetActive(true);
                if (roomCodeInputField != null)
                {
                    roomCodeInputField.text = "";
                    roomCodeInputField.Select();
                }
            }
        }

        private void OnConfirmJoinClicked()
        {
            string code = roomCodeInputField != null ? roomCodeInputField.text.Trim() : "";
            if (string.IsNullOrEmpty(code))
            {
                ShowNotification("Please enter a room code!");
                return;
            }

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.JoinRoom(code);
            }

            if (joinRoomDialogPanel != null) joinRoomDialogPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (lobbyRoomUI != null) lobbyRoomUI.gameObject.SetActive(true);
        }

        private void OnSettingsClicked()
        {
            if (settingsManager != null)
            {
                settingsManager.gameObject.SetActive(true);
                settingsManager.LoadSettings();
            }
        }

        private void OnHowToPlayClicked()
        {
            if (howToPlayUI != null)
            {
                howToPlayUI.gameObject.SetActive(true);
                howToPlayUI.ShowOverview();
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleGameStartRequested()
        {
            LaunchGameBoard();
        }

        public void LaunchGameBoard()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (lobbyRoomUI != null) lobbyRoomUI.gameObject.SetActive(false);
            if (settingsManager != null) settingsManager.gameObject.SetActive(false);
            if (howToPlayUI != null) howToPlayUI.gameObject.SetActive(false);

            if (gameBoardCanvas != null)
            {
                gameBoardCanvas.SetActive(true);
            }

            // Trigger GameManager initialize if present
            if (GameManager.Instance != null)
            {
                GameManager.Instance.InitializeGame();
            }
        }

        public void ShowNotification(string msg)
        {
            if (notificationBannerText != null)
            {
                notificationBannerText.text = msg;
            }
            Debug.Log($"[PANAHON Menu] {msg}");
        }
    }
}
