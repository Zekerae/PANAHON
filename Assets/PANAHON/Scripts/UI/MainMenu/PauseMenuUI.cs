using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Panahon.UI.MainMenu
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private HowToPlayUI howToPlayUI;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitToMenuButton;

        private bool isPaused = false;

        private void Start()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
            if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(QuitToMainMenu);

            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (pausePanel != null) pausePanel.SetActive(isPaused);

            // If resuming, close subpanels
            if (!isPaused)
            {
                if (settingsManager != null) settingsManager.gameObject.SetActive(false);
                if (howToPlayUI != null) howToPlayUI.gameObject.SetActive(false);
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsManager != null) settingsManager.gameObject.SetActive(false);
            if (howToPlayUI != null) howToPlayUI.gameObject.SetActive(false);
        }

        public void OpenSettings()
        {
            if (settingsManager != null)
            {
                settingsManager.gameObject.SetActive(true);
                settingsManager.LoadSettings();
            }
        }

        public void OpenHowToPlay()
        {
            if (howToPlayUI != null)
            {
                howToPlayUI.gameObject.SetActive(true);
                howToPlayUI.ShowOverview();
            }
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            // If in SampleScene, we can either reload scene with menu open or switch scene
            var menuMgr = FindAnyObjectByType<MainMenuManager>();
            if (menuMgr != null)
            {
                ResumeGame();
                menuMgr.ShowMainMenuScreen();
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
