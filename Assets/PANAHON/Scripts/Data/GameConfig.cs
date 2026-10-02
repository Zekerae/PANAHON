using System;
using UnityEngine;

namespace Panahon.Data
{
    /// <summary>
    /// Central configuration ScriptableObject for tuning all PANAHON game balance parameters.
    /// Create via: Assets > Create > PANAHON > Game Config
    /// </summary>
    [CreateAssetMenu(fileName = "PanahonGameConfig", menuName = "PANAHON/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Layout (2.5D Tabletop)")]
        [Tooltip("Number of tiles per era track loop")]
        [Range(8, 16)] public int tilesPerEra = 10;
        [Tooltip("Radius of each era track ring from its center")]
        public float eraTrackRadius = 5.5f;
        [Tooltip("Spacing between era track centers")]
        public float eraTrackSpacing = 8f;
        [Tooltip("Tile scale for 2.5D quads")]
        public Vector2 tileScale = new Vector2(1.4f, 1.4f);
        [Tooltip("Height offset for landing positions (Z-axis in 2.5D)")]
        public float landingZOffset = -0.35f;

        [Header("Player Settings")]
        [Range(2, 4)] public int playerCount = 4;
        public int defaultStartingEnergy = 10;
        public int defaultMaxEnergy = 15;
        public int defaultMaxParadox = 5;
        public int defaultAbilityCooldown = 2;

        [Header("Time Gate")]
        [Tooltip("Base energy cost to travel through a Time Gate")]
        public int timeGateEnergyCost = 2;
        [Tooltip("Discounted cost for Rift-Walker (Tala)")]
        public int riftWalkerEnergyCost = 1;

        [Header("Timeline Meter (Shared Cooperative)")]
        [Range(5, 20)] public int maxTimelineLimit = 10;
        [Range(1, 15)] public int criticalWarningThreshold = 7;
        [Tooltip("Timeline stabilization bonus when artifact is correctly returned")]
        public int correctArtifactStabilization = 2;

        [Header("Paradox Settings")]
        [Tooltip("Energy penalty when individual paradox maxes out")]
        public int paradoxOverflowEnergyPenalty = 3;
        [Tooltip("Shared timeline damage when individual paradox overflows")]
        public int paradoxOverflowTimelineDamage = 1;

        [Header("Scoring")]
        public int missionCompletionPoints = 50;
        public int correctArtifactReturnPoints = 30;
        public int clueCollectionPoints = 5;
        public int artifactAcquisitionPoints = 15;
        public int energyToPointsRatio = 2;
        public int paradoxPenaltyPerPoint = 10;
        public int timelineStableBonusHigh = 20;
        public int timelineStableBonusMid = 10;
        public int timelineStableThresholdHigh = 3;
        public int timelineStableThresholdMid = 6;
        public int wrongArtifactParadoxPenalty = 2;

        [Header("Game Timer")]
        [Tooltip("Game time limit in real-time minutes")]
        public float gameTimeLimitMinutes = 40f;

        [Header("Movement Animation (2.5D)")]
        [Tooltip("Duration per tile hop in seconds")]
        public float moveStepDuration = 0.28f;
        [Tooltip("Arc height of the hop animation")]
        public float hopHeight = 0.45f;
        [Tooltip("Y component of hop (vertical in 2.5D)")]
        public float hopYFactor = 0.4f;
        [Tooltip("Z component of hop (depth in 2.5D)")]
        public float hopZFactor = 0.6f;
        [Tooltip("Delay between turns for readability")]
        public float turnTransitionDelay = 0.8f;

        [Header("Dice")]
        [Tooltip("Animation duration for dice roll visual")]
        public float diceRollAnimationDuration = 0.6f;
        public int diceMinValue = 1;
        public int diceMaxValue = 6;

        [Header("Camera (2.5D Orthographic)")]
        public float cameraOrthoSize = 13.5f;
        public Vector3 cameraPosition = new Vector3(0, 22f, -14f);
        public Vector3 cameraRotation = new Vector3(58f, 0, 0);
        public Color cameraBackgroundColor = new Color(0.06f, 0.08f, 0.12f);

        /// <summary>
        /// Returns the tile pattern for a standard era track.
        /// </summary>
        public TileType[] GetDefaultTilePattern()
        {
            return new TileType[]
            {
                TileType.Start,
                TileType.Clue,
                TileType.Event,
                TileType.Danger,
                TileType.Artifact,
                TileType.TimeGate,
                TileType.Clue,
                TileType.Event,
                TileType.Danger,
                TileType.TimeGate
            };
        }
    }
}
