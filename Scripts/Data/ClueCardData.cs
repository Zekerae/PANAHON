using System;
using UnityEngine;

namespace Panahon.Data
{
    public enum ClueCategory
    {
        Appearance,
        Purpose,
        HistoricalConnection,
        Location
    }

    [CreateAssetMenu(fileName = "NewClueCard", menuName = "PANAHON/Cards/Clue Card")]
    public class ClueCardData : CardData
    {
        [Header("Clue Details")]
        [SerializeField] private string targetArtifactId;
        [SerializeField] private ClueCategory category;
        [TextArea(2, 4)]
        [SerializeField] private string clueDetails;

        public string TargetArtifactId => targetArtifactId;
        public ClueCategory Category => category;
        public string ClueDetails => clueDetails;

        public void SetupClue(string id, string clueTitle, string desc, EraType eraType,
            string artifactId, ClueCategory cat, string details)
        {
            Initialize(id, clueTitle, desc, eraType);
            targetArtifactId = artifactId;
            category = cat;
            clueDetails = details;
        }
    }
}
