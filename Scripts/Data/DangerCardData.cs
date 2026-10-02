using System;
using UnityEngine;

namespace Panahon.Data
{
    [CreateAssetMenu(fileName = "NewDangerCard", menuName = "PANAHON/Cards/Danger Card")]
    public class DangerCardData : CardData
    {
        [Header("Hazard Penalties")]
        [SerializeField] private int energyLoss = 2;
        [SerializeField] private int individualParadoxIncrease = 1;
        [SerializeField] private int timelineDestabilization = 1; // + to shared timeline meter
        [SerializeField] private int pointsPenalty = 5;
        [SerializeField] private bool forcedRelocationToStart = false;
        [SerializeField] private bool loseNextTurn = false;

        public int EnergyLoss => energyLoss;
        public int IndividualParadoxIncrease => individualParadoxIncrease;
        public int TimelineDestabilization => timelineDestabilization;
        public int PointsPenalty => pointsPenalty;
        public bool ForcedRelocationToStart => forcedRelocationToStart;
        public bool LoseNextTurn => loseNextTurn;

        public void SetupDanger(string id, string dangerTitle, string desc, EraType eraType,
            int energyDrain = 2, int paradoxInc = 1, int timelineDmg = 1, int penalty = 5, bool sendToStart = false, bool skipTurn = false)
        {
            Initialize(id, dangerTitle, desc, eraType);
            energyLoss = energyDrain;
            individualParadoxIncrease = paradoxInc;
            timelineDestabilization = timelineDmg;
            pointsPenalty = penalty;
            forcedRelocationToStart = sendToStart;
            loseNextTurn = skipTurn;
        }
    }
}
