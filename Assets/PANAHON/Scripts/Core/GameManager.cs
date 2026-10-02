using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;
using Panahon.Player;
using Panahon.Board;
using Panahon.Cards;
using Panahon.UI;
using Panahon.UI.MainMenu;

namespace Panahon.Core
{
    public enum GameState
    {
        Initializing,
        RollingForTurnOrder,
        ActiveGameplay,
        Paused,
        TimelineCollapseLoss,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Initializing;

        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

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
        public GameConfig Config => gameConfig;

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

        [Header("Startup Flow")]
        [SerializeField] private bool autoStartGame = false;

        private void Start()
        {
            var menuMgr = FindAnyObjectByType<MainMenuManager>();
            if (menuMgr == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    var uiMgr = FindAnyObjectByType<UIManager>();
                    if (uiMgr != null)
                    {
                        uiMgr.SetGameplayHUDVisible(false);
                    }
                    menuMgr = MainMenuUIBuilder.BuildMenuHierarchy(canvas);
                }
            }

            // Only auto-start if configured and no menu is active
            if (autoStartGame && menuMgr == null)
            {
                InitializeGame();
            }
        }

        public void InitializeGame()
        {
            currentState = GameState.Initializing;
            OnGameStateChanged?.Invoke(currentState);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetGameplayHUDVisible(true);
            }

            // Try to load config from Resources if not assigned
            if (gameConfig == null)
            {
                gameConfig = Resources.Load<GameConfig>("PanahonGameConfig");
            }

            // Connect managers if not assigned in inspector
            if (boardManager == null) boardManager = FindAnyObjectByType<BoardManager>();
            if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();
            if (timelineManager == null) timelineManager = FindAnyObjectByType<TimelineManager>();
            if (paradoxManager == null) paradoxManager = FindAnyObjectByType<ParadoxManager>();
            if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();
            if (deckManager == null) deckManager = FindAnyObjectByType<DeckManager>();
            if (cardDatabase == null) cardDatabase = FindAnyObjectByType<CardDatabase>();

            // Subscribe to critical events
            if (timelineManager != null)
            {
                timelineManager.OnTimelineCollapse -= HandleTimelineCollapse;
                timelineManager.OnTimelineCollapse += HandleTimelineCollapse;
            }

            if (scoreManager != null)
            {
                scoreManager.OnTimeExpired -= HandleTimeExpired;
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

            Debug.Log("[GameManager] P.A.N.A.H.O.N. game initialized. All systems online.");

            // Start First Turn Roll-Off
            StartCoroutine(StartTurnOrderSequence());
        }

        private void OnDestroy()
        {
            if (timelineManager != null)
            {
                timelineManager.OnTimelineCollapse -= HandleTimelineCollapse;
            }
            if (scoreManager != null)
            {
                scoreManager.OnTimeExpired -= HandleTimeExpired;
            }
        }

        private void SetupDefaultPlayers()
        {
            if (players.Count == 0)
            {
                // Find any existing players in scene
                PlayerController[] found = FindObjectsByType<PlayerController>();
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
                // 2.5D: Use a flat quad sprite token instead of 3D cylinder
                GameObject pObj = new GameObject($"Player_{i + 1}_{characterConfigs[i].CharacterName.Replace(" ", "_")}");

                // Add a SpriteRenderer for 2.5D visual
                SpriteRenderer sr = pObj.AddComponent<SpriteRenderer>();
                sr.color = characterConfigs[i].TokenColor;
                sr.sortingOrder = 10 + i; // Players render above tiles

                // Create a simple circular sprite token procedurally
                Texture2D tex = CreateCircleTexture(64, characterConfigs[i].TokenColor);
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
                pObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

                PlayerController controller = pObj.AddComponent<PlayerController>();

                BoardTile startTile = boardManager != null ? boardManager.GetStartTile(characterConfigs[i].StartingEra) : null;
                controller.Initialize(i, characterConfigs[i], startTile);
                players.Add(controller);
            }
        }

        /// <summary>
        /// Creates a simple filled circle texture for 2.5D player tokens.
        /// </summary>
        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;
            float radiusSq = radius * radius;
            Color border = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float distSq = dx * dx + dy * dy;

                    if (distSq < (radius - 2) * (radius - 2))
                    {
                        // Inner fill with slight gradient
                        float grad = 1f - (distSq / radiusSq) * 0.3f;
                        tex.SetPixel(x, y, color * grad);
                    }
                    else if (distSq < radiusSq)
                    {
                        // Border ring
                        tex.SetPixel(x, y, border);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return tex;
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
            if (currentState == GameState.TimelineCollapseLoss) return; // Prevent duplicate
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
            if (currentState == GameState.Victory) return; // Prevent duplicate
            currentState = GameState.Victory;
            OnGameStateChanged?.Invoke(currentState);
            if (scoreManager != null) scoreManager.PauseTimer();

            Debug.Log("[GameManager] Game Concluded! Timeline preserved. Calculating individual scores.");
        }

        public void PauseGame()
        {
            if (currentState == GameState.ActiveGameplay)
            {
                currentState = GameState.Paused;
                OnGameStateChanged?.Invoke(currentState);
                if (scoreManager != null) scoreManager.PauseTimer();
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                currentState = GameState.ActiveGameplay;
                OnGameStateChanged?.Invoke(currentState);
                if (scoreManager != null) scoreManager.StartTimer();
                Time.timeScale = 1f;
            }
        }
    }
}
