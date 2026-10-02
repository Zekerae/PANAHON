using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Core;

namespace Panahon.UI
{
    public class DiceRollerUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button rollButton;
        [SerializeField] private Text rollButtonText;
        [SerializeField] private Text diceResultText;
        [SerializeField] private Image diceIcon;

        [Header("Animation Settings")]
        [SerializeField] private float rollDuration = 0.6f;

        private bool isRolling = false;

        private void Start()
        {
            if (rollButton != null)
            {
                rollButton.onClick.AddListener(OnRollButtonClicked);
            }

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            }
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
            bool canRoll = (phase == TurnPhase.TurnStart || phase == TurnPhase.RollForFirstTurn);
            if (rollButton != null)
            {
                rollButton.interactable = canRoll && !isRolling;
            }

            if (rollButtonText != null)
            {
                rollButtonText.text = phase == TurnPhase.RollForFirstTurn ? "Roll for Order" : "Roll Movement";
            }
        }

        private void OnRollButtonClicked()
        {
            if (isRolling) return;
            StartCoroutine(AnimateDiceRoll());
        }

        private IEnumerator AnimateDiceRoll()
        {
            isRolling = true;
            if (rollButton != null) rollButton.interactable = false;

            float elapsed = 0f;
            int lastNum = 1;

            while (elapsed < rollDuration)
            {
                elapsed += 0.05f;
                lastNum = UnityEngine.Random.Range(1, 7);
                if (diceResultText != null) diceResultText.text = lastNum.ToString();
                yield return new WaitForSeconds(0.05f);
            }

            isRolling = false;

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.RollMovementDice();
            }
        }
    }
}
