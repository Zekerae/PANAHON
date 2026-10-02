using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Core;
using Panahon.Player;
using Panahon.Data;

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

        public void SetGameplayHUDVisible(bool visible)
        {
            if (timelineMeterUI != null && timelineMeterUI.transform.parent != null)
            {
                timelineMeterUI.transform.parent.gameObject.SetActive(visible);
            }
            if (diceRollerUI != null) diceRollerUI.gameObject.SetActive(visible);
            if (actionLogUI != null) actionLogUI.gameObject.SetActive(visible);
            if (playerHUDCards != null)
            {
                foreach (var card in playerHUDCards)
                {
                    if (card != null) card.gameObject.SetActive(visible);
                }
            }
        }

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
            // Subscribe to turn events
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted += HandleTurnStarted;
                TurnManager.Instance.OnTurnEnded += HandleTurnEnded;
                TurnManager.Instance.OnRoundChanged += HandleRoundChanged;
            }

            // Subscribe to timer events
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTimerTick += HandleTimerTick;
            }

            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            BindAllPlayers();
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted -= HandleTurnStarted;
                TurnManager.Instance.OnTurnEnded -= HandleTurnEnded;
                TurnManager.Instance.OnRoundChanged -= HandleRoundChanged;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTimerTick -= HandleTimerTick;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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

            if (turnStatusBannerText != null)
            {
                turnStatusBannerText.text = $"Current Turn: {activePlayer.CharacterData?.CharacterName} ({activePlayer.CurrentEra.GetDisplayName()})";
            }

            // Highlight active player's HUD card
            for (int i = 0; i < playerHUDCards.Count; i++)
            {
                if (playerHUDCards[i] != null)
                {
                    playerHUDCards[i].SetActiveTurn(i == activePlayer.PlayerIndex);
                }
            }
        }

        private void HandleTurnEnded(PlayerController player)
        {
            // Deactivate all turn highlights
            for (int i = 0; i < playerHUDCards.Count; i++)
            {
                if (playerHUDCards[i] != null)
                {
                    playerHUDCards[i].SetActiveTurn(false);
                }
            }
        }

        private void HandleRoundChanged(int roundNumber)
        {
            if (roundText != null)
            {
                roundText.text = $"Time: {ScoreManager.Instance?.FormattedTimeRemaining ?? "—"}\n<size=13>Round {roundNumber}</size>";
            }
        }

        private void HandleTimerTick(float remainingSeconds)
        {
            if (gameTimerText != null && ScoreManager.Instance != null)
            {
                int roundNum = TurnManager.Instance != null ? TurnManager.Instance.CurrentRoundNumber : 1;
                gameTimerText.text = $"Time: {ScoreManager.Instance.FormattedTimeRemaining}\n<size=13>Round {roundNum}</size>";
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Disable dice roller during non-gameplay states
            if (diceRollerUI != null)
            {
                diceRollerUI.gameObject.SetActive(state == GameState.ActiveGameplay || state == GameState.RollingForTurnOrder);
            }
        }
    }
}
