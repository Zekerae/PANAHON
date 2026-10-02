using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Panahon.Core;
using Panahon.Player;

namespace Panahon.UI
{
    public class GameOverModalUI : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Image bannerImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text scoreboardText;
        [SerializeField] private Button restartButton;

        [Header("Colors")]
        [SerializeField] private Color victoryColor = new Color(0.15f, 0.65f, 0.95f);
        [SerializeField] private Color defeatColor = new Color(0.85f, 0.15f, 0.15f);

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.TimelineCollapseLoss)
            {
                ShowDefeat();
            }
            else if (state == GameState.Victory)
            {
                ShowVictory();
            }
        }

        public void ShowDefeat()
        {
            if (bannerImage != null) bannerImage.color = defeatColor;
            if (titleText != null) titleText.text = "TIMELINE COLLAPSE";
            if (subtitleText != null)
            {
                subtitleText.text = "The shared Timeline Meter reached maximum instability! " +
                                    "A catastrophic temporal paradox has erased the spacetime continuum. ALL PLAYERS LOSE.";
            }

            if (scoreboardText != null)
            {
                scoreboardText.text = "<b>FINAL STATUS: COOPERATIVE TOTAL DEFEAT</b>\n\n" +
                                      "Neither personal points nor incomplete missions could save history from collapsing.\n" +
                                      "Remember: Balance personal ambition with timeline preservation!";
            }

            if (modalPanel != null) modalPanel.SetActive(true);
        }

        public void ShowVictory()
        {
            if (bannerImage != null) bannerImage.color = victoryColor;
            if (titleText != null) titleText.text = "TIMELINE RESTORED!";
            if (subtitleText != null)
            {
                subtitleText.text = "The timeline was successfully preserved! Calculating individual score tallies...";
            }

            if (scoreboardText != null && ScoreManager.Instance != null && GameManager.Instance != null)
            {
                List<PlayerScoreReport> reports = ScoreManager.Instance.GenerateAllReports(GameManager.Instance.Players);
                string text = "<b>FINAL LEADERBOARD</b>\n\n";

                for (int i = 0; i < reports.Count; i++)
                {
                    var r = reports[i];
                    string medal = i == 0 ? "[★ WINNER ★] " : $"#{i + 1} ";
                    string missionStatus = r.CompletedMission ? "Mission Complete (+50)" : "Mission Incomplete";

                    text += $"{medal}<b>{r.CharacterName}</b> - <color=#FFDD44>{r.TotalScore} PTS</color>\n";
                    text += $"   • {missionStatus} | Artifacts: +{r.ArtifactPoints} | Clues: +{r.CluePoints}\n";
                    text += $"   • Energy Bonus: +{r.EnergyPoints} | Paradox Penalty: -{r.ParadoxPenalty} | Timeline Bonus: +{r.TimelineStabilityBonus}\n\n";
                }

                scoreboardText.text = text;
            }

            if (modalPanel != null) modalPanel.SetActive(true);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Hide()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }
    }
}
