using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;
using Panahon.Player;
using Panahon.Board;
using Panahon.Cards;

namespace Panahon.Core
{
    public enum GameState
    {
        Initializing,
        RollingForTurnOrder,
        ActiveGameplay,
        TimelineCollapseLoss,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Initializing;

        [Header("References")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TimelineManager timelineManager;
        [SerializeField] private ParadoxManager paradoxManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private CardDatabase cardDatabase;

        [Header("Player Prefab / Roster")]
        [SerializeField] private List<PlayerController> players = new List<PlayerController>();

        public GameState CurrentState => currentState;
        public IReadOnlyList<PlayerController> Players => players;

        public event Action<GameState> OnGameStateChanged;

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
            InitializeGame();
        }

        public void InitializeGame()
        {
            currentState = GameState.Initializing;
            OnGameStateChanged?.Invoke(currentState);

            // Connect managers if not assigned in inspector
            if (boardManager == null) boardManager = FindFirstObjectByType<BoardManager>();
            if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
            if (timelineManager == null) timelineManager = FindFirstObjectByType<TimelineManager>();
            if (paradoxManager == null) paradoxManager = FindFirstObjectByType<ParadoxManager>();
            if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
            if (deckManager == null) deckManager = FindFirstObjectByType<DeckManager>();
            if (cardDatabase == null) cardDatabase = FindFirstObjectByType<CardDatabase>();

            // Subscribe to critical events
            if (timelineManager != null)
            {
                timelineManager.OnTimelineCollapse += HandleTimelineCollapse;
            }

            if (scoreManager != null)
            {
                scoreManager.OnTimeExpired += HandleTimeExpired;
            }

            // Initialize Card Decks
            if (deckManager != null)
            {
                deckManager.InitializeDecks();
            }

            // Setup Players if not manually placed
            SetupDefaultPlayers();

            // Deal unique mission cards to each player
            AssignInitialMissions();

            // Start First Turn Roll-Off
            StartCoroutine(StartTurnOrderSequence());
        }

        private void SetupDefaultPlayers()
        {
            if (players.Count == 0)
            {
                // Find any existing players in scene
                PlayerController[] found = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID);
                if (found.Length > 0)
                {
                    players.AddRange(found);
                }
            }

            // If still no players, instantiate 4 tokens
            if (players.Count == 0)
            {
                CreateDefaultPlayers();
            }

            if (turnManager != null)
            {
                turnManager.RegisterPlayers(players);
            }
        }

        private void CreateDefaultPlayers()
        {
            var char1 = ScriptableObject.CreateInstance<PlayerCharacterData>();
            char1.SetupCharacter(0, "Dr. Mateo Cruz", "Chrono-Archivist", new Color(0.2f, 0.6f, 1f),
                "Renowned historian specialized in Spanish colonial documents.", "Archival Mastery", "Restores +3 Energy and -1 Paradox.", 10, EraType.SpanishEra);

            var char2 = ScriptableObject.CreateInstance<PlayerCharacterData>();
            char2.SetupCharacter(1, "Tala Dimaculangan", "Rift-Walker", new Color(0.95f, 0.4f, 0.8f),
                "Quantum physicist attuned to temporal anomalies.", "Warp Harmonic", "Gains +4 Energy for Time Gate jumps.", 10, EraType.AmericanEra);

            var char3 = ScriptableObject.CreateInstance<PlayerCharacterData>();
            char3.SetupCharacter(2, "Capt. Andres Valiente", "Paradox Dampener", new Color(0.95f, 0.3f, 0.2f),
                "Veteran tactician dedicated to temporal containment.", "Timeline Anchor", "Directly reduces -2 Individual Paradox.", 10, EraType.JapaneseEra);

            var char4 = ScriptableObject.CreateInstance<PlayerCharacterData>();
            char4.SetupCharacter(3, "Maya Santos", "Quantum Scavenger", new Color(0.3f, 0.9f, 0.4f),
                "Tech salvager who refashions displaced relic energy.", "Temporal Salvage", "Gains +20 Points and +2 Energy.", 10, EraType.MythLegendEra);

            PlayerCharacterData[] characterConfigs = { char1, char2, char3, char4 };

            for (int i = 0; i < 4; i++)
            {
                GameObject pObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pObj.name = $"Player_{i + 1}_{characterConfigs[i].CharacterName.Replace(" ", "_")}";
                pObj.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);

                PlayerController controller = pObj.AddComponent<PlayerController>();

                BoardTile startTile = boardManager != null ? boardManager.GetStartTile(characterConfigs[i].StartingEra) : null;
                controller.Initialize(i, characterConfigs[i], startTile);
                players.Add(controller);
            }
        }

        private void AssignInitialMissions()
        {
            if (deckManager == null) return;

            for (int i = 0; i < players.Count; i++)
            {
                MissionCardData mission = deckManager.DrawMission();
                if (mission != null)
                {
                    players[i].Inventory.AssignMission(mission);
                    Debug.Log($"[GameManager] Assigned Mission to {players[i].CharacterData?.CharacterName}: {mission.Title} (Find: {mission.TargetArtifactName})");
                }
            }
        }

        private IEnumerator StartTurnOrderSequence()
        {
            currentState = GameState.RollingForTurnOrder;
            OnGameStateChanged?.Invoke(currentState);

            yield return new WaitForSeconds(0.5f);

            if (turnManager != null)
            {
                yield return turnManager.DetermineFirstTurnRoutine((firstPlayer, results) => {
                    currentState = GameState.ActiveGameplay;
                    OnGameStateChanged?.Invoke(currentState);

                    if (scoreManager != null) scoreManager.StartTimer();
                    turnManager.StartTurn();
                });
            }
        }

        private void HandleTimelineCollapse()
        {
            currentState = GameState.TimelineCollapseLoss;
            OnGameStateChanged?.Invoke(currentState);
            if (scoreManager != null) scoreManager.PauseTimer();

            Debug.LogError("[GameManager] DEFEAT! The Timeline has collapsed due to extreme temporal destabilization! All players have lost.");
        }

        private void HandleTimeExpired()
        {
            TriggerVictory();
        }

        public void TriggerVictory()
        {
            currentState = GameState.Victory;
            OnGameStateChanged?.Invoke(currentState);
            if (scoreManager != null) scoreManager.PauseTimer();

            Debug.Log("[GameManager] Game Concluded! Timeline preserved. Calculating individual scores.");
        }
    }
}
