using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Core;
using Panahon.Player;

namespace Panahon.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Header Elements")]
        [SerializeField] private Text gameTimerText;
        [SerializeField] private Text roundText;
        [SerializeField] private Text turnStatusBannerText;

        [Header("Sub-Panels")]
        [SerializeField] private TimelineMeterUI timelineMeterUI;
        [SerializeField] private DiceRollerUI diceRollerUI;
        [SerializeField] private TimeGateDialogUI timeGateDialogUI;
        [SerializeField] private CardModalUI cardModalUI;
        [SerializeField] private ActionLogUI actionLogUI;
        [SerializeField] private GameOverModalUI gameOverModalUI;

        [Header("Player HUD Panels")]
        [SerializeField] private List<PlayerHUDCard> playerHUDCards = new List<PlayerHUDCard>();

        public TimelineMeterUI TimelineMeterUI => timelineMeterUI;
        public DiceRollerUI DiceRollerUI => diceRollerUI;
        public TimeGateDialogUI TimeGateDialogUI => timeGateDialogUI;
        public CardModalUI CardModalUI => cardModalUI;
        public ActionLogUI ActionLogUI => actionLogUI;
        public GameOverModalUI GameOverModalUI => gameOverModalUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted += HandleTurnStarted;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTimerTick += HandleTimerTick;
            }

            BindAllPlayers();
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted -= HandleTurnStarted;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTimerTick -= HandleTimerTick;
            }
        }

        public void BindAllPlayers()
        {
            var players = GameManager.Instance != null ? GameManager.Instance.Players : null;
            if (players == null || players.Count == 0) return;

            for (int i = 0; i < playerHUDCards.Count && i < players.Count; i++)
            {
                if (playerHUDCards[i] != null && players[i] != null)
                {
                    playerHUDCards[i].BindPlayer(players[i]);
                }
            }
        }

        private void HandleTurnStarted(PlayerController activePlayer)
        {
            if (activePlayer == null) return;

            if (roundText != null && TurnManager.Instance != null)
            {
                roundText.text = $"Round {TurnManager.Instance.CurrentRoundNumber}";
            }

            if (turnStatusBannerText != null)
            {
                turnStatusBannerText.text = $"Current Turn: {activePlayer.CharacterData?.CharacterName} ({activePlayer.CurrentEra})";
            }

            for (int i = 0; i < playerHUDCards.Count; i++)
            {
                if (playerHUDCards[i] != null)
                {
                    playerHUDCards[i].SetActiveTurn(i == activePlayer.PlayerIndex);
                }
            }
        }

        private void HandleTimerTick(float remainingSeconds)
        {
            if (gameTimerText != null && ScoreManager.Instance != null)
            {
                gameTimerText.text = $"Time: {ScoreManager.Instance.FormattedTimeRemaining}";
            }
        }
    }
}
