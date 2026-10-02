using System;
using UnityEngine;

namespace Panahon.Data
{
    [CreateAssetMenu(fileName = "NewArtifactCard", menuName = "PANAHON/Cards/Artifact Card")]
    public class ArtifactCardData : CardData
    {
        [Header("Artifact Specifications")]
        [SerializeField] private EraType correctOriginEra;
        [SerializeField] private EraType displacedFoundEra;
        [SerializeField] private int pointsReward = 30;
        [TextArea(2, 4)]
        [SerializeField] private string temporalSignificance;

        public EraType CorrectOriginEra => correctOriginEra;
        public EraType DisplacedFoundEra => displacedFoundEra;
        public int PointsReward => pointsReward;
        public string TemporalSignificance => temporalSignificance;

        public void SetupArtifact(string id, string artifactTitle, string desc, EraType originEra,
            EraType foundEra, int points = 30, string significance = "")
        {
            Initialize(id, artifactTitle, desc, foundEra);
            correctOriginEra = originEra;
            displacedFoundEra = foundEra;
            pointsReward = points;
            temporalSignificance = significance;
        }
    }
}
