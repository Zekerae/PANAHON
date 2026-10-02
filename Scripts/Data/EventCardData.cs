using System;
using UnityEngine;

namespace Panahon.Data
{
    [CreateAssetMenu(fileName = "NewEventCard", menuName = "PANAHON/Cards/Event Card")]
    public class EventCardData : CardData
    {
        [Header("Event Outcomes")]
        [SerializeField] private int energyChange = 0;       // Positive gains energy, negative loses
        [SerializeField] private int timelineStabilityChange = 0; // Positive increases meter (bad/destabilizing), negative heals/stabilizes
        [SerializeField] private int individualParadoxChange = 0; // +/- individual paradox
        [SerializeField] private int bonusMovement = 0;
        [SerializeField] private int pointsBonus = 0;
        [SerializeField] private bool allowsFreeEraWarp = false;

        public int EnergyChange => energyChange;
        public int TimelineStabilityChange => timelineStabilityChange;
        public int IndividualParadoxChange => individualParadoxChange;
        public int BonusMovement => bonusMovement;
        public int PointsBonus => pointsBonus;
        public bool AllowsFreeEraWarp => allowsFreeEraWarp;

        public void SetupEvent(string id, string eventTitle, string desc, EraType eraType,
            int energy = 0, int timelineDelta = 0, int paradoxDelta = 0, int bonusMove = 0, int points = 0, bool freeWarp = false)
        {
            Initialize(id, eventTitle, desc, eraType);
            energyChange = energy;
            timelineStabilityChange = timelineDelta;
            individualParadoxChange = paradoxDelta;
            bonusMovement = bonusMove;
            pointsBonus = points;
            allowsFreeEraWarp = freeWarp;
        }
    }
}
