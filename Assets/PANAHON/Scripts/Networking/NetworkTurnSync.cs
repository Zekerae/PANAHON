using System;
using UnityEngine;
using Panahon.Core;
using Panahon.Data;

namespace Panahon.Networking
{
    public class NetworkTurnSync : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TimelineManager timelineManager;

        private void Start()
        {
            if (turnManager == null) turnManager = TurnManager.Instance;
            if (timelineManager == null) timelineManager = TimelineManager.Instance;

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnNetworkDiceRollReceived += HandleRemoteDiceRoll;
                NetworkManager.Instance.OnNetworkWarpReceived += HandleRemoteWarp;
                NetworkManager.Instance.OnNetworkTimelineSyncReceived += HandleRemoteTimelineSync;
            }

            if (turnManager != null)
            {
                turnManager.OnDiceRolled += HandleLocalDiceRolled;
            }

            if (timelineManager != null)
            {
                timelineManager.OnTimelineChanged += HandleLocalTimelineChanged;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnNetworkDiceRollReceived -= HandleRemoteDiceRoll;
                NetworkManager.Instance.OnNetworkWarpReceived -= HandleRemoteWarp;
                NetworkManager.Instance.OnNetworkTimelineSyncReceived -= HandleRemoteTimelineSync;
            }

            if (turnManager != null)
            {
                turnManager.OnDiceRolled -= HandleLocalDiceRolled;
            }

            if (timelineManager != null)
            {
                timelineManager.OnTimelineChanged -= HandleLocalTimelineChanged;
            }
        }

        private void HandleLocalDiceRolled(int roll)
        {
            if (NetworkManager.Instance != null && turnManager != null)
            {
                NetworkManager.Instance.BroadcastDiceRoll(turnManager.ActivePlayerIndex, roll);
            }
        }

        private void HandleLocalTimelineChanged(int newStability, string reason)
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsHost)
            {
                NetworkManager.Instance.BroadcastTimelineStability(newStability);
            }
        }

        private void HandleRemoteDiceRoll(int playerIndex, int roll)
        {
            // If local player isn't active player, apply remote roll
            if (turnManager != null && turnManager.ActivePlayerIndex == playerIndex)
            {
                Debug.Log($"[NetworkTurnSync] Remote player {playerIndex} rolled: {roll}");
            }
        }

        private void HandleRemoteWarp(int playerIndex, EraType destinationEra)
        {
            Debug.Log($"[NetworkTurnSync] Remote player {playerIndex} warped to {destinationEra}");
        }

        private void HandleRemoteTimelineSync(int newStability)
        {
            if (timelineManager != null && NetworkManager.Instance != null && !NetworkManager.Instance.IsHost)
            {
                Debug.Log($"[NetworkTurnSync] Timeline stability synced from host: {newStability}/10");
            }
        }
    }
}
