using UnityEngine;
using UnityEngine.UI;
using Panahon.Player;
using Panahon.Core;

namespace Panahon.UI
{
    public class PlayerHUDCard : MonoBehaviour
    {
        [Header("Card UI References")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerRoleText;
        [SerializeField] private Image characterColorBanner;
        [SerializeField] private Text eraLocationText;
        [SerializeField] private Text energyText;
        [SerializeField] private Slider energySlider;
        [SerializeField] private Text paradoxText;
        [SerializeField] private Slider paradoxSlider;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text missionText;
        [SerializeField] private Text cluesCounterText;
        [SerializeField] private Button abilityButton;
        [SerializeField] private Text abilityButtonText;
        [SerializeField] private GameObject activeTurnHighlight;

        private PlayerController boundPlayer;

        public void BindPlayer(PlayerController player)
        {
            boundPlayer = player;
            if (boundPlayer == null) return;

            boundPlayer.OnStatsChanged += RefreshHUD;
            if (abilityButton != null)
            {
                abilityButton.onClick.RemoveAllListeners();
                abilityButton.onClick.AddListener(OnAbilityClicked);
            }

            RefreshHUD(boundPlayer);
        }

        private void OnDestroy()
        {
            if (boundPlayer != null)
            {
                boundPlayer.OnStatsChanged -= RefreshHUD;
            }
        }

        public void SetActiveTurn(bool isActive)
        {
            if (activeTurnHighlight != null)
            {
                activeTurnHighlight.SetActive(isActive);
            }

            if (abilityButton != null)
            {
                abilityButton.interactable = isActive && boundPlayer != null && boundPlayer.CanUseAbility();
            }
        }

        public void RefreshHUD(PlayerController player)
        {
            if (player == null) return;

            if (playerNameText != null)
                playerNameText.text = player.CharacterData != null ? player.CharacterData.CharacterName : $"Player {player.PlayerIndex + 1}";

            if (playerRoleText != null)
                playerRoleText.text = player.CharacterData != null ? player.CharacterData.RoleTitle : "Temporal Castaway";

            if (characterColorBanner != null && player.CharacterData != null)
                characterColorBanner.color = player.CharacterData.TokenColor;

            if (eraLocationText != null)
                eraLocationText.text = $"Era: {player.CurrentEra}";

            if (energyText != null)
                energyText.text = $"Energy: {player.CurrentEnergy}/{player.MaxEnergy}";

            if (energySlider != null)
                energySlider.value = (float)player.CurrentEnergy / player.MaxEnergy;

            if (paradoxText != null)
                paradoxText.text = $"Paradox: {player.IndividualParadox}/{player.MaxParadox}";

            if (paradoxSlider != null)
                paradoxSlider.value = (float)player.IndividualParadox / player.MaxParadox;

            if (scoreText != null)
                scoreText.text = $"Score: {player.Score} pts";

            if (missionText != null)
            {
                var mis = player.Inventory.ActiveMission;
                if (mis != null)
                {
                    bool done = player.Inventory.IsMissionComplete();
                    missionText.text = done ? $"<b>[COMPLETED]</b> {mis.Title}" : $"Target: {mis.TargetArtifactName} -> {mis.TargetDeliveryEra}";
                }
                else
                {
                    missionText.text = "No mission assigned.";
                }
            }

            if (cluesCounterText != null)
            {
                missionText.text += $" | Clues: {player.Inventory.CollectedClues.Count} | Artifacts: {player.Inventory.HeldArtifacts.Count}";
            }

            if (abilityButtonText != null && player.CharacterData != null)
            {
                if (player.AbilityCooldown > 0)
                    abilityButtonText.text = $"CD: {player.AbilityCooldown} turns";
                else
                    abilityButtonText.text = $"{player.CharacterData.AbilityName}";
            }
        }

        private void OnAbilityClicked()
        {
            if (boundPlayer != null && boundPlayer.CanUseAbility())
            {
                boundPlayer.TriggerAbility(out string summary);
                Debug.Log($"[PlayerHUD] {summary}");
            }
        }
    }
}
