using System;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Player
{
    [Serializable]
    public class PlayerInventory
    {
        [SerializeField] private MissionCardData activeMission;
        [SerializeField] private List<ClueCardData> collectedClues = new List<ClueCardData>();
        [SerializeField] private List<ArtifactCardData> heldArtifacts = new List<ArtifactCardData>();
        [SerializeField] private List<ArtifactCardData> returnedArtifacts = new List<ArtifactCardData>();

        public MissionCardData ActiveMission => activeMission;
        public IReadOnlyList<ClueCardData> CollectedClues => collectedClues;
        public IReadOnlyList<ArtifactCardData> HeldArtifacts => heldArtifacts;
        public IReadOnlyList<ArtifactCardData> ReturnedArtifacts => returnedArtifacts;

        public event Action<MissionCardData> OnMissionAssigned;
        public event Action<ClueCardData> OnClueAdded;
        public event Action<ArtifactCardData> OnArtifactObtained;
        public event Action<ArtifactCardData> OnArtifactReturned;

        public void AssignMission(MissionCardData mission)
        {
            activeMission = mission;
            OnMissionAssigned?.Invoke(mission);
        }

        public void AddClue(ClueCardData clue)
        {
            if (clue != null && !collectedClues.Contains(clue))
            {
                collectedClues.Add(clue);
                OnClueAdded?.Invoke(clue);
            }
        }

        public int GetClueCountForArtifact(string artifactId)
        {
            int count = 0;
            foreach (var clue in collectedClues)
            {
                if (clue != null && clue.TargetArtifactId == artifactId)
                {
                    count++;
                }
            }
            return count;
        }

        public bool CanAcquireArtifact(ArtifactCardData artifact)
        {
            if (artifact == null) return false;

            // Player can acquire if it's their mission artifact or if they have clues
            if (activeMission != null && activeMission.TargetArtifactId == artifact.CardId)
            {
                int cluesCount = GetClueCountForArtifact(artifact.CardId);
                return cluesCount >= activeMission.RequiredCluesCount;
            }

            // General artifact acquisition rule: requires at least 1 clue for non-mission artifacts
            return GetClueCountForArtifact(artifact.CardId) >= 1;
        }

        public void AcquireArtifact(ArtifactCardData artifact)
        {
            if (artifact != null && !heldArtifacts.Contains(artifact))
            {
                heldArtifacts.Add(artifact);
                OnArtifactObtained?.Invoke(artifact);
            }
        }

        public bool CanDeliverArtifact(ArtifactCardData artifact, EraType currentEra)
        {
            if (artifact == null || !heldArtifacts.Contains(artifact)) return false;

            // Check if returned to the correct timeline
            return artifact.CorrectOriginEra == currentEra;
        }

        public bool ReturnArtifact(ArtifactCardData artifact, EraType currentEra, out bool isCorrectEra)
        {
            isCorrectEra = false;
            if (artifact == null || !heldArtifacts.Contains(artifact)) return false;

            heldArtifacts.Remove(artifact);
            returnedArtifacts.Add(artifact);

            isCorrectEra = (artifact.CorrectOriginEra == currentEra);
            OnArtifactReturned?.Invoke(artifact);
            return true;
        }

        public bool IsMissionComplete()
        {
            if (activeMission == null) return false;

            foreach (var returned in returnedArtifacts)
            {
                if (returned != null && returned.CardId == activeMission.TargetArtifactId && returned.CorrectOriginEra == activeMission.TargetDeliveryEra)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
