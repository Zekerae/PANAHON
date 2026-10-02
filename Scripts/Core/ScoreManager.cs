using System;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Player;

namespace Panahon.Core
{
    [Serializable]
    public struct PlayerScoreReport
    {
        public string CharacterName;
        public int MissionPoints;
        public int ArtifactPoints;
        public int CluePoints;
        public int EnergyPoints;
        public int ParadoxPenalty;
        public int TimelineStabilityBonus;
        public int TotalScore;
        public bool CompletedMission;
    }

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Game Time Limit")]
        [SerializeField] private float gameTimeLimitMinutes = 40f;
        [SerializeField] private float remainingTimeSeconds = 2400f;
        [SerializeField] private bool isTimerRunning = false;

        public float RemainingTimeSeconds => remainingTimeSeconds;
        public string FormattedTimeRemaining
        {
            get
            {
                int minutes = Mathf.FloorToInt(remainingTimeSeconds / 60f);
                int seconds = Mathf.FloorToInt(remainingTimeSeconds % 60f);
                return $"{minutes:00}:{seconds:00}";
            }
        }

        public event Action<float> OnTimerTick;
        public event Action OnTimeExpired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            remainingTimeSeconds = gameTimeLimitMinutes * 60f;
        }

        public void StartTimer()
        {
            isTimerRunning = true;
        }

        public void PauseTimer()
        {
            isTimerRunning = false;
        }

        private void Update()
        {
            if (!isTimerRunning) return;

            if (remainingTimeSeconds > 0)
            {
                remainingTimeSeconds -= Time.deltaTime;
                OnTimerTick?.Invoke(remainingTimeSeconds);

                if (remainingTimeSeconds <= 0f)
                {
                    remainingTimeSeconds = 0f;
                    isTimerRunning = false;
                    Debug.Log("[ScoreManager] Time limit reached (40 minutes)! Calculating final scores.");
                    OnTimeExpired?.Invoke();
                }
            }
        }

        public PlayerScoreReport CalculateReport(PlayerController player)
        {
            PlayerScoreReport report = new PlayerScoreReport();
            if (player == null) return report;

            report.CharacterName = player.CharacterData != null ? player.CharacterData.CharacterName : $"Player {player.PlayerIndex + 1}";
            report.CompletedMission = player.Inventory.IsMissionComplete();

            // 1. Mission Completion Points (50 pts)
            report.MissionPoints = report.CompletedMission ? 50 : 0;

            // 2. Correct Artifacts Returned (30 pts each)
            report.ArtifactPoints = player.Inventory.ReturnedArtifacts.Count * 30;

            // 3. Clues collected (5 pts each)
            report.CluePoints = player.Inventory.CollectedClues.Count * 5;

            // 4. Remaining Energy (2 pts each)
            report.EnergyPoints = player.CurrentEnergy * 2;

            // 5. Individual Paradox penalty (-10 pts per paradox point)
            report.ParadoxPenalty = player.IndividualParadox * 10;

            // 6. Cooperative timeline bonus
            int timelineMeter = TimelineManager.Instance != null ? TimelineManager.Instance.CurrentTimelineMeter : 0;
            if (timelineMeter <= 3) report.TimelineStabilityBonus = 20;
            else if (timelineMeter <= 6) report.TimelineStabilityBonus = 10;
            else report.TimelineStabilityBonus = 0;

            report.TotalScore = Mathf.Max(0,
                report.MissionPoints +
                report.ArtifactPoints +
                report.CluePoints +
                report.EnergyPoints +
                report.TimelineStabilityBonus -
                report.ParadoxPenalty);

            return report;
        }

        public List<PlayerScoreReport> GenerateAllReports(IReadOnlyList<PlayerController> players)
        {
            List<PlayerScoreReport> reports = new List<PlayerScoreReport>();
            foreach (var p in players)
            {
                reports.Add(CalculateReport(p));
            }
            // Sort by highest score first
            reports.Sort((a, b) => b.TotalScore.CompareTo(a.TotalScore));
            return reports;
        }
    }
}
