using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Data;
using Panahon.Core;

namespace Panahon.UI
{
    public class TimeGateDialogUI : MonoBehaviour
    {
        [Header("Dialog Panel")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text energyCostText;

        [Header("Buttons")]
        [SerializeField] private Button spanishEraButton;
        [SerializeField] private Button americanEraButton;
        [SerializeField] private Button japaneseEraButton;
        [SerializeField] private Button mythEraButton;
        [SerializeField] private Button passButton;

        private void Start()
        {
            if (spanishEraButton != null) spanishEraButton.onClick.AddListener(() => OnSelectEra(EraType.SpanishEra));
            if (americanEraButton != null) americanEraButton.onClick.AddListener(() => OnSelectEra(EraType.AmericanEra));
            if (japaneseEraButton != null) japaneseEraButton.onClick.AddListener(() => OnSelectEra(EraType.JapaneseEra));
            if (mythEraButton != null) mythEraButton.onClick.AddListener(() => OnSelectEra(EraType.MythLegendEra));
            if (passButton != null) passButton.onClick.AddListener(OnPass);

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            }

            HideDialog();
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(TurnPhase phase)
        {
            if (phase == TurnPhase.TimeGateDecision)
            {
                ShowDialog();
            }
            else
            {
                HideDialog();
            }
        }

        public void ShowDialog()
        {
            var activePlayer = TurnManager.Instance?.ActivePlayer;
            if (activePlayer == null) return;

            int cost = activePlayer.PlayerIndex == 1 ? 1 : 2; // Tala discount

            if (energyCostText != null)
            {
                energyCostText.text = $"Energy Cost: {cost} Energy (You have: {activePlayer.CurrentEnergy})";
            }

            // Disable button of current era
            if (spanishEraButton != null) spanishEraButton.interactable = (activePlayer.CurrentEra != EraType.SpanishEra && activePlayer.CurrentEnergy >= cost);
            if (americanEraButton != null) americanEraButton.interactable = (activePlayer.CurrentEra != EraType.AmericanEra && activePlayer.CurrentEnergy >= cost);
            if (japaneseEraButton != null) japaneseEraButton.interactable = (activePlayer.CurrentEra != EraType.JapaneseEra && activePlayer.CurrentEnergy >= cost);
            if (mythEraButton != null) mythEraButton.interactable = (activePlayer.CurrentEra != EraType.MythLegendEra && activePlayer.CurrentEnergy >= cost);

            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }
        }

        public void HideDialog()
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
        }

        private void OnSelectEra(EraType era)
        {
            HideDialog();
            TurnManager.Instance?.TravelThroughTimeGate(era);
        }

        private void OnPass()
        {
            HideDialog();
            TurnManager.Instance?.PassTimeGate();
        }
    }
}
