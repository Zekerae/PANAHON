using System;
using UnityEngine;

namespace Panahon.Networking
{
    [Serializable]
    public class NetworkPlayerSlot
    {
        public int SlotIndex;
        public string PlayerId;
        public string PlayerName;
        public int SelectedCharacterIndex = -1; // 0: Mateo, 1: Tala, 2: Andres, 3: Maya
        public bool IsReady;
        public bool IsHost;
        public bool IsLocal;
        public int PingMs = 35;

        public bool IsOccupied => !string.IsNullOrEmpty(PlayerId);

        public NetworkPlayerSlot(int slotIndex)
        {
            SlotIndex = slotIndex;
            PlayerId = string.Empty;
            PlayerName = $"Slot {slotIndex + 1}";
            SelectedCharacterIndex = -1;
            IsReady = false;
            IsHost = false;
            IsLocal = false;
        }

        public void Assign(string id, string name, bool isHost, bool isLocal)
        {
            PlayerId = id;
            PlayerName = name;
            IsHost = isHost;
            IsLocal = isLocal;
            IsReady = false;
            SelectedCharacterIndex = -1;
        }

        public void Clear()
        {
            PlayerId = string.Empty;
            PlayerName = $"Slot {SlotIndex + 1}";
            SelectedCharacterIndex = -1;
            IsReady = false;
            IsHost = false;
            IsLocal = false;
        }
    }
}
