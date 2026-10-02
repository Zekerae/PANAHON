using System;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Data;

namespace Panahon.UI
{
    public class CardModalUI : MonoBehaviour
    {
        public static CardModalUI Instance { get; private set; }

        [Header("Modal Components")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Image cardHeaderBackground;
        [SerializeField] private Text cardTypeBadgeText;
        [SerializeField] private Text cardTitleText;
        [SerializeField] private Text cardEraText;
        [SerializeField] private Text cardDescriptionText;
        [SerializeField] private Text cardEffectText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseModal);
            }
            Hide();
        }

        public void ShowCard(string typeBadge, string title, string era, string description, string effects, Color headerColor)
        {
            if (cardTypeBadgeText != null) cardTypeBadgeText.text = typeBadge;
            if (cardTitleText != null) cardTitleText.text = title;
            if (cardEraText != null) cardEraText.text = era;
            if (cardDescriptionText != null) cardDescriptionText.text = description;
            if (cardEffectText != null) cardEffectText.text = effects;
            if (cardHeaderBackground != null) cardHeaderBackground.color = headerColor;

            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
        }

        public void CloseModal()
        {
            Hide();
        }

        public void Hide()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }
    }
}
