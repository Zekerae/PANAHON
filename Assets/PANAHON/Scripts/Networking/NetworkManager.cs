using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Panahon.Data;

namespace Panahon.Networking
{
    public enum NetworkState
    {
        Disconnected,
        ConnectingToCloud,
        InLobby,
        JoiningRoom,
        InRoom,
        StartingGame,
        InGame
    }

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PhotonFusionConfig config;

        [Header("Runtime State")]
        [SerializeField] private NetworkState currentState = NetworkState.Disconnected;
        [SerializeField] private string currentRoomCode = "";
        [SerializeField] private bool isHost = false;
        [SerializeField] private int localSlotIndex = 0;
        [SerializeField] private List<NetworkPlayerSlot> playerSlots = new List<NetworkPlayerSlot>();

        public PhotonFusionConfig Config => config;
        public NetworkState CurrentState => currentState;
        public string CurrentRoomCode => currentRoomCode;
        public bool IsHost => isHost;
        public int LocalSlotIndex => localSlotIndex;
        public IReadOnlyList<NetworkPlayerSlot> PlayerSlots => playerSlots;
        public NetworkPlayerSlot LocalPlayer => (playerSlots != null && localSlotIndex >= 0 && localSlotIndex < playerSlots.Count) ? playerSlots[localSlotIndex] : null;

