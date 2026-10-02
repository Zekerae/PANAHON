using System;
using UnityEngine;

namespace Panahon.Data
{
    [Serializable]
    public abstract class CardData : ScriptableObject
    {
        [Header("Card Metadata")]
        [SerializeField] private string cardId;
        [SerializeField] private string title;
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [SerializeField] private EraType era;

        public string CardId => cardId;
        public string Title => title;
        public string Description => description;
        public EraType Era => era;

        public virtual void Initialize(string id, string cardTitle, string desc, EraType eraType)
        {
            cardId = id;
            title = cardTitle;
            description = desc;
            era = eraType;
        }
    }
}
