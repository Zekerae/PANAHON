using System;
using UnityEngine;

namespace Panahon.Data
{
    [CreateAssetMenu(fileName = "NewMissionCard", menuName = "PANAHON/Cards/Mission Card")]
    public class MissionCardData : CardData
    {
        [Header("Mission Requirements")]
        [SerializeField] private string targetArtifactId;
        [SerializeField] private string targetArtifactName;
        [SerializeField] private EraType targetDeliveryEra;
        [SerializeField] private int requiredCluesCount = 2;
        [SerializeField] private int completionPoints = 50;
        [TextArea(1, 3)]
        [SerializeField] private string historicalContext;

        public string TargetArtifactId => targetArtifactId;
        public string TargetArtifactName => targetArtifactName;
        public EraType TargetDeliveryEra => targetDeliveryEra;
        public int RequiredCluesCount => requiredCluesCount;
        public int CompletionPoints => completionPoints;
        public string HistoricalContext => historicalContext;

        public void SetupMission(string id, string missionTitle, string desc, EraType sourceEra,
            string artifactId, string artifactName, EraType deliveryEra, int cluesCount = 2, int points = 50, string context = "")
        {
            Initialize(id, missionTitle, desc, sourceEra);
            targetArtifactId = artifactId;
            targetArtifactName = artifactName;
            targetDeliveryEra = deliveryEra;
            requiredCluesCount = cluesCount;
            completionPoints = points;
            historicalContext = context;
        }
    }
}