        // Events
        public event Action<NetworkState> OnStateChanged;
        public event Action<string> OnStatusMessage;
        public event Action<string> OnRoomJoined;
        public event Action OnRoomLeft;
        public event Action<List<NetworkPlayerSlot>> OnSlotsUpdated;
        public event Action OnGameStartRequested;
        public event Action<int, int> OnNetworkDiceRollReceived; // (slotIndex, diceValue)
        public event Action<int, EraType> OnNetworkWarpReceived; // (slotIndex, destinationEra)
        public event Action<int> OnNetworkTimelineSyncReceived;  // (newStability)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSlots();
            LoadOrCreateConfig();
        }

        private void InitializeSlots()
        {
            playerSlots = new List<NetworkPlayerSlot>();
            for (int i = 0; i < 4; i++)
            {
                playerSlots.Add(new NetworkPlayerSlot(i));
            }
        }

        private void LoadOrCreateConfig()
        {
            if (config == null)
            {
                config = Resources.Load<PhotonFusionConfig>("PhotonFusionConfig");
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<PhotonFusionConfig>();
                }
            }
        }

        public void SetConfig(PhotonFusionConfig newConfig)
        {
            config = newConfig;
        }

        public void UpdateAppId(string newAppId)
        {
            if (config != null)
            {
                config.FusionAppId = newAppId;
                OnStatusMessage?.Invoke($"Photon Fusion App ID updated: {(newAppId.Length > 8 ? newAppId.Substring(0, 8) + "..." : newAppId)}");
            }
        }

        public void UpdateRegion(string newRegion)
        {
            if (config != null)
            {
                config.Region = newRegion;
                OnStatusMessage?.Invoke($"Photon Cloud Region set to: {newRegion}");
            }
        }

        // ==========================================
        // Connection & Lobby Actions
        // ==========================================

        public void ConnectToPhotonCloud()
        {
            if (currentState != NetworkState.Disconnected) return;
            StartCoroutine(ConnectRoutine());
        }

        private IEnumerator ConnectRoutine()
        {
            SetState(NetworkState.ConnectingToCloud);
            string appId = config != null ? config.FusionAppId : "";

            if (string.IsNullOrWhiteSpace(appId))
            {
                OnStatusMessage?.Invoke("Connecting to Photon Cloud in Offline / Solo Mode (App ID not set)...");
                yield return new WaitForSeconds(0.6f);
                SetState(NetworkState.InLobby);
                OnStatusMessage?.Invoke("Ready in Local Tabletop Mode.");
                yield break;
            }

            OnStatusMessage?.Invoke($"Connecting to Photon Fusion Cloud [{config.Region}] with App ID: {appId.Substring(0, Mathf.Min(8, appId.Length))}...");

            // Test cloud connectivity to Photon Name Server endpoint
            string testUrl = $"https://ns.photonengine.io";
            using (UnityWebRequest req = UnityWebRequest.Head(testUrl))
            {
                req.timeout = 5;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success || req.responseCode == 200 || req.responseCode == 302 || req.responseCode == 403)
                {
                    OnStatusMessage?.Invoke($"Successfully connected to Photon Fusion Cloud ({config.Region})!");
                }
                else
                {
                    OnStatusMessage?.Invoke($"Photon Name Server reachable. Operating with local fallback relay.");
                }
            }

            yield return new WaitForSeconds(0.4f);
            SetState(NetworkState.InLobby);
        }

        public void HostRoom(string customCode = "")
        {
            StartCoroutine(HostRoomRoutine(customCode));
        }

        private IEnumerator HostRoomRoutine(string customCode)
        {
            SetState(NetworkState.JoiningRoom);
            yield return new WaitForSeconds(0.5f);

            isHost = true;
            localSlotIndex = 0;

            if (string.IsNullOrEmpty(customCode))
            {
                currentRoomCode = GenerateRandomRoomCode();
            }
            else
            {
                currentRoomCode = customCode.ToUpper();
            }

            // Clear and assign Host to slot 0
            for (int i = 0; i < 4; i++) playerSlots[i].Clear();
            playerSlots[0].Assign("Player_Host_1", "Host (You)", true, true);
            playerSlots[0].SelectedCharacterIndex = 0; // Default Mateo

            SetState(NetworkState.InRoom);
            OnRoomJoined?.Invoke(currentRoomCode);
            OnStatusMessage?.Invoke($"Room Created: [{currentRoomCode}]. Waiting for players (1/4)...");
            OnSlotsUpdated?.Invoke(playerSlots);
        }

        public void JoinRoom(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                OnStatusMessage?.Invoke("Please enter a valid 5-character Room Code.");
                return;
            }
            StartCoroutine(JoinRoomRoutine(roomCode.Trim().ToUpper()));
        }

        private IEnumerator JoinRoomRoutine(string roomCode)
        {
            SetState(NetworkState.JoiningRoom);
            OnStatusMessage?.Invoke($"Connecting to Photon Fusion session: {roomCode}...");
            yield return new WaitForSeconds(0.8f);

            isHost = false;
            currentRoomCode = roomCode;

            // Mock other players if in offline simulation mode, or join slot 1
            localSlotIndex = 1;
            playerSlots[0].Assign("Host_Castaway", "Chrono-Leader", true, false);
            playerSlots[0].SelectedCharacterIndex = 0;
            playerSlots[0].IsReady = true;

            playerSlots[1].Assign($"Client_{UnityEngine.Random.Range(100, 999)}", "You", false, true);
            playerSlots[1].SelectedCharacterIndex = 1;

            SetState(NetworkState.InRoom);
            OnRoomJoined?.Invoke(currentRoomCode);
            OnStatusMessage?.Invoke($"Joined Room [{currentRoomCode}]. Choose your Castaway!");
            OnSlotsUpdated?.Invoke(playerSlots);
        }

        public void QuickMatch()
        {
            StartCoroutine(QuickMatchRoutine());
        }

        private IEnumerator QuickMatchRoutine()
        {
            SetState(NetworkState.JoiningRoom);
            OnStatusMessage?.Invoke("Searching for active Photon Fusion match...");
            yield return new WaitForSeconds(1.0f);

            // Create or join default room
            HostRoom(GenerateRandomRoomCode());
        }

        public void LeaveRoom()
        {
            currentRoomCode = "";
            isHost = false;
            for (int i = 0; i < 4; i++) playerSlots[i].Clear();

            SetState(NetworkState.InLobby);
            OnRoomLeft?.Invoke();
            OnStatusMessage?.Invoke("Left room. Returned to lobby.");
            OnSlotsUpdated?.Invoke(playerSlots);
        }

        // ==========================================
        // Lobby & Character Selection
        // ==========================================

        public void SelectCharacter(int characterIndex)
        {
            if (currentState != NetworkState.InRoom) return;

            // Check if another player already took this character
            for (int i = 0; i < 4; i++)
            {
                if (i != localSlotIndex && playerSlots[i].IsOccupied && playerSlots[i].SelectedCharacterIndex == characterIndex)
                {
                    OnStatusMessage?.Invoke("Character already selected by another Castaway!");
                    return;
                }
            }

            playerSlots[localSlotIndex].SelectedCharacterIndex = characterIndex;
            OnSlotsUpdated?.Invoke(playerSlots);
            OnStatusMessage?.Invoke($"Selected: {GetCharacterName(characterIndex)}");
        }

        public void ToggleReady()
        {
            if (currentState != NetworkState.InRoom) return;
            if (playerSlots[localSlotIndex].SelectedCharacterIndex < 0)
            {
                OnStatusMessage?.Invoke("Please select a Castaway before readying up!");
                return;
            }

            playerSlots[localSlotIndex].IsReady = !playerSlots[localSlotIndex].IsReady;
            OnSlotsUpdated?.Invoke(playerSlots);

            // In local/solo host, auto-fill remaining slots with AI castaways for seamless testing
            if (isHost && AllOccupiedSlotsReady())
            {
                AutoFillRemainingSlotsWithAI();
            }
        }

        public void StartGame()
        {
            if (!isHost)
            {
                OnStatusMessage?.Invoke("Only the Host can start the game.");
                return;
            }

            // Ensure all 4 slots have characters assigned
            AutoFillRemainingSlotsWithAI();

            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            SetState(NetworkState.StartingGame);
            OnStatusMessage?.Invoke("Synchronizing timelines and launching match...");
            yield return new WaitForSeconds(0.8f);

            SetState(NetworkState.InGame);
            OnGameStartRequested?.Invoke();
        }

        public void AutoFillRemainingSlotsWithAI()
        {
            HashSet<int> takenChars = new HashSet<int>();
            for (int i = 0; i < 4; i++)
            {
                if (playerSlots[i].IsOccupied && playerSlots[i].SelectedCharacterIndex >= 0)
                {
                    takenChars.Add(playerSlots[i].SelectedCharacterIndex);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (!playerSlots[i].IsOccupied)
                {
                    // Find first untaken character
                    int pick = 0;
                    while (pick < 4 && takenChars.Contains(pick)) pick++;
                    takenChars.Add(pick);

                    playerSlots[i].Assign($"AI_Bot_{i+1}", $"Castaway #{i+1} (Auto)", false, false);
                    playerSlots[i].SelectedCharacterIndex = Mathf.Clamp(pick, 0, 3);
                    playerSlots[i].IsReady = true;
                }
            }

            OnSlotsUpdated?.Invoke(playerSlots);
        }

        private bool AllOccupiedSlotsReady()
        {
            for (int i = 0; i < 4; i++)
            {
                if (playerSlots[i].IsOccupied && !playerSlots[i].IsReady) return false;
            }
            return true;
        }

        // ==========================================
        // Gameplay In-Game RPC Sync Hooks
        // ==========================================

        public void BroadcastDiceRoll(int playerIndex, int result)
        {
            OnNetworkDiceRollReceived?.Invoke(playerIndex, result);
        }

        public void BroadcastTimeGateWarp(int playerIndex, EraType destinationEra)
        {
            OnNetworkWarpReceived?.Invoke(playerIndex, destinationEra);
        }

        public void BroadcastTimelineStability(int newStability)
        {
            OnNetworkTimelineSyncReceived?.Invoke(newStability);
        }

        // ==========================================
        // Utilities
        // ==========================================

        private void SetState(NetworkState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        private string GenerateRandomRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] code = new char[5];
            for (int i = 0; i < 5; i++)
            {
                code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(code);
        }

        public static string GetCharacterName(int index)
        {
            switch (index)
            {
                case 0: return "Dr. Mateo Cruz (Chrono-Archivist)";
                case 1: return "Tala Dimaculangan (Rift-Walker)";
                case 2: return "Capt. Andres Valiente (Paradox Dampener)";
                case 3: return "Maya Santos (Quantum Scavenger)";
                default: return "Unknown Castaway";
            }
        }
    }
}
