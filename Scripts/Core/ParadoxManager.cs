using System;
using UnityEngine;
using Panahon.Player;

namespace Panahon.Core
{
    public class ParadoxManager : MonoBehaviour
    {
        public static ParadoxManager Instance { get; private set; }

        public const int MaxParadox = 5;

        public event Action<PlayerController, int> OnParadoxPenalized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ApplyParadox(PlayerController player, int amount, string reason = "")
        {
            if (player == null) return;

            player.AddParadox(amount);
            Debug.Log($"[ParadoxManager] {player.CharacterData?.CharacterName} received {amount} Paradox ({reason}). Level: {player.IndividualParadox}/{MaxParadox}");

            // Check for high-paradox penalty
            if (player.IndividualParadox >= MaxParadox)
            {
                Debug.LogWarning($"[ParadoxManager] {player.CharacterData?.CharacterName} reached MAXIMUM PARADOX! Suffers temporal distortion!");
                player.SkipNextTurn = true;
                player.SpendEnergy(3);
                OnParadoxPenalized?.Invoke(player, player.IndividualParadox);

                // Severe paradox also destabilizes shared timeline
                if (TimelineManager.Instance != null)
                {
                    TimelineManager.Instance.IncreaseDestabilization(1, $"{player.CharacterData?.CharacterName} paradox overflow");
                }
            }
        }

        public void ReduceParadox(PlayerController player, int amount)
        {
            if (player == null) return;
            player.ReduceParadox(amount);
        }
    }
}
